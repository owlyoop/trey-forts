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

	public int BaseDamage;
	public float ShotsPerSecond;

	public int ClipSize;
	public float ReloadTime;

	public int MaxAmmo;

	public Damager.DamageTypes damageType;

	public List<WeaponComponent> weaponComponents;

}
