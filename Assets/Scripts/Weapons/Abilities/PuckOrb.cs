﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckOrb : AbilityMotor
{

    public float OrbLifetime = 4f;
    public float OrbSpeed = 16f;
    public int OrbAoeDamage = 50;
    public float OrbAoeRadius = 1f;

    public GameObject OrbPrefab;

    

    public bool orbIsActive = false;

    public GameObject activeOrb;

    
    

    private void Start()
    {

    }

    public override void OnPressAbilityButton()
    {
        if (!orbIsActive)
        {
            GameObject orb = Instantiate(OrbPrefab);
            orb.transform.position = this.transform.position;
            OrbProjectile orbObject = orb.GetComponent<OrbProjectile>();
            orbObject.moveDirection = owner.cam.transform.forward;
            activeOrb = orb;
            orbObject.ownersMotor = this.gameObject.GetComponent<PuckOrb>();
            orbObject.owner = base.owner;
            
        }
        else
        {
            if (activeOrb != null)
            {
                owner.CharControl.Motor.SetPosition(activeOrb.transform.position);
                activeOrb.GetComponent<OrbProjectile>().CancelInvoke();
                activeOrb.GetComponent<OrbProjectile>().Kill();
            }
        }
        
    }

    public override void OnSwitchAwayFromAbility()
    {

    }

    public override void OnCooldownStart()
    {

    }

    public override void OnCooldownEnd()
    {

    }
}