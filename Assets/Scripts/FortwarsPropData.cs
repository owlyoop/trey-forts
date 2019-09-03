using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Prop", menuName = "Fortwars/Prop")]
public class FortwarsPropData : ScriptableObject
{

	public string PropName;
	public string PropDescription;

	public Sprite icon;

	public GameObject propPrefab;

	public float maxHealth;
	public int currencyCost;

    public bool canPlaceInBuildPhase;
    public bool canPlaceInCombatPhase;


	public MaterialType material;

	public enum MaterialType { Wood, Concrete, Metal}

}
