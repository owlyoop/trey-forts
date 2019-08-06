using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMove : NetworkBehaviour
{

	public CharacterController charControl;
	public CapsuleCollider capsule;
	public Camera cam;

	public float moveSpeed;
	float baseMovespeed;
	public float jumpSpeed;
	private float startingJumpSpeed;
	public float runAcceleration = 14.0f;         // Ground accel
	public float runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
	public float airAcceleration = 2.0f;          // Air accel
	public float airDecceleration = 2.0f;         // Deacceleration experienced when ooposite strafing
	public float airControl = 0.3f;               // How precise air control is
	public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
	public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
	public float moveScale = 1.0f;
	public float gravity = 25f;

	public float friction = 6;
	private float playerFriction = 0.0f;

	private bool wishJump;
	private bool canJump;
	Vector3 groundNormal;
	RaycastHit debughit;

	public float snapToGround = 0.35f;
	float horiz;
	float forward;

	public float theCurrSlope;

	public Transform raynorth;
	public Transform raysouth;
	public Transform rayeast;
	public Transform raywest;

	public Vector3 playerVelocity = Vector3.zero;

	private void Start()
	{
		baseMovespeed = moveSpeed;
		charControl = GetComponent<CharacterController>();
		capsule = GetComponent<CapsuleCollider>();
		canJump = true;
		//charControl.material.dynamicFriction = 0f;
		//charControl.material.staticFriction = 0f;
		//charControl.material.frictionCombine = PhysicMaterialCombine.Minimum;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(debughit.point, 0.1f);
	}

	private void Update()
	{

		if (!isLocalPlayer)
		{
			cam.GetComponent<PlayerLook>().enabled = false;
			cam.enabled = false;
			return;
		}
		else
		{
			cam.GetComponent<PlayerLook>().enabled = true;
			cam.enabled = true;
		}

		if (charControl.enabled == false)
		{
			return;
		}


		horiz = Input.GetAxisRaw("Horizontal");
		forward = Input.GetAxisRaw("Vertical");


		QueueJump();
		if (IsGrounded())
		{
			snapToGround = 0.35f;
			GroundMove();
		}
		if (!charControl.isGrounded || !IsGrounded())
		{
			AirbourneMove();
		}
		QueueJump();


		HandleCameraInput();
		HandleCharacterInput();

		

		RaycastHit hit;

		int layermask = 1 << 9;
		layermask = ~layermask;

		if (Physics.Raycast(this.transform.position, Vector3.down, out hit, 1f, layermask)
			|| Physics.Raycast(raynorth.position, Vector3.down, out hit, 1f, layermask)
			|| Physics.Raycast(raysouth.position, Vector3.down, out hit, 1f, layermask)
			|| Physics.Raycast(rayeast.position, Vector3.down, out hit, 1f, layermask)
			|| Physics.Raycast(raywest.position, Vector3.down, out hit, 1f, layermask))
		{
			debughit = hit;
			groundNormal = hit.normal;
			Vector3 n = hit.normal;
			Vector3 groundParallel = Vector3.Cross(transform.up, n);
			Vector3 slopeParallel = Vector3.Cross(groundParallel, n);
			Debug.DrawRay(this.transform.position, Vector3.down, Color.yellow);
			Debug.DrawRay(raynorth.position, Vector3.down, Color.yellow);
			Debug.DrawRay(raysouth.position, Vector3.down, Color.yellow);
			Debug.DrawRay(rayeast.position, Vector3.down, Color.yellow);
			Debug.DrawRay(raywest.position, Vector3.down, Color.yellow);
			Debug.DrawRay(hit.point, slopeParallel * 10, Color.green);

			float currentSlope = Mathf.Round(Vector3.Angle(hit.normal, transform.up));
			//Debug.Log(currentSlope);

			theCurrSlope = currentSlope;

			if (currentSlope > 46f)
			{
				//charControl.material.dynamicFriction = 0f;
				//charControl.material.staticFriction = 0f;
				//charControl.material.frictionCombine = PhysicMaterialCombine.Minimum;
				wishJump = false;
				canJump = false;
				snapToGround = 0.1f;
				//charControl.transform.position += slopeParallel * (gravity / 2.1f) * Time.deltaTime;
				//charControl.Move(slopeParallel * (gravity / 2f) * Time.deltaTime);
				
				playerVelocity.x += slopeParallel.x * Time.deltaTime * 36f * (90 / currentSlope);
				playerVelocity.z += slopeParallel.z * Time.deltaTime * 36f * (90 / currentSlope);
				//playerVelocity.y += slopeParallel.y * Time.deltaTime * 36f * (90 / currentSlope);

				//playerVelocity.x = ((1f - slopeParallel.y) * slopeParallel.x) * 3f;
				//playerVelocity.z = ((1f - slopeParallel.y) * slopeParallel.z) * 3f;
			}
			else
			{
				moveSpeed = baseMovespeed + (currentSlope /75);
				canJump = true;

				if (currentSlope > 40f)
				{
					snapToGround = 0.45f;
					if (wishJump && canJump)
					{
						snapToGround = 0;
						playerVelocity.y = 0;
						playerVelocity.y = jumpSpeed;
						QueueJump();
					}
				}
			}


		}

		if (IsGrounded())
		{
			if (wishJump && canJump)
			{
				snapToGround = 0;
				playerVelocity.y = 0;
				playerVelocity.y = jumpSpeed;
				QueueJump();
			}
		}

		charControl.Move(playerVelocity * Time.deltaTime);
	}



	private bool IsGrounded()
	{
		if (charControl.isGrounded)
			return true;

		Vector3 bottom = charControl.transform.position - new Vector3(0, charControl.height / 2, 0);

		RaycastHit hit;
		if (Physics.Raycast(bottom, new Vector3(0, -1, 0), out hit, snapToGround))
		{
			charControl.Move(new Vector3(0, -hit.distance, 0));
			return true;
		}

		return false;
	}

	/**
	* Queues the next jump just like in Q3
	*/
	private void QueueJump()
	{
		if (canJump)
		{
			if (Input.GetButtonDown("Jump"))
			{
				snapToGround = 0;
				wishJump = true;
			}
			if (Input.GetButtonUp("Jump"))
			{
				wishJump = false;
				//snapToGround = 0.3f;
			}
		}

	}

	void GroundMove()
	{
		Vector3 wishDir;

		// Do not apply friction if the player is queueing up the next jump
		if (!wishJump)
			ApplyFriction(1.0f);
		else
		{
			ApplyFriction(0);
			snapToGround = 0;
		}
			

		wishDir = new Vector3(horiz, 0, forward);
		wishDir = transform.TransformDirection(wishDir);
		wishDir.Normalize();

		//wishDir = Vector3.ProjectOnPlane(wishDir, groundNormal);
		

		var wishSpeed = wishDir.magnitude;
		wishSpeed *= moveSpeed;

		playerVelocity.y = 0;

		Accelerate(wishDir, wishSpeed, runAcceleration);


	}

	void Accelerate(Vector3 wishdir, float wishspeed, float accel)
	{
		float addSpeed;
		float accelSpeed;
		float currentSpeed;

		currentSpeed = Vector3.Dot(playerVelocity, wishdir);
		addSpeed = wishspeed - currentSpeed;
		if (addSpeed <= 0)
			return;

		accelSpeed = accel * Time.deltaTime * wishspeed;
		if (accelSpeed > addSpeed)
			accelSpeed = addSpeed;

		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, charControl.radius, Vector3.down, out hitInfo,
						   charControl.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
		wishdir = Vector3.ProjectOnPlane(wishdir, hitInfo.normal).normalized;

		playerVelocity.x += accelSpeed * wishdir.x;
		playerVelocity.z += accelSpeed * wishdir.z;
	}

	void AirbourneMove()
	{
		Vector3 wishDir;
		float wishVel = airAcceleration;
		float accel;

		float scale = CmdScale();

		forward = Input.GetAxisRaw("Vertical");
		horiz = Input.GetAxisRaw("Horizontal");

		wishDir = new Vector3(horiz, 0, forward);
		wishDir = transform.TransformDirection(wishDir);

		float wishSpeed = wishDir.magnitude;
		wishSpeed *= moveSpeed;

		wishDir.Normalize();
		wishSpeed *= scale;

		// CPM air control
		float wishSpeed2 = wishSpeed;
		if (Vector3.Dot(playerVelocity, wishDir) < 0)
			accel = airDecceleration;
		else
			accel = airAcceleration;
		// If the player is ONLY strafing left or right
		if (forward == 0 && horiz != 0)
		{
			if (wishSpeed > sideStrafeSpeed)
				wishSpeed = sideStrafeSpeed;
			accel = sideStrafeAcceleration;
		}


		Accelerate(wishDir, wishSpeed, accel);

		if (airControl > 0)
			AirControl(wishDir, wishSpeed2);

		if (wishJump && canJump)
		{
			RaycastHit hit;
			Vector3 bottom = charControl.transform.position - new Vector3(0, charControl.height / 2, 0);
			if (Physics.Raycast(bottom, new Vector3(0, -1, 0), out hit, 0.4f))
			{
				snapToGround = 0;
				playerVelocity.y = 0;
				playerVelocity.y = jumpSpeed;
			}
		}

		playerVelocity.y -= gravity * Time.deltaTime;
	}

	private void AirControl(Vector3 wishDir, float wishSpeed)
	{
		float zSpeed;
		float speed;
		float dot;
		float k;

		if (Mathf.Abs(forward) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
			return;
		zSpeed = playerVelocity.y;
		playerVelocity.y = 0;

		speed = playerVelocity.magnitude;
		playerVelocity.Normalize();

		dot = Vector3.Dot(playerVelocity, wishDir);
		k = 32;
		k *= airControl * dot * dot * Time.deltaTime;

		// Change direction while slowing down
		if (dot > 0)
		{
			playerVelocity.x = playerVelocity.x * speed + wishDir.x * k;
			playerVelocity.y = playerVelocity.y * speed + wishDir.y * k;
			playerVelocity.z = playerVelocity.z * speed + wishDir.z * k;

			playerVelocity.Normalize();
		}

		playerVelocity.x *= speed;
		playerVelocity.y = zSpeed; // Note this line
		playerVelocity.z *= speed;
	}

	void ApplyFriction(float t)
	{
		Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();
		float speed;
		float newspeed;
		float control;
		float drop;

		vec.y = 0.0f;
		speed = vec.magnitude;
		drop = 0.0f;

		/* Only if the player is on the ground then apply friction */
		if (IsGrounded())
		{
			control = speed < runDeacceleration ? runDeacceleration : speed;
			drop = control * friction * Time.deltaTime * t;
		}

		newspeed = speed - drop;
		playerFriction = newspeed;
		if (newspeed < 0)
			newspeed = 0;
		if (speed > 0)
			newspeed /= speed;

		playerVelocity.x *= newspeed;
		playerVelocity.z *= newspeed;
	}

	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body != null && !body.isKinematic)
		{
			body.velocity += hit.controller.velocity / body.mass;
			playerVelocity += (body.velocity * (body.mass / 10)) * Time.deltaTime;
		}
			
	}

	public void AddKnockback(Vector3 hitOrigin, float force)
	{
		Vector3 wishdir;
		//hitOrigin = transform.TransformDirection(hitOrigin);
		//charCenter = (transform.TransformDirection(GetComponent<CapsuleCollider>().center));
		wishdir = hitOrigin - transform.position;
		wishdir = -wishdir.normalized;

		//playerVelocity += Vector3.Lerp(playerVelocity, wishdir, 0.8f).normalized * (force / 2f) * Time.deltaTime;
		playerVelocity += wishdir * force;

	}


	/*
============
PM_CmdScale

Returns the scale factor to apply to cmd movements
This allows the clients to use axial -127 to 127 values for all directions
without getting a sqrt(2) distortion in speed.
============
*/
	private float CmdScale()
	{
		int max;
		float total;
		float scale;

		max = (int)Mathf.Abs(forward);
		if (Mathf.Abs(horiz) > max)
			max = (int)Mathf.Abs(horiz);
		if (max <= 0)
			return 0;

		total = Mathf.Sqrt(forward * forward + horiz * horiz);
		scale = moveSpeed * max / (moveScale * total);

		return scale;
	}


	void HandleCharacterInput()
	{

	}

	void HandleCameraInput()
	{

	}
}
