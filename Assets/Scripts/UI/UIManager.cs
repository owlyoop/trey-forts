using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController.Examples;

public class UIManager : MonoBehaviour
{
	public PlayerInput player;
	public PlayerStats _playerStats;

	public PropSpawnMenu PropMenu;
	public ClassSelectMenu ClassMenu;

	public Canvas MainUICanvas;

	public GameObject PropSpawnMenu;
	public GameObject TeamSelectMenu;
	public GameObject ClassSelectMenu;

	public GameObject MainHud;

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

	public GameObject CurrentWeaponSlots;
    public GameObject CurrentAbilitySlots;

	public bool HasUnlockedMouseUIEnabled;

	public bool CanOpenPropMenu = true;
	public bool CanOpenTeamSelectMenu = true;
	public bool CanOpenClassSelectMenu = true;

	public Text TimerMinutes;
	public Text TimerSeconds;

	public Image FlagRadialBlue;
	public Image FlagRadialRed;

	public Text BlueScore;
	public Text RedScore;
	public Text GameStatusText;

	public List<Text> RespawnText = new List<Text>();
	public Text RespawnTimer;

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
		TeamSelectMenuSetActive(true);
		CurrentWeaponSlots.SetActive(false);
		SetActiveMainHud(false);

		EventManager.onWaitingForPlayersEnd += WaitingForPlayersEnd;
		EventManager.onBuildPhaseStart += BuildPhaseBegin;
		EventManager.onBuildPhaseEnd += BuildPhaseEnd;
		EventManager.onCombatPhaseStart += CombatPhaseBegin;
		EventManager.onCombatPhaseEnd += CombatPhaseEnd;

		BlueScore.text = player.playerStats._gameManager.BlueTeamScore.ToString();
		RedScore.text = player.playerStats._gameManager.RedTeamScore.ToString();
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
			var targetPosLocal = player.cam.transform.InverseTransformPoint(_playerStats._gameManager.blueFlag.transform.position);
			var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
			FlagRadialBlue.rectTransform.eulerAngles = new Vector3(0,0,targetAngle);
		}

		if (FlagRadialRed.isActiveAndEnabled)
		{
			var targetPosLocal = player.cam.transform.InverseTransformPoint(_playerStats._gameManager.redFlag.transform.position);
			var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
			FlagRadialRed.rectTransform.eulerAngles = new Vector3(0, 0, targetAngle);
		}
	}

	public void ShowWepSlotUI()
	{
		IsSlotUIOpen = true;
		slotTimer = Time.time;
		CurrentWeaponSlots.SetActive(true);
	}

	public void PropSpawnMenuSetActive(bool choice)
	{
		if (CanOpenPropMenu && _playerStats.CurrentClass.className != "Spectator")
		{
			if (choice == true)
			{
				PropSpawnMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				HasUnlockedMouseUIEnabled = true;
				//player.GetComponentInChildren<OrbitCamera>().viewLocked = true;
				CanOpenTeamSelectMenu = false;
				CanOpenClassSelectMenu = false;
			}
			else
			{
				PropSpawnMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				HasUnlockedMouseUIEnabled = false;
				//player.GetComponentInChildren<OrbitCamera>().viewLocked = false;
				CanOpenTeamSelectMenu = true;
				CanOpenClassSelectMenu = true;
			}
		}

	}

	public void TeamSelectMenuSetActive(bool choice)
	{
		if (CanOpenTeamSelectMenu)
		{
			if (choice == true)
			{
				TeamSelectMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				HasUnlockedMouseUIEnabled = true;
				CanOpenPropMenu = false;
				CanOpenClassSelectMenu = false;
				SetActiveMainHud(false);
			}
			else
			{
				TeamSelectMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				HasUnlockedMouseUIEnabled = false;
				CanOpenPropMenu = true;
				CanOpenClassSelectMenu = true;
			}
		}

	}

	public void ClassSelectMenuSetActive(bool choice)
	{
		if (CanOpenClassSelectMenu)
		{
			if (_playerStats.CurrentClass.className != "Spectator" || ClassMenu.cameFromTeamMenu)
			{
				if (choice == true)
				{
					ClassSelectMenu.SetActive(true);
					Cursor.lockState = CursorLockMode.None;
					HasUnlockedMouseUIEnabled = true;
					CanOpenPropMenu = false;
					CanOpenTeamSelectMenu = false;
                    ClassMenu.CurrentCurrency.text = _playerStats.CurrentCurrency.ToString();
				}
				else
				{
					ClassSelectMenu.SetActive(false);
					Cursor.lockState = CursorLockMode.Locked;
					HasUnlockedMouseUIEnabled = false;
					CanOpenPropMenu = true;
					CanOpenTeamSelectMenu = true;

				}
			}
			
		}
	}

	public void SetActiveMainHud(bool choice)
	{
		if (_playerStats.CurrentClass.className != "Spectator")
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
            PropSpawnMenuSetActive(false);
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
