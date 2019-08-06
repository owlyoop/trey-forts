using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerInput : NetworkBehaviour
{
	public PlayerMove player;
	public PlayerStats playerStats;
	public WeaponSlots playerWeapons;
	public Camera cam;
	public UIManager mainUI;

	[SyncVar]
	public int selectedPropMenuIndex;

	[SyncVar]
	public NetworkInstanceId holdingPropID;

	public GameObject holdingPropObj;

	[SyncVar]
	public string debugID;



	private Vector3 lookat;

	private void Update()
	{
		if (!isLocalPlayer)
			return;
		CheckForInput();
		//GetLook();
	}

	private void Start()
	{
		
	}

	void CheckForInput()
	{
		GameObject wep = null;
		if (playerWeapons.weaponSlots.Count > 0)
		{
			wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex];
		}
		//var wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex];
		if (Input.GetMouseButtonDown(0))
		{
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.activeSelf)
				{

					if (wep != null && wep.GetComponent<ProjectileLauncher>() != null)
					{
						GetLook();
						CmdPrimaryFireProjectile(lookat);
						return;
					}

					if (wep != null && wep.GetComponent<PropSpawner>() != null && mainUI.propMenu.hasPropSelected)
					{
						var wepProp = wep.GetComponent<PropSpawner>();
						wepProp.PrimaryFire();
						RigidbodyConstraints constraints = RigidbodyConstraints.FreezeAll;
						CmdSpawnFortwarsProp(wepProp.networkPropPosition, wepProp.networkPropRotation, constraints, selectedPropMenuIndex);
						Debug.Log(wepProp.ToString());
						return;
					}

					if (wep != null && wep.GetComponent<ObjectMover>() != null)
					{
						var objmov = wep.GetComponent<ObjectMover>();
						CmdPrimaryFire();

						

						return;
					}

					Debug.Log("primary fire");
					CmdPrimaryFire();
					
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.activeSelf)
				{
					if (wep != null && wep.GetComponent<ObjectMover>() != null)
					{
						Debug.Log("mouse button up objmov not null");
						bool useGrav = false;
						RigidbodyConstraints constraints = RigidbodyConstraints.FreezeAll;
						var id = wep.GetComponent<ObjectMover>().idOfObj;
						CmdPrimaryFireUp(useGrav, constraints, id);
					}

				}
			}
		}

		if (Input.GetMouseButtonDown(1))
		{
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].activeSelf)
				{
					playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().SecondaryFire();
				}
			}
		}


		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
			playerWeapons.SwitchActiveWeaponSlot(0);
			CmdActiveWeaponIndex(0);
			playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchToWeapon();
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
			playerWeapons.SwitchActiveWeaponSlot(1);
			CmdActiveWeaponIndex(1);
			playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchToWeapon();
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchAwayFromWeapon();
			playerWeapons.SwitchActiveWeaponSlot(2);
			CmdActiveWeaponIndex(2);
			playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().OnSwitchToWeapon();
		}

		if (Input.GetKeyDown(KeyCode.C))
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

		if (Input.GetKeyDown(KeyCode.Alpha0))
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

		if (Input.GetKeyDown(KeyCode.Alpha9))
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

		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].activeSelf)
			{
				//playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().ScrollWheelUp();
				CmdScrollUp();
			}
		}

		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].activeSelf)
			{
				//playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().ScrollWheelDown();
				CmdScrollDown();
			}
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().ReloadButton();
			}
		}

		if (Input.GetKey(KeyCode.E))
		{
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.GetComponent<ObjectMover>() != null)
				{
					cam.GetComponent<PlayerLook>().viewLocked = true;

					var objmov = wep.GetComponent<ObjectMover>();

					objmov.netRotY = Input.GetAxis("Mouse Y") * objmov.rotationSpeed * Mathf.Deg2Rad;
					objmov.netRotX = Input.GetAxis("Mouse X") * objmov.rotationSpeed * Mathf.Deg2Rad;
					//wep.GetComponent<ObjectMover>().UseButtonHolding();

					CmdUseButtonHolding(objmov.netRotY, objmov.netRotX);
				}
			}
		}

		if (Input.GetKeyUp(KeyCode.E))
		{
			if (!mainUI.hasUnlockedMouseUIEnabled)
			{
				if (wep != null && wep.GetComponent<ObjectMover>() != null)
				{
					cam.GetComponent<PlayerLook>().viewLocked = false;
					//wep.GetComponent<ObjectMover>().UseButtonUp();
					CmdUseButtonUp(wep.GetComponent<ObjectMover>().idOfObj);
				}

			}
		}
	}

	void GetLook()
	{
		if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>() != null)
		{
			lookat = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>().shotPoint;
		}
		
	}



	[Command]
	public void CmdPrimaryFire()
	{

		var objmov = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ObjectMover>();

		playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().PrimaryFire();


		if (objmov != null && objmov.isHoldingObject && holdingPropObj != null)
		{
			holdingPropObj = NetworkServer.FindLocalObject(holdingPropID);
			objmov.theObject = holdingPropObj.GetComponent<Rigidbody>();
			objmov.theObject.GetComponent<Smooth.SmoothSync>().sendRate = 24f;
		}

		Debug.Log("primary fire cmd");
	}

	[Command]
	public void CmdPrimaryFireProjectile(Vector3 look)
	{
		//playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].SetActive(true);
		playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().PrimaryFire();

		if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().networkObjToSpawn != null)
		{
			if (playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>() != null)
			{
				Debug.Log("obj nnot null");
				look = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>().shotPoint;
				playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>().networkObjToSpawn.transform.LookAt(look);
				NetworkServer.Spawn(playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ProjectileLauncher>().networkObjToSpawn);
				return;
			}

			Debug.Log("what the");
			NetworkServer.Spawn(playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>().networkObjToSpawn);
		}
	}

	[Command]
	public void CmdSpawnFortwarsProp(Vector3 position, Quaternion rotation, RigidbodyConstraints constraints, int id)
	{
		var wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<PropSpawner>();
		
		Debug.Log("spawn pls");

		var prop = Instantiate(mainUI.props[id].propPrefab, position, rotation);
		prop.transform.position = position;
		prop.transform.rotation = rotation;
		prop.layer = 11;
		prop.GetComponent<Collider>().enabled = true;
		prop.GetComponent<Rigidbody>().useGravity = false;
		prop.GetComponent<Rigidbody>().isKinematic = false;
		prop.GetComponent<Rigidbody>().constraints = constraints;
		prop.GetComponent<Smooth.SmoothSync>().sendRate = 1f;

		playerStats.currentCurrency = playerStats.currentCurrency - mainUI.props[id].currencyCost;
		playerStats.currencyAmount.text = playerStats.currentCurrency.ToString();

		NetworkServer.Spawn(prop);

		
	}

	[Command]
	public void CmdPrimaryFireUp(bool useGravity, RigidbodyConstraints constraint, NetworkInstanceId id)
	{
		var wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ObjectMover>();
		
		if (wep != null)
		{
			wep.PrimaryFireButtonUp();
			if (wep != null && wep.isHoldingObject && holdingPropObj != null)
			{
				holdingPropObj = NetworkServer.FindLocalObject(holdingPropID);
				wep.theObject = holdingPropObj.GetComponent<Rigidbody>();
				wep.theObject.GetComponent<Smooth.SmoothSync>().sendRate = 1f;
			}
		}
	}

	[Command]
	public void CmdUseButtonHolding(float roty, float rotx)
	{
		var objmov = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ObjectMover>();

		objmov.UseButtonHolding();

		objmov.theObject.transform.Rotate(Vector3.up, -rotx, Space.World);
		objmov.theObject.transform.Rotate(cam.transform.right, roty, Space.World);
		objmov.theObject.GetComponent<Smooth.SmoothSync>().sendRate = 24f;
		Debug.Log("cmd holding");

	}

	[Command]
	public void CmdUseButtonUp(NetworkInstanceId id)
	{
		var wep = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<WeaponMotor>();

		if (wep.GetComponent<ObjectMover>() != null)
		{
			holdingPropObj = NetworkServer.FindLocalObject(holdingPropID);
			var theObject = holdingPropObj.GetComponent<Rigidbody>();
			theObject.GetComponent<Smooth.SmoothSync>().sendRate = 1f;
		}


		wep.UseButtonUp();
	}

	[Command]
	public void CmdActiveWeaponIndex(int index)
	{
		for (int i = 0; i < playerWeapons.weaponSlots.Count; i++)
		{
			playerWeapons.weaponSlots[i].SetActive(false);
		}
		playerWeapons.weaponSlots[index].SetActive(true);
		playerWeapons.activeWeaponIndex = index;
		playerWeapons.ApplyWeaponStats(index);
	}

	[Command]
	public void CmdScrollUp()
	{
		var objmov = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ObjectMover>();

		if (objmov != null)
		{
			objmov.ScrollWheelUp();
		}
	}

	[Command]
	public void CmdScrollDown()
	{
		var objmov = playerWeapons.weaponSlots[playerWeapons.activeWeaponIndex].GetComponent<ObjectMover>();


		if (objmov != null)
		{
			objmov.ScrollWheelDown();
		}
	}

}
