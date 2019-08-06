using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using KinematicCharacterController.Examples;
using System;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    [DefaultExecutionOrder(-1000)]
    public class MyGameManager : MonoBehaviour, ISimulationController<MyWorldSnapshot>
    {
        public class ConnectionInfo
        {
            public int ConnectionId = -1;
            public List<int> PlayerControllerIds = new List<int>();
            public bool IsReady = false;
        }

        public Transform InitialSpawnPoint;
        public KinematicCharacterSubsystem KinematicCharacterSubsystem;
        public MyBotController BotController;

        [NonSerialized]
        public SimulationSystem<MyPlayerController, MyPlayerCommands, MyWorldSnapshot> SimulationSystem;
        [NonSerialized]
        public List<ConnectionInfo> ConnectionInfos = new List<ConnectionInfo>();

        private const int MaxLocalPlayers = 1;
        private const int MaxPlayers = 100;
        private const int MaxCharacters = 100;
        
        private void Awake()
        {
            GameStatics.GameManager = this;

            InitializeSubsystems();

            // Create the simulation system
            int snapshotsCapacity = Mathf.CeilToInt((0.001f * (float)GameStatics.OnlineSession.ConnectionConfig.DisconnectTimeout) / Time.fixedDeltaTime);
            SimulationSystem = new SimulationSystem<MyPlayerController, MyPlayerCommands, MyWorldSnapshot>();
            SimulationSystem.Initialize(
                this,
                GameStatics.OnlineSession,
                MaxLocalPlayers,
                MaxPlayers,
                Mathf.RoundToInt(MaxPlayers * 1.5f),
                snapshotsCapacity);
            
            // Build ConnectionInfos
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server)
            {
                foreach (ClientConnection conn in GameStatics.OnlineSession.ClientConnections)
                {
                    ConnectionInfo ci = new ConnectionInfo();
                    ci.ConnectionId = conn.ConnectionId;
                    ConnectionInfos.Add(ci);
                }
            }

            // If client, notify server the scene is loaded
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                SceneLoadingMsg lsm = new SceneLoadingMsg();
                lsm.SceneName = SceneManager.GetActiveScene().name;
                lsm.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToServer(OnlineSession.ReliableSequencedChannelId);
            }

            // If server with no connections, initiate game now
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server && ConnectionInfos.Count <= 0)
            {
                OnReadyToInitiateGame();
            }
        }

        public void InitializeSubsystems()
        {
            KinematicCharacterSubsystem.Initialize(MaxCharacters);

            foreach (MyCharacterController c in BotController.Characters)
            {
                KinematicCharacterSubsystem.RegisterCharacter(c, true);
            }
        }

        private void FixedUpdate()
        {
            KinematicCharacterSubsystem.OnPreTick(Time.fixedDeltaTime);

            SimulationSystem.OnTick(Time.fixedDeltaTime);

            KinematicCharacterSubsystem.OnPostTick(Time.fixedDeltaTime);
        }

        private void OnReadyToInitiateGame()
        {
            int characterCounter = 0;

            // Create and spawn local players & characters
            {
                // Player controller
                MyPlayerController newPlayer = Instantiate(GameStatics.GameData.PlayerPrefab);
                newPlayer.OrbitCamera = Instantiate<OrbitCamera>(GameStatics.GameData.CameraPrefab);
                SimulationSystem.RegisterPlayerController(newPlayer, -1);
                if (GameStatics.OnlineSession.IsOnline())
                {
                    NetworkSpawnPlayer(newPlayer, newPlayer.GetId(), -1);
                }

                // Character
                MyCharacterController newCharacter = Instantiate(GameStatics.GameData.CharacterPrefab);
                newPlayer.OrbitCamera.SetFollowTransform(newCharacter.CameraFollowPoint);
                newPlayer.OrbitCamera.IgnoredColliders = newCharacter.GetComponentsInChildren<Collider>();
                PlaceCharacterAtSpawnPointIndex(newCharacter, characterCounter);
                KinematicCharacterSubsystem.RegisterCharacter(newCharacter, true);
                if (GameStatics.OnlineSession.IsOnline())
                {
                    NetworkSpawnCharacter(newCharacter, newCharacter.GetId(), newPlayer.GetId());
                }

                // Assign character to player
                newPlayer.Character = newCharacter;
                newCharacter.OwningPlayer = newPlayer;

                characterCounter++;
            }

            // Create and spawn client players & characters
            foreach (ConnectionInfo ci in ConnectionInfos)
            {
                // Player controller
                MyPlayerController newPlayer = Instantiate(GameStatics.GameData.PlayerPrefab);
                SimulationSystem.RegisterPlayerController(newPlayer, ci.ConnectionId);
                if (GameStatics.OnlineSession.IsOnline())
                {
                    NetworkSpawnPlayer(newPlayer, newPlayer.GetId(), ci.ConnectionId);
                }

                // Register as player for that connection
                ci.PlayerControllerIds.Add(newPlayer.GetId());

                // Character
                MyCharacterController newCharacter = Instantiate(GameStatics.GameData.CharacterPrefab);
                PlaceCharacterAtSpawnPointIndex(newCharacter, characterCounter);
                KinematicCharacterSubsystem.RegisterCharacter(newCharacter, false);
                if (GameStatics.OnlineSession.IsOnline())
                {
                    NetworkSpawnCharacter(newCharacter, newCharacter.GetId(), newPlayer.GetId());
                }

                // Assign character to player
                newPlayer.Character = newCharacter;
                newCharacter.OwningPlayer = newPlayer;

                characterCounter++;
            }

            // Tell clients to initiate game
            if (GameStatics.OnlineSession.IsOnline())
            {
                SimpleEventMsg sem = new SimpleEventMsg();
                sem.Event = SimpleEventMsg.EventType.InitiateGame;
                sem.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToAllClients(OnlineSession.ReliableSequencedChannelId);
            }
        }

        public void PlaceCharacterAtSpawnPointIndex(MyCharacterController character, int spawnPointIndex)
        {
            Vector3 spawnPos = InitialSpawnPoint.position + (Vector3.right * spawnPointIndex * 2f);
            Quaternion spawnRot = InitialSpawnPoint.rotation;
            character.Motor.SetPositionAndRotation(spawnPos, spawnRot);
        }

        #region Networking Events
        public void OnConnect(int connectionId)
        {
            // Don't allow joining mid-game
            GameStatics.OnlineSession.Disconnect(connectionId);
        }

        public void OnDisconnect(int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server)
            {
                for(int i = ConnectionInfos.Count - 1; i >= 0; i--)
                {
                    if(ConnectionInfos[i].ConnectionId == connectionId)
                    {
                        // Unregister and Destroy associated players and characters
                        foreach(int playerId in ConnectionInfos[i].PlayerControllerIds)
                        {
                            MyPlayerController p = SimulationSystem.GetPlayerControllerWithId(playerId);
                            if(p != null)
                            {
                                KinematicCharacterSubsystem.UnregisterCharacter(p.Character.GetId());
                                Destroy(p.Character.gameObject);

                                SimulationSystem.UnregisterPlayerController(p.GetId());
                                Destroy(p.gameObject);
                            }
                        }

                        // Remove connection info
                        ConnectionInfos.RemoveAt(i);
                        break;
                    }
                }
            }
            else if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                SceneManager.LoadScene(GameStatics.GameData.LobbySceneName);
            }
        }

        public void OnData(int connectionId, int channelId, int receiveSize, int msgId)
        {
            NetworkMessageID iMsgId = (NetworkMessageID)msgId;
            switch (iMsgId)
            {
                case (NetworkMessageID.SceneLoading):
                    SceneLoadingMsg slm = new SceneLoadingMsg();
                    slm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleLoadSceneMsg(ref slm, connectionId);
                    break;
                case (NetworkMessageID.TickSync):
                    TickSyncMsg tsm = new TickSyncMsg();
                    tsm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleTickSyncMsg(ref tsm, connectionId);
                    break;
                case (NetworkMessageID.SimpleEvent):
                    SimpleEventMsg sem = new SimpleEventMsg();
                    sem.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleSimpleEventMsg(connectionId, sem.Event);
                    break;
                case (NetworkMessageID.SpawnPlayer):
                    SpawnPlayerMsg spm = new SpawnPlayerMsg();
                    spm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleSpawnPlayerMsg(ref spm, connectionId);
                    break;
                case (NetworkMessageID.SpawnCharacter):
                    SpawnCharacterMsg scm = new SpawnCharacterMsg();
                    scm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleSpawnCharacterMsg(ref scm);
                    break;
                case (NetworkMessageID.LazerHit):
                    LazerFireMsg lhm = new LazerFireMsg();
                    lhm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleLazerFireMsg(ref lhm);
                    break;
            }
        }

        private void HandleLoadSceneMsg(ref SceneLoadingMsg msg, int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server)
            {
                // Confirm client is in same scene, and send a synchronization message
                if (msg.SceneName == SceneManager.GetActiveScene().name)
                {
                    TickSyncMsg tsm = new TickSyncMsg();
                    tsm.Tick = SimulationSystem.PresentTick;
                    tsm.Timestamp = GameStatics.OnlineSession.GetTimestamp();
                    tsm.Serialize(GameStatics.OnlineSession.NetBuffer);
                    GameStatics.OnlineSession.SendBufferToConnection(connectionId, OnlineSession.ReliableSequencedChannelId);
                }
            }
            else if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                SceneManager.LoadScene(msg.SceneName);
            }
        }

        public void HandleTickSyncMsg(ref TickSyncMsg msg, int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                // Synchronize tick
                float delayTime = GameStatics.OnlineSession.GetRemoteDelayTimeMS(connectionId, msg.Timestamp) * 0.001f;
                int ticksFromNetworkDelay = Mathf.FloorToInt(delayTime / Time.fixedDeltaTime);

                // We want to be one half RTT ahead of server in terms of ticks
                SimulationSystem.InitializeAtTick(msg.Tick + ticksFromNetworkDelay + ticksFromNetworkDelay);

                // Send a msg to signal ready
                SimpleEventMsg sem = new SimpleEventMsg();
                sem.Event = SimpleEventMsg.EventType.TickSynced;
                sem.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToServer(OnlineSession.ReliableSequencedChannelId);
            }
        }

        public void HandleSimpleEventMsg(int connectionId, SimpleEventMsg.EventType eventType)
        {
            switch(eventType)
            {
                case SimpleEventMsg.EventType.TickSynced:
                    if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server)
                    {
                        // Update ready connections
                        bool everyoneIsReady = true;
                        for (int i = 0; i < ConnectionInfos.Count; i++)
                        {
                            if (ConnectionInfos[i].ConnectionId == connectionId)
                            {
                                ConnectionInfos[i].IsReady = true;
                            }

                            if (!ConnectionInfos[i].IsReady)
                            {
                                everyoneIsReady = false;
                            }
                        }

                        // See if we're ready to initiate game
                        if (everyoneIsReady)
                        {
                            OnReadyToInitiateGame();
                        }
                    }
                    break;
                case SimpleEventMsg.EventType.InitiateGame:
                    break;
            }
        }

        public void HandleSpawnPlayerMsg(ref SpawnPlayerMsg msg, int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                MyPlayerController spawnedPlayer = Instantiate(GameStatics.GameData.PlayerPrefab);
                SimulationSystem.RegisterPlayerControllerAtId(spawnedPlayer, msg.PlayerId, msg.IsLocalPlayer ? -1 : connectionId);

                // Handle local player camera
                if (msg.IsLocalPlayer)
                {
                    spawnedPlayer.OrbitCamera = Instantiate<OrbitCamera>(GameStatics.GameData.CameraPrefab);
                    spawnedPlayer.OrbitCamera.gameObject.tag = "MainCamera"; 
                }
            }
        }

        public void HandleSpawnCharacterMsg(ref SpawnCharacterMsg msg)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                MyCharacterController spawnedCharacter = Instantiate(GameStatics.GameData.CharacterPrefab);

                // Assign to player
                SimulationSystem.GetPlayerControllerWithId(msg.ForPlayerId).Character = spawnedCharacter;

                // if this is a local player's character, assign it to the local player
                bool hasOwnership = false;
                MyPlayerController forPlayer;
                if (SimulationSystem.RegisteredPlayerControllers.TryGetValue(msg.ForPlayerId, out forPlayer))
                {
                    if (forPlayer.GetIsLocal())
                    {
                        hasOwnership = true;
                        forPlayer.OrbitCamera.SetFollowTransform(spawnedCharacter.CameraFollowPoint);
                        forPlayer.OrbitCamera.IgnoredColliders = spawnedCharacter.GetComponentsInChildren<Collider>();
                    }
                }

                KinematicCharacterSubsystem.RegisterCharacterAtId(spawnedCharacter, msg.CharacterId, hasOwnership);
                spawnedCharacter.Motor.SetPositionAndRotation(msg.SpawnPosition, msg.SpawnRotation, true);
            }
        }

        public void HandleLazerFireMsg(ref LazerFireMsg msg)
        {
            // Show other players firing
            MyPlayerController firingPlayer;
            if (SimulationSystem.RegisteredPlayerControllers.TryGetValue(msg.FiringPlayerId, out firingPlayer))
            {
                if (!firingPlayer.GetIsLocal())
                {
                    firingPlayer.Character.LazerGun.Fire(msg.FiringRotation);
                }
            }

            // Handle real hit logic
            if (msg.HitCharacterId >= 0)
            {
                MyCharacterController hitCC;
                if (KinematicCharacterSubsystem.Characters.TryGetValue(msg.HitCharacterId, out hitCC))
                {
                    hitCC.OnHitFX();
                }
            }
        }

        public void NetworkSpawnPlayer(MyPlayerController spawnedPlayer, int spawnedPlayerId, int playerConnectionId)
        {
            // Player spawn msg
            SpawnPlayerMsg spm = new SpawnPlayerMsg();
            spm.PlayerId = spawnedPlayerId; 
            if (playerConnectionId >= 0)
            {
                // For that player's connection, spawn as local player
                spm.IsLocalPlayer = true;
                spm.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToConnection(playerConnectionId, OnlineSession.ReliableSequencedChannelId);

                // Otherwise, spawn as not local
                spm.IsLocalPlayer = false;
                spm.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToAllClientsExcept(playerConnectionId, OnlineSession.ReliableSequencedChannelId);
            }
            else
            {
                // Server's player
                spm.IsLocalPlayer = false;
                spm.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToAllClients(OnlineSession.ReliableSequencedChannelId);
            }
        }

        public void NetworkSpawnCharacter(MyCharacterController spawnedCharacter, int spawnedCharacterId, int forPlayerId)
        {
            // Character spawn msg
            SpawnCharacterMsg scm = new SpawnCharacterMsg();
            scm.CharacterId = spawnedCharacterId;
            scm.ForPlayerId = forPlayerId;
            scm.SpawnPosition = spawnedCharacter.transform.position;
            scm.SpawnRotation = spawnedCharacter.transform.rotation;
            scm.Serialize(GameStatics.OnlineSession.NetBuffer);
            GameStatics.OnlineSession.SendBufferToAllClients(OnlineSession.ReliableSequencedChannelId);
        }

        public ConnectionInfo GetConnectionInfoForConnectionId(int connectionId)
        {
            for(int i = 0; i < ConnectionInfos.Count; i++)
            {
                if(ConnectionInfos[i].ConnectionId == connectionId)
                {
                    return ConnectionInfos[i];
                }
            }

            return null;
        }
        #endregion

        #region Simulation Events
        public void SimulateStep(float deltaTime)
        {
            BotController.OnSimulate(deltaTime);
            KinematicCharacterSubsystem.OnSimulateStep(deltaTime);
        }

        public void InitializeSnapshot(ref MyWorldSnapshot worldSnapshot)
        {
            KinematicCharacterSubsystem.InitializeSnapshot(ref worldSnapshot.KinematicCharacterSubsystemState, MaxCharacters);
        }

        public void SaveStateToSnapshot(ref MyWorldSnapshot worldSnapshot)
        {
            KinematicCharacterSubsystem.SaveToSnapshot(ref worldSnapshot.KinematicCharacterSubsystemState);
        }

        public void RestoreStateFromSnapshot(ref MyWorldSnapshot worldSnapshot)
        {
            KinematicCharacterSubsystem.RestoreStateFromSnapshot(ref worldSnapshot.KinematicCharacterSubsystemState);
        }

        public void OnPreSnapshot()
        {
            KinematicCharacterSubsystem.HandlePreSnapshotInterpolation();
        }

        public void OnPostSnapshot()
        {
            KinematicCharacterSubsystem.HandlePostSnapshotInterpolation();
        }
        #endregion
    }
}