using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var target = other.GetComponent<PlayerStats>();
        if (target != null && target is PlayerStats)
        {
            target.StatusEffectManager.AddStatusEffect(BarbedWirePrefab);
            target.StatusEffectManager.BarbedWiresTouchingPlayer++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var target = other.GetComponent<PlayerStats>();
        if (target != null)
        {
            foreach (StatusEffect s in target.StatusEffectManager.CurrentStatusEffectsOnPlayer.ToList())
            {
                if (s is BarbedWireEffect)
                {
                    target.StatusEffectManager.BarbedWiresTouchingPlayer--;
                    if (target.StatusEffectManager.BarbedWiresTouchingPlayer == 0)
                    {
                        target.StatusEffectManager.RemoveStatusEffect(s);
                    }
                }
            }
        }
    }
}
