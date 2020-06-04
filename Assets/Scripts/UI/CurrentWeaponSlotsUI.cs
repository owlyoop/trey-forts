using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentWeaponSlotsUI : MonoBehaviour
{

	public List<GameObject> wepList;
	public UIManager mainUI;

	public GameObject slotPrefab;
	public GameObject contentGrid;


	private void Start()
	{
	}

	private void OnEnable()
	{

	}

	public void AddSlots()
	{
		if (wepList.Count > 0)
		{
			for (int x = 0; x < wepList.Count; x++)
			{

				Destroy(wepList[x].gameObject);
			}
		}
		for (int i = 0; i < mainUI.player.CurrentClass.weaponList.Count; i++)
		{

			GameObject slot = Instantiate(slotPrefab, contentGrid.transform);
			slot.GetComponent<CurrentWeaponsUI>().wep = mainUI.player.CurrentClass.weaponList[i];
			slot.GetComponent<CurrentWeaponsUI>().wepName.text = slot.GetComponent<CurrentWeaponsUI>().wep.weaponName;
			wepList.Add(slot);
		}
	}

}
