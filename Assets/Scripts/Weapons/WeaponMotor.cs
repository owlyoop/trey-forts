using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponMotor : NetworkBehaviour
{
    public PlayerStats player;
    public Camera cam;


    public int baseDamage;
    [System.NonSerialized]
    public Damager.DamageTypes damageType;

    public float shotsPerSecond;

    [SyncVar]
    public int maxAmmo;
    [SyncVar]
    public int currentAmmo;
    [SyncVar]
    public int clipSize;
    [SyncVar]
    public int currentAmmoInClip;

    protected bool isReloading;
    protected float reloadStartTime;
    public float lastFireTime;
    public float reloadTime;
    public int numAmmoPerReload;

    public float weaponSwitchAwayTime = 0f;
    public float weaponSwitchToTime = 0f;

    protected Vector3 rayOrigin;
    protected Vector3 shootDirection;
    protected Vector3 shotPoint;

    public Transform gunEnd;

    public LayerMask layermask;



    private void Start()
    {
        player = GetComponentInParent<WeaponSlots>().player;
        cam = player.cam;

        lastFireTime = 0f;
        currentAmmo = maxAmmo;
        currentAmmoInClip = clipSize;

        UpdateUI();

        isReloading = false;
    }

    private void Update()
    {
        ReloadTimerUpdate();
    }

    public virtual void UpdateUI()
    {
        player.OnChangeAmmoReservesAmount(currentAmmo);
        player.OnChangeAmmoInClip(currentAmmoInClip);
    }

    public abstract void PrimaryFire();
    public abstract void PrimaryFireHolding();
    public abstract void PrimaryFireButtonUp();
    public abstract void SecondaryFire();
    public abstract void SecondaryFireHolding();
    public abstract void ScrollWheelUp();
    public abstract void ScrollWheelDown();
    public abstract void OnSwitchAwayFromWeapon();
    public abstract void OnSwitchToWeapon();

    public virtual void ReloadKey()
    {
        if (currentAmmo > 0 && currentAmmoInClip < clipSize)
        {
            isReloading = true;
            reloadStartTime = Time.time;
            player.ui.RadialReload.StartReload(reloadTime);
        }
    }

    public virtual void ReloadTimerUpdate()
    {
        if (isReloading && Time.time > reloadStartTime + reloadTime)
        {
            currentAmmoInClip += numAmmoPerReload;
            currentAmmo -= numAmmoPerReload;
            
            reloadStartTime = Time.time;

            if (currentAmmoInClip > clipSize)
            {
                currentAmmo += currentAmmoInClip - clipSize;
                currentAmmoInClip = clipSize;
            }

            if (currentAmmoInClip == clipSize)
            {
                player.ui.RadialReload.StopReload();
                isReloading = false;
            }
            UpdateUI();
        }
    }

    public abstract void UseKeyHolding();
    public abstract void UseKeyUp();
    public virtual void GetWeaponStats(Weapon wep)
    {
        player = GetComponentInParent<WeaponSlots>().player;
        cam = player.cam;

        baseDamage = wep.baseDamage;
        clipSize = wep.clipSize;
        maxAmmo = wep.maxAmmo;
        currentAmmo = maxAmmo;
        currentAmmoInClip = clipSize;
        damageType = wep.damageType;
        reloadTime = wep.reloadTime;
        numAmmoPerReload = wep.numAmmoPerReload;
        shotsPerSecond = wep.shotsPerSecond;

        UpdateUI();
    }

}
