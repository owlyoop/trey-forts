using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectMenu : MonoBehaviour
{
	public PlayerStats player;

	public Button blueTeam;
	public Button redTeam;
	public Button spectateTeam;

	public Image background;


	void Start()
	{
		Cursor.lockState = CursorLockMode.None;
		player.GetComponentInChildren<PlayerLook>().viewLocked = true;
	}
	public void OnClickBlueTeamButton()
	{
		player.SetTeam(1);
		player.playerTeam = 1;
		ActivatePlayer();
		CloseUI();

		OpenClassMenu();
		player.OnDeath();
	}

	public void OnClickRedTeamButton()
	{
		player.SetTeam(2);
		player.playerTeam = 2;
		ActivatePlayer();
		CloseUI();

		OpenClassMenu();
		player.OnDeath();
	}

	public void OnClickSpectateTeamButton()
	{
		player.SetTeam(0);
		player.playerTeam = 0;
		ActivatePlayer();
		CloseUI();

	}

	void OpenClassMenu()
	{
		if (player.playerClass.className == "Spectator")
		{
			player.GetComponent<PlayerInput>().mainUI.ClassSelectMenuSetActive(true);
		}
	}

	void CloseUI()
	{
		Cursor.lockState = CursorLockMode.Locked;
		player.GetComponentInChildren<PlayerLook>().viewLocked = false;
		gameObject.SetActive(false);
	}

	public void ActivatePlayer()
	{
		player.GetComponent<CharacterController>().enabled = true;
		//player.playerModelRoot.SetActive(true);
	}
}
