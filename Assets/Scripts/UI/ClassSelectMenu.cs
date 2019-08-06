using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectMenu : MonoBehaviour
{

	public WeaponSet selectedClass;
	public PlayerStats player;

	public Text selectedClassName;

	public Button classSelectButton;

	public List<GameObject> selectedClassWeapons = new List<GameObject>();
	public List<Weapon> selectedClassWeaponData = new List<Weapon>();

	public List<GameObject> classListUISlots = new List<GameObject>();
	public List<WeaponSet> classListDataAll = new List<WeaponSet>();

	public GameObject classButtonPrefab;
	public GameObject weaponUIPrefab;

	public GameObject classGrid;
	public GameObject wepGrid;

	private void Start()
	{
		classSelectButton.interactable = false;
		
		for (int i = 0; i < classListDataAll.Count; i++)
		{
			GameObject slot = Instantiate(classButtonPrefab, classGrid.transform);
			slot.GetComponent<ClassSelectSlot>().icon.sprite = classListDataAll[i].icon;
			slot.GetComponent<ClassSelectSlot>().buttonText.text = classListDataAll[i].className;
			classListUISlots.Add(slot);
		}

	}

	public void OnClickSelectButton()
	{
		player.OnDeath();
		player.playerClass = selectedClass;
		player.GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
		player.GetComponent<PlayerInput>().mainUI.ClassSelectMenuSetActive(false);
		player.InitializeValues();
	}

	public void UpdateUI()
	{
		
	}

	public void SelectClass(WeaponSet _class)
	{

	}

}


