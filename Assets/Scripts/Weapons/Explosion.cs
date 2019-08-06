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


	private void Start()
	{
		explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, exRadius);

		foreach (Collider col in colliders)
		{
			PlayerMove player = col.GetComponent<PlayerMove>();
			if (player != null)
			{
				player.AddKnockback(explosionPos, force);
			}
			IDamagable target = col.GetComponent<IDamagable>();
			if (target != null)
			{
				var playerhp = col.GetComponent<PlayerStats>();
				if (playerhp != null)
				{
					playerhp.TakeDamage(baseDamage + (playerhp.currentHealth / 10), DamageTypes.Physical);
				}
				//DamageTarget(target);
			}
		}

		Invoke("Kill", 1);
	}

	public override void DamageTarget(IDamagable target)
	{
		
		target.TakeDamage(baseDamage, DamageTypes.Physical);
	}

	void Kill()
	{
		Destroy(gameObject);
	}
}
