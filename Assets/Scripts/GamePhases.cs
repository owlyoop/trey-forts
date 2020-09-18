using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GamePhases : NetworkBehaviour
{
    public GameData gameData;

    [Header("Game Rules")]
    public float waitingForPlayersLength = 30f;
	public float buildPhaseFreezeTime = 3f; //after waiting for players end, teleport players to spawnpoints and freeze their movement
	public float buildPhaseTimeLength = 180f; //length in seconds
	public float combatPhaseTimeLength = 600f; // length in seconds
    public int buildPhaseStartingMoney = 1000;
    public enum GameState { WaitingForPlayers, BuildPhase, CombatPhase, Overtime, EndGamePhase }

    public GameState currentGameState;

	public float respawnTime = 8.0f;

    [Header("Current Game Info")]
    public int blueTeamFlagCaptures;
	public int redTeamFlagCaptures;

    public List<PlayerStats> blueTeamPlayers = new List<PlayerStats>();
    public List<PlayerStats> redTeamPlayers = new List<PlayerStats>();
    public List<PlayerStats> spectatorTeamPlayers = new List<PlayerStats>();

    public int blueTeamScore;
    public int redTeamScore;
    public int blueTeamKills;
    public int redTeamKills;

    [Header("Map Elements")]
    public GameObject BuildPhaseGeometry;
    public List<Spawnpoint> teamSpectatorSpawnPoints = new List<Spawnpoint>();
	public List<Spawnpoint> teamBlueSpawnPoints = new List<Spawnpoint>();
    public List<Spawnpoint> teamRedSpawnPoints = new List<Spawnpoint>();
    public CaptureFlagZone blueCaptureZone;
    public CaptureFlagZone redCaptureZone;
	public Collider blueCaptureTrigger;
	public Collider redCaptureTrigger;
	public CaptureFlag blueFlag;
	public CaptureFlag redFlag;

	EventManager _eventManager;

	private void Start()
	{
		_eventManager = GetComponent<EventManager>();
        currentGameState = GameState.WaitingForPlayers;
		blueTeamFlagCaptures = 0;
		redTeamFlagCaptures = 0;
    }

	private void Update()
	{
		if (currentGameState == GameState.WaitingForPlayers)
		{
			waitingForPlayersLength -= Time.deltaTime;
			if (waitingForPlayersLength < 0)
			{
				EventManager.RaiseOnWaitingForPlayersEnd();
				BeginBuildPhase();
			}
		}
		if (currentGameState == GameState.BuildPhase)
		{
			buildPhaseTimeLength -= Time.deltaTime;
			if (buildPhaseTimeLength < 0)
			{
				EndBuildPhase();
				BeginCombatPhase();
			}
		}
		if (currentGameState == GameState.CombatPhase)
		{
			combatPhaseTimeLength -= Time.deltaTime;
			if (combatPhaseTimeLength < 0)
			{
				EndCombatPhase();
			}
		}
	}

	public void BeginBuildPhase()
	{
        currentGameState = GameState.BuildPhase;
		EventManager.RaiseOnBuildPhaseStart();
	}

	public void EndBuildPhase()
	{
		EventManager.RaiseOnBuildPhaseEnd();
		BuildPhaseGeometry.SetActive(false);
        currentGameState = GameState.CombatPhase;
	}

	public void BeginCombatPhase()
	{
		EventManager.RaiseOnCombatPhaseStart();
        foreach(PlayerStats player in blueTeamPlayers)
        {
            player.SetCurrencyAmount(500);
        }

        foreach (PlayerStats player in redTeamPlayers)
        {
            player.SetCurrencyAmount(500);
        }
    }

	public void EndCombatPhase()
	{
		EventManager.RaiseOnCombatPhaseEnd();
	}

    

}
