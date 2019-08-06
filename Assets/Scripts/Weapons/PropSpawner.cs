using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PropSpawner : WeaponMotor
{

	public PlayerInput player;
	PlayerStats playersStats;
	public UIManager playerUI;
	public PropSpawnMenu propUI;
	public WeaponSlots wepSlots;
	public FortwarsProp theProp;
	Camera cam;
	
	public GameObject thePropObj;
	
	public GameObject newProp;

	bool isPlacingObject;

	public float allowedRange = 10f;

	float angleOffsetY = 0f;
	float angleOffsetX = 0f;

	[SyncVar]
	public Vector3 networkPropPosition;
	[SyncVar]
	public Quaternion networkPropRotation;


	private void Start()
	{
		InitilaizeValues();

		if (player.playerWeapons.selectedProp != null)
		{
			SetSelectedProp();
		}
	}

	private void InitilaizeValues()
	{
		isPlacingObject = false;
		player = GetComponentInParent<WeaponSlots>().player.GetComponent<PlayerInput>();
		playerUI = player.mainUI;
		wepSlots = GetComponentInParent<WeaponSlots>();
		propUI = playerUI.GetComponentInChildren<PropSpawnMenu>();
		cam = player.cam;
	}

	public void SetSelectedProp()
	{
		InitilaizeValues();

		Destroy(thePropObj);
		if (player.GetComponent<PlayerStats>().currentCurrency > player.mainUI.props[player.selectedPropMenuIndex].currencyCost && player.mainUI.propMenu.hasPropSelected)
		{
			thePropObj = Instantiate(player.mainUI.props[player.selectedPropMenuIndex].propPrefab);
			theProp = thePropObj.GetComponent<FortwarsProp>();
			//thePropObj.layer = 10;
			isPlacingObject = true;
			thePropObj.GetComponent<Renderer>().material = theProp.placementModeMat;
			thePropObj.GetComponent<Collider>().enabled = false;
		}

	}


	private void Update()
	{
		if (thePropObj != null && isPlacingObject)
		{
			RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			Vector3 shootDirection = cam.transform.forward;

			int layermask = 1 << 10;
			layermask = ~layermask;
			
			if (Physics.Raycast(rayOrigin, shootDirection, out shot, allowedRange, layermask))
			{
				thePropObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, shot.normal);
				thePropObj.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.Cross(shot.normal, Vector3.up));
				thePropObj.transform.position = shot.point;

				thePropObj.transform.position = new Vector3(thePropObj.transform.position.x + shot.normal.x,
															thePropObj.transform.position.y + (shot.normal.y * -theProp.snapPositionBottom.transform.localPosition.y),
															thePropObj.transform.position.z + shot.normal.z);

				thePropObj.transform.Rotate(new Vector3(angleOffsetX, angleOffsetY, 0));

				Debug.DrawRay(shot.point, shot.normal * 5f, Color.cyan);
				Debug.DrawRay(theProp.snapPositionBottom.transform.position, Vector3.Cross(shot.normal, theProp.snapPositionBottom.transform.up) * 5f, Color.red);

			}
		}
	}

	public override void PrimaryFire()
	{
		if (thePropObj != null && isPlacingObject && player.mainUI.propMenu.hasPropSelected)
		{
			player.playerWeapons.playersProps.Add(thePropObj);

			networkPropPosition = thePropObj.transform.position;
			networkPropRotation = thePropObj.transform.rotation;

			angleOffsetY = 0f;
		}
	}

	public override void SecondaryFire()
	{
		//rotate the object

		if (thePropObj != null && isPlacingObject)
		{
			angleOffsetY += 45f;
		}
	}

	public override void ReloadButton()
	{
		if (thePropObj != null && isPlacingObject)
		{
			angleOffsetX += 45f;
		}
	}

	public override void OnSwitchToWeapon()
	{
		SetSelectedProp();
	}

	public override void OnSwitchAwayFromWeapon()
	{
		GameObject.Destroy(thePropObj);
	}

	public override void UpdateUI()
	{
		playersStats = GetComponentInParent<WeaponSlots>().player;
		playersStats.ammoAmount.text = "";
		playersStats.ammoInClip.text = "";
	}
}
