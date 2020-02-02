using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRpcCalls : MonoBehaviour
{

	
	public void FireProjectile(string name, Vector3 shotPoint, Vector3 spawnPosition, float projectileSpeed, float projectileLife, int baseDamage, Damager.DamageTypes damageType)
	{

		GameObject go = Instantiate(Resources.Load<GameObject>(name), spawnPosition, new Quaternion(0,0,0,0));
		go.transform.LookAt(shotPoint);


		go.GetComponent<Projectile>().player = GetComponent<PlayerStats>();
		go.GetComponent<Projectile>().speed = projectileSpeed;
		go.GetComponent<Projectile>().life = projectileLife;
		go.GetComponent<Projectile>().baseDamage = baseDamage;
		go.GetComponent<Projectile>().damageType = damageType;
	}

	
	public void SpawnExplosion(PlayerStats giver, Vector3 spawnPosition, int baseDamage, Damager.DamageTypes damageType)
	{
		GameObject explosion = Instantiate(Resources.Load<GameObject>("Explosion"), spawnPosition, new Quaternion(0, 0, 0, 0));
        explosion.GetComponent<Explosion>().player = giver;
		explosion.GetComponent<Explosion>().baseDamage = baseDamage;
		explosion.GetComponent<Explosion>().damageType = damageType;

	}

	
	public void SpawnFortwarsProp(string propPrefabName, PlayerStats player, Vector3 spawnPos, Quaternion spawnRot)
	{
		GameObject prop = Instantiate(Resources.Load<GameObject>(propPrefabName));
		prop.transform.position = spawnPos;
		prop.transform.rotation = spawnRot;
        prop.GetComponent<FortwarsProp>().player = player;
		prop.GetComponent<FortwarsProp>().player.PropsOwnedByPlayer.Add(prop);
        prop.GetComponent<FortwarsProp>().GhostMode = false;
        prop.GetComponent<FortwarsProp>().BuildTimeStart();

    }

	
	void UpdateFortwarsPropTransform(int propIndex, Vector3 newPos, Quaternion newRot)
	{
		//PhotonView.Find(ownerId).GetComponent<PlayerStats>().PropsOwnedByPlayer[propIndex].transform.position = newPos;
		//PhotonView.Find(ownerId).GetComponent<PlayerStats>().PropsOwnedByPlayer[propIndex].transform.rotation = newRot;
	}
}
