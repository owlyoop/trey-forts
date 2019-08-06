using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropMenuSlot : MonoBehaviour
{

	public Image icon;
	public Text iconText;

	public int propIndex;

	public FortwarsPropData prop;
	public PropSpawnMenu propMenuUI;

	private void Start()
	{
		icon.sprite = prop.icon;
		iconText.text = prop.PropName;
	}


	public void OnClickSlot()
	{
		propMenuUI.selectedPropIndex = propIndex;
		propMenuUI.selectedProp = propMenuUI.mainUI.props[propIndex];
		propMenuUI.selectedPropName.text = propMenuUI.mainUI.props[propIndex].PropName;
		propMenuUI.selectedPropDesc.text = propMenuUI.mainUI.props[propIndex].PropDescription;
		propMenuUI.selectedPropImage.sprite = propMenuUI.mainUI.props[propIndex].icon;
		propMenuUI.selectedPropCost.text = propMenuUI.mainUI.props[propIndex].currencyCost.ToString();
		
	}
}
