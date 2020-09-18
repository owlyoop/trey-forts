using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureFlagZone : MonoBehaviour
{
    public enum Team { Blue, Red}
    public Team team;

    GamePhases gameManager;
    public Collider col;

    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GamePhases>();
        col = GetComponent<Collider>();

        if (team == Team.Blue)
        {
            gameManager.blueCaptureZone = this;
            gameManager.blueCaptureTrigger = col;
        }
        else
        {
            gameManager.redCaptureZone = this;
            gameManager.redCaptureTrigger = col;
        }
    }
}
