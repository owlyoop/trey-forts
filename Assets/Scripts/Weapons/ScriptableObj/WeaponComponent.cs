using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComponent : ScriptableObject
{

	public PlayerStats player;

	public StatModifier statModifier;

	public float statModValue;
	public StatModType statModType;

	public enum ModifiedStat { MaxHealth, MaxMana, MoveSpeed, Damage, FireRate}

	public void CreateStatModifier()
	{
		statModifier = new StatModifier(statModValue, statModType);
	}
}
