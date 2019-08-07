using KinematicCharacterController;
using KinematicCharacterController.Owly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerStats : MonoBehaviourPunCallbacks, IDamagable, IPunObservable
{
	// 0 spectator, 1 blue, 2 red
	public int playerTeam; // 0 spectator, 1 blue, 2 red

	[Header("Stats")]
	public CharacterStat maxHealth;
	int maxHealthValue;
	public int currentHealth;
	public int currentCurrency;
	public int currentAmmoInClip;
	public int currentAmmoReserves;
	
	public bool canPickUpFlag;
	public bool isAlive;
	public bool hasFlag;

	

	public int startingCurrency = 1000;

	public WeaponSet playerClass;
	public WeaponSet queuedClass;


    public CharacterStat MovementSpeed;
	float baseMoveSpeed = 6.2f;
    StatModifier ClassMovespeedMultiplier;

    public CharacterStat JumpForce;
	float baseJumpForce = 7.24f;
    StatModifier ClassJumpforceMultiplier;

    [Header("Status Effects")]
	public StatusEffectReceiver StatusEffectManager;

	[Header("References")]
	public UIManager ui;
	public WeaponSlots wepSlots;
	public GamePhases _gameManager;
	public FloatingTextController dmgText;
	public MyCharacterController CharControl;
    public GameObject ragdollPrebaf;
    public Divekick divekickHitbox;

    [Header("Hitbox References")]
    public PlayerHitbox headHitbox;
    public PlayerHitbox spineHitbox;
    public PlayerHitbox hipsHitbox;
    public PlayerHitbox leftUpLegHitbox;
    public PlayerHitbox leftLegHitbox;
    public PlayerHitbox rightUpLegHitbox;
    public PlayerHitbox rightLegHitbox;
    public PlayerHitbox leftArmHitbox;
    public PlayerHitbox leftForearmHitbox;
    public PlayerHitbox rightArmHitbox;
    public PlayerHitbox rightForearmHitbox;
    public List<PlayerHitbox> hitboxCollection = new List<PlayerHitbox>();

    public List<GameObject> PropsOwnedByPlayer;

	public SkinnedMeshRenderer bodyToColor;
	public SkinnedMeshRenderer jointsToColor;
	public Material bodyMaterialBlue;
	public Material bodyMaterialRed;
	public Material jointsMaterialBlue;
	public Material jointsMaterialRed;

	public GameObject flagAttachParent;
	float _respawnTimer;

	CaptureFlag _flagref;
	public Animator anim;

	public CapsuleCollider charControlCollider;
	


	private void Start()
	{
        hitboxCollection.Add(headHitbox);
        hitboxCollection.Add(spineHitbox);
        hitboxCollection.Add(hipsHitbox);
        hitboxCollection.Add(leftUpLegHitbox);
        hitboxCollection.Add(leftLegHitbox);
        hitboxCollection.Add(rightUpLegHitbox);
        hitboxCollection.Add(rightLegHitbox);
        hitboxCollection.Add(leftArmHitbox);
        hitboxCollection.Add(leftForearmHitbox);
        hitboxCollection.Add(rightArmHitbox);
        hitboxCollection.Add(rightForearmHitbox);
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GamePhases>();
        divekickHitbox.gameObject.SetActive(false);

        if (!photonView.IsMine)
		{
			return;
		}

		Debug.Log(_gameManager.ToString());
		Physics.IgnoreLayerCollision(10, 12);
        Physics.IgnoreLayerCollision(10, 9);

        ui.dollar.enabled = false;


		ChangeToSpectator();
		ui.radialReload.enabled = false;

		SetTeam(playerTeam);

		currentCurrency = startingCurrency;
		OnChangeHealth(currentHealth);
		ui.SetActivateDeathUI(false);
		hasFlag = false;

        //statmodifiers
        MovementSpeed.BaseValue = baseMoveSpeed;
        ClassMovespeedMultiplier = new StatModifier(playerClass.moveSpeedPercentAdd, StatModType.PercentAdd);
        MovementSpeed.AddModifier(ClassMovespeedMultiplier);
        GetComponent<MyCharacterController>().MaxStableMoveSpeed = MovementSpeed.Value;
        Debug.Log("ps start()");

        JumpForce.BaseValue = baseJumpForce;
        ClassJumpforceMultiplier = new StatModifier(playerClass.jumpHeightPercentAdd, StatModType.PercentAdd);
        JumpForce.AddModifier(ClassJumpforceMultiplier);
        GetComponent<MyCharacterController>().JumpSpeed = JumpForce.Value;


		StatusEffectManager.AddPassiveStatusEffects();
		EventManager.onWaitingForPlayersEnd += WaitingForPlayersEnd;
		CharControl = GetComponent<MyCharacterController>();
	}

	private void OnDisable()
	{
		EventManager.onWaitingForPlayersEnd -= WaitingForPlayersEnd;
	}

	void WaitingForPlayersEnd()
	{
		RespawnAndInitialize();
	}

	void Update()
	{
		if (!photonView.IsMine)
		{
			return;
		}
	}

	public void ChangeToSpectator()
	{
		if (!photonView.IsMine)
		{
			return;
		}

		for (int i = 0; i < _gameManager.classList.Count; i++)
		{
			if (_gameManager.classList[i].className == "Spectator")
			{
				queuedClass = _gameManager.classList[i];
			}
		}
		GetComponent<MyCharacterController>().TransitionToState(CharacterState.Spectator);
		RpcRespawn();
	}
    
    // Re-initialize the movementpseed and jump character stat. use after i set something to a flat value, rather than modifiers.
    public void CalculateMovementValues()
    {
        MovementSpeed.RemoveModifier(ClassMovespeedMultiplier);
        ClassMovespeedMultiplier = new StatModifier(playerClass.moveSpeedPercentAdd, StatModType.PercentAdd);
        MovementSpeed.AddModifier(ClassMovespeedMultiplier);
        CharControl.MaxStableMoveSpeed = MovementSpeed.Value;
        CharControl.MaxAirMoveSpeed = baseMoveSpeed;

        JumpForce.RemoveModifier(ClassJumpforceMultiplier);
        ClassJumpforceMultiplier = new StatModifier(playerClass.jumpHeightPercentAdd, StatModType.PercentAdd);
        JumpForce.AddModifier(ClassJumpforceMultiplier);
        CharControl.JumpSpeed = JumpForce.Value;
    }

	public void ChangeAwayFromSpectator()
	{
		if (!photonView.IsMine)
		{
			return;
		}

		GetComponent<MyCharacterController>().TransitionToState(playerClass.defaultState);
	}

	//called at the end of rpcrespawn
	public void InitializeValues()
	{
		if (!photonView.IsMine)
			return;

		if (playerClass.className != "Spectator")
		{
			ui.SetActiveMainHud(true);
			ui.healthBar.gameObject.SetActive(true);
			ui.healthBarBackground.gameObject.SetActive(true);
			maxHealth.BaseValue = playerClass.maxHealth;
			maxHealthValue = Mathf.RoundToInt(maxHealth.Value);
			currentHealth = maxHealthValue;
			ui.dollar.enabled = true;
			ui.ammoInClip.text = "";
			ui.ammoAmount.text = "";
			ui.healthText.text = currentHealth.ToString();
			ui.radialReload.enabled = false;
			OnChangeHealth(currentHealth);
			ui.currencyAmount.text = currentCurrency.ToString();

            CalculateMovementValues();

			StatusEffectManager.AddPassiveStatusEffects();
		}
		else
		{
			ui.dollar.enabled = false;
			ui.healthBar.gameObject.SetActive(false);
			ui.healthBarBackground.gameObject.SetActive(false);
		}

	}

	public void OnDeath()
	{
		if (!photonView.IsMine)
			return;
		isAlive = false;
		_respawnTimer = _gameManager.respawnTime;
		StartCoroutine(RespawnCountdownTimer());
		ui.SetActivateDeathUI(true);

		
		if (_flagref != null)
		{
			_flagref.transform.parent = null;
		}
		hasFlag = false;
		GetComponent<MyCharacterController>().TransitionToState(CharacterState.Dead);
		

		if (playerClass != null)
		{
			this.GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
			GetComponent<PlayerInput>().playerWeapons.DecactivateAllWeapons();
		}
	}

	public void RespawnAndInitialize()
	{
		RpcRespawn();
	}

	void RpcRespawn()
	{
		if (!photonView.IsMine)
			return;

		Debug.Log(GetComponent<PhotonView>().Owner.ToString());

		if (_gameManager == null)
		{
			_gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		}

		if (playerTeam == 1)
		{
			Vector3 spawnPoint;
			spawnPoint = _gameManager.teamOneSpawnPoints[Random.Range(0, _gameManager.teamOneSpawnPoints.Length)].transform.position;
			GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint);
		}
		if (playerTeam == 2)
		{
			Vector3 spawnPoint;
			spawnPoint = _gameManager.teamTwoSpawnPoints[Random.Range(0, _gameManager.teamTwoSpawnPoints.Length)].transform.position;

			GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint);
		}

		isAlive = true;
		
		canPickUpFlag = true;
		ui.SetActivateDeathUI(false);
		playerClass = queuedClass;
		GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
		if (playerClass.className != "Spectator")
		{
			GetComponent<MyCharacterController>().TransitionToState(playerClass.defaultState);
		}
		InitializeValues();

	}

	public IEnumerator RespawnCountdownTimer()
	{
		ui.respawnTimer.text = _respawnTimer.ToString();
		while (_respawnTimer > 0)
		{
			yield return new WaitForSeconds(1.0f);
			_respawnTimer = _respawnTimer - 1f;
			ui.respawnTimer.text = _respawnTimer.ToString();
		}
		if (_respawnTimer <= 0)
		{
			RpcRespawn();
		}

	}

	public void TakeDamage(int GiverPunID, int damageTaken, Damager.DamageTypes damageType)
	{
		if (isAlive)
		{
			if (playerClass.name == "Spectator")
				return;
			photonView.RPC("RpcTakeDamage", RpcTarget.AllViaServer, GiverPunID, damageTaken);
		}
	}

	[PunRPC]
	void RpcTakeDamage(int GiverPunID, int dmg)
	{
		dmg = StatusEffectManager.OnBeforeDamageTaken(dmg);
		currentHealth = currentHealth - dmg;
		OnChangeHealth(currentHealth);
		PhotonView.Find(GiverPunID).GetComponent<PlayerStats>().dmgText.CreateFloatingText(dmg.ToString(), this.transform);
		if (currentHealth <= 0)
		{
			currentHealth = 0;
			OnDeath();
			isAlive = false;
		}
	}

	public void OnChangeAmmoReservesAmount(int newCurrentAmmoReserves)
	{
		currentAmmoReserves = newCurrentAmmoReserves;
		ui.ammoAmount.text = currentAmmoReserves.ToString();
	}

	public void OnChangeAmmoInClip(int newCurrentAmmoInClip)
	{
		currentAmmoInClip = newCurrentAmmoInClip;
		ui.ammoInClip.text = currentAmmoInClip.ToString();
	}

	void OnChangeHealth (int currentHealth)
	{
		ui.healthBar.sizeDelta = new Vector2(((float)currentHealth / (float)maxHealthValue) * ui.healthBarBackground.sizeDelta.x, 32);
		ui.healthText.text = currentHealth.ToString();
	}

	public void OnChangeCurrencyAmount(int newMoney)
	{
		currentCurrency = newMoney;
		ui.currencyAmount.text = currentCurrency.ToString();
	}
	
	void HideOwnPlayerModel(bool choice)
	{
		if (!photonView.IsMine)
			return;

		if (choice == true)
		{
			//bodyToColor.enabled = false;
			//jointsToColor.enabled = false;
		}
		else
		{
		//	bodyToColor.enabled = true;
			//jointsToColor.enabled = true;
		}


	}

	public void SpawnRagdoll()
	{

	}
	
	public void SetTeam(int team)
	{
		HideOwnPlayerModel(false);
		playerTeam = team;
		if (bodyToColor.enabled == true && jointsToColor.enabled == true)
		{
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
		HideOwnPlayerModel(true);
	}

	public bool CheckIfCanPickupFlag()
	{
		if (playerTeam == 0)
		{
			canPickUpFlag = false;
		}
		
		return canPickUpFlag;
	}

	private void OnTriggerEnter(Collider other)
	{
		CaptureFlag flag = other.GetComponentInParent<CaptureFlag>();
		if (flag != null && flag.isBeingHeld == false)
		{
			if ((flag.team == CaptureFlag.Team.Blue && playerTeam == 2) || (flag.team == CaptureFlag.Team.Red && playerTeam == 1))
			{
				flag.transform.parent = flagAttachParent.transform;
				flag.transform.localPosition = new Vector3(0, 0, 0);
				flag.isBeingHeld = true;
				_flagref = flag;
				hasFlag = true;
			}
		}


		if (_gameManager == null)
		{
			_gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		}
		if (_gameManager != null && other == _gameManager.blueCaptureZone && hasFlag)
		{
			_flagref.transform.parent = null;
			_flagref.transform.position = _flagref.flagSpawn;
			_flagref.isBeingHeld = false;
			hasFlag = false;
			RpcAddTeamScore(1);
		}
		if (_gameManager != null && other == _gameManager.redCaptureZone && hasFlag)
		{
			_flagref.transform.parent = null;
			_flagref.transform.position = _flagref.flagSpawn;
			_flagref.isBeingHeld = false;
			hasFlag = false;
			RpcAddTeamScore(2);
		}
	}
	
	public void RpcAddTeamScore(int team)
	{
		if (team == 1)
		{
			_gameManager.BlueTeamScore += 1;
			ui.bluScore.text = _gameManager.BlueTeamScore.ToString();
		}

		if (team == 2)
		{
			_gameManager.RedTeamScore += 1;
			ui.redScore.text = _gameManager.RedTeamScore.ToString();
		}
	}

	#region IPunObservable implementation
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.IsWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(playerTeam);
			stream.SendNext(isAlive);

			stream.SendNext(currentHealth);
			stream.SendNext(currentCurrency);
			stream.SendNext(currentAmmoInClip);
			stream.SendNext(currentAmmoReserves);

		}
		else
		{
			// Network player, receive data
			this.playerTeam = (int)stream.ReceiveNext();
			this.isAlive = (bool)stream.ReceiveNext();

			this.currentHealth = (int)stream.ReceiveNext();
			this.currentCurrency = (int)stream.ReceiveNext();
			this.currentAmmoInClip = (int)stream.ReceiveNext();
			this.currentAmmoReserves = (int)stream.ReceiveNext();
		}
	}
	#endregion
}
