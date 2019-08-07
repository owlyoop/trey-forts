using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ProjectileLauncher : WeaponMotor
{

	public GameObject projectilePrefab;
	public Transform gunEnd;

	public float shotsPerSecond;
	public float projectileSpeed;
	public float projectileLife;
	public int baseDamage;
	public int clipSize;
	public int maxAmmo;
	
	public int currentAmmoInClip;
	public int currentAmmoReserves;
	[System.NonSerialized]
	public Damager.DamageTypes damageType;
	[System.NonSerialized]
	public float reloadTime;

	private float lastFireTime;
	public Camera cam;

	public Vector3 rayOrigin;
	public Vector3 shootDirection;
	
	public Vector3 shotPoint;

	public PlayerStats playersStats;
	private float reloadStartTime;
	private bool isReloading;

	private void Start()
	{
		cam = GetComponentInParent<WeaponSlots>().GetComponentInParent<Camera>();
		playersStats = GetComponentInParent<WeaponSlots>().player;
		lastFireTime = 0f;
		currentAmmoInClip = clipSize;
		currentAmmoReserves = maxAmmo;
		playersStats.OnChangeAmmoInClip(currentAmmoInClip);
		playersStats.OnChangeAmmoReservesAmount(currentAmmoReserves);
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
		if (Time.time > lastFireTime + (1 / shotsPerSecond) && currentAmmoInClip > 0)
		{
			isReloading = false;
			playersStats.ui.radialReload.enabled = false;
			RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			shootDirection = cam.transform.forward;

			int layermask = 1 << 12;
			layermask = ~layermask;

			//GameObject go = PhotonNetwork.Instantiate("Missile", gunEnd.transform.position, gunEnd.rotation);
			//GameObject go = (GameObject)Instantiate(projectilePrefab);
			//Physics.IgnoreCollision(GetComponentInParent<Collider>(), go.GetComponent<Collider>());
			//go.transform.position = gunEnd.transform.position;
			
			

			if (Physics.Raycast(rayOrigin, shootDirection, out shot, 2000f, layermask))
			{
				shotPoint = shot.point;

			}
			else
			{
				shotPoint = rayOrigin + (shootDirection * 2000f);
			}
			//go.transform.LookAt(shotPoint);

			//go.GetComponent<Projectile>().speed = projectileSpeed;
			//go.GetComponent<Projectile>().life = projectileLife;
			//go.GetComponent<Projectile>().baseDamage = baseDamage;
			//go.GetComponent<Projectile>().damageType = damageType;

			lastFireTime = Time.time;
			currentAmmoInClip = currentAmmoInClip - 1;
			playersStats.OnChangeAmmoInClip(currentAmmoInClip);


			GetComponentInParent<WeaponSlots>().player.GetComponent<PhotonView>().RPC("FireProjectile", RpcTarget.AllViaServer,
				GetComponentInParent<WeaponSlots>().player.GetComponent<PhotonView>().ViewID, projectilePrefab.name,
				shotPoint, gunEnd.transform.position,
				projectileSpeed, projectileLife, baseDamage, damageType);

			

		}
		else
		{
			networkObjToSpawn = null;
		}
	}

	public override void GetWeaponStats(RangedProjectile wep)
	{
		shotsPerSecond = wep.shotsPerSecond;
		projectileSpeed = wep.projectileSpeed;
		projectileLife = wep.projectileLife;
		baseDamage = wep.baseDamage;
		clipSize = wep.clipSize;
		maxAmmo = wep.maxAmmo;
		damageType = wep.damageType;
		reloadTime = wep.reloadTime;
	}

	public override void UpdateUI()
	{
		playersStats = GetComponentInParent<WeaponSlots>().player;
		playersStats.OnChangeAmmoInClip(currentAmmoInClip);
		playersStats.OnChangeAmmoReservesAmount(currentAmmoReserves);
	}

	public override void ReloadButton()
	{
		if (currentAmmoReserves > 0 && currentAmmoInClip < clipSize)
		{
			playersStats.ui.radialReload.enabled = true;
			reloadStartTime = Time.time;
			isReloading = true;
		}
	}

	public void ReloadTimerUpdate()
	{
		if (isReloading)
		{
			float reloadEndTime = reloadStartTime + reloadTime;
			playersStats.ui.radialReload.fillAmount = (Time.time - reloadStartTime) / (reloadEndTime - reloadStartTime);
		}

		if (isReloading && Time.time > reloadStartTime + reloadTime)
		{
			currentAmmoInClip = currentAmmoInClip + 1;
			currentAmmoReserves = currentAmmoReserves - 1;

			UpdateUI();
			reloadStartTime = Time.time;

			if (currentAmmoInClip == clipSize)
			{
				isReloading = false;
				playersStats.ui.radialReload.enabled = false;
			}
			else
			{
				reloadStartTime = Time.time;
			}
			
		}
	}
}
