using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GamePhases : NetworkBehaviour
{
    [Header("Game Rules")]
    public float WaitingForPlayersLength = 30f;
	public bool isWaitingForPlayers;
	public float BuildPhaseFreezeTime = 3f; //after waiting for players end, teleport players to spawnpoints and freeze their movement
	public float BuildPhaseTimeLength = 180f; //length in seconds
	public bool isInBuildPhase;
	public float CombatPhaseTimeLength = 600f; // length in seconds
	public bool isInCombatPhase;
	public bool isInEndGamePhase;
	public float respawnTime = 8.0f;
    public List<WeaponSet> classList = new List<WeaponSet>();
    public List<FortwarsPropData> buildPhaseProps = new List<FortwarsPropData>();
    public List<FortwarsPropData> combatPhaseProps = new List<FortwarsPropData>();

    [Header("Current Game Info")]
    public int blueTeamFlagCaptures;
	public int redTeamFlagCaptures;

    public List<PlayerStats> blueTeamPlayers = new List<PlayerStats>();
    public List<PlayerStats> redTeamPlayers = new List<PlayerStats>();
    public List<PlayerStats> spectatorTeamPlayers = new List<PlayerStats>();

    public int blueTeamKills;
    public int blueTeamScore;
    public int redTeamKills;
    public int redTeamScore;

    [Header("Map Elements")]
    public GameObject BuildPhaseGeometry;
	public Spawnpoint[] teamSpectatorSpawnPoints;
	public Spawnpoint[] teamOneSpawnPoints;
	public Spawnpoint[] teamTwoSpawnPoints;
	public Collider blueCaptureZone;
	public Collider redCaptureZone;
	public CaptureFlag blueFlag;
	public CaptureFlag redFlag;

	EventManager _eventManager;



	private void Start()
	{
		_eventManager = GetComponent<EventManager>();
		isWaitingForPlayers = true;
		blueTeamFlagCaptures = 0;
		redTeamFlagCaptures = 0;
    }

	private void Update()
	{
		if (isWaitingForPlayers)
		{
			WaitingForPlayersLength -= Time.deltaTime;
			if (WaitingForPlayersLength < 0)
			{
				EventManager.RaiseOnWaitingForPlayersEnd();
				BeginBuildPhase();
			}
		}
		if (isInBuildPhase)
		{
			BuildPhaseTimeLength -= Time.deltaTime;
			if (BuildPhaseTimeLength < 0)
			{
				EndBuildPhase();
				BeginCombatPhase();
			}
		}
		if (isInCombatPhase)
		{
			CombatPhaseTimeLength -= Time.deltaTime;
			if (CombatPhaseTimeLength < 0)
			{
				EndCombatPhase();
			}
		}
	}



	public void BeginBuildPhase()
	{
		isWaitingForPlayers = false;
		isInBuildPhase = true;
		EventManager.RaiseOnBuildPhaseStart();
		
	}

	public void EndBuildPhase()
	{
		EventManager.RaiseOnBuildPhaseEnd();
		BuildPhaseGeometry.SetActive(false);
		isInBuildPhase = false;
		isInCombatPhase = true;
	}

	public void EndCombatPhase()
	{
		EventManager.RaiseOnCombatPhaseEnd();
	}

	public void BeginCombatPhase()
	{
		EventManager.RaiseOnCombatPhaseStart();
	}
}
