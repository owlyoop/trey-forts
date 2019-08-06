using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GamePhases : NetworkBehaviour
{
	public NetworkManager network;

	[SyncVar]
	public float WaitingForPlayersLength = 30f;
	[SyncVar]
	public bool isWaitingForPlayers;


	[SyncVar]
	public float BuildPhaseFreezeTime = 3f; //after waiting for players end, teleport players to spawnpoints and freeze their movement
	[SyncVar]
	public float BuildPhaseTimeLength = 180f; //length in seconds
	[SyncVar]
	public bool isInBuildPhase;

	[SyncVar]
	public float CombatPhaseTimeLength = 600f; // length in seconds
	[SyncVar]
	public bool isInCombatPhase;

	[SyncVar]
	public bool isInEndGamePhase;

	[SyncVar]
	public bool isMasterTimer = false;

	GamePhases serverTimer;

	public GameObject BuildPhaseGeometry;

	public Spawnpoint[] teamSpectatorSpawnPoints;
	public Spawnpoint[] teamOneSpawnPoints;
	public Spawnpoint[] teamTwoSpawnPoints;

	public Collider blueCaptureZone;
	public Collider redCaptureZone;

	public CaptureFlag blueFlag;
	public CaptureFlag redFlag;

	private void Start()
	{
		if (isServer)
		{
			isMasterTimer = true;
			isWaitingForPlayers = true;
		}
		else if (isLocalPlayer)
		{
			GamePhases[] timers = FindObjectsOfType<GamePhases>();
			for (int i = 0; i < timers.Length; i++)
			{
				if (timers[i].isMasterTimer)
				{
					serverTimer = timers[i];
				}
			}
		}
	}

	private void Update()
	{

		if (isMasterTimer)
		{
			if (isWaitingForPlayers)
			{
				WaitingForPlayersLength -= Time.deltaTime;
				if (WaitingForPlayersLength < 0)
				{
					BeginBuildPhase();
				}
			}

			if (isInBuildPhase)
			{
				BuildPhaseTimeLength -= Time.deltaTime;
				if (BuildPhaseTimeLength < 0)
				{
					EndBuildPhase();
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
		
	}

	public void BeginBuildPhase()
	{
		isWaitingForPlayers = false;
		isInBuildPhase = true;
	}

	public void EndBuildPhase()
	{
		BuildPhaseGeometry.SetActive(false);
		isInBuildPhase = false;
		isInCombatPhase = true;
	}

	public void EndCombatPhase()
	{

	}
}
