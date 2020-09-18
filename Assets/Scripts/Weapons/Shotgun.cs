using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponMotor
{

	public int numPellets;
	public float randomSpread = 0.04f;

	public int damagePerPellet;

	public GameObject hitPrefab;

	public override void PrimaryFire()
	{
		if (Time.time > lastFireTime + (1 / shotsPerSecond) && currentAmmoInClip > 0)
		{
			isReloading = false;
            player.ui.RadialReload.StopReload();
			RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			shootDirection = cam.transform.forward;

            List<IDamagable> targets = new List<IDamagable>();

			for (int i = 0; i < numPellets; i++)
			{
				shootDirection.x += Random.Range(randomSpread, -randomSpread);
				shootDirection.y += Random.Range(randomSpread, -randomSpread);
				shootDirection.z += Random.Range(randomSpread, -randomSpread);

				bool spawnDecal = false;
				if (Physics.Raycast(rayOrigin, shootDirection, out shot, 2000f, layermask))
				{
					shotPoint = shot.point;
					spawnDecal = true;
                    if (shot.collider.GetComponent<IDamagable>() != null)
                    {
                        targets.Add(shot.collider.GetComponent<IDamagable>());
                    }
				}
				else
				{
					shotPoint = rayOrigin + (shootDirection * 2000f);
					spawnDecal = false;
				}

				if (spawnDecal)
				{
					GameObject go = Instantiate(hitPrefab);
					go.transform.position = shotPoint;
					go.transform.rotation = Quaternion.FromToRotation(Vector3.forward, shot.normal);
				}
			}

            foreach(IDamagable target in targets)
            {
                target.TakeDamage(damagePerPellet, Damager.DamageTypes.Physical, player, player.transform.position, PlayerStats.DamageIndicatorType.Directional);
            }
			lastFireTime = Time.time;
			currentAmmoInClip = currentAmmoInClip - 1;
			player.OnChangeAmmoInClip(currentAmmoInClip);
		}
	}

	public override void GetWeaponStats(Weapon wep)
	{
        base.GetWeaponStats(wep);
        damagePerPellet = wep.baseDamage;
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
