using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileLauncher : WeaponMotor
{

	public GameObject projectilePrefab;
	public Transform gunEnd;

	public float shotsPerSecond;
	[SyncVar]
	public float projectileSpeed;
	[SyncVar]
	public float projectileLife;
	public int baseDamage;
	public int clipSize;
	public int maxAmmo;

	[SyncVar]
	public int currentAmmoInClip;
	[SyncVar]
	public int currentAmmoReserves;
	[System.NonSerialized]
	public Damager.DamageTypes damageType;
	[System.NonSerialized]
	public float reloadTime;

	private float lastFireTime;
	public Camera cam;

	public Vector3 rayOrigin;
	public Vector3 shootDirection;
	LineRenderer line;

	[SyncVar]
	public Vector3 shotPoint;

	public PlayerStats playersStats;
	private float reloadStartTime;
	private bool isReloading;

	private void Start()
	{
		cam = GetComponentInParent<WeaponSlots>().GetComponentInParent<Camera>();
		line = GetComponent<LineRenderer>();
		playersStats = GetComponentInParent<WeaponSlots>().player;
		lastFireTime = 0f;
		currentAmmoInClip = clipSize;
		currentAmmoReserves = maxAmmo;
		playersStats.ammoInClip.text = currentAmmoInClip.ToString();
		playersStats.ammoAmount.text = currentAmmoReserves.ToString();
		isReloading = false;
	}

	private void OnEnable()
	{
		cam = GetComponentInParent<WeaponSlots>().GetComponentInParent<Camera>();
		line = GetComponent<LineRenderer>();
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
			playersStats.radialReload.enabled = false;
			RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			line.SetPosition(0, gunEnd.position);
			shootDirection = cam.transform.forward;

			int layermask = 1 << 9;
			layermask = ~layermask;

			GameObject go = (GameObject)Instantiate(projectilePrefab);
			Physics.IgnoreCollision(GetComponentInParent<CharacterController>(), go.GetComponent<Collider>());
			go.transform.position = gunEnd.transform.position;

			if (Physics.Raycast(rayOrigin, shootDirection, out shot, 2000f, layermask))
			{
				shotPoint = shot.point;
				line.SetPosition(1, shot.point);
				go.transform.LookAt(shot.point);

			}
			else
			{
				shotPoint = rayOrigin + (shootDirection * 2000f);
				line.SetPosition(1, rayOrigin + (shootDirection * 2000f));
				go.transform.LookAt(rayOrigin + (shootDirection * 2000f));
			}


			go.GetComponent<Projectile>().speed = projectileSpeed;
			go.GetComponent<Projectile>().life = projectileLife;
			go.GetComponent<Projectile>().baseDamage = baseDamage;
			go.GetComponent<Projectile>().damageType = damageType;

			networkObjToSpawn = go;

			lastFireTime = Time.time;
			currentAmmoInClip = currentAmmoInClip - 1;
			playersStats.currentAmmoInClip = currentAmmoInClip;

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
		playersStats.currentAmmoInClip = currentAmmoInClip;
		playersStats.currentAmmoReserves = currentAmmoReserves;
	}

	public override void ReloadButton()
	{
		if (currentAmmoReserves > 0 && currentAmmoInClip < clipSize)
		{
			playersStats.radialReload.enabled = true;
			reloadStartTime = Time.time;
			isReloading = true;
		}
	}

	public void ReloadTimerUpdate()
	{
		if (isReloading)
		{
			float reloadEndTime = reloadStartTime + reloadTime;
			playersStats.radialReload.fillAmount = (Time.time - reloadStartTime) / (reloadEndTime - reloadStartTime);
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
				playersStats.radialReload.enabled = false;
			}
			else
			{
				reloadStartTime = Time.time;
			}
			
		}
	}
}
