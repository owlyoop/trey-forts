using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityMotor : MonoBehaviour
{

    public enum AbilityType { Normal, Toggle, Charges, Channel }
    public float Cooldown; //seconds
    public float CastStartupTime;
    public float CastRecoveryTime;

    public bool AbilityPressDisablesWeapon = false;

    public PlayerStats owner;

    public int AbilitySlot;



    public virtual void OnPressAbilityButton()
    {

    }


    public virtual void OnSwitchAwayFromAbility()
    {

    }

    public virtual void OnCooldownStart()
    {

    }

    public virtual void OnCooldownEnd()
    {

    }
}
