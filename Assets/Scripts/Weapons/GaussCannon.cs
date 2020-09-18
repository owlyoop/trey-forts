using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussCannon : WeaponMotor
{
    [Header("References")]
    public UIManager ui;

    public int StartingDamage = 20;

    public int MaxCharge = 100; 
    public int ChargePerSecond = 40;
    public int DamagePerCharge = 1;

    //todo: make this use the weaponmotor version
    public float ShotDelay = 0.5f;

    public float MinPushBackForce = 1f;
    public float MaxPushBackForce = 12f;

    public float MaxDistance = 100f;

    float heldStartTime;
    float nextChargeTime = 0f;
    bool isCharging = false;
    int charge = 0;

    int OwnerPunID;


    private void Start()
    {
        cam = GetComponentInParent<WeaponSlots>().player.cam;
        player = GetComponentInParent<WeaponSlots>().player;
        //OwnerPunID = player.photonView.ViewID;
        ui = player.ui;

    }

    private void Update()
    {
        if (isCharging)
        {
            if (Time.time > nextChargeTime)
            {
                charge++;
                if (charge > MaxCharge)
                    charge = MaxCharge;
                nextChargeTime = Time.time + (1f/ChargePerSecond);
            }
        }
    }

    public override void GetWeaponStats(Weapon wep)
    {
        base.GetWeaponStats(wep);
        StartingDamage = wep.baseDamage;
    }

    public override void PrimaryFire()
    {
        isCharging = true;
        heldStartTime = Time.time;
        charge = 0;
    }

    public override void PrimaryFireHolding()
    {

    }

    public override void PrimaryFireButtonUp()
    {
        isCharging = false;
        nextChargeTime = Time.time + ShotDelay;

        RaycastHit shot;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out shot, MaxDistance, layermask))
        {
            shotPoint = shot.point;
        }
        else
        {
            shotPoint = cam.transform.forward * MaxDistance;
        }

        var target = shot.collider.GetComponent<IDamagable>();
        if (target != null)
        {
            int RealDamage = charge + StartingDamage;
            target.TakeDamage(RealDamage, Damager.DamageTypes.Physical, player, player.transform.position, PlayerStats.DamageIndicatorType.Directional);
        }

        charge = 0;
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
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

    public override void ReloadKey()
    {
        
    }

    public override void UseKeyHolding()
    {
        
    }

    public override void UseKeyUp()
    {
        
    }
}
