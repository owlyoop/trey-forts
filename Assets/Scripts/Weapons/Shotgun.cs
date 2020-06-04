using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponMotor
{

	public int numPellets;
	public float randomSpread = 0.04f;

	public Damager.DamageTypes damageType;
	public int damagePerPellet;
	public float shotsPerSecond;
	public int clipSize;


	public Camera cam;

	float lastFireTime;
	bool isReloading = false;
	Vector3 shootDirection;
	public Vector3 shotPoint;
	public Transform gunEnd;

    public LayerMask layermask;

    int OwnerPunID;
	public GameObject hitPrefab;

	private void Start()
	{
		
		player = GetComponentInParent<WeaponSlots>().player;
        cam = player.cam;
        //OwnerPunID = player.photonView.ViewID;
		lastFireTime = 0f;
		CurrentAmmoInClip = clipSize;
		CurrentAmmo = MaxAmmo;
		player.OnChangeAmmoInClip(CurrentAmmoInClip);
		player.OnChangeAmmoReservesAmount(CurrentAmmo);
	}

	private void Update()
	{

	}


	public override void PrimaryFire()
	{
		if (Time.time > lastFireTime + (1 / shotsPerSecond) && CurrentAmmoInClip > 0)
		{
			isReloading = false;
			player.ui.RadialReload.enabled = false;
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
			CurrentAmmoInClip = CurrentAmmoInClip - 1;
			player.OnChangeAmmoInClip(CurrentAmmoInClip);
		}
	}
	

	public override void GetWeaponStats(Weapon wep)
	{
        clipSize = wep.ClipSize;
        damagePerPellet = wep.BaseDamage;
        shotsPerSecond = wep.ShotsPerSecond;
        base.GetWeaponStats(wep);
	}

	public override void ReloadButton()
	{
		
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

    public override void UseButtonHolding()
    {
        
    }

    public override void UseButtonUp()
    {
        
    }
}
