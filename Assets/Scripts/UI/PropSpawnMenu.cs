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
	

	public delegate void PropSelected();
	public static event PropSelected OnPropSelected;

	private void Start()
	{
		
		hasPropSelected = false;

		for (int i = 0; i < playersStats._gameManager.buildPhaseProps.Count; i++)
		{
			GameObject slot = Instantiate(slotPrefab, propGrid);
			slot.GetComponent<PropMenuSlot>().prop = playersStats._gameManager.buildPhaseProps[i];
			slot.GetComponent<PropMenuSlot>().propMenuUI = this.GetComponent<PropSpawnMenu>();
			slot.GetComponent<PropMenuSlot>().icon.sprite = playersStats._gameManager.buildPhaseProps[i].icon;
			slot.GetComponent<PropMenuSlot>().iconText.text = playersStats._gameManager.buildPhaseProps[i].PropName;
		}
	}

	public void OnClickCreateButton()
	{
        bool isValid = false;
		if (selectedProp != null && playersStats.currentCurrency > selectedProp.currencyCost)
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
                mainUI.player.playerWeapons.SetSelectedProp(selectedProp);

                Debug.Log("poopoo");

                if (selectedProp.currencyCost > playersStats.currentCurrency)
                {
                    hasPropSelected = false;
                    mainUI.player.playerWeapons.SetSelectedProp(null);
                }
                mainUI.PropSpawnMenuSetActive(false);
            }
			
		}
	}

}
