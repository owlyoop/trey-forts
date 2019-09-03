using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using KinematicCharacterController;
using KinematicCharacterController.Owly;
using Photon.Pun.Demo.PunBasics;

public struct PlayerActionInputs
{
	public bool PrimaryFire; //default mouse1
	public bool PrimaryFireUp;
	public bool SecondaryFire; // default mouse2

	public float MouseScroll;

	public bool Interact; // default e
	public bool InteractHeld;
	public bool InteractUp;
	public bool Reload; // default r
	public bool Weapon1; // default 1
	public bool Weapon2; // default 2
	public bool Weapon3; // default 3
	public bool Weapon4; // default 4
	public bool Weapon5; // default 5

	public bool PropSpawner; // default t
	public bool PropMover; // default g

	public bool Ability1; // default q
	public bool Ability2; // default shift
	public bool Ability3; //default e

	public bool MeleeKick; // default v
	public bool Jump; // default space
	public bool Crouch; // default c

	public bool OpenTeamSelectMenu; // default 0
	public bool OpenClassSelectMenu; // default 9

    

}

public class PlayerInput : MonoBehaviourPunCallbacks
{
	//public PlayerMove player;
	public PlayerStats playerStats;
	public WeaponSlots playerWeapons;
	public Camera cam;
	public UIManager mainUI;
	

	public GameObject holdingPropObj;
	
	public string debugID;
	public PlayerActionInputs playerInputs;


	private Vector3 lookat;


    private void Update()
	{
		if (!photonView.IsMine)
		{
			GetComponent<KinematicCharacterMotor>().enabled = false;
			return;
		}
		HandlePlayerInput();
		CheckForInput();
		//GetLook();
	}



	private void Start()
	{
		playerWeapons.DecactivateAllWeapons();
	}

	void HandlePlayerInput()
	{

		playerInputs.PrimaryFire = Input.GetMouseButtonDown(0);
		playerInputs.PrimaryFireUp = Input.GetMouseButtonUp(0);
		playerInputs.SecondaryFire = Input.GetMouseButtonDown(1);
		playerInputs.MouseScroll = Input.GetAxis("Mouse ScrollWheel"); // > 0 up, < 0 down

		playerInputs.Weapon1 = Input.GetKeyDown(KeyCode.Alpha1);
		playerInputs.Weapon2 = Input.GetKeyDown(KeyCode.Alpha2);
		playerInputs.Weapon3 = Input.GetKeyDown(KeyCode.Alpha3);
		playerInputs.Weapon4 = Input.GetKeyDown(KeyCode.Alpha4);
		playerInputs.Weapon5 = Input.GetKeyDown(KeyCode.Alpha5);

		playerInputs.Ability1 = Input.GetKeyDown(KeyCode.Q);
		playerInputs.Ability2 = Input.GetKeyDown(KeyCode.LeftShift);
		playerInputs.Ability3 = Input.GetKeyDown(KeyCode.F);

		playerInputs.MeleeKick = Input.GetKeyDown(KeyCode.V);
		playerInputs.Jump = Input.GetKey(KeyCode.Space);
		playerInputs.Crouch = Input.GetKey(KeyCode.C);

		playerInputs.PropSpawner = Input.GetKey(KeyCode.T);
		playerInputs.PropMover = Input.GetKey(KeyCode.G);

		playerInputs.Interact = Input.GetKeyDown(KeyCode.E);
		playerInputs.InteractHeld = Input.GetKey(KeyCode.E);
		playerInputs.InteractUp = Input.GetKeyUp(KeyCode.E);
		playerInputs.Reload = Input.GetKeyDown(KeyCode.R);

        playerInputs.OpenTeamSelectMenu = Input.GetKeyDown(KeyCode.Alpha9);
        playerInputs.OpenClassSelectMenu = Input.GetKeyDown(KeyCode.Alpha0);
    }

    void SwitchWeapon(int ToIndex)
    {
        mainUI.PropSpawnMenuSetActive(false);
        if (playerWeapons.propWepSlots[1].activeSelf)
            playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();

        if (playerWeapons.propWepSlots[0].activeSelf)
            playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
        playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
        playerWeapons.SwitchActiveWeaponSlot(ToIndex);
        CmdActiveWeaponIndex(ToIndex);
        playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchToWeapon();
        mainUI.ShowWepSlotUI();
    }

	void CheckForInput()
	{
		GameObject wep = null;
		if (playerWeapons.weaponSlots.Count > 0)
		{
			wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex];
		}
		//var wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex];
		if (playerInputs.PrimaryFire)
		{
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.activeSelf && playerStats.isAlive)
				{
					wep.GetComponent<WeaponMotor>().PrimaryFire();

					Debug.Log("primary fire");
					
				}

