using KinematicCharacterController;
using KinematicCharacterController.Owly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering.PostProcessing;

public class PlayerStats : NetworkBehaviour, IDamagable
{
    public enum PlayerTeam
    {
        Spectator,
        Blue,
        Red
    }

    public enum DamageIndicatorType
    {
        Directional,
        Generic,
        None
    }

    [Header("Network Info")]
    public int ping = 0;
    public string Username;
    [SyncVar]
    public PlayerTeam playerTeam;

    public WeaponSet currentClass;
    [SyncVar]
    public string currentClassName;

    public bool HasPreviouslyPlayed = false;

    [Header("Character Stats")]
	public CharacterStat maxHealth;
    [SyncVar]
	int finalMaxHealthValue;

    [SyncVar]
    public int currentHealth;

    [SyncVar]
    public bool isAlive;

    [SyncVar]
    public int currentCurrency;

    //for UI. probably should move this to the UIManager
	int currentAmmoInClip;
	int currentAmmoReserves;

    [SyncVar]
    public bool canPickUpFlag;
    [SyncVar]
    public bool hasFlag;

    public CharacterStat MovementSpeed;
	const float BaseMoveSpeed = 6.2f;
    StatModifier ClassMovespeedMultiplier;

    public CharacterStat JumpForce;
	const float BaseJumpForce = 7.24f;
    StatModifier ClassJumpforceMultiplier;

    [SerializeField]
    PlayerTeam queuedTeam;
    public PlayerTeam QueuedTeam
    {
        get { return queuedTeam; }
        private set { queuedTeam = value; }
    }

    [SerializeField]
    WeaponSet queuedClass;
    public WeaponSet QueuedClass
    {
        get { return queuedClass; }
        private set { queuedClass = value; }
    }

    [SerializeField]
    WeaponSet previousClass;
    public WeaponSet PreviousClass
    {
        get { return previousClass; }
        private set { previousClass = value; }
    }

    public Dictionary<string, int> eliteClassLives = new Dictionary<string, int>();

    [Header("Ingame Score")]
    public int score = 0;
    public int kills = 0;
    public int assists = 0;
    public int deaths = 0;

    [Header("Status Effects")]
	public StatusEffectReceiver StatusEffectManager;

	[Header("References")]
	public UIManager ui;
	public WeaponSlots wepSlots;
	public GamePhases gameManager;
	public FloatingTextController dmgText;
	public MyCharacterController CharControl;
    public Divekick divekickHitbox;
    public Camera cam;
    public Transform ParticleParent;

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
    public PlayerRagdoll ragdollPrefab;
    PlayerRagdoll ragdoll;
    float ragdollForceToAdd = 0;
    Vector3 ragdollForceDirection;

    [Header("Props")]
    public List<GameObject> PropsOwnedByPlayer;
    public bool hasPlacedMoneyPrinter = false;

    [Header("Body")]
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

    private void Awake()
    {
        CharControl = GetComponent<MyCharacterController>();
        gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GamePhases>();
    }

    private void Start()
	{
        if (!netIdentity.isLocalPlayer)
        {
            CharControl.enabled = false;
            GetComponent<KinematicCharacterMotor>().enabled = false;
            cam.enabled = false;
            cam.GetComponent<FlareLayer>().enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
            cam.GetComponent<MyCamera>().enabled = false;
            cam.GetComponent<PostProcessLayer>().enabled = false;
            cam.GetComponentInChildren<Camera>().gameObject.SetActive(false);
            GetComponent<PlayerInput>().enabled = false;
            ui.MainUICanvas.gameObject.SetActive(false);
            return;
        }

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

        divekickHitbox.gameObject.SetActive(false);

        EnableOwnHitbox(false);

		Physics.IgnoreLayerCollision(10, 12);
        Physics.IgnoreLayerCollision(10, 9);
        Physics.IgnoreLayerCollision(12, 9, true);

        ui.DollarSign.enabled = false;
        ui.RadialReload.StopReload();

		SetTeam(playerTeam);

        currentCurrency = gameManager.buildPhaseStartingMoney;
		OnChangeHealth(currentHealth);
		ui.SetActivateDeathUI(false);
		hasFlag = false;

		StatusEffectManager.AddPassiveStatusEffects();
		EventManager.onWaitingForPlayersEnd += WaitingForPlayersEnd;
        
    }

