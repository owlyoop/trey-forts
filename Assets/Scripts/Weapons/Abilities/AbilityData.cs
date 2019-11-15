using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Weapon/Ability")]
public class AbilityData : ScriptableObject
{

    public string AbilityName;
    public string AbilityDescription;

    public int AbilitySlot;

    public float Cooldown; // in seconds
    public float CastStartupTime; //time where player can't do anything after ability button is pressed
    public float CastRecoveryTime; //time where player can't do anything after ability is executed

    public bool CanMoveDuringStartupTime = true;
    public bool CanMoveDuringRecoveryTime = true;

    public Sprite icon;

    public GameObject AbilityPrefab;

    public AbilityMotor.AbilityType AbilityType;

}
