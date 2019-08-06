using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : NetworkBehaviour
{
	public PlayerInput player;

	public PropSpawnMenu propMenu;
	public ClassSelectMenu classMenu;

	public Canvas mainUICanvas;

	public List<FortwarsPropData> props = new List<FortwarsPropData>();

	public GameObject propSpawnMenu;
	public GameObject teamSelectMenu;
	public GameObject classSelectMenu;

	public bool hasUnlockedMouseUIEnabled;

	public bool canOpenPropMenu = true;
	public bool canOpenTeamSelectMenu = true;
	public bool canOpenClassSelectMenu = true;

	public Text timerMinutes;
	public Text timerSeconds;

	public GamePhases serverTimer;

	private void Start()
	{

		serverTimer = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		hasUnlockedMouseUIEnabled = false;
		propSpawnMenu.SetActive(false);
		classSelectMenu.SetActive(false);

		Debug.Log(GetComponent<NetworkIdentity>().isLocalPlayer.ToString());

		if (!isLocalPlayer)
		{
			mainUICanvas.enabled = false;
		}

	}

	private void Update()
	{
		if (serverTimer.isWaitingForPlayers)
		{
			var min = (int)serverTimer.WaitingForPlayersLength / 60;
			timerMinutes.text = min.ToString();
			var secs = (int)serverTimer.WaitingForPlayersLength % 60;
			timerSeconds.text = secs.ToString();
		}

		if (serverTimer.isInBuildPhase)
		{
			var min = (int)serverTimer.BuildPhaseTimeLength / 60;
			timerMinutes.text = min.ToString();
			var secs = (int)serverTimer.BuildPhaseTimeLength % 60;
			timerSeconds.text = secs.ToString();
		}
	}

	public void PropSpawnMenuSetActive(bool choice)
	{
		if (canOpenPropMenu)
		{
			if (choice == true)
			{
				propSpawnMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				hasUnlockedMouseUIEnabled = true;
				player.GetComponentInChildren<PlayerLook>().viewLocked = true;
				canOpenTeamSelectMenu = false;
				canOpenClassSelectMenu = false;
			}
			else
			{
				propSpawnMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				hasUnlockedMouseUIEnabled = false;
				player.GetComponentInChildren<PlayerLook>().viewLocked = false;
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
				player.GetComponentInChildren<PlayerLook>().viewLocked = true;
				canOpenPropMenu = false;
				canOpenClassSelectMenu = false;
			}
			else
			{
				teamSelectMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				hasUnlockedMouseUIEnabled = false;
				player.GetComponentInChildren<PlayerLook>().viewLocked = false;
				canOpenPropMenu = true;
				canOpenClassSelectMenu = true;
			}
		}

	}

	public void ClassSelectMenuSetActive(bool choice)
	{
		if (canOpenClassSelectMenu)
		{
			if (choice == true)
			{
				classSelectMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				hasUnlockedMouseUIEnabled = true;
				player.GetComponentInChildren<PlayerLook>().viewLocked = true;
				canOpenPropMenu = false;
				canOpenTeamSelectMenu = false;
			}
			else
			{
				classSelectMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
				hasUnlockedMouseUIEnabled = false;
				player.GetComponentInChildren<PlayerLook>().viewLocked = false;
				canOpenPropMenu = true;
				canOpenTeamSelectMenu = true;

			}
		}
	}

}
