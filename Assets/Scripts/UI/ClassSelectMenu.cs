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

	public int QueuedTeam;

	public bool cameFromTeamMenu = false;

	private void Start()
	{
		classSelectButton.interactable = false;
		
		for (int i = 0; i < classListDataAll.Count; i++)
		{
			GameObject slot = Instantiate(classButtonPrefab, classGrid.transform);
			slot.GetComponent<ClassSelectSlot>()._weaponSet = classListDataAll[i];

			slot.GetComponent<ClassSelectSlot>().icon.sprite = classListDataAll[i].icon;
			slot.GetComponent<ClassSelectSlot>().buttonText.text = classListDataAll[i].className;
			classListUISlots.Add(slot);
		}

	}

	public void OnClickSelectButton()
	{
		if (!player.photonView.IsMine)
			return;
		if (player.playerClass.className == "Spectator")
		{
			player.queuedClass = selectedClass;
			player.ChangeAwayFromSpectator();
			SetTeamIfWasSpectator(QueuedTeam);
			player.ui.SetActiveMainHud(true);
		}
		else
		{
			player.queuedClass = selectedClass;
			player.ui.SetActiveMainHud(true);
		}



		player.GetComponent<PlayerInput>().mainUI.ClassSelectMenuSetActive(false);

		cameFromTeamMenu = false;

	}

	public void SetTeamIfWasSpectator(int team)
	{
		player.SetTeam(team);
		player.playerTeam = team;
		player.playerClass = selectedClass;
		player.RespawnAndInitialize();
	}

	public void UpdateUI()
	{
		
	}

	public void SelectClass(WeaponSet _class)
	{

	}

}


