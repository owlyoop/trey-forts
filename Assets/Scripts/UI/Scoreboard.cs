using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public ScoreboardPlayerInfo scoreboardPlayerPrefab;

    Transform bluePlayerGrid;
    Transform redPlayerGrid;

    public List<ScoreboardPlayerInfo> bluePlayers = new List<ScoreboardPlayerInfo>();
    public List<ScoreboardPlayerInfo> redPlayers = new List<ScoreboardPlayerInfo>();

    private void Start()
    {
        
    }

    public void AddPlayerToScoreboard(PlayerStats player)
    {
        if (player.playerTeam == PlayerStats.PlayerTeam.Blue)
        {
            ScoreboardPlayerInfo go = Instantiate(scoreboardPlayerPrefab, bluePlayerGrid);
            go.UpdatePlayerInfo(player);
            bluePlayers.Add(go);

        }
        else if (player.playerTeam == PlayerStats.PlayerTeam.Red)
        {
            ScoreboardPlayerInfo go = Instantiate(scoreboardPlayerPrefab, redPlayerGrid);
            go.UpdatePlayerInfo(player);
            redPlayers.Add(go);
        }
        else
        {
            //spectator
        }
    }

    public void RemovePlayerFromScoreboard()
    {

    }

    public void SortScoreboardByScore(PlayerStats.PlayerTeam teamGrid)
    {
        if (teamGrid == PlayerStats.PlayerTeam.Blue)
        {

        }
        else if (teamGrid == PlayerStats.PlayerTeam.Red)
        {

        }
    }

}
