using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bleed : StatusEffect
{

    public int BaseDamagePerTick;
    public float TickDelay = 0.2f;
    public int AdditionalDamagePerStack;
    float timer = 0f;
    float nextTickTime = 0f;
    float nextRemoveStackTime = 0f;

    

    private void Start()
    {
        AllowStacking = true;
    }

    private void Update()
    {
        timer = Time.time;
        if (Time.time > nextTickTime)
        {
            int _damage = 0;
            nextTickTime = Time.time + TickDelay;

            if (CurrentStacks > 1)
            { 
                _damage = BaseDamagePerTick + (AdditionalDamagePerStack * CurrentStacks);
            }
            else if (CurrentStacks == 1)
            {
                _damage = BaseDamagePerTick;
            }
            // Damage source isnt the receivers location. Fix this whenever I implement damage indicators that dont have a location
            receiver.TakeDamage(_damage, Damager.DamageTypes.Physical, receiver, receiver.transform.position, PlayerStats.DamageIndicatorType.None);
        }

        if (CurrentStacks > 0 && Time.time > nextRemoveStackTime)
        {
            CurrentStacks--;
            if (CurrentStacks == 0)
            {
                OnUnapplyStatusEffect();
            }
        }

    }

    public override void OnApplyStatusEffect()
    {
        CurrentStacks++;
        nextRemoveStackTime = Time.time + Duration;
    }

    public override void OnUnapplyStatusEffect()
    {
        
    }

}
