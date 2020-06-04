using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController.Examples;


public enum PlayerUIState
{
    MainMenu,
    OptionsMenu,
    PropMenu,
    ClassSelectMenu,
    TeamSelectMenu,
    IngameHUD,
    Scoreboard,
    None
}

public class UIManager : MonoBehaviour
{
    [Header("Player")]
    public PlayerInput playerInput;
	public PlayerStats player;
    public PlayerUIState CurrentUIState;
    public PlayerUIState previousState;

	public Canvas MainUICanvas;

    [Header("Menu References")]
	public PropSpawnMenu PropMenu;
	public ClassSelectMenu ClassMenu;
    public GameObject PropSpawnMenu;
	public GameObject TeamSelectMenu;
	public GameObject ClassSelectMenu;
    public MainMenu MainEscMenu;
    public GameObject Scoreboard;

	public GameObject MainHud;

    [Header("Main Ingame Hud")]
    public Text HealthText;
	public Text CurrencyAmount;
	public Text AmmoInClip;
	public Text AmmoAmount;
	public Text DollarSign;

    public RadialReload RadialReload;

    public GameObject DamageIndicators;
    public GameObject DamageIndicatorPrefab;

	public RectTransform HealthBar;
	public RectTransform HealthBarBackground;

	public Text TimerMinutes;
	public Text TimerSeconds;

	public Image FlagRadialBlue;
	public Image FlagRadialRed;

	public Text BlueScore;
	public Text RedScore;
	public Text GameStatusText;

    public Text HelpText;

	public List<Text> RespawnText = new List<Text>();
	public Text RespawnTimer;

    [Header("StatusEffects")]
    public GameObject StatusEffectIcons;
    public GameObject StatusEffectIconPrefab;

    [Header("Weapons/Abilities")]
    public GameObject CurrentWeaponSlots;
    public GameObject CurrentAbilitySlots;

	public bool HasUnlockedMouseUIEnabled;


	public GamePhases ServerTimer;

	public bool IsSlotUIOpen;
	private float slotTimer;
	

	private void Start()
	{
		slotTimer = 0f;
		ServerTimer = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		HasUnlockedMouseUIEnabled = false;
		PropSpawnMenu.SetActive(false);
		ClassSelectMenu.SetActive(false);
        TeamSelectMenu.SetActive(false);
		CurrentWeaponSlots.SetActive(false);
		SetActiveMainHud(false);

		EventManager.onWaitingForPlayersEnd += WaitingForPlayersEnd;
		EventManager.onBuildPhaseStart += BuildPhaseBegin;
		EventManager.onBuildPhaseEnd += BuildPhaseEnd;
		EventManager.onCombatPhaseStart += CombatPhaseBegin;
		EventManager.onCombatPhaseEnd += CombatPhaseEnd;

		BlueScore.text = playerInput.playerStats._gameManager.blueTeamFlagCaptures.ToString();
		RedScore.text = playerInput.playerStats._gameManager.redTeamFlagCaptures.ToString();

        Scoreboard.gameObject.SetActive(false);
        MainEscMenu.gameObject.SetActive(false);

        previousState = PlayerUIState.None;
        TransitionToState(PlayerUIState.None);
        TransitionToState(PlayerUIState.TeamSelectMenu);
	}

    public void OnStateEnter(PlayerUIState state, PlayerUIState fromState)
    {
        switch (state)
        {
            case PlayerUIState.MainMenu:
                {
                    MainEscMenu.gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    HasUnlockedMouseUIEnabled = true;
                    break;
                }
            case PlayerUIState.PropMenu:
                {
                    PropSpawnMenu.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    HasUnlockedMouseUIEnabled = true;
                    break;
                }
            case PlayerUIState.TeamSelectMenu:
                {
                    TeamSelectMenu.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    HasUnlockedMouseUIEnabled = true;
                    break;
                }
            case PlayerUIState.ClassSelectMenu:
                {
                    ClassSelectMenu.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    HasUnlockedMouseUIEnabled = true;
                    break;
                }
            case PlayerUIState.OptionsMenu:
                {
                    break;
                }
            case PlayerUIState.Scoreboard:
                {
                    Scoreboard.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    break;
                }
            case PlayerUIState.None:
                {
                    MainEscMenu.gameObject.SetActive(false);
                    PropSpawnMenu.SetActive(false);
                    TeamSelectMenu.SetActive(false);
                    ClassSelectMenu.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    HasUnlockedMouseUIEnabled = false;
                    break;
                }
        }
    }

    public bool OnStateExit(PlayerUIState state, PlayerUIState toState)
    {
        bool isValid = true;
        switch (state)
        {
            case PlayerUIState.MainMenu:
                {
                    if (toState == PlayerUIState.TeamSelectMenu || toState == PlayerUIState.ClassSelectMenu || toState == PlayerUIState.PropMenu)
                    {
                        //cant go from main menu to prop/team/class menu
                        Debug.Log("Tried to leave the main menu for an invalid menu. " + state.ToString() + toState.ToString());
                        isValid = false;
                    }
                    else
                    {
                        MainEscMenu.gameObject.SetActive(false);
                        
                    }
                    break;
                }
            case PlayerUIState.PropMenu:
                {
                    PropSpawnMenu.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    HasUnlockedMouseUIEnabled = false;
                    break;
                }
            case PlayerUIState.TeamSelectMenu:
                {
                    TeamSelectMenu.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    HasUnlockedMouseUIEnabled = false;
                    break;
                }
            case PlayerUIState.ClassSelectMenu:
                {
                    ClassSelectMenu.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    HasUnlockedMouseUIEnabled = false;
                    break;
                }
            case PlayerUIState.OptionsMenu:
                {
                    break;
                }
            case PlayerUIState.Scoreboard:
                {
                    Scoreboard.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                }
        }

        return isValid;
    }

