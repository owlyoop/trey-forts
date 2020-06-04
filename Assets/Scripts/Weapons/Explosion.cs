using KinematicCharacterController.Owly;
using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public int OwnerID;
	public PlayerStats player;

    public List<IDamagable> uniqueTargets = new List<IDamagable>();

    public StatusEffect firePrefab;

    private void Start()
	{
		explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, exRadius);
        uniqueTargets = base.GetUniqueDamagableTargets(colliders);

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
        foreach (IDamagable targets in uniqueTargets)
        {
            targets.TakeDamage(baseDamage, damageType, player, this.transform.position, PlayerStats.DamageIndicatorType.Directional);
        }
		Invoke("Kill", 1);
	}

	public override void DamageTarget(IDamagable target)
	{
		target.TakeDamage(baseDamage, DamageTypes.Physical, player, this.transform.position, PlayerStats.DamageIndicatorType.Directional);
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
