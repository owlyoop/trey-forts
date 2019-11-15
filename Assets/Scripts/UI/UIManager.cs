using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController.Examples;
using Photon.Pun;

public class UIManager : MonoBehaviourPunCallbacks
{
	public PlayerInput player;
	public PlayerStats _playerStats;

	public PropSpawnMenu propMenu;
	public ClassSelectMenu classMenu;

	public Canvas mainUICanvas;

	public GameObject propSpawnMenu;
	public GameObject teamSelectMenu;
	public GameObject classSelectMenu;

	public GameObject MainHud;

	public Text healthText;
	public Text currencyAmount;
	public Text ammoInClip;
	public Text ammoAmount;
	public Text dollar;
    public RadialReload radialReload;



	public RectTransform healthBar;
	public RectTransform healthBarBackground;

	public GameObject currentWeaponSlots;
    public GameObject currentAbilitySlots;

	public bool hasUnlockedMouseUIEnabled;

	public bool canOpenPropMenu = true;
	public bool canOpenTeamSelectMenu = true;
	public bool canOpenClassSelectMenu = true;

	public Text timerMinutes;
	public Text timerSeconds;

	public Image flagRadialBlue;
	public Image flagRadialRed;

	public Text bluScore;
	public Text redScore;
	public Text gameStatusText;

	public List<Text> respawnText = new List<Text>();
	public Text respawnTimer;

	public GamePhases serverTimer;

	public bool isSlotUIOpen;
	private float slotTimer;
	

	private void Start()
	{
		slotTimer = 0f;
		if (!photonView.IsMine)
		{
			mainUICanvas.enabled = false;
			
			return;
		}
		serverTimer = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		
		if (photonView.IsMine)
		{
			hasUnlockedMouseUIEnabled = false;
			propSpawnMenu.SetActive(false);
			classSelectMenu.SetActive(false);
			TeamSelectMenuSetActive(true);
			currentWeaponSlots.SetActive(false);
			SetActiveMainHud(false);
		}
		EventManager.onWaitingForPlayersEnd += WaitingForPlayersEnd;
		EventManager.onBuildPhaseStart += BuildPhaseBegin;
		EventManager.onBuildPhaseEnd += BuildPhaseEnd;
		EventManager.onCombatPhaseStart += CombatPhaseBegin;
		EventManager.onCombatPhaseEnd += CombatPhaseEnd;

		bluScore.text = player.playerStats._gameManager.BlueTeamScore.ToString();
		redScore.text = player.playerStats._gameManager.RedTeamScore.ToString();
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
		gameStatusText.text = "Build Phase";
	}

	void BuildPhaseEnd()
	{

	}

	void CombatPhaseBegin()
	{
		gameStatusText.text = "Combat Phase";
	}

	void CombatPhaseEnd()
	{

	}


	private void Update()
	{
		if (!photonView.IsMine)
		{
			return;
		}
		if (serverTimer != null && serverTimer.isWaitingForPlayers)
		{
			var min = (int)serverTimer.WaitingForPlayersLength / 60;
			timerMinutes.text = min.ToString();
			var secs = (int)serverTimer.WaitingForPlayersLength % 60;
			timerSeconds.text = secs.ToString();
		}

		if (serverTimer != null && serverTimer.isInBuildPhase)
		{
			var min = (int)serverTimer.BuildPhaseTimeLength / 60;
			timerMinutes.text = min.ToString();
			var secs = (int)serverTimer.BuildPhaseTimeLength % 60;
			timerSeconds.text = secs.ToString();

			
		}
		
		if (isSlotUIOpen)
		{
			if (Time.time > slotTimer + 2f)
			{
				isSlotUIOpen = false;
				currentWeaponSlots.SetActive(false);
			}
		}

		// rotate flag arrow ui to point at the flags
		if (flagRadialBlue.isActiveAndEnabled)
		{
			var targetPosLocal = player.cam.transform.InverseTransformPoint(_playerStats._gameManager.blueFlag.transform.position);
			var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
			flagRadialBlue.rectTransform.eulerAngles = new Vector3(0,0,targetAngle);
		}

		if (flagRadialRed.isActiveAndEnabled)
		{
			var targetPosLocal = player.cam.transform.InverseTransformPoint(_playerStats._gameManager.redFlag.transform.position);
			var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
			flagRadialRed.rectTransform.eulerAngles = new Vector3(0, 0, targetAngle);
		}
	}

