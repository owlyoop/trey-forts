using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectSlot : MonoBehaviour
{

	public Text buttonText;
	public Image icon;

	public WeaponSet _weaponSet;
	public ClassSelectMenu menu;


	private void Start()
	{
		menu = GetComponentInParent<GridReference>().menu;
		buttonText.text = _weaponSet.className;
	}

	public void OnClickSlot()
	{
		//fix
		menu.selectedClassName.text = _weaponSet.className;
		menu.selectedClass = _weaponSet;
		menu.classSelectButton.interactable = true;
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
