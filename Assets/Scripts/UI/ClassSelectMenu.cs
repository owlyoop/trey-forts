using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectMenu : MonoBehaviour
{

    [Header("Current Selected Class")]
	public WeaponSet selectedClass;
	public List<GameObject> selectedClassWeapons = new List<GameObject>();
	public List<Weapon> selectedClassWeaponData = new List<Weapon>();

    [Header("Class Lists")]
	public List<ClassSelectSlot> classListUISlots = new List<ClassSelectSlot>();
    public List<ClassSelectSlot> eliteClassListUISlots = new List<ClassSelectSlot>();

    [Header("References")]
    public PlayerStats player;

    public Text selectedClassName;

    public Button ClassSelectButton;
    public Button EliteClassButton;
    public Button SpawnAsEliteButton;

    public GameObject classButtonPrefab;
    public GameObject eliteClassButtonPrefab;
	public GameObject weaponUIPrefab;

	public GameObject classGrid;
    public GameObject eliteClassGrid;
	public GameObject wepGrid;

    public Text CurrentCurrency;
    public Text dollarSign;
    public Text moneyText;

    GameData gameData;

	private void Start()
	{
        if (!player.netIdentity.isLocalPlayer)
            return;
        gameData = player.gameManager.gameData;

        foreach(WeaponSet elite in gameData.eliteClassList)
        {
            player.eliteClassLives.Add(elite.className, 0);
        }

		ClassSelectButton.interactable = false;
        EliteClassButton.interactable = false;
        SpawnAsEliteButton.interactable = false;

		
		for (int i = 0; i < gameData.classList.Count; i++)
		{
			GameObject slot = Instantiate(classButtonPrefab, classGrid.transform);
            ClassSelectSlot slotcomp = slot.GetComponent<ClassSelectSlot>();
            slotcomp.weaponSet = gameData.classList[i];
			slotcomp.icon.sprite = gameData.classList[i].icon;
			slotcomp.buttonText.text = gameData.classList[i].className;

			classListUISlots.Add(slotcomp);
		}

        for (int i = 0; i < gameData.eliteClassList.Count; i++)
        {
            GameObject slot = Instantiate(eliteClassButtonPrefab, eliteClassGrid.transform);
            ClassSelectSlot slotcomp = slot.GetComponent<ClassSelectSlot>();
            slotcomp.weaponSet = gameData.eliteClassList[i];
            slotcomp.isElite = true;
            slotcomp.dollarSign.enabled = true;
            slotcomp.cost.enabled = true;
            slotcomp.cost.text = gameData.eliteClassList[i].eliteMoneyCost.ToString();
            slotcomp.icon.sprite = gameData.eliteClassList[i].icon;
            slotcomp.buttonText.text = gameData.eliteClassList[i].className;
            slotcomp.numberOfLives.text = player.eliteClassLives[slotcomp.weaponSet.className].ToString();

            eliteClassListUISlots.Add(slotcomp);
        }
        
        if (!player.HasPreviouslyPlayed)
        {
            CurrentCurrency.text = "";
        }
    }

    public void OnEnable()
    {
        UpdateEliteClassSlots();
    }

    //Incase the player's money changes which makes them able to afford an elite class
    public void UpdateClassSelectMenu()
    {
        if (selectedClass.isElite && player.currentCurrency >= selectedClass.eliteMoneyCost)
        {
            EliteClassButton.interactable = true;
        }
        else EliteClassButton.interactable = false;
    }

	public void OnClickSelectButton()
	{
		if (player.currentClass.className == "Spectator")
		{
            player.SetQueuedClass(selectedClass);
			SetTeamIfWasSpectator(player.QueuedTeam);
			player.ui.SetActiveMainHud(true);
		}
		else
		{
            player.SetQueuedClass(selectedClass);
            player.ui.SetActiveMainHud(true);
		}

        if (!player.HasPreviouslyPlayed)
        {
            player.SetCurrencyAmount(player.gameManager.buildPhaseStartingMoney);
        }
        player.HasPreviouslyPlayed = true;
        player.ui.TransitionToState(PlayerUIState.None);
	}

    public void OnClickBuyEliteButton()
    {
        if (player.currentClass.className != "Spectator")
        {
            player.SetCurrencyAmount(player.currentCurrency - selectedClass.eliteMoneyCost);
            player.SetQueuedClass(selectedClass);
            player.ui.SetActiveMainHud(true);
            player.AddEliteClassLife(selectedClass);
            player.ui.TransitionToState(PlayerUIState.None);
        }
    }

    public void OnClickSpawnAsEliteButton()
    {
        player.SetQueuedClass(selectedClass);
    }

	public void SetTeamIfWasSpectator(PlayerStats.PlayerTeam team)
	{
		player.SetTeam(team);
        player.SetQueuedClass(selectedClass);
        player.RespawnPlayer();
	}

    public void UpdateEliteClassSlots()
    {
        foreach(ClassSelectSlot slot in eliteClassListUISlots)
        {
            slot.UpdateLivesText();
        }
    }
}


