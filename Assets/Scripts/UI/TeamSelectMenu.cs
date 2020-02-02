using KinematicCharacterController.Examples;
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
	}

	public void OnClickBlueTeamButton()
	{
		if (player.CurrentClass.className != "Spectator")
		{
			player.SetTeam(1);
            //player.TakeDamage(player.photonView.ViewID, 50000, Damager.DamageTypes.Physical);
			CloseUI();
			OpenClassMenu();
		}
		else
		{
			// player is spectator, we dont want to spawn them until they pick a class
			player.GetComponent<PlayerInput>().mainUI.ClassMenu.QueuedTeam = 1;
			player.GetComponent<PlayerInput>().mainUI.ClassMenu.cameFromTeamMenu = true;
			CloseUI();
			OpenClassMenu();
		}
	}

	public void OnClickRedTeamButton()
	{
		if (player.CurrentClass.className != "Spectator")
		{
			player.SetTeam(2);
            //player.TakeDamage(player.photonView.ViewID, 50000, Damager.DamageTypes.Physical);
			CloseUI();
			OpenClassMenu();
		}
		else
		{
			player.GetComponent<PlayerInput>().mainUI.ClassMenu.QueuedTeam = 2;
			player.GetComponent<PlayerInput>().mainUI.ClassMenu.cameFromTeamMenu = true;
			CloseUI();
			OpenClassMenu();
			
		}

	}

	public void OnClickSpectateTeamButton()
	{
		player.SetTeam(0);
		player.ChangeToSpectator();
		CloseUI();
	}

	void OpenClassMenu()
	{
		player.GetComponent<PlayerInput>().mainUI.TeamSelectMenuSetActive(false);
		player.GetComponent<PlayerInput>().mainUI.ClassSelectMenuSetActive(true);
	}

	void CloseUI()
	{
		Cursor.lockState = CursorLockMode.Locked;
		//player.GetComponentInChildren<OrbitCamera>().viewLocked = false;
		player.GetComponent<PlayerInput>().mainUI.TeamSelectMenuSetActive(false);
	}
	
}
