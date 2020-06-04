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
			player.SetTeam(PlayerStats.PlayerTeam.Blue);
            player.ui.TransitionToState(PlayerUIState.ClassSelectMenu);
		}
		else
		{
			// player is spectator, we dont want to spawn them until they pick a class
			player.ui.ClassMenu.QueuedTeam = PlayerStats.PlayerTeam.Blue;
            player.ui.TransitionToState(PlayerUIState.ClassSelectMenu);
        }
	}

	public void OnClickRedTeamButton()
	{
		if (player.CurrentClass.className != "Spectator")
		{
			player.SetTeam(PlayerStats.PlayerTeam.Red);
            player.ui.TransitionToState(PlayerUIState.ClassSelectMenu);
        }
		else
		{
			player.GetComponent<PlayerInput>().mainUI.ClassMenu.QueuedTeam = PlayerStats.PlayerTeam.Red;
            player.ui.TransitionToState(PlayerUIState.ClassSelectMenu);
        }

	}

	public void OnClickSpectateTeamButton()
	{
		player.SetTeam(PlayerStats.PlayerTeam.Spectator);
		player.ChangeToSpectator();
        player.ui.TransitionToState(PlayerUIState.None);
	}

	
}
