using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarbedWire : Damager
{

    public StatusEffect BarbedWirePrefab;

    List<PlayerStats> affectedPlayers = new List<PlayerStats>();

    public override void DamageTarget(IDamagable target)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        var hitbox = other.GetComponent<PlayerHitbox>();
        if (hitbox != null)
        {
            bool alreadyHit = false;
            foreach (PlayerStats player in affectedPlayers)
            {
                if (player == hitbox.player)
                {
                    alreadyHit = true;
                }
            }
            hitbox.player.StatusEffectManager.NumHitboxesTouchingBarbeds++;

            bool alreadyAffected = false;
            if (!alreadyHit)
            {
                foreach (StatusEffect s in hitbox.player.StatusEffectManager.CurrentStatusEffectsOnPlayer)
                {
                    if (s is BarbedWireEffect)
                    {
                        alreadyAffected = true;
                    }
                }

                if (!alreadyAffected && hitbox.player.StatusEffectManager.NumHitboxesTouchingBarbeds == 1)
                {
                    hitbox.player.StatusEffectManager.AddStatusEffect(BarbedWirePrefab);
                }
            }
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
                }
            }
        }
    }
}
