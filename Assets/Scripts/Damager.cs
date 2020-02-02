using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{

	public enum DamageTypes { Physical, Fire, Frost }
    public List<IDamagable> damagedTargets = new List<IDamagable>();

	public virtual void DamageTarget(IDamagable target)
	{

	}

    //FIX THIS FUNCTION WHENEVER I NEED IT
    public virtual void DamageTarget(IDamagable target, int damage, DamageTypes damageType, bool damageSelf)
    {
        PlayerStats player = target as PlayerStats;
        PlayerHitbox playerhb = target as PlayerHitbox;
        if (player != null)
        {
            if (damageSelf)
            {
                //target.TakeDamage(damage, damageType, this.gameObject);
            }
        }
        else if (playerhb == null)
        {
            //target.TakeDamage(damage, damageType, this.gameObject);
        }
        
    }

    //FIX THIS FUNCTION WHENEVER I NEED IT
    //Make sure we dont damage player multiple times for every playerhitbox. Used for triggers that last more than 1 frame. damages on contact
    public PlayerStats DamageUniqueTargetsOnTriggerEnter(Collider col, int damage, DamageTypes damageType, bool damageSelf)
    {
        var target = col.GetComponent<IDamagable>();
        if (target != null)
        {
            bool isHit = false;
            for (int i = 0; i < damagedTargets.Count; i++)
            {
                if (target is PlayerStats)
                {
                    if (damagedTargets[i] == target)
                    {
                        isHit = true;
                    }
                }
            }
            if (!isHit)
            {
                damagedTargets.Add(target);
                DamageTarget(target, damage, damageType, damageSelf);
                return target as PlayerStats;
            }
            else return null;

        }
        else return null;
    }

    // Gets unique damage targets all at once.
    public List<PlayerStats> GetUniqueTargets(Collider[] targets)
    {
        List<PlayerStats> hitPlayers = new List<PlayerStats>();
        foreach (Collider col in targets)
        {
            IDamagable target = col.GetComponent<IDamagable>();
            if (target != null)
            {
                //Explosion can hit multiple hitboxes on a single player. We only want to damage the player once.
                var playerHit = col.GetComponent<PlayerHitbox>();
                if (playerHit != null)
                {
                    if (hitPlayers.Count == 0)
                    {
                        hitPlayers.Add(playerHit.player);
                    }
                    else
                    {
                        bool isHit = false;
                        for (int i = 0; i < hitPlayers.Count; i++)
                        {
                            if (hitPlayers[i] == playerHit.player)
                            {
                                isHit = true;
                            }
                        }
                        if (!isHit)
                        {
                            hitPlayers.Add(playerHit.player);
                        }
                    }
                }
            }
        }

        return hitPlayers;
    }

    public List<IDamagable> GetUniqueDamagableTargets(Collider[] targets)
    {
        List<IDamagable> hitTargets = new List<IDamagable>();
        foreach (Collider col in targets)
        {
            IDamagable target = col.GetComponent<IDamagable>();
            if (target != null)
            {
                //Explosion can hit multiple hitboxes on a single player. We only want to damage the player once.
                var playerHit = col.GetComponent<PlayerStats>();
                if (playerHit != null)
                {
                    hitTargets.Add(playerHit);
                }

                // Props with multiple colliders
                var propHit = col.GetComponent<PropColliderRef>();
                if (propHit != null)
                {
                    if (hitTargets.Count == 0)
                    {
                        hitTargets.Add((IDamagable)propHit.PropRigidbody.GetComponent<FortwarsProp>());
                    }
                    else
                    {
                        bool isHit = false;
                        for (int i = 0; i < hitTargets.Count; i++)
                        {
                            if (hitTargets[i] == (IDamagable)propHit.PropRigidbody.GetComponent<FortwarsProp>())
                            {
                                isHit = true;
                            }
                        }
                        if (!isHit)
                        {
                            hitTargets.Add((IDamagable)propHit.PropRigidbody.GetComponent<FortwarsProp>());
                        }
                    }
                }

                var trainingDummy = col.GetComponent<TrainingDummy>();
                if (trainingDummy != null)
                {
                    hitTargets.Add((IDamagable)trainingDummy);
                }
            }
        }

        return hitTargets;
    }

}