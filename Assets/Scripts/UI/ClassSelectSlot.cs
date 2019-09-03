using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectSlot : MonoBehaviour
{

	public Text buttonText;
	public Image icon;

    public bool isElite;
    public Text cost;
    public Text dollarSign;

	public WeaponSet _weaponSet;
	public ClassSelectMenu menu;


	private void Start()
	{
		menu = GetComponentInParent<GridReference>().menu;
		buttonText.text = _weaponSet.className;
        if (!isElite)
        {
            cost.enabled = false;
            dollarSign.enabled = false;
        }
	}

	public void OnClickSlot()
	{
		//fix
		menu.selectedClassName.text = _weaponSet.className;
		menu.selectedClass = _weaponSet;
		

        if (_weaponSet.isElite)
        {
            if (menu.player.currentCurrency >= menu.selectedClass.eliteMoneyCost)
            {
                menu.classSelectButton.interactable = true;
            }
            else
            {
                menu.classSelectButton.interactable = false;
            }
        }
        else
        {
            menu.classSelectButton.interactable = true;
        }


		//clear
		if (_weaponSet.weaponList.Count > 0 && menu.selectedClassWeapons.Count > 0)
		{
			for (int i = 0; i < menu.selectedClassWeapons.Count; i++)
			{
				Destroy(menu.selectedClassWeapons[i]);
			}
		}


		for (int i = 0; i < _weaponSet.weaponList.Count; i++)
		{
			GameObject newwep = Instantiate(menu.weaponUIPrefab, menu.wepGrid.transform);
			newwep.GetComponent<WeaponSlotUI>().wep = _weaponSet.weaponList[i];
			newwep.GetComponent<WeaponSlotUI>().icon.sprite = _weaponSet.weaponList[i].icon;
			newwep.GetComponent<WeaponSlotUI>().weaponName.text = _weaponSet.weaponList[i].weaponName;
			menu.selectedClassWeapons.Add(newwep);
		}
	}

}
