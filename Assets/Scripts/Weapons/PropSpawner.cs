using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PropSpawner : WeaponMotor
{

	public PlayerInput player;
	public PlayerStats playersStats;
	public UIManager playerUI;
	public PropSpawnMenu propUI;
	public WeaponSlots wepSlots;
	public FortwarsProp theProp;
	Camera cam;
	
	public GameObject ghostProp;
	
	public GameObject newProp;

	bool isPlacingObject;

	public float allowedRange = 10f;

	float angleOffsetY = 0f;
	public float angleOffsetX = 0f;

	public GamePhases gameManager;

    public LayerMask allowedLayers;

	private void Start()
	{
		gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		InitilaizeValues();

		if (player.playerWeapons.selectedProp != null)
		{
			SetSelectedProp();
		}

		
	}

	private void InitilaizeValues()
	{
		isPlacingObject = false;
		cam = player.cam;
	}

	public void SetSelectedProp()
	{
		InitilaizeValues();

		Destroy(ghostProp);
		Debug.Log("pp 2");
		Debug.Log(player.mainUI.propMenu.hasPropSelected.ToString());
		if (playersStats.currentCurrency > playersStats.wepSlots.selectedProp.currencyCost)
		{
			if (player.mainUI.propMenu.hasPropSelected)
			{
				Debug.Log("peepee ");
				ghostProp = Instantiate(playersStats.wepSlots.selectedProp.propPrefab);
				theProp = ghostProp.GetComponent<FortwarsProp>();
				//thePropObj.layer = 10;
				isPlacingObject = true;
				ghostProp.GetComponent<FortwarsProp>().rend.material = theProp.placementModeMat;
				ghostProp.GetComponent<FortwarsProp>().col.enabled = false;
			}
			
		}

	}


	private void Update()
	{
		if (ghostProp != null && isPlacingObject)
		{
			RaycastHit shot;
			Vector3 rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
			Vector3 shootDirection = cam.transform.forward;

			
			if (Physics.Raycast(rayOrigin, shootDirection, out shot, allowedRange, allowedLayers))
			{
				ghostProp.transform.rotation = Quaternion.FromToRotation(Vector3.up, shot.normal);
				ghostProp.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.Cross(shot.normal, Vector3.up));
				ghostProp.transform.Rotate(new Vector3(angleOffsetX, angleOffsetY, 0));
				ghostProp.transform.position = shot.point;

				ghostProp.transform.position = new Vector3(ghostProp.transform.position.x + (shot.normal.x * theProp.snapPositionRight.transform.localPosition.x),
											ghostProp.transform.position.y + (shot.normal.y * -theProp.snapPositionBottom.transform.localPosition.y),
											ghostProp.transform.position.z + (shot.normal.z * theProp.snapPositionRight.transform.localPosition.x));
				
				if (angleOffsetX == 45f || angleOffsetX == 135f)
				{
					ghostProp.transform.position = new Vector3(ghostProp.transform.position.x,
							ghostProp.transform.position.y - -theProp.snapPositionBottom.localPosition.y/3.2f,
							ghostProp.transform.position.z);
				}

				if (angleOffsetX == 225f || angleOffsetX == 315f)
				{
					ghostProp.transform.position = new Vector3(ghostProp.transform.position.x,
							ghostProp.transform.position.y - -theProp.snapPositionBottom.localPosition.y / 3.2f,
							ghostProp.transform.position.z);
				}

				Debug.DrawRay(shot.point, shot.normal * 5f, Color.cyan);
				Debug.DrawRay(theProp.snapPositionBottom.transform.position, Vector3.Cross(shot.normal, theProp.snapPositionBottom.transform.up) * 5f, Color.red);

			}
		}
	}

	public override void PrimaryFire()
	{
		if (ghostProp != null && isPlacingObject && player.mainUI.propMenu.hasPropSelected)
		{
			//angleOffsetY = 0f;

			//player rpc call to spawn
			string prefabName = playersStats.wepSlots.selectedProp.propPrefab.name;
			Debug.Log(prefabName);

            bool validprop = true;
			if (playersStats.currentCurrency - theProp.currencyCost > 0)
			{
                if (theProp is MoneyPrinter && playersStats.hasPlacedMoneyPrinter == true)
                {
                    validprop = false;
                }

                if (validprop)
                {
                    player.GetComponent<PhotonView>().RPC("SpawnFortwarsProp", RpcTarget.AllViaServer,
                        playersStats.GetComponent<PhotonView>().ViewID, prefabName, ghostProp.transform.position, ghostProp.transform.rotation);

                    playersStats.OnChangeCurrencyAmount(playersStats.currentCurrency - theProp.currencyCost);
                }
                if (theProp is MoneyPrinter)
                {
                    playersStats.hasPlacedMoneyPrinter = true;
                }

			}
		}
	}

	public override void SecondaryFire()
	{
		//rotate the object

		if (ghostProp != null && isPlacingObject)
		{
			angleOffsetY += 45f;
			if (angleOffsetY == 360)
				angleOffsetY = 0;
		}
	}

	public override void ReloadButton()
	{
		if (ghostProp != null && isPlacingObject)
		{
			
			angleOffsetX += 45f;
			if (angleOffsetX == 360)
				angleOffsetX = 0;
		}
	}

	public override void OnSwitchToWeapon()
	{
		if (player.playerWeapons.selectedProp != null)
		{
			SetSelectedProp();
		}
	}

	public override void OnSwitchAwayFromWeapon()
	{
		GameObject.Destroy(ghostProp);
	}

	public override void UpdateUI()
	{
		playersStats = GetComponentInParent<WeaponSlots>().player;
		playersStats.ui.ammoAmount.text = "";
		playersStats.ui.ammoInClip.text = "";
	}
}
