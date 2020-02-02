using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropColliderRef : MonoBehaviour, IDamagable
{

    public Rigidbody PropRigidbody;

    public void OnDeath()
    {
        
    }

    public void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation)
    {
        PropRigidbody.GetComponent<FortwarsProp>().TakeDamage(damageTaken, damageType, giver, damageSourceLocation);
    }
}
