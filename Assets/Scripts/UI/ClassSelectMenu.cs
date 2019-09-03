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

    public List<WeaponSet> eliteClassListDataAll = new List<WeaponSet>();
    public List<GameObject> eliteClassListUISlots = new List<GameObject>();

    public GameObject classButtonPrefab;
    public GameObject eliteClassButtonPrefab;
	public GameObject weaponUIPrefab;

	public GameObject classGrid;
    public GameObject eliteClassGrid;
	public GameObject wepGrid;

	public int QueuedTeam;

	public bool cameFromTeamMenu = false;

	private void Start()
	{
		classSelectButton.interactable = false;
		
		for (int i = 0; i < classListDataAll.Count; i++)
		{
			GameObject slot = Instantiate(classButtonPrefab, classGrid.transform);
            ClassSelectSlot slotcomp = slot.GetComponent<ClassSelectSlot>();
            slotcomp._weaponSet = classListDataAll[i];
			slotcomp.icon.sprite = classListDataAll[i].icon;
			slotcomp.buttonText.text = classListDataAll[i].className;

			classListUISlots.Add(slot);
		}

        for (int i = 0; i < eliteClassListDataAll.Count; i++)
        {
            GameObject slot = Instantiate(eliteClassButtonPrefab, eliteClassGrid.transform);
            ClassSelectSlot slotcomp = slot.GetComponent<ClassSelectSlot>();
            slotcomp._weaponSet = eliteClassListDataAll[i];
            slotcomp.dollarSign.enabled = true;
            slotcomp.cost.enabled = true;
            slotcomp.cost.text = eliteClassListDataAll[i].eliteMoneyCost.ToString();
            slotcomp.icon.sprite = eliteClassListDataAll[i].icon;
            slotcomp.buttonText.text = eliteClassListDataAll[i].className;

            eliteClassListUISlots.Add(slot);
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


