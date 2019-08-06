using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerStats : NetworkBehaviour, IDamagable
{
	// 0 spectator, 1 blue, 2 red
	[SyncVar]
	public int playerTeam; // 0 spectator, 1 blue, 2 red

	public CharacterStat maxHealth;

	[SyncVar]
	public int maxHealthValue;
	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth;

	[SyncVar]
	public int currentCurrency;
	

	[SyncVar(hook = "OnChangeAmmoInClip")]
	public int currentAmmoInClip;

	[SyncVar(hook = "OnChangeAmmoReservesAmount")]
	public int currentAmmoReserves;

	public int startingCurrency = 1000;

	public WeaponSet playerClass;
	
	public RectTransform healthBar;
	public RectTransform healthBarBackground;
	public RectTransform armorBar;
	public RectTransform armorBarBackground;
	public Text healthText;
	public Text currencyAmount;
	public Text ammoInClip;
	public Text ammoAmount;
	public Image radialReload;

	public GamePhases gameManager;

	public SkinnedMeshRenderer bodyToColor;
	public SkinnedMeshRenderer jointsToColor;
	public Material bodyMaterialBlue;
	public Material bodyMaterialRed;
	public Material jointsMaterialBlue;
	public Material jointsMaterialRed;

	public GameObject flagAttachParent;

	private void Start()
	{
		gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		
		OnDeath();
		ammoInClip.text = "";
		ammoAmount.text = "";
		healthText.text = currentHealth.ToString();
		radialReload.enabled = false;

		SetTeam(playerTeam);

		OnChangeHealth(currentHealth);
	}

	void StartAsSpectator()
	{

	}

	public void InitializeValues()
	{
		maxHealth.BaseValue = playerClass.maxHealth;
		maxHealthValue = Mathf.RoundToInt(maxHealth.Value);
		currentHealth = maxHealthValue;
		currentCurrency = startingCurrency;
		currencyAmount.text = currentCurrency.ToString();
		ammoInClip.text = "";
		ammoAmount.text = "";
		healthText.text = currentHealth.ToString();
		radialReload.enabled = false;
	}

	public void OnDeath()
	{
		RpcRespawn();
		if (playerClass != null)
		{
			this.GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
		}
		InitializeValues();
		
	}

	public void TakeDamage(int damageTaken, Damager.DamageTypes damageType)
	{
		// only server can call damage
		if (!isServer)
		{
			return;
		}

		currentHealth = currentHealth - damageTaken;
		if (currentHealth <= 0)
		{
			currentHealth = maxHealthValue;
			OnDeath();
		}

		
	}

	public void OnChangeAmmoReservesAmount(int currentAmmoReserves)
	{
		ammoAmount.text = currentAmmoReserves.ToString();
	}

	public void OnChangeAmmoInClip(int currentAmmoInClip)
	{
		ammoInClip.text = currentAmmoInClip.ToString();
	}

	void OnChangeHealth (int currentHealth)
	{
		healthBar.sizeDelta = new Vector2(((float)currentHealth / (float)maxHealthValue) * healthBarBackground.sizeDelta.x, 32);
		healthText.text = currentHealth.ToString();
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer)
		{

			if (playerTeam == 1)
			{
				Vector3 spawnPoint;
				spawnPoint = gameManager.teamOneSpawnPoints[Random.Range(0, gameManager.teamOneSpawnPoints.Length)].transform.position;

				transform.position = spawnPoint;
			}

			if (playerTeam == 2)
			{
				Vector3 spawnPoint;
				spawnPoint = gameManager.teamTwoSpawnPoints[Random.Range(0, gameManager.teamTwoSpawnPoints.Length)].transform.position;

				transform.position = spawnPoint;
			}
			
		}
	}

	[ClientRpc]
	void RpcSetTeam(int team)
	{
		playerTeam = team;
		if (team == 1)
		{
			bodyToColor.material = bodyMaterialBlue;
			jointsToColor.material = jointsMaterialBlue;
		}
		if (team == 2)
		{
			bodyToColor.material = bodyMaterialRed;
			jointsToColor.material = jointsMaterialRed;
		}
		if (team == 0)
		{
			//GetComponent<CharacterController>().enabled = false;
			//playerModelRoot.SetActive(false);
		}
	}

	[Command]
	void CmdSetColors(int team)
	{
		playerTeam = team;
		if (team == 1)
		{
			bodyToColor.material = bodyMaterialBlue;
			jointsToColor.material = jointsMaterialBlue;
		}
		if (team == 2)
		{
			bodyToColor.material = bodyMaterialRed;
			jointsToColor.material = jointsMaterialRed;
		}
	}

	public void SetTeam(int team)
	{
		playerTeam = team;
		if (isServer)
		{
			//RpcSetTeam(team);
		}

		if (team == 1)
		{
			bodyToColor.material = bodyMaterialBlue;
			jointsToColor.material = jointsMaterialBlue;
		}
		if (team == 2)
		{
			bodyToColor.material = bodyMaterialRed;
			jointsToColor.material = jointsMaterialRed;
		}
		if (isLocalPlayer)
		{
			CmdSetColors(team);
		}
		
	}
}
