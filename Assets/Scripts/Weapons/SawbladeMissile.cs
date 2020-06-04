using KinematicCharacterController.Owly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawbladeMissile : Projectile
{
	

	float accel = 0f;
	public float accelSpeed = 0.03f;
	public float accelCap = 0.3f;
	public float stickLife = 2f;

	public int maxBounces = 1;
	public int numBounces = 0;


	public int DamagePerTick = 2;
	public float TickDelay = 0.1f;
	private float nextActionTime = 0f;

	Rigidbody rb;

	Vector3 currentDirection;
	Vector3 newDir;
	Vector3 oldDir;
	ContactPoint hitpnt;

	public GameObject mesh;

	float timer = 0f;

	bool isDamaging = false;
	bool isStuck = false;

    public LayerMask layermask;

	Vector3 startPos;
	

	private void Start()
	{
		player.GetComponent<MyCharacterController>().IgnoredColliders.Add(this.GetComponent<BoxCollider>());
		currentDirection = transform.forward;
		startPos = this.transform.position;
	}

	private void FixedUpdate()
	{
		if (numBounces <= maxBounces)
		{
			accel += accelSpeed;
			if (accel > accelCap)
				accel = accelCap;

			transform.position += currentDirection * speed * Time.fixedDeltaTime;
			currentDirection += Vector3.down * accel * Time.fixedDeltaTime;

			transform.rotation = Quaternion.LookRotation(currentDirection);

			Debug.DrawRay(this.transform.position, currentDirection, Color.blue);
			//Debug.DrawRay(hitpnt.point, hitpnt.normal, Color.red);
			//Debug.DrawRay(hitpnt.point, newDir, Color.green);
		}

		if (numBounces > maxBounces)
		{
			transform.position += oldDir * (speed / 100f) * Time.fixedDeltaTime;
		}
	}

	private void Update()
	{
		mesh.transform.Rotate(Vector3.up * 400f * Time.deltaTime);
	}

	void Kill()
	{
		isDamaging = false;
		Destroy(gameObject);
	}

	void DealDamage(IDamagable target)
	{
		timer = Time.time;
		if (isDamaging)
		{
			if (Time.time > nextActionTime)
			{
				nextActionTime = Time.time + TickDelay;
				target.TakeDamage(DamagePerTick, damageType, player, this.transform.position, PlayerStats.DamageIndicatorType.Directional);
			}
		}
	}

	private void OnTriggerEnter(Collider col)
	{
        if (col.gameObject.GetComponent<PlayerStats>() != player && layermask == (layermask | (1 << col.gameObject.layer)))
        {
            if (numBounces <= maxBounces && col.GetComponent<IDamagable>() != null)
            {
                IDamagable target = col.GetComponent<IDamagable>();

                target.TakeDamage(baseDamage, damageType, player, this.transform.position, PlayerStats.DamageIndicatorType.Directional);
            }

        }
    }

	private void OnCollisionEnter(Collision col)
	{
        if (col.gameObject.GetComponent<PlayerStats>() != player && layermask == (layermask | (1 << col.gameObject.layer)))
        {
            if (numBounces <= maxBounces)
            {

                hitpnt = col.GetContact(0);

                newDir = Vector3.Reflect(currentDirection, hitpnt.normal);
                accel = 0f;
                oldDir = currentDirection;
                currentDirection = newDir;

                //transform.rotation = Quaternion.LookRotation(currentDirection);
                numBounces++;

            }

            if (numBounces > maxBounces)
            {
                Invoke("Kill", stickLife);
                isStuck = true;
                Debug.Log("stuck now");
                currentDirection = Vector3.zero;
            }

        }
    }

	private void OnTriggerStay(Collider other)
	{
		if (isStuck)
		{
			if (other.GetComponent<IDamagable>() != null)
			{
				IDamagable target = other.GetComponent<IDamagable>();
				isDamaging = true;
				DealDamage(target);
			}
		}
	}



	private void OnTriggerExit(Collider other)
	{
		if (isStuck)
		{
			if (other.GetComponent<IDamagable>() != null)
			{
				isDamaging = false;
			}
		}
	}
}
