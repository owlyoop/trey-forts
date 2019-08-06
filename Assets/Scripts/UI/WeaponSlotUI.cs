using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
	public Image icon;
	public Text weaponName;
	public Weapon wep;

	private void Start()
	{
		icon.sprite = wep.icon;
		weaponName.text = wep.weaponName;
	}
}
