using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponSlots : NetworkBehaviour
{

	public PlayerStats player;
	public List<GameObject> weaponSlots;
	
	public FortwarsPropData selectedProp;

	public List<GameObject> playersProps;

	[SyncVar]
	public int activeWeaponIndex;

	private void Start()
	{
		
	}

	public void InitializeWeapons()
	{
		if (player.playerClass.weaponList.Count > 0)
		{
			if (weaponSlots.Count > 0)
			{
				for (int i = 0; i < weaponSlots.Count; i++)
				{
					weaponSlots[i].SetActive(true);
					Destroy(weaponSlots[i]);
					
				}
				weaponSlots.Clear();
			}

			for (int i = 0; i < player.playerClass.weaponList.Count; i++)
			{
				GameObject temp = Instantiate(player.playerClass.weaponList[i].weaponPrefab, transform);
				weaponSlots.Add(temp);
				//weaponSlots[i] = temp;
				ApplyWeaponStats(i);
				weaponSlots[i].SetActive(false);
			}
			weaponSlots[0].SetActive(true);
		}

	}

	public void SwitchActiveWeaponSlot(int index)
	{
		for (int i = 0; i < weaponSlots.Count; i++)
		{
			weaponSlots[i].SetActive(false);
		}

		if (index < weaponSlots.Count)
		{
			weaponSlots[index].SetActive(true);
			weaponSlots[index].GetComponent<WeaponMotor>().UpdateUI();
		}

		activeWeaponIndex = index;
	}

	
	public void ApplyWeaponStats(int index)
	{
		if (player.playerClass.weaponList[index] is RangedProjectile)
		{
			RangedProjectile tempWep = player.playerClass.weaponList[index] as RangedProjectile;
			weaponSlots[index].GetComponent<ProjectileLauncher>().GetWeaponStats(tempWep);
		}
	}

	public void SetSelectedProp(FortwarsPropData prop)
	{
		selectedProp = prop;

		if (weaponSlots[activeWeaponIndex].GetComponent<PropSpawner>() != null)
		{
			weaponSlots[activeWeaponIndex].GetComponent<PropSpawner>().SetSelectedProp();
		}
	}
}
