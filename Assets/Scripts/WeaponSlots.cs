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
    public WeaponMotor CurrentWeapon;

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

		for (int i = 0; i < player.currentClass.weaponList.Count; i++)
		{
			GameObject temp = Instantiate(player.currentClass.weaponList[i].weaponPrefab, transform);
			weaponSlots.Add(temp);
			ApplyWeaponStats(i);
            weaponSlots[i].GetComponent<WeaponMotor>().player = player;
			weaponSlots[i].SetActive(false);
		}

        if (weaponSlots.Count > 0)
        {
            weaponSlots[0].SetActive(true);
            CurrentWeapon = weaponSlots[0].GetComponent<WeaponMotor>();
        }

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

        for (int i = 0; i < player.currentClass.abilityList.Count; i++)
        {
            GameObject temp = Instantiate(player.currentClass.abilityList[i].AbilityPrefab, abilitySlotsTransform);
            temp.GetComponent<AbilityMotor>().owner = player;
            abilitySlots.Add(temp);
        }
    }

	public void SwitchActiveWeaponSlot(int index)
	{
		DecactivateAllWeapons();
        player.ui.RadialReload.StopReload();

		if (index < weaponSlots.Count)
		{
			weaponSlots[index].SetActive(true);
            weaponSlots[index].GetComponent<WeaponMotor>().OnSwitchToWeapon();
			weaponSlots[index].GetComponent<WeaponMotor>().UpdateUI();
		}

		activeWeaponIndex = index;
        CurrentWeapon = weaponSlots[index].GetComponent<WeaponMotor>();

    }

	public void SwitchToPropSpawner()
	{
		DecactivateAllWeapons();
		propWepSlots[0].SetActive(true);
	}

	public void SwitchToPropMover()
	{
		if (player.gameManager.currentGameState != GamePhases.GameState.CombatPhase)
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
        weaponSlots[index].GetComponent<WeaponMotor>().GetWeaponStats(player.currentClass.weaponList[index]);
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
