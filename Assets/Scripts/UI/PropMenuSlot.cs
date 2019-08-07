using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropMenuSlot : MonoBehaviour
{

	public Image icon;
	public Text iconText;

	//public int propIndex;

	public FortwarsPropData prop;
	public PropSpawnMenu propMenuUI;

	GamePhases gameManager;

	private void Start()
	{
		
		gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GamePhases>();
		icon.sprite = prop.icon;
		iconText.text = prop.PropName;
	}


	public void OnClickSlot()
	{
		propMenuUI.selectedProp = prop;
		propMenuUI.selectedPropName.text = prop.PropName;
		propMenuUI.selectedPropDesc.text = prop.PropDescription;
		propMenuUI.selectedPropImage.sprite = prop.icon;
		propMenuUI.selectedPropCost.text = prop.currencyCost.ToString();
		
	}
}
