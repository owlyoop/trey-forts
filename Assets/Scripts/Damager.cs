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

    public virtual void DamageTarget(IDamagable target, int giverPunID, int damage, DamageTypes damageType, bool damageSelf)
    {
        PlayerStats player = target as PlayerStats;
        PlayerHitbox playerhb = target as PlayerHitbox;
        if (player != null)
        {
            if (player.photonView.ViewID == giverPunID)
            {
                if (damageSelf)
                {
                    target.TakeDamage(giverPunID, damage, damageType);
                }
            }
        }
        else if (playerhb == null)
        {
            target.TakeDamage(giverPunID, damage, damageType);
        }
        
    }

    //Make sure we dont damage player multiple times for every playerhitbox. Used for triggers that last more than 1 frame. damages on contact
    public PlayerStats DamageUniqueTargetsOnTriggerEnter(Collider col, int giverPunID, int damage, DamageTypes damageType, bool damageSelf)
    {
        var target = col.GetComponent<IDamagable>();
        if (target != null)
        {
            bool isHit = false;
            for (int i = 0; i < damagedTargets.Count; i++)
            {
                if (target is PlayerHitbox)
                {
                    var hitbox = target as PlayerHitbox;
                    PlayerStats targetPlayer = damagedTargets[i] as PlayerStats;
                    if (targetPlayer != null && hitbox.player == targetPlayer)
                    {
                        isHit = true;
                    }

                    PlayerHitbox damagedHitbox = damagedTargets[i] as PlayerHitbox;
                    if (damagedHitbox != null && hitbox.player == damagedHitbox.player)
                    {
                        isHit = true;
                    }
                }
                if (target == damagedTargets[i])
                {
                    isHit = true;
                }
                if (target is PlayerStats)
                {
                    var targetPlayer = target as PlayerStats;
                    PlayerHitbox targetHitbox = damagedTargets[i] as PlayerHitbox;
                    if (targetPlayer != null && targetHitbox != null && targetPlayer == targetHitbox.player)
                    {
                        isHit = true;
                    }
                }
            }
            if (!isHit && target is PlayerStats)
            {
                damagedTargets.Add(target);
                DamageTarget(target, giverPunID, damage, damageType, damageSelf);
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

}