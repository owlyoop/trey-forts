using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarbedWire : Damager
{

    public StatusEffect BarbedWirePrefab;

    public FortwarsProp baseProp;

    List<PlayerStats> affectedPlayers = new List<PlayerStats>();

    public override void DamageTarget(IDamagable target)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = DamageUniqueTargetsOnTriggerEnter(other, baseProp.idOfOwner, 0, DamageTypes.Physical, true);
        bool alreadyAffected = false;
        if (target != null && target is PlayerStats)
        {
            foreach (StatusEffect s in target.StatusEffectManager.CurrentStatusEffectsOnPlayer)
            {
                if (s is BarbedWireEffect)
                    alreadyAffected = true;
            }

            if (!alreadyAffected)
                target.StatusEffectManager.AddStatusEffect(BarbedWirePrefab);
        }

        var hitbox = other.GetComponent<PlayerHitbox>();

        if (hitbox != null)
        {
            hitbox.player.StatusEffectManager.NumHitboxesTouchingBarbeds++;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        var hitbox = other.GetComponent<PlayerHitbox>();
        if (hitbox != null)
        {
            hitbox.player.StatusEffectManager.NumHitboxesTouchingBarbeds--;

            if (hitbox.player.StatusEffectManager.NumHitboxesTouchingBarbeds <= 0)
            {
                StatusEffect sRef = null;
                foreach (StatusEffect s in hitbox.player.StatusEffectManager.CurrentStatusEffectsOnPlayer)
                {
                    if (s is BarbedWireEffect)
                    {
                        sRef = s;
                    }
                }

                if (sRef != null)
                {
                    hitbox.player.StatusEffectManager.RemoveStatusEffect(sRef);
                    foreach (PlayerStats targets in base.damagedTargets)
                    {
                        if (targets == hitbox.player)
                        {
                            //damagedTargets.Remove(targets);
                        }
                    }

                    for (int i = 0; i < base.damagedTargets.Count; i++)
                    {
                        if (hitbox.player == damagedTargets[i] as PlayerStats)
                        {
                            damagedTargets.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
