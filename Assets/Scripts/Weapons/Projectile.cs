using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
	[SyncVar]
	public float speed = 1;
	[SyncVar]
	public float life = 5;
	[System.NonSerialized]
	public int baseDamage;
	[System.NonSerialized]
	public Damager.DamageTypes damageType;


	public Explosion explosion;


	private void Start()
	{
		Invoke("Kill", life);
	}

	void Kill()
	{
		Destroy(gameObject);
	}

	private void FixedUpdate()
	{
		transform.position += transform.forward * speed * Time.fixedDeltaTime;
	}

	private void OnTriggerEnter(Collider col)
	{
		//CmdOnCollision();

		if (isServer)
		{
			RpcExplode();
		}
	}

	[Command]
	void CmdOnCollision()
	{
		RpcExplode();
	}

	[ClientRpc]
	void RpcExplode()
	{
		Explosion newExp = Instantiate(explosion, gameObject.transform.position, transform.rotation);
		newExp.baseDamage = baseDamage;
		newExp.damageType = damageType;
		
		//Debug.Log(baseDamage.ToString() + "projectile");
		Kill();
	}
}
