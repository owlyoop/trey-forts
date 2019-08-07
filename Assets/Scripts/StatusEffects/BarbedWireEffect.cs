using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarbedWireEffect : StatusEffect
{
    public float MovespeedWhileSlowed = 2f;
    public int DamagePerTick = 4;
    public float TickDelay = 0.2f;
    float timer = 0f;
    float nextActionTime = 0f;

    private void Update()
    {
        timer = Time.time;

        if (Time.time > nextActionTime)
        {
            nextActionTime = Time.time + TickDelay;
            receiver.TakeDamage(receiver.photonView.ViewID, DamagePerTick, Damager.DamageTypes.Physical);
        }
    }
    public override void OnAfterDamageTaken()
    {
        
    }

    public override void OnApplyStatusEffect()
    {
        receiver.CharControl.MaxStableMoveSpeed = MovespeedWhileSlowed;
        receiver.CharControl.MaxAirMoveSpeed = MovespeedWhileSlowed;
    }

    public override int OnBeforeDamageTaken(int damage)
    {
        return damage;
    }

    public override void OnPlayerDealsDamage()
    {
        
    }

    public override void OnPlayerDeath()
    {
        
    }

    public override void OnPlayerFiresWeapon()
    {
        
    }

    public override void OnPlayerGetsKill()
    {
        
    }

    public override void OnPlayerIsMoving(float moveSpeed)
    {
        
    }

    public override void OnPlayerJump()
    {
        
    }

    public override void OnPlayerTakesFatalDamage()
    {
        
    }

    public override void OnUnapplyStatusEffect()
    {
        receiver.CalculateMovementValues();
    }

}
