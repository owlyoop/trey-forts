using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Owly;

public struct PlayerActionInputs
{
	public bool PrimaryFire; //default mouse1
	public bool PrimaryFireUp;
    public bool PrimaryFireHolding;
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
    public bool OpenMainMenu; // default escape key
    public bool OpenScoreboard; //default tab

    public bool HelpText; // default F2
    

}

public class PlayerInput : MonoBehaviour
{
	//public PlayerMove player;
	public PlayerStats player;
	public WeaponSlots playerWeapons;
	public Camera cam;
	public UIManager mainUI;
	

	public GameObject holdingPropObj;
	
	public string debugID;
	public PlayerActionInputs playerInputs;


	private Vector3 lookat;

    private void Update()
	{
        if (!player.netIdentity.isLocalPlayer)
            return;

        HandlePlayerInput();
        CheckForInput();
    }

    private void Start()
	{
        player = GetComponent<PlayerStats>();
        playerWeapons = player.wepSlots;
        playerWeapons.DecactivateAllWeapons();
	}

	void HandlePlayerInput()
	{
		playerInputs.PrimaryFire = Input.GetMouseButtonDown(0);
		playerInputs.PrimaryFireUp = Input.GetMouseButtonUp(0);
        playerInputs.PrimaryFireHolding = Input.GetMouseButton(0);
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

		playerInputs.PropSpawner = Input.GetKeyDown(KeyCode.T);
		playerInputs.PropMover = Input.GetKeyDown(KeyCode.G);

		playerInputs.Interact = Input.GetKeyDown(KeyCode.E);
		playerInputs.InteractHeld = Input.GetKey(KeyCode.E);
		playerInputs.InteractUp = Input.GetKeyUp(KeyCode.E);
		playerInputs.Reload = Input.GetKeyDown(KeyCode.R);

        playerInputs.OpenTeamSelectMenu = Input.GetKeyDown(KeyCode.Alpha9);
        playerInputs.OpenClassSelectMenu = Input.GetKeyDown(KeyCode.Alpha0);
        playerInputs.OpenMainMenu = Input.GetKeyDown(KeyCode.Escape);
        playerInputs.OpenScoreboard = Input.GetKeyDown(KeyCode.Tab);

        playerInputs.HelpText = Input.GetKeyDown(KeyCode.F2);
    }

    void SwitchWeapon(int ToIndex)
    {
        mainUI.PropSpawnMenu.SetActive(false);
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
        //TODO: optimize this
		if (playerWeapons.weaponSlots.Count > 0)
		{
			wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex];
		}

