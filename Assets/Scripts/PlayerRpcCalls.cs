using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerRpcCalls : MonoBehaviourPunCallbacks
{

	[PunRPC]
	void FireProjectile(int OwnerPunID, string name, Vector3 shotPoint, Vector3 spawnPosition, float projectileSpeed, float projectileLife, int baseDamage, Damager.DamageTypes damageType)
	{

		GameObject go = Instantiate(Resources.Load<GameObject>(name), spawnPosition, new Quaternion(0,0,0,0));
		go.transform.LookAt(shotPoint);


		go.GetComponent<Projectile>().player = GetComponent<PlayerStats>();
		go.GetComponent<Projectile>().OwnerPunID = OwnerPunID;
		go.GetComponent<Projectile>().speed = projectileSpeed;
		go.GetComponent<Projectile>().life = projectileLife;
		go.GetComponent<Projectile>().baseDamage = baseDamage;
		go.GetComponent<Projectile>().damageType = damageType;
	}

	[PunRPC]
	void SpawnExplosion(int OwnerPunID,  Vector3 spawnPosition, int baseDamage, Damager.DamageTypes damageType)
	{
		GameObject explosion = Instantiate(Resources.Load<GameObject>("Explosion"), spawnPosition, new Quaternion(0, 0, 0, 0));
		explosion.GetComponent<Explosion>().player = GetComponent<PlayerStats>();
		explosion.GetComponent<Explosion>().OwnerPunID = OwnerPunID;
		explosion.GetComponent<Explosion>().baseDamage = baseDamage;
		explosion.GetComponent<Explosion>().damageType = damageType;
	}

	[PunRPC]
	void SpawnFortwarsProp(int viewId, string propPrefabName, Vector3 spawnPos, Quaternion spawnRot)
	{
		GameObject prop = Instantiate(Resources.Load<GameObject>(propPrefabName));
		prop.transform.position = spawnPos;
		prop.transform.rotation = spawnRot;
		prop.GetComponent<FortwarsProp>().idOfOwner = viewId;
		prop.GetComponent<FortwarsProp>().player = PhotonView.Find(viewId).GetComponent<PlayerStats>();
		prop.GetComponent<FortwarsProp>().player.PropsOwnedByPlayer.Add(prop);
	}

	[PunRPC]
	void UpdateFortwarsPropTransform(int ownerId, int propIndex, Vector3 newPos, Quaternion newRot)
	{
		PhotonView.Find(ownerId).GetComponent<PlayerStats>().PropsOwnedByPlayer[propIndex].transform.position = newPos;
		PhotonView.Find(ownerId).GetComponent<PlayerStats>().PropsOwnedByPlayer[propIndex].transform.rotation = newRot;
	}
}
