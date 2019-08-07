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

    public void TakeDamage(int GiverPunID, int damageTaken, Damager.DamageTypes damageType)
    {
        player.TakeDamage(GiverPunID, damageTaken, damageType);
    }

    public void OnDeath()
    {
        
    }
}
