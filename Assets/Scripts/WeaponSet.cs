using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Class", menuName = "Fortwars Class")]
public class WeaponSet : ScriptableObject
{

	public int maxHealth;
	public float moveSpeedMultiplier = 1f;
	public float jumpHeightMultiplier = 1f;

	public List<Weapon> weaponList;

	public string className;
	public Sprite icon;
}
