using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{

    public enum Team { Blue, Red, Spectator}

    public Team team;
    GamePhases gameManager;

    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GamePhases>();


        if (team == Team.Blue)
        {
            gameManager.teamBlueSpawnPoints.Add(this);
        }
        else if (team == Team.Red)
        {
            gameManager.teamRedSpawnPoints.Add(this);
        }
        else if (team == Team.Spectator)
        {
            gameManager.teamSpectatorSpawnPoints.Add(this);
        }
    }

}
