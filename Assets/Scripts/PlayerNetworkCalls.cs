using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworkCalls : NetworkBehaviour
{
    [ClientRpc]
	void RpcFireProjectile(string prefabName, Vector3 shotPoint, Vector3 spawnPosition, float projectileSpeed, float projectileLife, int baseDamage, Damager.DamageTypes damageType)
	{
		GameObject go = Instantiate(Resources.Load<GameObject>(prefabName), spawnPosition, new Quaternion(0,0,0,0));
		go.transform.LookAt(shotPoint);
        Projectile proj = go.GetComponent<Projectile>();
		proj.player = GetComponent<PlayerStats>();
		proj.speed = projectileSpeed;
		proj.life = projectileLife;
		proj.baseDamage = baseDamage;
		proj.damageType = damageType;
	}

    [Command]
    public void CmdFireProjectile(NetworkIdentity shooterID, string prefabName, Vector3 shotPoint, Vector3 spawnPosition, float projectileSpeed, float projectileLife, int baseDamage, Damager.DamageTypes damageType)
    {
        //check if player can actually fire the projectile
        bool isValid = false;

        PlayerStats player = shooterID.GetComponent<PlayerStats>();

        WeaponMotor curWeapon = player.wepSlots.CurrentWeapon;

        if (curWeapon != null && Time.time > curWeapon.lastFireTime + (1 / curWeapon.shotsPerSecond) && curWeapon.currentAmmoInClip > 0)
        {
            isValid = true;
            curWeapon.lastFireTime = Time.time;
        }

        if (isValid)
        {
            curWeapon.currentAmmoInClip -= curWeapon.numAmmoPerReload;
            player.OnChangeAmmoInClip(curWeapon.currentAmmoInClip);

            RpcFireProjectile(prefabName, shotPoint, spawnPosition, projectileSpeed, projectileLife, baseDamage, damageType);
        }
    }

	[ClientRpc]
	public void RpcSpawnExplosion(NetworkIdentity giverID, Vector3 spawnPosition, int baseDamage, Damager.DamageTypes damageType)
	{
		GameObject explosion = Instantiate(Resources.Load<GameObject>("Explosion"), spawnPosition, new Quaternion(0, 0, 0, 0));
        Explosion exp = explosion.GetComponent<Explosion>();
        exp.player = giverID.GetComponent<PlayerStats>();
		exp.baseDamage = baseDamage;
		exp.damageType = damageType;
	}

	[ClientRpc]
	public void RpcSpawnFortwarsProp(string propPrefabName, NetworkIdentity propOwnerID, Vector3 spawnPos, Quaternion spawnRot)
	{
		GameObject prop = Instantiate(Resources.Load<GameObject>(propPrefabName));
		prop.transform.position = spawnPos;
		prop.transform.rotation = spawnRot;
        var fwProp = prop.GetComponent<FortwarsProp>();
        fwProp.player = propOwnerID.GetComponent<PlayerStats>();
		fwProp.player.PropsOwnedByPlayer.Add(prop);
        fwProp.GhostMode = false;
        fwProp.BuildTimeStart();
    }

	
    [ClientRpc]
	void RpcUpdateFortwarsPropTransform(int propIndex, Vector3 newPos, Quaternion newRot)
	{
		//PhotonView.Find(ownerId).GetComponent<PlayerStats>().PropsOwnedByPlayer[propIndex].transform.position = newPos;
		//PhotonView.Find(ownerId).GetComponent<PlayerStats>().PropsOwnedByPlayer[propIndex].transform.rotation = newRot;
	}
}
