using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GameData", menuName = "Fortwars/GameData")]
public class GameData : ScriptableObject
{
    public WeaponSet spectatorClass;

    public List<WeaponSet> classList;
    public List<WeaponSet> eliteClassList;

    public List<FortwarsPropData> buildPhaseProps = new List<FortwarsPropData>();
    public List<FortwarsPropData> combatPhaseProps = new List<FortwarsPropData>();
}
