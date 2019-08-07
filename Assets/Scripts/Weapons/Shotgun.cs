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
	public int maxAmmo;

	public int currentAmmoInClip;
	public int currentAmmoReserves;

	public Camera cam;
	public PlayerStats player;

	float lastFireTime;
	bool isReloading = false;
	Vector3 shootDirection;
	public Vector3 shotPoint;
	public Transform gunEnd;


	public GameObject hitPrefab;

	private void Start()
	{
		cam = GetComponentInParent<WeaponSlots>().GetComponentInParent<Camera>();
		player = GetComponentInParent<WeaponSlots>().player;
		lastFireTime = 0f;
		currentAmmoInClip = clipSize;
		currentAmmoReserves = maxAmmo;
		player.OnChangeAmmoInClip(currentAmmoInClip);
		player.OnChangeAmmoReservesAmount(currentAmmoReserves);
	}

	private void Update()
	{

	}


	public override void PrimaryFire()
	{
		if (Time.time > lastFireTime + (1 / shotsPerSecond) && currentAmmoInClip > 0)
		{
			isReloading = false;
			player.ui.radialReload.enabled = false;
			RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			shootDirection = cam.transform.forward;

			int layermask = 1 << 9;
			layermask = ~layermask;




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
				
				shootDirection = cam.transform.forward;
			}


			lastFireTime = Time.time;
			currentAmmoInClip = currentAmmoInClip - 1;
			player.OnChangeAmmoInClip(currentAmmoInClip);
		}
	}
	

	public override void GetWeaponStats(Weapon wep)
	{

	}

	public override void ReloadButton()
	{
		base.ReloadButton();
	}
}
