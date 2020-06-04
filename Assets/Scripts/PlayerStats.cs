using KinematicCharacterController;
using KinematicCharacterController.Owly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamagable
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
    public string Username;
    public PlayerTeam playerTeam;
    public int ping = 0;

    [Header("Character Stats")]
	public CharacterStat maxHealth;
	int maxHealthValue;

    [SerializeField]
    private int currentHealth;
    public int CurrentHealth
    {
        get { return currentHealth; }
        private set { currentHealth = value; }
    }

	public bool isAlive;

    [SerializeField]
    private int currentCurrency;
    public int CurrentCurrency
    {
        get { return currentCurrency; }
        private set { currentCurrency = value; }
    }

	public int startingCurrency = 1000;

	int currentAmmoInClip;
	int currentAmmoReserves;
	
	public bool canPickUpFlag;
	public bool hasFlag;
    public bool HasPreviouslyPlayed = false;

    [SerializeField]
    WeaponSet currentClass;

    public WeaponSet CurrentClass
    {
        get { return currentClass;  }
        private set { currentClass = value;  }
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

    public int EliteClassLivesLeft = 0;

    public CharacterStat MovementSpeed;
	float baseMoveSpeed = 6.2f;
    StatModifier ClassMovespeedMultiplier;

    public CharacterStat JumpForce;
	float baseJumpForce = 7.24f;
    StatModifier ClassJumpforceMultiplier;

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
	public GamePhases _gameManager;
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
        CharControl = GetComponent<MyCharacterController>();
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GamePhases>();
        divekickHitbox.gameObject.SetActive(false);

        EnableOwnHitbox(false);

		Physics.IgnoreLayerCollision(10, 12);
        Physics.IgnoreLayerCollision(10, 9);
        Physics.IgnoreLayerCollision(12, 9, true);

        ui.DollarSign.enabled = false;
        ui.RadialReload.StopReload();

		SetTeam(playerTeam);

		currentCurrency = startingCurrency;
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
		RespawnAndInitialize();
	}

	void Update()
	{
	}

	public void ChangeToSpectator()
	{
		for (int i = 0; i < _gameManager.classList.Count; i++)
		{
			if (_gameManager.classList[i].className == "Spectator")
			{
                SetQueuedClass(_gameManager.classList[i]);
                //player first connects, the class is null for some reason
                if (currentClass == null)
                {
                    SwitchPlayerClass(_gameManager.classList[i]);
                }
			}
		}
		GetComponent<MyCharacterController>().TransitionToState(CharacterState.Spectator);
        SetCollidersActive(false);
        RespawnAndInitialize();
	}

    public void AddMaxHealthModifier(StatModifier mod)
    {
        maxHealth.AddModifier(mod);
        OnChangeHealth(CurrentHealth);

    }

    public void RemoveMaxHealthModifier(StatModifier mod)
    {
        maxHealth.RemoveModifier(mod);
        OnChangeHealth(CurrentHealth);
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
			maxHealthValue = Mathf.RoundToInt(maxHealth.Value);
			currentHealth = maxHealthValue;
			ui.DollarSign.enabled = true;
			ui.AmmoInClip.text = "";
			ui.AmmoAmount.text = "";
			ui.HealthText.text = currentHealth.ToString();
            ui.RadialReload.StopReload();
			OnChangeHealth(currentHealth);
			ui.CurrencyAmount.text = currentCurrency.ToString();

			StatusEffectManager.AddPassiveStatusEffects();

            MovementSpeed.BaseValue = baseMoveSpeed;
            ClassMovespeedMultiplier = new StatModifier(currentClass.moveSpeedPercentAdd, StatModType.PercentAdd);
            MovementSpeed.RemoveAllModifiersFromSource(this);
            MovementSpeed.AddModifier(ClassMovespeedMultiplier);
            CharControl.MaxStableMoveSpeed = MovementSpeed.Value;

            JumpForce.BaseValue = baseJumpForce;
            ClassJumpforceMultiplier = new StatModifier(currentClass.jumpHeightPercentAdd, StatModType.PercentAdd);
            JumpForce.RemoveAllModifiersFromSource(this);
            JumpForce.AddModifier(ClassJumpforceMultiplier);
            CharControl.JumpSpeed = JumpForce.Value;
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
            EliteClassLivesLeft = EliteClassLivesLeft - 1;

        Debug.Log("player died");
        SpawnRagdoll(hitboxCollection);
		isAlive = false;
		_respawnTimer = _gameManager.respawnTime;
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
			this.GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
			GetComponent<PlayerInput>().playerWeapons.DecactivateAllWeapons();
		}
	}

    public void SetCollidersActive(bool choice)
    {
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

	public void RespawnAndInitialize()
	{
        //Move player to a spawnpoint
		if (_gameManager == null)
		{
			_gameManager = GameObject.Find("Game Manager").GetComponent<GamePhases>();
		}

		if (playerTeam == PlayerTeam.Blue)
		{
			Vector3 spawnPoint;
			spawnPoint = _gameManager.teamOneSpawnPoints[Random.Range(0, _gameManager.teamOneSpawnPoints.Length)].transform.position;
			GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint);
		}
		if (playerTeam == PlayerTeam.Red)
		{
			Vector3 spawnPoint;
			spawnPoint = _gameManager.teamTwoSpawnPoints[Random.Range(0, _gameManager.teamTwoSpawnPoints.Length)].transform.position;

			GetComponent<KinematicCharacterMotor>().SetPosition(spawnPoint);
		}


		isAlive = true;
        SetCollidersActive(true);
		canPickUpFlag = true;
		ui.SetActivateDeathUI(false);
        ui.SetActiveMainHud(true);

        SwitchPlayerClass(queuedClass);

		GetComponent<PlayerInput>().playerWeapons.InitializeWeapons();
		if (currentClass.className != "Spectator")
		{
			GetComponent<MyCharacterController>().TransitionToState(currentClass.defaultState);
		}
		InitializeValues();

	}

	public IEnumerator RespawnCountdownTimer()
	{
		ui.RespawnTimer.text = _respawnTimer.ToString();
		while (_respawnTimer > 0)
		{
			yield return new WaitForSeconds(1.0f);
			_respawnTimer = _respawnTimer - 1f;
			ui.RespawnTimer.text = _respawnTimer.ToString();
		}
		if (_respawnTimer <= 0)
		{
			RespawnAndInitialize();
		}

	}

	public void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation, DamageIndicatorType uiType)
	{
		if (isAlive)
		{
			if (currentClass.name == "Spectator")
				return;
            RpcTakeDamage(damageTaken, damageType, giver, damageSourceLocation);
		}
	}

    public void TakeHeal(int healTaken, Damager.DamageTypes damageType)
    {

    }

    void RpcTakeDamage(int dmg, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation)
	{
		dmg = StatusEffectManager.OnBeforeDamageTaken(dmg);
		currentHealth = currentHealth - dmg;
		OnChangeHealth(currentHealth);

        if (giver != null)
		    giver.dmgText.CreateFloatingText(dmg.ToString(), this.transform);

        GameObject dmgUi = Instantiate(ui.DamageIndicatorPrefab, ui.DamageIndicators.transform);
        DamageIndicator indicator = dmgUi.GetComponent<DamageIndicator>();
        indicator.damageSourcePosition = damageSourceLocation;
        indicator.ui = ui;
        float alpha = (float)dmg / (float)maxHealthValue;
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
		ui.HealthBar.sizeDelta = new Vector2(((float)currentHealth / (float)maxHealthValue) * ui.HealthBarBackground.sizeDelta.x, 32);
		ui.HealthText.text = currentHealth.ToString();
	}

    public void OnHealthPickup(float percentAdd)
    {
        int HpToAdd = (int)((percentAdd/100) * maxHealthValue);
        if (HpToAdd + CurrentHealth > maxHealthValue)
        {
            HpToAdd = (HpToAdd + CurrentHealth) - maxHealthValue;
        }
        //TakeDamage(photonView.ViewID, -HpToAdd, Damager.DamageTypes.Physical);
    }

	public void OnChangeCurrencyAmount(int newMoney)
	{
        int difference = newMoney - currentCurrency;
        string diftext = difference.ToString();
		currentCurrency = newMoney;
		ui.CurrencyAmount.text = currentCurrency.ToString();
        dmgText.CreateMoneyText(diftext, dmgText.moneyParent);
	}

    public void OnChangeCurrencyAmountNoUI(int newMoney)
    {
        currentCurrency = newMoney;
        ui.CurrencyAmount.text = currentCurrency.ToString();
    }
	
	public void HideOwnPlayerModel(bool choice)
	{
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
		HideOwnPlayerModel(false);
		playerTeam = team;
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
			_gameManager.blueTeamFlagCaptures += 1;
			ui.BlueScore.text = _gameManager.blueTeamFlagCaptures.ToString();
		}

		if (team == 2)
		{
			_gameManager.redTeamFlagCaptures += 1;
			ui.RedScore.text = _gameManager.redTeamFlagCaptures.ToString();
		}
	}

    public void SwitchPlayerClass(WeaponSet newClass)
    {
        currentClass = newClass;
        previousClass = currentClass;
    }

    public void SetQueuedClass(WeaponSet nextClass)
    {
        queuedClass = nextClass;
    }
}
