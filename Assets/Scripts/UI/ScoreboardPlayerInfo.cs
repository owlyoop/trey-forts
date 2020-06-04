using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardPlayerInfo : MonoBehaviour
{
    public PlayerStats player;

    public Text username;
    public Text score;
    public Text kills;
    public Text assists;
    public Text ping;

    public Image classicon;

    public ScoreboardPlayerInfo()
    {

    }

    private void Start()
    {
        UpdatePlayerInfo(player);
    }

    public void UpdatePlayerInfo(PlayerStats player)
    {
        username.text = player.Username;
        score.text = player.score.ToString();
        kills.text = player.kills.ToString();
        assists.text = player.assists.ToString();
        ping.text = player.ping.ToString();
        classicon.sprite = player.CurrentClass.icon;
    }
}
