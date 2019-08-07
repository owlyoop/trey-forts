using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectMover : WeaponMotor
{

	float distance;
	public Camera cam;
	public Transform gunEnd;

	PlayerStats playersStats;
	
	private Vector3 shootDirection;
	private LineRenderer laserLine;
	public bool isHoldingObject;
	public bool isRotatingObject;
	Quaternion origRot;
	Quaternion origCamRot;
	public Rigidbody theObject;
	bool isFrozen;

	public float mCorrectionForce = 2000.0f;
	public float allowedRange = 12f;

	public float rotationSpeed = 100f;
	
	
	public string debugID;


	private void Start()
	{
		
		//cam = GetComponentInParent<WeaponSlots>().GetComponentInParent<Camera>();
		playersStats = cam.GetComponentInParent<PlayerStats>();
		laserLine = GetComponent<LineRenderer>();
		isHoldingObject = false;
		isRotatingObject = false;
		isFrozen = false;

		networkObjToSpawn = null;
	}

	public override void PrimaryFire()
	{
		RaycastHit shot;
		Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
		shootDirection = cam.transform.forward;

		int layermask = 1 << 9;
		layermask = ~layermask;

		if (Physics.Raycast(rayOrigin, shootDirection, out shot, allowedRange, layermask) && !isHoldingObject)
		{
			if (shot.collider.GetComponent<FortwarsProp>() != null)
			{
				theObject = shot.rigidbody;
				distance = shot.distance;
				origRot = theObject.transform.rotation;
				origCamRot = cam.transform.rotation;
				isHoldingObject = true;
				isFrozen = false;
				theObject.useGravity = false;
				theObject.isKinematic = false;
				theObject.constraints = RigidbodyConstraints.None;
				theObject.freezeRotation = false;
				//idOfObj = shot.collider.GetComponent<FortwarsProp>().nid;
				//cam.GetComponentInParent<PlayerInput>().holdingPropID = shot.collider.GetComponent<FortwarsProp>().nid;
				//cam.GetComponentInParent<PlayerInput>().debugID = shot.collider.GetComponent<FortwarsProp>().nid.ToString();
				//Debug.Log(idOfObj.ToString());
				//debugID = idOfObj.ToString();
			}
		}
		
	}

	private void FixedUpdate()
	{

		if (isHoldingObject)
		{
			Vector3 targetPoint = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
			targetPoint += cam.transform.forward * distance;
			Vector3 force = targetPoint - theObject.transform.position;

			laserLine.SetPosition(0, gunEnd.position);
			laserLine.SetPosition(1, targetPoint);

			

			if (!isFrozen)
			{
				if (!isRotatingObject)
				{
					theObject.transform.eulerAngles = new Vector3(origRot.eulerAngles.x,
					(cam.transform.eulerAngles.y - origCamRot.eulerAngles.y) + origRot.eulerAngles.y,
					(cam.transform.eulerAngles.z - origCamRot.eulerAngles.z) + origRot.eulerAngles.z);
				}
			}

			if (isRotatingObject)
			{
				UseButtonHolding();

				float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;
				float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
				

				theObject.freezeRotation = false;

				theObject.transform.Rotate(Vector3.up, -rotX, Space.World);
				theObject.transform.Rotate(cam.transform.right, rotY, Space.World);
				
			}

			theObject.velocity = force.normalized * theObject.velocity.magnitude;
			theObject.AddForce(force * mCorrectionForce);
			theObject.velocity *= Mathf.Min(1.0f, force.magnitude / 2);
			theObject.angularVelocity *= Mathf.Min(1.0f, force.magnitude / 2);

			if (theObject.velocity.magnitude > 10f)
			{
				theObject.velocity = theObject.velocity / 2.4f;
				//theObject.AddForce(-theObject.velocity /2f);
			}
			
			

		}

		//UseButtonHolding();
	}

	public override void PrimaryFireButtonUp()
	{
		if (isHoldingObject && theObject != null)
		{
			theObject.constraints = RigidbodyConstraints.FreezeAll;
			theObject.freezeRotation = true;
			theObject.useGravity = false;
			isFrozen = true;
			isHoldingObject = false;

			int propIndex = 0;

			if (theObject.GetComponent<FortwarsProp>().player.PropsOwnedByPlayer != null)
			{
				for (int i = 0; i < theObject.GetComponent<FortwarsProp>().player.PropsOwnedByPlayer.Count; i++)
				{
					if (theObject.gameObject == theObject.GetComponent<FortwarsProp>().player.PropsOwnedByPlayer[i])
					{
						propIndex = i;
						Debug.Log(propIndex.ToString());
					}
				}
			}
			

		
			
			playersStats.GetComponent<PhotonView>().RPC("UpdateFortwarsPropTransform", RpcTarget.AllViaServer,
				theObject.GetComponent<FortwarsProp>().idOfOwner, propIndex, theObject.transform.position, theObject.transform.rotation);

			propIndex = 0;
		}
	}

	public override void SecondaryFire()
	{

	}

	public override void UseButtonHolding()
	{
		isRotatingObject = true;
		Cursor.lockState = CursorLockMode.None;

	}

	public override void UseButtonUp()
	{
		origRot = theObject.transform.rotation;
		origCamRot = cam.transform.rotation;
		isRotatingObject = false;
		theObject.freezeRotation = true;
		//cam.GetComponent<PlayerLook>().viewLocked = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public override void ScrollWheelUp()
	{
		if (isHoldingObject)
		{
			distance = distance + 0.5f;
			if (distance > allowedRange)
			{
				distance = allowedRange;
			}
		}
	}

	public override void ScrollWheelDown()
	{
		if (isHoldingObject)
		{
			distance = distance - 0.5f;
			if (distance < 1.5f)
				distance = 1.5f;
		}
	}

	public override void UpdateUI()
	{
		playersStats = GetComponentInParent<WeaponSlots>().player;
		playersStats.ui.ammoAmount.text = "";
		playersStats.ui.ammoInClip.text = "";
	}
}
