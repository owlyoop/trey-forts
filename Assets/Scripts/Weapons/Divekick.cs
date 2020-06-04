using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Divekick : Damager
{
    int baseDamage = 50;
    public float KnockbackForce = 10f;

    public PlayerStats player;
   
    public bool isValidDamageTarget = false;
    public bool isDivekickActive = false;

    public int iter = 0;

    private float _divekickExpireTime;
    public float DivekickLifetime = 1.5f;

    public IDamagable _target;
    private void Start()
    {
        foreach (PlayerHitbox cols in player.hitboxCollection)
        {
            Physics.IgnoreCollision(cols.col, this.GetComponent<Collider>(), true);
        }
    }

    private void Update()
    {
        if (isDivekickActive)
        {
            if (Time.time > _divekickExpireTime)
            {
                player.CharControl.TransitionToState(player.CurrentClass.defaultState);
                DeactivateDivekick();
            }
        }
    }

    public void ActivateDivekick()
    {
        this.gameObject.SetActive(true);
        isDivekickActive = true;
        iter = 0;
        _divekickExpireTime = Time.time + DivekickLifetime;
        player.HideOwnPlayerModel(false);
    }

    public void DeactivateDivekick()
    {
        isValidDamageTarget = false;
        isDivekickActive = false;
        _target = null;
        player.HideOwnPlayerModel(true);
        this.gameObject.SetActive(false);
        
    }


    // divekick hitbox can only damage players once during its lifetime. make sure it doesnt duplicate damage
    private void OnTriggerEnter(Collider col)
    {
        isValidDamageTarget = true;
        var target = col.GetComponent<IDamagable>();
        var playerHit = col.GetComponent<PlayerHitbox>();
        //Damagable found
        if (target != null && isDivekickActive)
        {
            //Check if collider is a player's hitbox, we need to ignore our own
            if (playerHit != null)
            {
                if (playerHit.player == player)
                {
                    //Debug.Log("is own");
                    isValidDamageTarget = false;
                }
                else
                {
                    //Debug.Log("enemy hit");
                    isValidDamageTarget = true;
                    _target = target;
                }

            }
            else if (col.GetComponent<TrainingDummy>() != null || col.GetComponent<FortwarsProp>() != null) //collider is a prop or a training dummy
            {
                isValidDamageTarget = true;
                _target = target;
            }

            if (isValidDamageTarget && _target != null && iter == 0)
            {
                DealDamage(_target);
                Debug.Log(target.ToString());
                isDivekickActive = false;
                isValidDamageTarget = false;
                AddKnockback(col.gameObject, false);
                DeactivateDivekick();
                
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        
    }


    void AddKnockback(GameObject victim, bool victimIsPlayer)
    {
        Vector3 wishDirection = (victim.transform.position - player.transform.position);
        Vector3 velocityToAdd = -wishDirection.normalized * KnockbackForce;
        player.CharControl.AddVelocity(-player.CharControl.Motor.Velocity);
        player.CharControl.AddVelocity(velocityToAdd);

        if (victimIsPlayer)
        {

        }
    }

    void DealDamage(IDamagable target)
    {
        if (player.CharControl.CurrentCharacterState == KinematicCharacterController.Owly.CharacterState.Divekick)
        {
            iter++;
            target.TakeDamage(baseDamage, DamageTypes.Physical, player, player.transform.position, PlayerStats.DamageIndicatorType.Directional);
            player.CharControl.TransitionToState(player.CurrentClass.defaultState);
            
        }

    }

}
