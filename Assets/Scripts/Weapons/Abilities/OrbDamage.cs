using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbDamage : Damager
{
    OrbProjectile orb;
    public int damage;
    public List<IDamagable> damagedPlayers = new List<IDamagable>();


    private void Start()
    {
        orb = GetComponentInParent<OrbProjectile>();
    }

    private void Update()
    {
        
    }

    public override void DamageTarget(IDamagable target)
    {

    }


    private void OnTriggerEnter(Collider col)
    {
        //DamageUniqueTargetsOnTriggerEnter(col, orb.owner.photonView.ViewID, damage, DamageTypes.Physical, false);
    }
}