				if (playerWeapons.propWepSlots[0].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().PrimaryFire();
				}
				if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().PrimaryFire();
				}
			}
		}


		if (playerInputs.PrimaryFireUp)
		{
			
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.activeSelf && playerStats.isAlive)
				{
					wep.GetComponent<WeaponMotor>().PrimaryFireButtonUp();
				}
				if (playerWeapons.propWepSlots[0].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().PrimaryFireButtonUp();
				}
				if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().PrimaryFireButtonUp();
				}
			}
		}


		if (playerInputs.SecondaryFire)
		{
			if (!mainUI.hasUnlockedMouseUIEnabled && playerWeapons.weaponSlots.Count > 0)
			{
				if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].activeSelf && playerStats.isAlive)
				{
					playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().SecondaryFire();
				}
				if (playerWeapons.propWepSlots[0].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().SecondaryFire();
				}
				if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().SecondaryFire();
				}
			}
		}




		if (playerInputs.Weapon1)
		{
			if (playerStats.isAlive && playerWeapons.weaponSlots.Count > 0)
			{
                SwitchWeapon(0);
			}

		}


		if (playerInputs.Weapon2)
		{
			if (playerStats.isAlive && playerWeapons.weaponSlots.Count > 1)
			{
                SwitchWeapon(1);
			}

		}


		if (playerInputs.Weapon3)
		{
			if (playerStats.isAlive && playerWeapons.weaponSlots.Count > 2)
			{
                SwitchWeapon(2);
			}
		}


		if (playerInputs.PropSpawner)
		{
			if (wep != null && playerStats.isAlive)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
				playerWeapons.SwitchToPropSpawner();
				playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchToWeapon();
			}
		}


		if (playerInputs.PropMover)
		{
			if (wep != null && playerStats.isAlive)
			{
				playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
				playerWeapons.SwitchToPropMover();
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchToWeapon();
			}
		}


		if (playerInputs.Ability1)
		{
			if (wep != null && playerStats.isAlive)
			{
				if (playerWeapons.propWepSlots[0].activeSelf)
				{
					if (mainUI.propSpawnMenu.activeSelf)
					{
						mainUI.PropSpawnMenuSetActive(false);
					}
					else
					{
						mainUI.PropSpawnMenuSetActive(true);
					}
				}
                else
                {
                    playerWeapons.ActivateAbility(0);
                }
                
			}
		}

		if (playerInputs.MeleeKick)
		{
			if (!playerStats.CharControl.Motor.GroundingStatus.FoundAnyGround && playerStats.CharControl.CurrentCharacterState != CharacterState.Divekick)
			{
                playerStats.CharControl.TransitionToState(CharacterState.Divekick);
            }
			else
			{
				
			}
		}


		if (playerInputs.OpenTeamSelectMenu)
		{
			if (mainUI.teamSelectMenu.activeSelf)
			{
				mainUI.TeamSelectMenuSetActive(false);
			}
			else
			{
				mainUI.TeamSelectMenuSetActive(true);
			}
		}

		if (playerInputs.OpenClassSelectMenu)
		{
			if (playerStats.isAlive)
			{
				if (mainUI.classSelectMenu.activeSelf)
				{
					mainUI.ClassSelectMenuSetActive(false);
				}
				else
				{
					mainUI.ClassSelectMenuSetActive(true);
				}
			}
		}

        if (Input.GetKeyDown(KeyCode.L))
        {
            //PhotonNetwork.Instantiate("Character", new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
        }

		if (playerInputs.MouseScroll > 0f)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().ScrollWheelUp();
			}

		}

		if (playerInputs.MouseScroll < 0f)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().ScrollWheelDown();
			}

		}

		if (playerInputs.Reload)
		{
			if (playerStats.isAlive)
			{
				if (!mainUI.hasUnlockedMouseUIEnabled)
				{
					playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().ReloadButton();
				}
				if (playerWeapons.propWepSlots[0].activeSelf && playerStats.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().ReloadButton();
				}
			}

		}

		if (playerInputs.Interact)
		{
		}

		if (playerInputs.InteractHeld)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive && !mainUI.hasUnlockedMouseUIEnabled)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().UseButtonHolding();
			}
		}

		if (playerInputs.InteractUp)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && playerStats.isAlive && !mainUI.hasUnlockedMouseUIEnabled)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().UseButtonUp();
			}
		}

	}

	void GetLook()
	{
		if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>() != null && playerStats.isAlive)
		{
			lookat = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>().shotPoint;
		}
		
	}
	
	public void CmdActiveWeaponIndex(int index)
	{
		playerWeapons.SwitchActiveWeaponSlot(index);
	}
}
