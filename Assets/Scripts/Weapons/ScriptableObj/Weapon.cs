using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Generic Weapon")]
public class Weapon : ScriptableObject
{

	public string weaponName;
	public string weaponDescription;

	public Sprite icon;

	public GameObject weaponPrefab;

	public int baseDamage;
	public float shotsPerSecond;

	public int clipSize;
	public float reloadTime;

	public int maxAmmo;

	public Damager.DamageTypes damageType;

	public List<WeaponComponent> weaponComponents;

}
