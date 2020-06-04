using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropSpawnMenu : MonoBehaviour
{
	public UIManager mainUI;
	public PlayerStats playersStats;

	public FortwarsPropData selectedProp;

	public GameObject slotPrefab;
	public Transform propGrid;

	public Image selectedPropImage;
	public Text selectedPropName;
	public Text selectedPropDesc;
	public Text selectedPropCost;

	public Button createPropButton;

	public bool hasPropSelected;

    public List<PropMenuSlot> slots = new List<PropMenuSlot>();

	public delegate void PropSelected();
	public static event PropSelected OnPropSelected;

	private void Start()
	{
        EventManager.onCombatPhaseStart += ReloadPropMenu;
		hasPropSelected = false;
        LoadListOfProps();
	}

    public void LoadListOfProps()
    {
        if (playersStats._gameManager.isInCombatPhase)
        {
            if (playersStats._gameManager.buildPhaseProps.Count > 0)
            {
                for (int i = 0; i < playersStats._gameManager.combatPhaseProps.Count; i++)
                {
                    GameObject slot = Instantiate(slotPrefab, propGrid);
                    PropMenuSlot propSlot = slot.GetComponent<PropMenuSlot>();
                    propSlot.prop = playersStats._gameManager.combatPhaseProps[i];
                    propSlot.propMenuUI = this.GetComponent<PropSpawnMenu>();
                    propSlot.icon.sprite = playersStats._gameManager.combatPhaseProps[i].icon;
                    propSlot.iconText.text = playersStats._gameManager.combatPhaseProps[i].PropName;
                    slots.Add(propSlot);
                }
            }
            else
            {
                Debug.Log("No props in the game manager combat-phase prop list");
            }
        }
        else
        {
            if (playersStats._gameManager.buildPhaseProps.Count > 0)
            {
                for (int i = 0; i < playersStats._gameManager.buildPhaseProps.Count; i++)
                {
                    GameObject slot = Instantiate(slotPrefab, propGrid);
                    PropMenuSlot propSlot = slot.GetComponent<PropMenuSlot>();
                    propSlot.prop = playersStats._gameManager.buildPhaseProps[i];
                    propSlot.propMenuUI = this.GetComponent<PropSpawnMenu>();
                    propSlot.icon.sprite = playersStats._gameManager.buildPhaseProps[i].icon;
                    propSlot.iconText.text = playersStats._gameManager.buildPhaseProps[i].PropName;
                    slots.Add(propSlot);
                }
            }
            else
            {
                Debug.Log("No props in the game manager build-phase prop list");
            }
        }
    }

    //Used when the game goes from Build Phase to Combat Phase. The list of props are different.
    public void ReloadPropMenu()
    {
        foreach(PropMenuSlot slot in slots)
        {
            Destroy(slot.gameObject);
        }
        slots.Clear();

        LoadListOfProps();
    }

	public void OnClickCreateButton()
	{
        bool isValid = false;
		if (selectedProp != null && playersStats.CurrentCurrency > selectedProp.currencyCost)
		{
            if (playersStats._gameManager.isInBuildPhase && selectedProp.canPlaceInBuildPhase)
            {
                isValid = true;
            }
            if (playersStats._gameManager.isInCombatPhase && selectedProp.canPlaceInCombatPhase)
            {
                isValid = true;
            }

            if (selectedProp.propPrefab.GetComponent<FortwarsProp>() is MoneyPrinter && playersStats.hasPlacedMoneyPrinter)
                isValid = false;
            if (isValid)
            {
                hasPropSelected = true;
                mainUI.playerInput.playerWeapons.SetSelectedProp(selectedProp);

                Debug.Log("poopoo");

                if (selectedProp.currencyCost > playersStats.CurrentCurrency)
                {
                    hasPropSelected = false;
                    mainUI.playerInput.playerWeapons.SetSelectedProp(null);
                }
                mainUI.TransitionToState(PlayerUIState.None);
            }
			
		}
	}

}
