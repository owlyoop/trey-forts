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

	public Transform flagSpawn;

	public enum Team { Blue, Red}

	public PlayerStats playerHoldingFlag;

	private void Start()
	{
		flagSpawn = this.transform;
	}

	private void Update()
	{
		
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			PlayerStats player = other.GetComponent<PlayerStats>();

		}
	}
}
