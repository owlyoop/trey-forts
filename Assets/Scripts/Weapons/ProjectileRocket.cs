using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRocket : Projectile
{
	

	public Explosion explosion;


	private void Start()
	{
		Physics.IgnoreCollision(this.GetComponent<Collider>(), player.GetComponent<Collider>());
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
		if (player.photonView.IsMine)
		{
			if (OwnerPunID == player.GetComponent<PhotonView>().ViewID && col.GetComponent<PlayerStats>() != player)
			{
                bool isValid = true;
                if (col.GetComponent<PlayerHitbox>() != null && col.GetComponent<PlayerHitbox>().player == player)
                {
                    isValid = false;
                }

                if (isValid)
                {
                    player.GetComponent<PhotonView>().RPC("SpawnExplosion", RpcTarget.AllViaServer, OwnerPunID, transform.position, baseDamage, damageType);
                    Kill();
                }
			}
		}
	}
	

}
