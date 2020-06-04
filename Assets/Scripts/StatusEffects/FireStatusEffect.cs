using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStatusEffect : StatusEffect
{

    public int DamagePerTick = 3;
    public float TickDelay = 0.4f;
    

    public GameObject FireParticlesPrefab;
    public Transform FireParticlesParent;
    GameObject fire;

    float timer = 0f;
    float nextActionTime = 0f;
    float timeApplied = 0f;

    public void Update()
    {
        timer = Time.time;

        if (Time.time > nextActionTime)
        {
            nextActionTime = Time.time + TickDelay;
            //receiver.TakeDamage(receiver.photonView.ViewID, DamagePerTick, Damager.DamageTypes.Physical);
        }

        if (Time.time > timeApplied + Duration)
        {
            receiver.StatusEffectManager.RemoveStatusEffect(this.GetComponent<FireStatusEffect>());
        }
    }
    public override void OnAfterDamageTaken()
    {
        
    }

    public override void OnApplyStatusEffect()
    {
        timeApplied = Time.time;
        FireParticlesParent = receiver.ParticleParent;
        fire = Instantiate(FireParticlesPrefab, FireParticlesParent);
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
        Destroy(fire);
        
    }
}
