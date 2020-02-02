using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponMotor : MonoBehaviour
{
    public PlayerStats player;
    public float WeaponSwitchAwayTime = 0f;
    public float WeaponSwitchToTime = 0f;

    public int MaxAmmo;
    public int CurrentAmmo;
    public int ClipSize;
    public int CurrentAmmoInClip;

    private void Start()
    {
        CurrentAmmo = MaxAmmo;
    }
    public virtual void UpdateUI()
    {
        player.OnChangeAmmoReservesAmount(CurrentAmmo);
        player.OnChangeAmmoInClip(CurrentAmmoInClip);
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

    public abstract void ReloadButton();

    public abstract void UseButtonHolding();
    public abstract void UseButtonUp();
    public virtual void GetWeaponStats(Weapon wep)
    {
        MaxAmmo = wep.MaxAmmo;
        CurrentAmmo = MaxAmmo;
    }
    public virtual void GetWeaponStats(RangedProjectile wep)
    {

    }

}
