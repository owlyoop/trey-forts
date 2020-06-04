using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarbedWireEffect : StatusEffect
{
    public static float MovementPercentSubtract = 0.5f;
    public int DamagePerTick = 4;
    public float TickDelay = 0.2f;
    float timer = 0f;
    float nextActionTime = 0f;


    public static StatModifier movementSlow = new StatModifier(-MovementPercentSubtract, StatModType.PercentAdd);

    private void Start()
    {
        AllowStacking = false;
    }
    private void Update()
    {
        timer = Time.time;

        if (Time.time > nextActionTime)
        {
            nextActionTime = Time.time + TickDelay;
            // Damage source isnt the receivers location. Fix this whenever I implement damage indicators that dont have a location
            receiver.TakeDamage(DamagePerTick, Damager.DamageTypes.Physical, receiver, receiver.transform.position, PlayerStats.DamageIndicatorType.Generic);
        }
    }
    public override void OnApplyStatusEffect()
    {
        receiver.AddMovementModifier(movementSlow);
    }
    public override void OnUnapplyStatusEffect()
    {
        receiver.RemoveMovementModifier(movementSlow);
    }

}
