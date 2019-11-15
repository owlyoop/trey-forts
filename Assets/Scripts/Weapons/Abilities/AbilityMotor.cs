using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityMotor : MonoBehaviour
{

    public AbilityData baseAbility;
    public enum AbilityType { Normal, Toggle, Charges, Channel }
    AbilityType _abilityType;
    public float Cooldown; //seconds
    public float CastStartupTime;
    public float CastRecoveryTime;

    public bool AbilityPressDisablesWeapon = false;

    public Sprite icon;
    public PlayerStats owner;

    public int AbilitySlot;

    private void Start()
    {
        Cooldown = baseAbility.Cooldown;
        _abilityType = baseAbility.AbilityType;
        CastStartupTime = baseAbility.CastStartupTime;
        CastRecoveryTime = baseAbility.CastRecoveryTime;
    }

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
