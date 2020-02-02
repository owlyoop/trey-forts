using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : WeaponMotor
{

	public GameObject projectilePrefab;
	public Transform gunEnd;

	public float shotsPerSecond;
	public float projectileSpeed;
	public float projectileLife;
	public int baseDamage;
	public int clipSize;
	
	[System.NonSerialized]
	public Damager.DamageTypes damageType;
	[System.NonSerialized]
	public float reloadTime;

	private float lastFireTime;
	public Camera cam;

	public Vector3 rayOrigin;
	public Vector3 shootDirection;
	
	public Vector3 shotPoint;

	private float reloadStartTime;
	private bool isReloading;

    public LayerMask layermask;

	private void Start()
	{
        cam = GetComponentInParent<WeaponSlots>().player.cam;
		player = GetComponentInParent<WeaponSlots>().player;
		lastFireTime = 0f;
		CurrentAmmoInClip = clipSize;
        CurrentAmmo = MaxAmmo;
		player.OnChangeAmmoInClip(CurrentAmmoInClip);
		player.OnChangeAmmoReservesAmount(CurrentAmmo);
		isReloading = false;
	}

	private void OnEnable()
	{
		cam = GetComponentInParent<WeaponSlots>().GetComponentInParent<Camera>();
	}

	private void Update()
	{
		ReloadTimerUpdate();
	}


	public override void PrimaryFire()
	{
		if (Time.time > lastFireTime + (1 / shotsPerSecond) && CurrentAmmoInClip > 0)
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

			lastFireTime = Time.time;
			CurrentAmmoInClip = CurrentAmmoInClip - 1;
			player.OnChangeAmmoInClip(CurrentAmmoInClip);

            //GetComponentInParent<WeaponSlots>().player.GetComponent<PhotonView>().RPC("FireProjectile", RpcTarget.AllViaServer,
            //GetComponentInParent<WeaponSlots>().player.GetComponent<PhotonView>().ViewID, projectilePrefab.name,
            //shotPoint, gunEnd.transform.position,
            //	projectileSpeed, projectileLife, baseDamage, damageType);

            player.GetComponent<PlayerRpcCalls>().FireProjectile(projectilePrefab.name, shotPoint, gunEnd.transform.position, projectileSpeed, projectileLife, baseDamage, damageType);


		}
		else
		{

		}
	}

	public override void GetWeaponStats(RangedProjectile wep)
	{
		shotsPerSecond = wep.ShotsPerSecond;
		projectileSpeed = wep.projectileSpeed;
		projectileLife = wep.projectileLife;
		baseDamage = wep.BaseDamage;
		clipSize = wep.ClipSize;
        MaxAmmo = wep.MaxAmmo;
		damageType = wep.damageType;
		reloadTime = wep.ReloadTime;
	}

	public override void UpdateUI()
	{
        base.UpdateUI();
		
	}

	public override void ReloadButton()
	{
		if (CurrentAmmo > 0 && CurrentAmmoInClip < clipSize)
		{
            isReloading = true;
            reloadStartTime = Time.time;
            player.ui.RadialReload.StartReload(reloadTime);
		}
	}

	public void ReloadTimerUpdate()
	{
		if (isReloading && Time.time > reloadStartTime + reloadTime)
		{
			CurrentAmmoInClip = CurrentAmmoInClip + 1;
			CurrentAmmo = CurrentAmmo - 1;

			UpdateUI();
			reloadStartTime = Time.time;

			if (CurrentAmmoInClip == clipSize)
			{
                isReloading = false;
                player.ui.RadialReload.StopReload();
			}
			else
			{
				reloadStartTime = Time.time;
			}
			
		}
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

    public override void GetWeaponStats(Weapon wep)
    {
        
    }
}
