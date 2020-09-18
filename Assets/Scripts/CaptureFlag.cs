using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureFlag : MonoBehaviour
{
	public GamePhases game;
	//public GameObject flagModel;
	public float rotationSpeed;
	
	//public Material blueFlagMaterial;
	//public Material bluePoleMaterial;
	//public Material redFlagMaterial;
	//public Material redPoleMaterial;

	public Collider pickupCollider;

	public Vector3 flagSpawn;

	public enum Team { Blue, Red}
	public CaptureFlag.Team team;

	public PlayerStats playerHoldingFlag;

	public bool isBeingHeld = false;

    GamePhases gameManager;

	private void Start()
	{
        gameManager = GameObject.FindObjectOfType<GamePhases>();
		flagSpawn = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        if (team == Team.Blue)
            gameManager.blueFlag = this;
        else gameManager.redFlag = this;

	}
	
	public void SetIsRotating(bool choice)
	{
		if (choice == true)
		{

		}

		if (choice == false)
		{

		}
	}
}
