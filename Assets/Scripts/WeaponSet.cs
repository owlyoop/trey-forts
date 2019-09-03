using KinematicCharacterController.Owly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Class", menuName = "Fortwars Class")]
public class WeaponSet : ScriptableObject
{
	public bool isElite;
    public int eliteMoneyCost;

	public int maxHealth;
    public float moveSpeedPercentAdd = 0f;
    public float jumpHeightPercentAdd = 0f;

	public List<Weapon> weaponList;

    public List<GameObject> abilityList;

	public List<StatusEffect> PassiveEffects;



	public string className;
	public Sprite icon;

	public CharacterState defaultState;

}