	public void ShowWepSlotUI()
	{
		isSlotUIOpen = true;
		slotTimer = Time.time;
		currentWeaponSlots.SetActive(true);
	}

	public void PropSpawnMenuSetActive(bool choice)
	{
		if (canOpenPropMenu && _playerStats.playerClass.className != "Spectator")
		{
			if (choice == true)
			{
				propSpawnMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				hasUnlockedMouseUIEnabled = true;
				//player.GetComponentInChildren<OrbitCamera>().viewLocked = true;
				canOpenTeamSelectMenu = false;
				canOpenClassSelectMenu = false;
			}
			else
			{
				propSpawnMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				hasUnlockedMouseUIEnabled = false;
				//player.GetComponentInChildren<OrbitCamera>().viewLocked = false;
				canOpenTeamSelectMenu = true;
				canOpenClassSelectMenu = true;
			}
		}

	}

	public void TeamSelectMenuSetActive(bool choice)
	{
		if (canOpenTeamSelectMenu)
		{
			if (choice == true)
			{
				teamSelectMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				hasUnlockedMouseUIEnabled = true;
				canOpenPropMenu = false;
				canOpenClassSelectMenu = false;
				SetActiveMainHud(false);
			}
			else
			{
				teamSelectMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				hasUnlockedMouseUIEnabled = false;
				canOpenPropMenu = true;
				canOpenClassSelectMenu = true;
			}
		}

	}

	public void ClassSelectMenuSetActive(bool choice)
	{
		if (canOpenClassSelectMenu)
		{
			if (_playerStats.playerClass.className != "Spectator" || classMenu.cameFromTeamMenu)
			{
				if (choice == true)
				{
					classSelectMenu.SetActive(true);
					Cursor.lockState = CursorLockMode.None;
					hasUnlockedMouseUIEnabled = true;
					canOpenPropMenu = false;
					canOpenTeamSelectMenu = false;
				}
				else
				{
					classSelectMenu.SetActive(false);
					Cursor.lockState = CursorLockMode.Locked;
					hasUnlockedMouseUIEnabled = false;
					canOpenPropMenu = true;
					canOpenTeamSelectMenu = true;

				}
			}
			
		}
	}

	public void SetActiveMainHud(bool choice)
	{
		if (_playerStats.playerClass.className != "Spectator")
		{
			if (choice)
			{
				currentWeaponSlots.SetActive(true);
				currentWeaponSlots.GetComponent<CurrentWeaponSlotsUI>().AddSlots();
                currentAbilitySlots.GetComponent<CurrentAbilitiesUI>().AddSlots();
				healthBar.gameObject.SetActive(true);
				healthBarBackground.gameObject.SetActive(true);
				healthText.enabled = true;
				currencyAmount.enabled = true;
				ammoAmount.enabled = true;
				ammoInClip.enabled = true;
				dollar.enabled = true;
				ShowWepSlotUI();

			}
			else
			{
				currentWeaponSlots.SetActive(false);
				healthBar.gameObject.SetActive(false);
				healthBarBackground.gameObject.SetActive(false);
				healthText.enabled = false;
				currencyAmount.enabled = false;
				ammoAmount.enabled = false;
				ammoInClip.enabled = false;
				dollar.enabled = false;
			}
		}
	}



	public void SetActivateDeathUI(bool choice)
	{
		if (choice == true)
		{
			for (int i = 0; i < respawnText.Count; i++)
			{
				respawnText[i].enabled = true;
			}
			respawnTimer.enabled = true;
            PropSpawnMenuSetActive(false);
		}

		if (choice == false)
		{
			for (int i = 0; i < respawnText.Count; i++)
			{
				respawnText[i].enabled = false;
			}
			respawnTimer.enabled = false;
		}
	}

}