	private void OnDisable()
	{
		EventManager.onWaitingForPlayersEnd -= WaitingForPlayersEnd;
	}

	void WaitingForPlayersEnd()
	{
        RespawnPlayer();
	}

	public void ChangeToSpectator()
    {
        //SetTeam(PlayerTeam.Spectator);
        SetQueuedTeam(PlayerTeam.Spectator);
        SetQueuedClass(gameManager.gameData.spectatorClass);
        SwitchPlayerClass(gameManager.gameData.spectatorClass);

		GetComponent<MyCharacterController>().TransitionToState(CharacterState.Spectator);
        SetCollidersActive(false);
        StopCoroutine(RespawnCountdownTimer());
        RespawnPlayer();
	}

    public void AddMaxHealthModifier(StatModifier mod)
    {
        maxHealth.AddModifier(mod);
        OnChangeHealth(currentHealth);

    }

    public void RemoveMaxHealthModifier(StatModifier mod)
    {
        maxHealth.RemoveModifier(mod);
        OnChangeHealth(currentHealth);
    }

    public void AddMovementModifier(StatModifier mod)
    {
        MovementSpeed.AddModifier(mod);
        CharControl.MaxStableMoveSpeed = MovementSpeed.Value;
    }

    public void RemoveMovementModifier(StatModifier mod)
    {
        MovementSpeed.RemoveModifier(mod);
        CharControl.MaxStableMoveSpeed = MovementSpeed.Value;
    }

    public void AddJumpModifier(StatModifier mod)
    {
        JumpForce.AddModifier(mod);
        CharControl.JumpSpeed = JumpForce.Value;
    }

    public void RemoveJumpModifier(StatModifier mod)
    {
        JumpForce.RemoveModifier(mod);
        CharControl.JumpSpeed = JumpForce.Value;
    }


	//called at the end of rpcrespawn
	public void InitializeValues()
    {
        if (currentClass.className != "Spectator")
		{
			ui.SetActiveMainHud(true);
			ui.HealthBar.gameObject.SetActive(true);
			ui.HealthBarBackground.gameObject.SetActive(true);
			maxHealth.BaseValue = currentClass.maxHealth;
			finalMaxHealthValue = Mathf.RoundToInt(maxHealth.Value);
			currentHealth = finalMaxHealthValue;
			ui.DollarSign.enabled = true;
			ui.AmmoInClip.text = "";
			ui.AmmoAmount.text = "";
			ui.HealthText.text = currentHealth.ToString();
            ui.RadialReload.StopReload();
			OnChangeHealth(currentHealth);
			ui.CurrencyAmount.text = currentCurrency.ToString();

			StatusEffectManager.AddPassiveStatusEffects();

            MovementSpeed.BaseValue = BaseMoveSpeed;
            ClassMovespeedMultiplier = new StatModifier(currentClass.moveSpeedPercentAdd, StatModType.PercentAdd);
            MovementSpeed.RemoveAllModifiersFromSource(this);
            MovementSpeed.AddModifier(ClassMovespeedMultiplier);
            CharControl.MaxStableMoveSpeed = MovementSpeed.Value;

            JumpForce.BaseValue = BaseJumpForce;
            ClassJumpforceMultiplier = new StatModifier(currentClass.jumpHeightPercentAdd, StatModType.PercentAdd);
            JumpForce.RemoveAllModifiersFromSource(this);
            JumpForce.AddModifier(ClassJumpforceMultiplier);
            CharControl.JumpSpeed = JumpForce.Value;

            GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
        }
		else
		{
			ui.DollarSign.enabled = false;
			ui.HealthBar.gameObject.SetActive(false);
			ui.HealthBarBackground.gameObject.SetActive(false);
		}
	}

	public void OnDeath()
    {
        if (currentClass.isElite)
        {
            RemoveEliteClassLife(currentClass);
            eliteClassLives.TryGetValue(currentClass.className, out int lives);
            if (lives <= 0)
            {
                eliteClassLives[currentClass.className] = 0;
                SetQueuedClass(previousClass);
            }
        }

        Debug.Log("player died");
        SpawnRagdoll(hitboxCollection);
		isAlive = false;
		_respawnTimer = gameManager.respawnTime;
		StartCoroutine(RespawnCountdownTimer());
		ui.SetActivateDeathUI(true);
        StatusEffectManager.ClearAllStatusEffects();
		
		if (_flagref != null)
		{
			_flagref.transform.parent = null;
		}
		hasFlag = false;
		GetComponent<MyCharacterController>().TransitionToState(CharacterState.Dead);
        SetCollidersActive(false);


        if (currentClass != null)
		{
            GetComponent<PlayerInput>().CmdActiveWeaponIndex(0);
            GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
			GetComponent<PlayerInput>().playerWeapons.DecactivateAllWeapons();
		}
	}

