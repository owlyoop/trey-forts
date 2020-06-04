using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : WeaponMotor
{

	public PlayerInput playerInput;
	public UIManager playerUI;
	public PropSpawnMenu propUI;
	public WeaponSlots wepSlots;
	public FortwarsProp theProp;
	Camera cam;
	
	public GameObject ghostProp;
	
	public GameObject newProp;

	bool isPlacingObject;
    bool canPlaceProp;

	public float allowedRange = 10f;

	float angleOffsetY = 0f;
	public float angleOffsetX = 0f;

	public GamePhases gameManager;

    public LayerMask allowedLayers;

	private void Start()
	{
        player = playerInput.playerStats;
		gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		InitilaizeValues();

		if (playerInput.playerWeapons.selectedProp != null)
		{
			SetSelectedProp();
		}

		
	}

	private void InitilaizeValues()
	{
		isPlacingObject = false;
		cam = playerInput.cam;
	}

	public void SetSelectedProp()
	{
		InitilaizeValues();

		Destroy(ghostProp);
		Debug.Log("pp 2");
		Debug.Log(playerInput.mainUI.PropMenu.hasPropSelected.ToString());
		if (player.CurrentCurrency > player.wepSlots.selectedProp.currencyCost)
		{
			if (playerInput.mainUI.PropMenu.hasPropSelected)
			{
				Debug.Log("peepee ");
				ghostProp = Instantiate(player.wepSlots.selectedProp.propPrefab);
                ghostProp.GetComponent<FortwarsProp>().GhostMode = true;
				theProp = ghostProp.GetComponent<FortwarsProp>();
				//thePropObj.layer = 10;
				isPlacingObject = true;
				ghostProp.GetComponent<FortwarsProp>().rend.material = theProp.placementModeMat;
                foreach (Collider col in ghostProp.GetComponent<FortwarsProp>().cols)
                {
                    col.enabled = false;
                }
				
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
                if (!ghostProp.activeSelf)
                    ghostProp.SetActive(true);
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
							ghostProp.transform.position.y - -theProp.snapPositionBottom.localPosition.y/ 3.14f,
							ghostProp.transform.position.z);
				}

				if (angleOffsetX == 225f || angleOffsetX == 315f)
				{
					ghostProp.transform.position = new Vector3(ghostProp.transform.position.x,
							ghostProp.transform.position.y - -theProp.snapPositionBottom.localPosition.y / 3.14f,
							ghostProp.transform.position.z);
				}

                if (angleOffsetX == 90f || angleOffsetX == 270f)
                {
                    ghostProp.transform.position = new Vector3(ghostProp.transform.position.x ,
                            ghostProp.transform.position.y - (theProp.snapPositionTop.localPosition.y) + theProp.snapPositionFront.localPosition.z,
                            ghostProp.transform.position.z);
                }

				Debug.DrawRay(shot.point, shot.normal * 5f, Color.cyan);
				Debug.DrawRay(theProp.snapPositionBottom.transform.position, Vector3.Cross(shot.normal, theProp.snapPositionBottom.transform.up) * 5f, Color.red);
                canPlaceProp = true;

			}
            else // prop out of range
            {
                canPlaceProp = false;
                ghostProp.SetActive(false);
            }
		}
	}

	public override void PrimaryFire()
	{
		if (ghostProp != null && isPlacingObject && playerInput.mainUI.PropMenu.hasPropSelected && canPlaceProp)
		{
			//player rpc call to spawn
			string prefabName = player.wepSlots.selectedProp.propPrefab.name;
			Debug.Log(prefabName);

            bool validprop = true;
			if (player.CurrentCurrency - theProp.currencyCost > 0)
			{
                if (theProp is MoneyPrinter && player.hasPlacedMoneyPrinter == true)
                {
                    validprop = false;
                }

                if (validprop)
                {
                    player.GetComponent<PlayerRpcCalls>().SpawnFortwarsProp(prefabName, player, ghostProp.transform.position, ghostProp.transform.rotation);
                    player.OnChangeCurrencyAmount(player.CurrentCurrency - theProp.currencyCost);
                }

                if (theProp is MoneyPrinter)
                {
                    player.hasPlacedMoneyPrinter = true;
                }
			}
		}
	}

	public override void SecondaryFire()
	{
		//rotate the object

		if (ghostProp != null && isPlacingObject && canPlaceProp)
		{
			angleOffsetY += 45f;
			if (angleOffsetY == 360)
				angleOffsetY = 0;
		}
	}

	public override void ReloadButton()
	{
		if (ghostProp != null && isPlacingObject && canPlaceProp)
		{
			angleOffsetX += 45f;
			if (angleOffsetX == 360)
				angleOffsetX = 0;
		}
	}

	public override void OnSwitchToWeapon()
	{
		if (playerInput.playerWeapons.selectedProp != null)
		{
			SetSelectedProp();
		}
	}

	public override void OnSwitchAwayFromWeapon()
	{
        if (playerUI.CurrentUIState == PlayerUIState.PropMenu)
            playerUI.TransitionToState(PlayerUIState.None);
		GameObject.Destroy(ghostProp);
	}

	public override void UpdateUI()
	{
		player = GetComponentInParent<WeaponSlots>().player;
		player.ui.AmmoAmount.text = "";
		player.ui.AmmoInClip.text = "";
	}

    public override void PrimaryFireHolding()
    {
        
    }

    public override void PrimaryFireButtonUp()
    {
        
    }

    public override void SecondaryFireHolding()
    {
        
    }

    public override void ScrollWheelUp()
    {
        
    }

    public override void ScrollWheelDown()
    {
        
    }

    public override void UseButtonHolding()
    {
        
    }

    public override void UseButtonUp()
    {
        
    }

    public override void GetWeaponStats(Weapon wep)
    {
        
    }

    public override void GetWeaponStats(RangedProjectile wep)
    {
        
    }
}