		if (playerInputs.PrimaryFire)
		{
			if (!mainUI.HasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.activeSelf && player.isAlive)
				{
					wep.GetComponent<WeaponMotor>().PrimaryFire();
				}

				if (playerWeapons.propWepSlots[0].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().PrimaryFire();
				}
				if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().PrimaryFire();
				}
			}
		}

		if (playerInputs.PrimaryFireUp)
		{
			
			if (!mainUI.HasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.activeSelf && player.isAlive)
				{
					wep.GetComponent<WeaponMotor>().PrimaryFireButtonUp();
				}
				if (playerWeapons.propWepSlots[0].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().PrimaryFireButtonUp();
				}
				if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().PrimaryFireButtonUp();
				}
			}
		}

        if (playerInputs.PrimaryFireHolding)
        {
            if (!mainUI.HasUnlockedMouseUIEnabled)
            {
                if (wep != null && wep.activeSelf && player.isAlive)
                {
                    wep.GetComponent<WeaponMotor>().PrimaryFireHolding();
                }
            }
        }

		if (playerInputs.SecondaryFire)
		{
			if (!mainUI.HasUnlockedMouseUIEnabled && playerWeapons.weaponSlots.Count > 0)
			{
				if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].activeSelf && player.isAlive)
				{
					playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().SecondaryFire();
				}
				if (playerWeapons.propWepSlots[0].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().SecondaryFire();
				}
				if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().SecondaryFire();
				}
			}
		}

		if (playerInputs.Weapon1)
		{
			if (player.isAlive && playerWeapons.weaponSlots.Count > 0)
			{
                SwitchWeapon(0);
			}

		}

		if (playerInputs.Weapon2)
		{
			if (player.isAlive && playerWeapons.weaponSlots.Count > 1)
			{
                SwitchWeapon(1);
			}

		}

		if (playerInputs.Weapon3)
		{
			if (player.isAlive && playerWeapons.weaponSlots.Count > 2)
			{
                SwitchWeapon(2);
			}
		}

		if (playerInputs.PropSpawner)
		{
			if (wep != null && player.isAlive)
			{
                if (playerWeapons.propWepSlots[0].activeSelf)
                {
                    if (mainUI.CurrentUIState == PlayerUIState.None)
                    {
                        mainUI.TransitionToState(PlayerUIState.PropMenu);
                    }
                    else if (mainUI.CurrentUIState == PlayerUIState.PropMenu)
                    {
                        mainUI.TransitionToState(PlayerUIState.None);
                    }
                }
                else
                {
                    playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
                    playerWeapons.SwitchToPropSpawner();
                    playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchToWeapon();
                }

			}
		}

		if (playerInputs.PropMover)
		{
			if (wep != null && player.isAlive)
			{
				playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
				playerWeapons.SwitchToPropMover();
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().OnSwitchToWeapon();
			}
		}

		if (playerInputs.Ability1)
		{
			if (wep != null && player.isAlive)
			{
                playerWeapons.ActivateAbility(0);
			}
		}

		if (playerInputs.MeleeKick)
		{
			if (!player.CharControl.Motor.GroundingStatus.FoundAnyGround && player.CharControl.CurrentCharacterState != CharacterState.Divekick)
			{
                player.CharControl.TransitionToState(CharacterState.Divekick);
            }
			else
			{
				
			}
		}


		if (playerInputs.OpenTeamSelectMenu)
		{
            if (mainUI.CurrentUIState == PlayerUIState.TeamSelectMenu)
            {
                mainUI.TransitionToState(PlayerUIState.None);
            }
            else if (mainUI.CurrentUIState != PlayerUIState.MainMenu)
            {
                mainUI.TransitionToState(PlayerUIState.TeamSelectMenu);
            }
		}

		if (playerInputs.OpenClassSelectMenu)
		{
			if (mainUI.CurrentUIState == PlayerUIState.ClassSelectMenu)
			{
                mainUI.TransitionToState(PlayerUIState.None);
		    }
		    else if (mainUI.CurrentUIState != PlayerUIState.MainMenu)
			{
                mainUI.TransitionToState(PlayerUIState.ClassSelectMenu);
			}
		}

        if (playerInputs.OpenMainMenu)
        {
            if (mainUI.CurrentUIState == PlayerUIState.MainMenu)
            {
                mainUI.TransitionToState(PlayerUIState.None);
            }
            else
            {
                mainUI.TransitionToState(PlayerUIState.MainMenu);
            }
        }

        if (playerInputs.OpenScoreboard)
        {
            if (mainUI.CurrentUIState == PlayerUIState.Scoreboard)
            {
                mainUI.TransitionToState(PlayerUIState.None);
            }
            else
            {
                mainUI.TransitionToState(PlayerUIState.Scoreboard);
            }
        }

		if (playerInputs.MouseScroll > 0f)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().ScrollWheelUp();
			}

		}

		if (playerInputs.MouseScroll < 0f)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().ScrollWheelDown();
			}

		}

		if (playerInputs.Reload)
		{
			if (player.isAlive)
			{
				if (!mainUI.HasUnlockedMouseUIEnabled && !playerWeapons.propWepSlots[0].activeSelf)
				{
					playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().ReloadKey();
				}
				if (playerWeapons.propWepSlots[0].activeSelf && player.isAlive)
				{
					playerWeapons.propWepSlots[0].GetComponent<WeaponMotor>().ReloadKey();
				}
			}

		}

		if (playerInputs.Interact)
		{
		}

		if (playerInputs.InteractHeld)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive && !mainUI.HasUnlockedMouseUIEnabled)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().UseKeyHolding();
			}
		}

		if (playerInputs.InteractUp)
		{
			if (playerWeapons.propWepSlots[1].activeSelf && player.isAlive && !mainUI.HasUnlockedMouseUIEnabled)
			{
				playerWeapons.propWepSlots[1].GetComponent<WeaponMotor>().UseKeyUp();
			}
		}

        if (playerInputs.HelpText)
        {
            if (mainUI.isActiveAndEnabled)
            {
                if (mainUI.HelpText.gameObject.activeSelf)
                {
                    mainUI.HelpText.gameObject.SetActive(false);
                }
                else
                {
                    mainUI.HelpText.gameObject.SetActive(true);
                }
            }
        }
	}


	public void CmdActiveWeaponIndex(int index)
	{
		playerWeapons.SwitchActiveWeaponSlot(index);
	}
}
