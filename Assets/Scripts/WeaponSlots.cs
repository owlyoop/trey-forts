using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlots : MonoBehaviour
{

	public PlayerStats player;
	public List<GameObject> weaponSlots;
	public List<GameObject> propWepSlots;
	
	public FortwarsPropData selectedProp;
	
	public int activeWeaponIndex;

	private void Start()
	{
		EventManager.onBuildPhaseEnd += EndOfBuildPhase;
	}

	private void OnDisable()
	{
		EventManager.onBuildPhaseEnd -= EndOfBuildPhase;
	}

	public void InitializeWeapons()
	{

		Debug.Log("init wep func");
		if (player.playerClass.weaponList.Count > 0)
		{
			if (weaponSlots.Count > 0)
			{
				for (int i = 0; i < weaponSlots.Count; i++)
				{
					weaponSlots[i].SetActive(true);
                    weaponSlots[i].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
					Destroy(weaponSlots[i]);
					
				}
				weaponSlots.Clear();
			}

			for (int i = 0; i < player.playerClass.weaponList.Count; i++)
			{
				GameObject temp = Instantiate(player.playerClass.weaponList[i].weaponPrefab, transform);
				weaponSlots.Add(temp);
				ApplyWeaponStats(i);
				weaponSlots[i].SetActive(false);
			}
			weaponSlots[0].SetActive(true);
		}

	}

	public void SwitchActiveWeaponSlot(int index)
	{
		DecactivateAllWeapons();

		if (index < weaponSlots.Count)
		{
			weaponSlots[index].SetActive(true);
			weaponSlots[index].GetComponent<WeaponMotor>().UpdateUI();
		}

		activeWeaponIndex = index;
	}

	public void SwitchToPropSpawner()
	{
		DecactivateAllWeapons();
		propWepSlots[0].SetActive(true);
	}

	public void SwitchToPropMover()
	{
		if (!player._gameManager.isInCombatPhase)
		{
			DecactivateAllWeapons();
			propWepSlots[1].SetActive(true);
		}
	}

	void EndOfBuildPhase()
	{
		if (propWepSlots[1].activeSelf)
		{
			SwitchActiveWeaponSlot(0);
		}
	}

	public void DecactivateAllWeapons()
	{
		for (int i = 0; i < weaponSlots.Count; i++)
		{
			weaponSlots[i].SetActive(false);
		}

		for (int i = 0; i < propWepSlots.Count; i++)
		{
			propWepSlots[i].SetActive(false);
		}
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

		if (propWepSlots[0] != null)
		{
			propWepSlots[0].GetComponent<PropSpawner>().SetSelectedProp();
		}
	}
}