    public void TransitionToState(PlayerUIState newState)
    {
        PlayerUIState tempState = CurrentUIState;
        if (OnStateExit(tempState, newState))
        {
            CurrentUIState = newState;
            OnStateEnter(newState, tempState);
        }

    }

	private void OnDisable()
	{
		EventManager.onWaitingForPlayersEnd -= WaitingForPlayersEnd;
		EventManager.onBuildPhaseStart -= BuildPhaseBegin;
		EventManager.onBuildPhaseEnd -= BuildPhaseEnd;
		EventManager.onCombatPhaseStart -= CombatPhaseBegin;
		EventManager.onCombatPhaseEnd -= CombatPhaseEnd;
	}

	void WaitingForPlayersEnd()
	{

	}

	void BuildPhaseBegin()
	{
		GameStatusText.text = "Build Phase";
	}

	void BuildPhaseEnd()
	{

	}

	void CombatPhaseBegin()
	{
		GameStatusText.text = "Combat Phase";
	}

	void CombatPhaseEnd()
	{

	}


	private void Update()
	{
		if (ServerTimer != null && ServerTimer.isWaitingForPlayers)
		{
			var min = (int)ServerTimer.WaitingForPlayersLength / 60;
			TimerMinutes.text = min.ToString();
			var secs = (int)ServerTimer.WaitingForPlayersLength % 60;
			TimerSeconds.text = secs.ToString();
		}

		if (ServerTimer != null && ServerTimer.isInBuildPhase)
		{
			var min = (int)ServerTimer.BuildPhaseTimeLength / 60;
			TimerMinutes.text = min.ToString();
			var secs = (int)ServerTimer.BuildPhaseTimeLength % 60;
			TimerSeconds.text = secs.ToString();
		}

        if (ServerTimer != null && ServerTimer.isInCombatPhase)
        {
            var min = (int)ServerTimer.CombatPhaseTimeLength / 60;
            TimerMinutes.text = min.ToString();
            var secs = (int)ServerTimer.CombatPhaseTimeLength % 60;
            TimerSeconds.text = secs.ToString();
        }

        if (IsSlotUIOpen)
		{
			if (Time.time > slotTimer + 2f)
			{
				IsSlotUIOpen = false;
				CurrentWeaponSlots.SetActive(false);
			}
		}

		// rotate flag arrow ui to point at the flags
		if (FlagRadialBlue.isActiveAndEnabled)
		{
			var targetPosLocal = playerInput.cam.transform.InverseTransformPoint(player._gameManager.blueFlag.transform.position);
			var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
			FlagRadialBlue.rectTransform.eulerAngles = new Vector3(0,0,targetAngle);
		}

		if (FlagRadialRed.isActiveAndEnabled)
		{
			var targetPosLocal = playerInput.cam.transform.InverseTransformPoint(player._gameManager.redFlag.transform.position);
			var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
			FlagRadialRed.rectTransform.eulerAngles = new Vector3(0, 0, targetAngle);
		}
	}

    public StatusEffectIcon AddStatusEffectIcon(StatusEffect effect)
    {
        GameObject s = Instantiate(StatusEffectIconPrefab, StatusEffectIcons.transform);
        StatusEffectIcon sei = s.GetComponent<StatusEffectIcon>();
        effect.ActiveIcon = sei;
        effect.ActiveIcon.icon.sprite = effect.icon;
        return effect.ActiveIcon;
    }

    public void RemoveStatusEffectIcon(StatusEffect effect)
    {
        Destroy(effect.ActiveIcon.gameObject);
        effect.ActiveIcon = null;
    }

    public void UpdateStatusEffectIcon(StatusEffect s)
    {
        if (s.AllowStacking)
        {
            s.ActiveIcon.StacksText.text = s.CurrentStacks.ToString();
        }
        else
        {
            s.ActiveIcon.StacksText.text = "";
        }
    }

	public void ShowWepSlotUI()
	{
		IsSlotUIOpen = true;
		slotTimer = Time.time;
		CurrentWeaponSlots.SetActive(true);
	}

	public void SetActiveMainHud(bool choice)
	{
		if (player.CurrentClass.className != "Spectator")
		{
			if (choice)
			{
                MainHud.SetActive(true);
				CurrentWeaponSlots.SetActive(true);
				CurrentWeaponSlots.GetComponent<CurrentWeaponSlotsUI>().AddSlots();
                CurrentAbilitySlots.GetComponent<CurrentAbilitiesUI>().AddSlots();
				ShowWepSlotUI();
			}
			else
			{
                MainHud.SetActive(false);
			}
		}
	}

	public void SetActivateDeathUI(bool choice)
	{
		if (choice == true)
		{
			for (int i = 0; i < RespawnText.Count; i++)
			{
				RespawnText[i].enabled = true;
			}
			RespawnTimer.enabled = true;
            if (CurrentUIState == PlayerUIState.PropMenu)
                TransitionToState(PlayerUIState.None);
		}

		if (choice == false)
		{
			for (int i = 0; i < RespawnText.Count; i++)
			{
				RespawnText[i].enabled = false;
			}
			RespawnTimer.enabled = false;
		}
	}

}
