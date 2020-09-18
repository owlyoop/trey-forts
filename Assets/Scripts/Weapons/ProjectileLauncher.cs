using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : WeaponMotor
{

	public GameObject projectilePrefab;

	public float projectileSpeed;
	public float projectileLife;

	public override void PrimaryFire()
	{
		if (Time.time > lastFireTime + (1 / shotsPerSecond) && currentAmmoInClip > 0)
		{
			isReloading = false;
            player.ui.RadialReload.StopReload();
            RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			shootDirection = cam.transform.forward;

            if (Physics.Raycast(rayOrigin, shootDirection, out shot, 2000f, layermask))
            {
                shotPoint = shot.point;
            }
            else
            {
                shotPoint = rayOrigin + (shootDirection * 2000f);
            }

            player.GetComponent<PlayerNetworkCalls>().CmdFireProjectile(player.netIdentity, projectilePrefab.name, shotPoint, gunEnd.transform.position, projectileSpeed, projectileLife, baseDamage, damageType);
		}
	}

	public override void GetWeaponStats(Weapon wep)
	{
        base.GetWeaponStats(wep);
        RangedProjectile rwep = (RangedProjectile)wep;
		projectileSpeed = rwep.projectileSpeed;
		projectileLife = rwep.projectileLife;

	}

	public override void UpdateUI()
	{
        base.UpdateUI();
	}

    public override void PrimaryFireHolding()
    {
        
    }

    public override void PrimaryFireButtonUp()
    {
        
    }

    public override void SecondaryFire()
    {
        
    }

    public override void SecondaryFireHolding()
    {
        
    }

    public override void ScrollWheelUp()
    {
        
    }

    public override void ScrollWheelDown()
    {
        
    }

    public override void OnSwitchAwayFromWeapon()
    {
        
    }

    public override void OnSwitchToWeapon()
    {
        
    }

    public override void UseKeyHolding()
    {
        
    }

    public override void UseKeyUp()
    {
        
    }
}
