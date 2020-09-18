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
        player.SetQueuedTeam(PlayerStats.PlayerTeam.Blue);
        player.ui.TransitionToState(PlayerUIState.ClassSelectMenu);
    }

	public void OnClickRedTeamButton()
	{
        player.SetQueuedTeam(PlayerStats.PlayerTeam.Red);
        player.ui.TransitionToState(PlayerUIState.ClassSelectMenu);
    }

	public void OnClickSpectateTeamButton()
	{
		player.ChangeToSpectator();
        player.ui.TransitionToState(PlayerUIState.None);
	}

	
}
