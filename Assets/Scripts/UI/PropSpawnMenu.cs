using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropSpawnMenu : MonoBehaviour
{
	public UIManager mainUI;
	public PlayerStats player;

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

    GameData gameData;

	private void Start()
	{
        gameData = player.gameManager.gameData;
        EventManager.onCombatPhaseStart += ReloadPropMenu;
		hasPropSelected = false;
        LoadListOfProps();
	}

    public void LoadListOfProps()
    {
        if (player.gameManager.currentGameState == GamePhases.GameState.CombatPhase)
        {
            if (gameData.buildPhaseProps.Count > 0)
            {
                for (int i = 0; i < gameData.combatPhaseProps.Count; i++)
                {
                    GameObject slot = Instantiate(slotPrefab, propGrid);
                    PropMenuSlot propSlot = slot.GetComponent<PropMenuSlot>();
                    propSlot.prop = gameData.combatPhaseProps[i];
                    propSlot.propMenuUI = this.GetComponent<PropSpawnMenu>();
                    propSlot.icon.sprite = gameData.combatPhaseProps[i].icon;
                    propSlot.iconText.text = gameData.combatPhaseProps[i].PropName;
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
            if (gameData.buildPhaseProps.Count > 0)
            {
                for (int i = 0; i < gameData.buildPhaseProps.Count; i++)
                {
                    GameObject slot = Instantiate(slotPrefab, propGrid);
                    PropMenuSlot propSlot = slot.GetComponent<PropMenuSlot>();
                    propSlot.prop = gameData.buildPhaseProps[i];
                    propSlot.propMenuUI = this.GetComponent<PropSpawnMenu>();
                    propSlot.icon.sprite = gameData.buildPhaseProps[i].icon;
                    propSlot.iconText.text = gameData.buildPhaseProps[i].PropName;
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
		if (selectedProp != null && player.currentCurrency > selectedProp.currencyCost)
		{
            if (player.gameManager.currentGameState == GamePhases.GameState.BuildPhase && selectedProp.canPlaceInBuildPhase)
            {
                isValid = true;
            }
            if (player.gameManager.currentGameState == GamePhases.GameState.CombatPhase && selectedProp.canPlaceInCombatPhase)
            {
                isValid = true;
            }

            if (selectedProp.propPrefab.GetComponent<FortwarsProp>() is MoneyPrinter && player.hasPlacedMoneyPrinter)
                isValid = false;
            if (isValid)
            {
                hasPropSelected = true;
                mainUI.playerInput.playerWeapons.SetSelectedProp(selectedProp);

                Debug.Log("poopoo");

                if (selectedProp.currencyCost > player.currentCurrency)
                {
                    hasPropSelected = false;
                    mainUI.playerInput.playerWeapons.SetSelectedProp(null);
                }
                mainUI.TransitionToState(PlayerUIState.None);
            }
			
		}
	}

}
