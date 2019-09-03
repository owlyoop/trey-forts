using KinematicCharacterController.Owly;
using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Explosion : Damager
{
	public float exRadius = 4.0f;
	public float force = 10;
	private Vector3 explosionPos;

	[System.NonSerialized]
	public int baseDamage;
	[System.NonSerialized]
	public Damager.DamageTypes damageType;

	private Vector3 impact;
	private Vector3 velocityToAdd;
	private Vector3 wishDirection;
	private Vector3 playerPos;
	private Vector3 playerCenter;

	public bool damageThePlayer = true; // should the explosion damage the source/player?

	public int OwnerPunID;
	public PlayerStats player;

    public List<PlayerStats> hitPlayers = new List<PlayerStats>();

    private void Start()
	{
		explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, exRadius);
        List<PlayerStats> uniquePlayers = base.GetUniqueTargets(colliders);

        foreach (Collider col in colliders)
		{
            // Character controller check to add explosion force
			MyCharacterController player = col.GetComponent<MyCharacterController>();
			if (player != null)
			{
				player.GetComponent<KinematicCharacterMotor>().ForceUnground();
				playerCenter = player.GetComponent<KinematicCharacterMotor>().transform.position;

				playerPos = playerCenter + new Vector3(0, 1f, 0);
				wishDirection = transform.position - playerPos;
				velocityToAdd = -wishDirection.normalized * force;

				player.AddVelocity(velocityToAdd);
			}

            //Actual damage check. Idamagable.
			IDamagable target = col.GetComponent<IDamagable>();
			if (target != null)
			{
                
                var propHit = col.GetComponent<FortwarsProp>();

                if (propHit != null)
                {
                    propHit.TakeDamage(OwnerPunID, baseDamage, damageType);
                }

                if (col.GetComponent<TrainingDummy>() != null)
                {
                    target.TakeDamage(OwnerPunID, baseDamage, damageType);
                }
			}

            // Rigidbody test. ragdolls and physic props.
			Rigidbody rb = col.GetComponent<Rigidbody>();
			if (rb != null)
			{
				if (rb.GetComponent<CharacterJoint>() != null)
				{
					rb.AddForce(GetVelocityToAdd(rb.gameObject, force * 48f));
				}
				else
				{
					rb.AddForce(GetVelocityToAdd(rb.gameObject, force));
				}
				
			}
		}


        foreach (PlayerStats targets in uniquePlayers)
        {
            targets.TakeDamage(OwnerPunID, baseDamage, damageType);
        }
		Invoke("Kill", 1);
	}

	public override void DamageTarget(IDamagable target)
	{
		
		target.TakeDamage(OwnerPunID, baseDamage, DamageTypes.Physical);
	}

	Vector3 GetVelocityToAdd(GameObject go, float _force)
	{
		Vector3 VelocityToAdd;
		Vector3 wishDirection;

		wishDirection = transform.position - go.transform.position;
		VelocityToAdd = -wishDirection.normalized * _force;
		return VelocityToAdd;
	}

	void Kill()
	{
		Destroy(gameObject);
	}
}
