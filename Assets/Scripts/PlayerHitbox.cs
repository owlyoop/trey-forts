using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour, IDamagable
{
    [System.NonSerialized]
    public Collider col;

    public PlayerStats player;

    public float damageMultiplier = 1f;

    public enum BodyPart { Head, Spine, Hips, LeftUpLeg, LeftLeg, RightUpLeg, RightLeg, LeftForearm, LeftArm, RightForearm, RightArm}

    public BodyPart bodyPart;

    private void Start()
    {
        col = GetComponent<Collider>();
    }

    //Should only call takedamage from the hitbox with things that will only damage 1 thing. ex. Explosions, i dont want to use this because it would deal damage for every single hitbox.
    public void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation)
    {
        player.TakeDamage(damageTaken, damageType, giver, damageSourceLocation);
    }

    public void OnDeath()
    {
        
    }
}
