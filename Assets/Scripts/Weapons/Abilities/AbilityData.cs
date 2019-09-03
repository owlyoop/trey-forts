using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityData : ScriptableObject
{

    public string AbilityName;
    public string AbilityDescription;

    public int AbilitySlot;

    public Sprite icon;

    public GameObject AbilityPrefab;

    public AbilityMotor.AbilityType AbilityType;

}
