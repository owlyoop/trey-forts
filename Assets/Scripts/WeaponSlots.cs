using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlots : MonoBehaviour
{

	public PlayerStats player;
	public List<GameObject> weaponSlots;
	public List<GameObject> propWepSlots;

    public List<GameObject> abilitySlots;
	
	public FortwarsPropData selectedProp;

    public Transform abilitySlotsTransform;
	
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

            propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
            propWepSlots[0].SetActive(false);
            propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
            propWepSlots[1].SetActive(false);

            if (abilitySlots.Count > 0)
            {
                for (int i = 0; i < abilitySlots.Count; i++)
                {
                    Destroy(abilitySlots[i]);

                }
                abilitySlots.Clear();
            }

            for (int i = 0; i < player.playerClass.abilityList.Count; i++)
            {
                GameObject temp = Instantiate(player.playerClass.abilityList[i].AbilityPrefab, abilitySlotsTransform);
                temp.GetComponent<AbilityMotor>().owner = player;
                abilitySlots.Add(temp);
            }
		}

	}

	public void SwitchActiveWeaponSlot(int index)
	{
		DecactivateAllWeapons();

		if (index < weaponSlots.Count)
		{
			weaponSlots[index].SetActive(true);
            weaponSlots[index].GetComponent<WeaponMotor>().OnSwitchToWeapon();
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
            propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
            SwitchActiveWeaponSlot(0);
		}
        if (propWepSlots[0].activeSelf)
        {
            propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
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

    public void ActivateAbility(int index)
    {
        if (abilitySlots[index].GetComponent<AbilityMotor>().AbilityPressDisablesWeapon)
        {
            DecactivateAllWeapons();
        }
        Debug.Log("ability cast");
        abilitySlots[index].GetComponent<AbilityMotor>().OnPressAbilityButton();
    }
}
