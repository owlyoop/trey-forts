using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropSpawnMenu : MonoBehaviour
{
	public UIManager mainUI;
	PlayerStats playersStats;

	public FortwarsPropData selectedProp;
	public int selectedPropIndex;

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
		playersStats = mainUI.GetComponent<PlayerStats>();
		hasPropSelected = false;
	}

	public void OnClickCreateButton()
	{
		if (selectedProp != null && playersStats.currentCurrency > selectedProp.currencyCost)
		{
			mainUI.player.playerWeapons.player.GetComponent<PlayerInput>().selectedPropMenuIndex = selectedPropIndex;
			mainUI.player.playerWeapons.SetSelectedProp(selectedProp);
			hasPropSelected = true;

			if (selectedProp.currencyCost > playersStats.currentCurrency)
			{
				hasPropSelected = false;
				mainUI.player.playerWeapons.SetSelectedProp(null);
			}
			mainUI.PropSpawnMenuSetActive(false);

		}
	}

}