    public void RespawnPlayer()
    {
        //Move player to a spawnpoint
        if (gameManager == null)
        {
            gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
        }

        if (QueuedTeam != playerTeam)
            SetTeam(QueuedTeam);

        if (playerTeam == PlayerTeam.Blue)
        {
            Vector3 spawnPoint;
            spawnPoint = gameManager.teamBlueSpawnPoints[Random.Range(0, gameManager.teamBlueSpawnPoints.Count)].transform.position;
            GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint);
        }
        if (playerTeam == PlayerTeam.Red)
        {
            Vector3 spawnPoint;
            spawnPoint = gameManager.teamRedSpawnPoints[Random.Range(0, gameManager.teamRedSpawnPoints.Count)].transform.position;

            GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint);
        }


        isAlive = true;

        canPickUpFlag = true;
        ui.SetActivateDeathUI(false);
        ui.SetActiveMainHud(true);
        EnableOwnHitbox(false);

        SwitchPlayerClass(queuedClass);

        GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
        GetComponent<MyCharacterController>().TransitionToState(currentClass.defaultState);
        if (currentClass.className == "Spectator")
        {
            SetCollidersActive(false);
        }
        else
        {
            SetCollidersActive(true);
        }
        InitializeValues();

    }

    public void SetCollidersActive(bool choice)
    {
        if (!netIdentity.isLocalPlayer)
            return;
        if (choice)
        {
            CharControl.Motor.SetCapsuleCollisionsActivation(true);
            this.GetComponent<Collider>().enabled = true;
            foreach (PlayerHitbox hb in hitboxCollection)
            {
                hb.gameObject.SetActive(true);
            }
        }
        else
        {
            CharControl.Motor.SetCapsuleCollisionsActivation(false);
            this.GetComponent<Collider>().enabled = false;
            foreach (PlayerHitbox hb in hitboxCollection)
            {
                hb.gameObject.SetActive(false);
            }
        }
    }

    public void SpawnRagdoll(List<PlayerHitbox> hitboxes)
    {
        if (!netIdentity.isLocalPlayer)
            return;
        EnableOwnHitbox(true);
        ragdoll = Instantiate(ragdollPrefab, this.transform.position, this.transform.rotation);

        for (int i = 0; i < hitboxes.Count; i++)
        {
            ragdoll.colliderCollection[i].transform.SetPositionAndRotation(hitboxes[i].transform.position, hitboxes[i].transform.rotation);
            ragdoll.colliderCollection[i].attachedRigidbody.velocity = hitboxes[i].GetComponent<Rigidbody>().velocity;
        }
        
        EnableOwnHitbox(false);
        ragdoll.colliderCollection[1].attachedRigidbody.AddForce(ragdollForceDirection.normalized * ragdollForceToAdd, ForceMode.Impulse);
        Debug.Log(ragdollForceToAdd.ToString());
        Debug.Log(ragdollForceDirection.ToString());
        if (playerTeam == PlayerTeam.Blue)
        {
            ragdoll.SetRagdollMaterials(bodyMaterialBlue, jointsMaterialBlue);
        }
        else if (playerTeam == PlayerTeam.Red)
        {
            ragdoll.SetRagdollMaterials(bodyMaterialRed, jointsMaterialRed);
        }
    }

	public IEnumerator RespawnCountdownTimer()
    {
        ui.RespawnTimer.text = _respawnTimer.ToString();
		while (_respawnTimer > 0)
		{
            ui.SetActiveYouWillSpawnAsText(true);
			yield return new WaitForSeconds(1.0f);
			_respawnTimer = _respawnTimer - 1f;
			ui.RespawnTimer.text = _respawnTimer.ToString();
		}
		if (_respawnTimer <= 0)
		{
            ui.SetActiveYouWillSpawnAsText(false);
			RespawnPlayer();
		}

	}

	public void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation, DamageIndicatorType uiType)
    {
        if (isAlive)
		{
			if (currentClass == gameManager.gameData.spectatorClass)
				return;

            CmdTakeDamage(damageTaken, damageType, giver.netIdentity, damageSourceLocation);
		}
	}

    public void TakeHeal(int healTaken, Damager.DamageTypes damageType)
    {
        if (!netIdentity.isLocalPlayer)
            return;
    }

    
    [Command]
    void CmdTakeDamage(int dmg, Damager.DamageTypes damageType, NetworkIdentity giverID, Vector3 damageSourceLocation)
    {
        var giver = giverID.GetComponent<PlayerStats>();
        dmg = StatusEffectManager.OnBeforeDamageTaken(dmg);
		currentHealth = currentHealth - dmg;
		OnChangeHealth(currentHealth);

        if (giver != null)
		    giver.dmgText.CreateFloatingText(dmg.ToString(), this.transform);

        GameObject dmgUi = Instantiate(ui.DamageIndicatorPrefab, ui.DamageIndicators.transform);
        DamageIndicator indicator = dmgUi.GetComponent<DamageIndicator>();
        indicator.damageSourcePosition = damageSourceLocation;
        indicator.ui = ui;
        float alpha = (float)dmg / (float)finalMaxHealthValue;
        alpha = Mathf.Lerp(indicator.MinAlpha, indicator.MaxAlpha, alpha);
        indicator.Indicator.color = new Color(1,0,0,alpha);
        
		if (currentHealth <= 0 && isAlive)
		{
            ragdollForceToAdd = dmg * 2f;
            ragdollForceDirection = this.transform.position - damageSourceLocation;
			currentHealth = 0;
			OnDeath();
            Debug.Log("rpctakedamage");

            isAlive = false;
		}
	}

	public void OnChangeAmmoReservesAmount(int newCurrentAmmoReserves)
    {
        if (!netIdentity.isLocalPlayer)
            return;
        currentAmmoReserves = newCurrentAmmoReserves;
		ui.AmmoAmount.text = currentAmmoReserves.ToString();
	}

	public void OnChangeAmmoInClip(int newCurrentAmmoInClip)
    {
        currentAmmoInClip = newCurrentAmmoInClip;
		ui.AmmoInClip.text = currentAmmoInClip.ToString();
	}

	void OnChangeHealth (int currentHealth)
    {
        ui.HealthBar.sizeDelta = new Vector2(((float)currentHealth / (float)finalMaxHealthValue) * ui.HealthBarBackground.sizeDelta.x, 32);
		ui.HealthText.text = currentHealth.ToString();
	}

    public void OnHealthPickup(float percentAdd)
    {
        int HpToAdd = (int)((percentAdd/100) * finalMaxHealthValue);
        if (HpToAdd + currentHealth > finalMaxHealthValue)
        {
            HpToAdd = (HpToAdd + currentHealth) - finalMaxHealthValue;
        }
        //TakeDamage(photonView.ViewID, -HpToAdd, Damager.DamageTypes.Physical);
    }

	public void SetCurrencyAmount(int newMoney)
    {
        int difference = newMoney - currentCurrency;
        string diftext = difference.ToString();
		currentCurrency = newMoney;
		ui.CurrencyAmount.text = currentCurrency.ToString();
        dmgText.CreateMoneyText(diftext, dmgText.moneyParent);

        if (ui.CurrentUIState == PlayerUIState.ClassSelectMenu)
            ui.ClassMenu.UpdateClassSelectMenu();
	}

    public void AddCurrency(int currencyToAdd)
    {
        SetCurrencyAmount(currentCurrency + currencyToAdd);
    }

    public void OnChangeCurrencyAmountNoUI(int newMoney)
    {
        currentCurrency = newMoney;
        ui.CurrencyAmount.text = currentCurrency.ToString();
    }
	
    public void AddEliteClassLife(WeaponSet eliteClass)
    {
        eliteClassLives[eliteClass.className] += 1;
        ui.ClassSelectMenu.GetComponent<ClassSelectMenu>().UpdateEliteClassSlots();
    }

    public void RemoveEliteClassLife(WeaponSet eliteClass)
    {
        eliteClassLives[eliteClass.className] -= 1;
        ui.ClassSelectMenu.GetComponent<ClassSelectMenu>().UpdateEliteClassSlots();
    }

	public void HideOwnPlayerModel(bool choice)
    {
        if (!netIdentity.isLocalPlayer)
            return;
        if (choice == true)
		{
			bodyToColor.enabled = false;
			jointsToColor.enabled = false;
		}
		else
		{
			bodyToColor.enabled = true;
			jointsToColor.enabled = true;
		}
	}

    //Here because weapon raycasts can hit the player's self. All the hitboxes of everyone are on the same layer so we cant make the raycast ignore a layer.
    void EnableOwnHitbox(bool choice)
    {
        if (!netIdentity.isLocalPlayer)
            return;
        if (choice)
        {
            foreach (PlayerHitbox hb in hitboxCollection)
            {
                hb.GetComponent<Collider>().enabled = true;
            }
        }
        else
        {
            foreach (PlayerHitbox hb in hitboxCollection)
            {
                hb.GetComponent<Collider>().enabled = false;
            }
        }
    }

	public void SetTeam(PlayerTeam team)
    {
        PlayerTeam previousTeam = playerTeam;
        PlayerStats player = this.GetComponent<PlayerStats>();

        switch(previousTeam)
        {
            case PlayerTeam.Blue:
                gameManager.blueTeamPlayers.Remove(player);
                break;
            case PlayerTeam.Red:
                gameManager.redTeamPlayers.Remove(player);
                break;
            case PlayerTeam.Spectator:
                gameManager.spectatorTeamPlayers.Remove(player);
                break;
        }

		playerTeam = team;

        switch(team)
        {
            case PlayerTeam.Blue:
                gameManager.blueTeamPlayers.Add(player);
                break;
            case PlayerTeam.Red:
                gameManager.redTeamPlayers.Add(player);
                break;
            case PlayerTeam.Spectator:
                gameManager.spectatorTeamPlayers.Add(player);
                break;
        }

		HideOwnPlayerModel(false);
		if (bodyToColor.enabled == true && jointsToColor.enabled == true)
		{
			if (team == PlayerTeam.Blue)
			{
				bodyToColor.material = bodyMaterialBlue;
				jointsToColor.material = jointsMaterialBlue;
			}
			if (team == PlayerTeam.Red)
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
        if (!netIdentity.isLocalPlayer)
            return;
        CaptureFlag flag = other.GetComponentInParent<CaptureFlag>();
		if (flag != null && flag.isBeingHeld == false)
		{
			if ((flag.team == CaptureFlag.Team.Blue && playerTeam == PlayerTeam.Red) || (flag.team == CaptureFlag.Team.Red && playerTeam == PlayerTeam.Blue))
			{
				flag.transform.parent = flagAttachParent.transform;
				flag.transform.localPosition = new Vector3(0, 0, 0);
				flag.isBeingHeld = true;
				_flagref = flag;
				hasFlag = true;
			}
		}


		if (gameManager == null)
		{
			gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		}
		if (gameManager != null && other == gameManager.blueCaptureTrigger && hasFlag)
		{
			_flagref.transform.parent = null;
			_flagref.transform.position = _flagref.flagSpawn;
			_flagref.isBeingHeld = false;
			hasFlag = false;
			RpcAddTeamScore(1);
		}
		if (gameManager != null && other == gameManager.redCaptureTrigger && hasFlag)
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
			gameManager.blueTeamFlagCaptures += 1;
			ui.BlueScore.text = gameManager.blueTeamFlagCaptures.ToString();
		}

		if (team == 2)
		{
			gameManager.redTeamFlagCaptures += 1;
			ui.RedScore.text = gameManager.redTeamFlagCaptures.ToString();
		}
	}

    public void SwitchPlayerClass(WeaponSet newClass)
    {
        if (!currentClass.isElite)
            previousClass = currentClass;

        currentClass = newClass;

        if (previousClass == null || previousClass.className == "Spectator")
            previousClass = currentClass;
    }

    public void SetQueuedClass(WeaponSet nextClass)
    {
        ui.spawnAlertTimer = ui.spawnAlertTextDuration;
        ui.StartCoroutine(ui.ShowYouWillSpawnAsText());
        queuedClass = nextClass;
    }

    public void SetQueuedTeam(PlayerTeam nextTeam)
    {
        queuedTeam = nextTeam;
    }
}
