using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectMenu : MonoBehaviour
{

	public WeaponSet selectedClass;
	public PlayerStats player;

	public Text selectedClassName;

	public Button ClassSelectButton;
    public Button EliteClassButton;

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

    public Text CurrentCurrency;
    public Text dollarSign;
    public Text moneyText;

	public PlayerStats.PlayerTeam QueuedTeam;

	private void Start()
	{
		ClassSelectButton.interactable = false;
        EliteClassButton.interactable = false;

        EventManager.onCombatPhaseStart += EnableEliteBuyButton;
		
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
            slotcomp.isElite = true;
            slotcomp.dollarSign.enabled = true;
            slotcomp.cost.enabled = true;
            slotcomp.cost.text = eliteClassListDataAll[i].eliteMoneyCost.ToString();
            slotcomp.icon.sprite = eliteClassListDataAll[i].icon;
            slotcomp.buttonText.text = eliteClassListDataAll[i].className;

            eliteClassListUISlots.Add(slot);
        }
        
        if (!player.HasPreviouslyPlayed)
        {
            CurrentCurrency.text = "";
        }
    }

    //Do this whenever combat phase begins
    public void EnableEliteBuyButton()
    {
        EliteClassButton.interactable = true;
    }

	public void OnClickSelectButton()
	{
		if (player.CurrentClass.className == "Spectator")
		{
            player.SetQueuedClass(selectedClass);
			SetTeamIfWasSpectator(QueuedTeam);
			player.ui.SetActiveMainHud(true);
		}
		else
		{
            player.SetQueuedClass(selectedClass);
            player.ui.SetActiveMainHud(true);
		}

        if (!player.HasPreviouslyPlayed)
        {
            player.OnChangeCurrencyAmount(player.startingCurrency);
        }
        player.HasPreviouslyPlayed = true;
        player.ui.TransitionToState(PlayerUIState.None);
	}

    public void OnClickBuyEliteButton()
    {
        if (player.CurrentClass.className == "Spectator")
        {
            player.SetQueuedClass(selectedClass);
            SetTeamIfWasSpectator(QueuedTeam);
            player.ui.SetActiveMainHud(true);
        }
        else
        {
            player.SetQueuedClass(selectedClass);
            player.ui.SetActiveMainHud(true);
            player.EliteClassLivesLeft = 1;
        }

        player.ui.TransitionToState(PlayerUIState.None);
    }

	public void SetTeamIfWasSpectator(PlayerStats.PlayerTeam team)
	{
		player.SetTeam(team);
        player.SetQueuedClass(selectedClass);
        player.RespawnAndInitialize();
	}
}


