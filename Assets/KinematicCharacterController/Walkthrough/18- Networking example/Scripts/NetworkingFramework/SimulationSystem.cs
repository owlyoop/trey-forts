using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{

    public struct CommandsSnapshot<CMD> where CMD : struct, IPlayerCommands
    {
        public List<IdentifiedData<CMD>> PlayerCommands;

        public void Initialize(int playersCapacity)
        {
            PlayerCommands = new List<IdentifiedData<CMD>>(playersCapacity);
        }
    }

    public struct GlobalSnapshot<CMD, WS> 
        where CMD : struct, IPlayerCommands
        where WS : struct, IWorldSnapshot<WS>
    {
        public List<ISimulationEvent> SimulationEvents;
        public CommandsSnapshot<CMD> CommandsSnapshot;
        public WS WorldSnapshot;
    }

    public struct IdentifiedData<T> where T : struct
    {
        public int DataId;
        public T DataStruct;

        public IdentifiedData(int dataId, T dataStruct)
        {
            DataId = dataId;
            DataStruct = dataStruct;
        }
    }

    public struct TimestampedData<T> where T : struct
    {
        public int DataTimestamp;
        public T DataStruct;

        public TimestampedData(int dataTimestamp, T dataStruct)
        {
            DataTimestamp = dataTimestamp;
            DataStruct = dataStruct;
        }
    }

    public struct TimestampedIdentifiedData<T> where T : struct
    {
        public int DataId;
        public int DataTimestamp;
        public T DataStruct;

        public TimestampedIdentifiedData(int dataId, int dataTimestamp, T dataStruct)
        {
            DataId = dataId;
            DataTimestamp = dataTimestamp;
            DataStruct = dataStruct;
        }
    }

    public struct TimestampedIdentifiedConnectionData<T> where T : struct
    {
        public int DataId;
        public int DataTimestamp;
        public int ConnectionId;
        public T DataStruct;
        
        public TimestampedIdentifiedConnectionData(int dataId, int dataTimestamp, int connectionId, T dataStruct)
        {
            DataId = dataId;
            DataTimestamp = dataTimestamp;
            ConnectionId = connectionId;
            DataStruct = dataStruct;
        }
    }

    public class SimulationSystem<PC, CMD, WS> : INetworkEventHandler
        where PC : class, IPlayerController<CMD>
        where CMD : struct, IPlayerCommands
        where WS : struct, IWorldSnapshot<WS>
    {
        public OnlineSession OnlineSession;
        
        public int PresentTick;
        public int SimulationTick;
        public int EarliestTickIndex;
        public int PresentTickIndex;
        public int SnapshotsCount;
        public int SnapshotsCapacity;
        public GlobalSnapshot<CMD, WS>[] SnapshotsRingBuffer;

        public Dictionary<int, PC> LocalPlayerControllers = new Dictionary<int, PC>();
        public Dictionary<int, PC> RegisteredPlayerControllers;

        public List<TimestampedIdentifiedConnectionData<CMD>> ReceivedPlayerCommandsBuffer;

        private int _playerIdCounter = 0;
        private int _tickOfLatestReceivedGlobalSnapshot = -1;
        private ISimulationController<WS> _simulationController;
        private List<int> _tmpPlayerIdsToApplyInputEvents = new List<int>(100);
        private Dictionary<int, int> _tmpConnectionsToUpdateBuffer = new Dictionary<int, int>();
        private Dictionary<int, int> _lastReceivedCommandTick = new Dictionary<int, int>();

        public void Initialize(
            ISimulationController<WS> simulationController,
            OnlineSession onlineSession,
            int localPlayerControllersCapacity,
            int totalPlayerControllersCapacity,
            int initialReceivedPlayerCommandsCapacity,
            int snapshotsRingBufferCapacity)
        {
            _simulationController = simulationController;
            OnlineSession = onlineSession;
            LocalPlayerControllers = new Dictionary<int, PC>(localPlayerControllersCapacity);
            RegisteredPlayerControllers = new Dictionary<int, PC>(totalPlayerControllersCapacity);
            ReceivedPlayerCommandsBuffer = new List<TimestampedIdentifiedConnectionData<CMD>>(initialReceivedPlayerCommandsCapacity);

            OnlineSession.NetworkEventHandler = this;

            // Initialize snapshots
            SnapshotsCapacity = snapshotsRingBufferCapacity;
            SnapshotsRingBuffer = new GlobalSnapshot<CMD, WS>[SnapshotsCapacity];
            for (int i = 0; i < SnapshotsCapacity; i++)
            {
                SnapshotsRingBuffer[i].SimulationEvents = new List<ISimulationEvent>();
                SnapshotsRingBuffer[i].CommandsSnapshot.Initialize(totalPlayerControllersCapacity);
                _simulationController.InitializeSnapshot(ref SnapshotsRingBuffer[i].WorldSnapshot);
            }
            
            _tmpConnectionsToUpdateBuffer = new Dictionary<int, int>(totalPlayerControllersCapacity);

            InitializeAtTick(0);
        }

        public PC GetPlayerControllerWithId(int id)
        {
            PC p;
            if (RegisteredPlayerControllers.TryGetValue(id, out p))
            {
                return p;
            }

            return null;
        }

        public int RegisterPlayerController(PC playerController, int forConnection = -1)
        {
            _playerIdCounter++;

            RegisterPlayerControllerAtId(playerController, _playerIdCounter, forConnection);

            return _playerIdCounter;
        }

        public void RegisterPlayerControllerAtId(PC playerController, int atId, int forConnection = -1)
        {
            bool isLocal = (forConnection >= 0) ? false : true;

            playerController.SetId(atId);
            playerController.SetIsLocal(isLocal);
            playerController.SetConnectionId(forConnection);

            RegisteredPlayerControllers[atId] = playerController;

            if (isLocal)
            {
                LocalPlayerControllers[atId] = playerController;
            }
        }

        public void UnregisterPlayerController(int playerId)
        {
            PC p;
            if (RegisteredPlayerControllers.TryGetValue(playerId, out p))
            {
                // Remove from local players
                if (p.GetIsLocal())
                {
                    p.SetIsLocal(false);
                    LocalPlayerControllers.Remove(playerId);
                }

                p.SetId(-1);
                RegisteredPlayerControllers.Remove(playerId);
            }
        }

        public bool IsPlayerIdValidForConnection(int playerId, int connectionId)
        {
            PC p;
            if(RegisteredPlayerControllers.TryGetValue(playerId, out p))
            {
                if(p.GetConnectionId() == connectionId)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void OnTick(float deltaTime)
        {            
            // Receive network messages
            if (OnlineSession.Mode != OnlineSessionMode.Offline)
            {
                OnlineSession.ReceiveIncomingMessages();
            }

            // Clear events
            SnapshotsRingBuffer[GetSnapshotIndexForTick(PresentTick)].SimulationEvents.Clear();

            // Resimulation
            HandleReceivedData(deltaTime);

            // Restore all commands to defaults
            foreach (var p in RegisteredPlayerControllers.Values)
            {
                p.SetCommands(p.GetNewCommands(GetPlayerCommandsFromTick(p.GetId(), PresentTick - 1)));
            }

            // Local player commands handling
            foreach (var p in LocalPlayerControllers.Values)
            {
                p.UpdateLocalCommands(deltaTime);

                if (OnlineSession.IsClient())
                {
                    SerializePlayerCommands(OnlineSession.NetBuffer, p, PresentTick, false);
                    OnlineSession.SendBufferToServer(OnlineSession.UnreliableChannelId);
                }
            }

            // Apply all player commands
            foreach (var p in RegisteredPlayerControllers.Values)
            {
                p.ApplyCommands(p.GetIsLocal());
            }

            // Apply simulationEvents
            foreach(ISimulationEvent sEvent in SnapshotsRingBuffer[GetSnapshotIndexForTick(PresentTick)].SimulationEvents)
            {
                sEvent.Apply();
            }

            // Save snapshot
            SaveSnapshotAtTick(PresentTick);

            // Simulate
            _simulationController.SimulateStep(deltaTime);

            // Increment present tick
            PresentTick++;
            SimulationTick = PresentTick;
            if (SnapshotsCount < SnapshotsCapacity)
            {
                SnapshotsCount++;
            }
            PresentTickIndex++;
            while (PresentTickIndex >= SnapshotsCapacity)
            {
                PresentTickIndex -= SnapshotsCapacity;
            }
            EarliestTickIndex = PresentTickIndex - (SnapshotsCount - 1);
            while (EarliestTickIndex < 0)
            {
                EarliestTickIndex += SnapshotsCapacity;
            }
        }

        private void HandleReceivedData(float deltaTime)
        {
            bool resimulationError = false;
            if (OnlineSession.Mode == OnlineSessionMode.Server)
            {
                _tmpPlayerIdsToApplyInputEvents.Clear();
                _tmpConnectionsToUpdateBuffer.Clear();
                if (ReceivedPlayerCommandsBuffer.Count > 0)
                {
                    // Revert world to earliest received tick
                    SimulationTick = ReceivedPlayerCommandsBuffer[0].DataTimestamp;

                    if (SimulationTick < PresentTick &&
                        RestoreWorldStateFromTick(SimulationTick))
                    {
                        while (SimulationTick < PresentTick && !resimulationError)
                        {
                            // Restore commands of that tick
                            if (RestoreCommandsFromTick(SimulationTick))
                            {
                                // Restore all remote player commands to defaults on ticks after last received state
                                if (SimulationTick > _tickOfLatestReceivedGlobalSnapshot)
                                {
                                    foreach (var pc in RegisteredPlayerControllers.Values)
                                    {
                                        if (!pc.GetIsLocal())
                                        {
                                            pc.SetCommands(pc.GetNewCommands(GetPlayerCommandsFromTick(pc.GetId(), SimulationTick - 1)));
                                        }
                                    }
                                }

                                // Apply and remove all received commands corresponding to that tick
                                while (ReceivedPlayerCommandsBuffer.Count > 0 && ReceivedPlayerCommandsBuffer[0].DataTimestamp == SimulationTick)
                                {
                                    PC playerController = GetPlayerControllerWithId(ReceivedPlayerCommandsBuffer[0].DataId);
                                    if (playerController != null)
                                    {
                                        playerController.SetCommands(ReceivedPlayerCommandsBuffer[0].DataStruct);                                        
                                    }

                                    _tmpPlayerIdsToApplyInputEvents.Add(ReceivedPlayerCommandsBuffer[0].DataId);
                                    _tmpConnectionsToUpdateBuffer[ReceivedPlayerCommandsBuffer[0].ConnectionId] = SimulationTick;

                                    ReceivedPlayerCommandsBuffer.RemoveAt(0);
                                }

                                // Apply all player commands
                                foreach (var p in RegisteredPlayerControllers.Values)
                                {
                                    p.ApplyCommands(_tmpPlayerIdsToApplyInputEvents.Contains(p.GetId()));
                                }

                                // Apply simulationEvents
                                foreach (ISimulationEvent sEvent in SnapshotsRingBuffer[GetSnapshotIndexForTick(SimulationTick)].SimulationEvents)
                                {
                                    sEvent.Apply();
                                }

                                // Save snapshot
                                SaveSnapshotAtTick(SimulationTick);

                                // Simulate
                                _simulationController.SimulateStep(deltaTime);

                                // Increment tick
                                SimulationTick++;
                            }
                            else
                            {
                                resimulationError = true;
                            }

                        }
                    }
                    else
                    {
                        resimulationError = true;
                    }
                }

                // Send latest worldStates to connections
                foreach (var connAndTick in _tmpConnectionsToUpdateBuffer)
                {
                    // Send world snapshot of that tick to that connection
                    SerializeGlobalSnapshot(OnlineSession.NetBuffer, ref SnapshotsRingBuffer[GetSnapshotIndexForTick(connAndTick.Value)], connAndTick.Value, false);
                    OnlineSession.SendBufferToConnection(connAndTick.Key, OnlineSession.UnreliableFragmentedChannelId);
                }
            }
            else if (OnlineSession.Mode == OnlineSessionMode.Client)
            {
                // Resimulate from last received snapshot if any
                if (_tickOfLatestReceivedGlobalSnapshot >= 0 && PresentTick > _tickOfLatestReceivedGlobalSnapshot)
                {
                    SimulationTick = _tickOfLatestReceivedGlobalSnapshot;

                    if (RestoreWorldStateFromTick(SimulationTick))
                    {
                        while (SimulationTick < PresentTick && !resimulationError)
                        {
                            if (RestoreCommandsFromTick(SimulationTick))
                            {
                                // Restore all remote player commands to defaults on ticks after last received state
                                if (SimulationTick > _tickOfLatestReceivedGlobalSnapshot)
                                {
                                    foreach (var pc in RegisteredPlayerControllers.Values)
                                    {
                                        if (!pc.GetIsLocal())
                                        {
                                            pc.SetCommands(pc.GetNewCommands(GetPlayerCommandsFromTick(pc.GetId(), SimulationTick - 1)));
                                        }
                                    }
                                }

                                // Apply all player commands
                                foreach (var p in RegisteredPlayerControllers.Values)
                                {
                                    p.ApplyCommands(false);
                                }

                                // Apply simulationEvents
                                foreach (ISimulationEvent sEvent in SnapshotsRingBuffer[GetSnapshotIndexForTick(SimulationTick)].SimulationEvents)
                                {
                                    sEvent.Apply();
                                }

                                // Save snapshot
                                SaveSnapshotAtTick(SimulationTick);

                                // Simulate
                                _simulationController.SimulateStep(deltaTime);

                                // Increment tick
                                SimulationTick++;
                            }
                            else
                            {
                                resimulationError = true;
                            }
                        }
                    }
                    else
                    {
                        resimulationError = true;
                    }

                    _tickOfLatestReceivedGlobalSnapshot = -1;
                    
                    _simulationController.OnPostSnapshot();
                }
            }

            if(resimulationError || SimulationTick != PresentTick)
            {
                //RestoreWorldStateFromTick(PresentTick);
                //RestoreCommandsFromTick(PresentTick);
                SimulationTick = PresentTick;
            }
        }

        public void InitializeAtTick(int tick)
        {
            PresentTick = tick;
            SimulationTick = PresentTick;
            EarliestTickIndex = 0;
            PresentTickIndex = 0;
            SnapshotsCount = 1;
        }

        public bool HasTick(int tick)
        {
            if (SnapshotsCount > 0 &&
                tick <= PresentTick &&
                tick > PresentTick - SnapshotsCount)
            {
                return true;
            }

            return false;
        }

        public int GetSnapshotIndexForTick(int forTick)
        {
            if (HasTick(forTick))
            {
                int index = PresentTickIndex + (forTick - PresentTick);
                while (index < 0)
                {
                    index += SnapshotsCapacity;
                }
                return index;
            }

            return -1;
        }

        public bool SaveSnapshotAtTick(int atTick)
        {
            int sIndex = GetSnapshotIndexForTick(atTick);
            if (sIndex >= 0)
            {
                SnapshotsRingBuffer[sIndex].CommandsSnapshot.PlayerCommands.Clear();
                foreach (var p in RegisteredPlayerControllers.Values)
                {
                    IdentifiedData<CMD> ipi = new IdentifiedData<CMD>(p.GetId(), p.GetCommands());
                    SnapshotsRingBuffer[sIndex].CommandsSnapshot.PlayerCommands.Add(ipi);
                }

                _simulationController.SaveStateToSnapshot(ref SnapshotsRingBuffer[sIndex].WorldSnapshot);

                return true;
            }

            return false;
        }

        public bool RestoreCommandsFromTick(int tick)
        {
            int sIndex = GetSnapshotIndexForTick(tick);
            if (sIndex >= 0)
            {
                RestoreCommandsFromSnapshotIndex(sIndex);
                return true;
            }
            return false;
        }
        
        public void RestoreCommandsFromSnapshotIndex(int index)
        {
            PC p;
            foreach (IdentifiedData<CMD> i in SnapshotsRingBuffer[index].CommandsSnapshot.PlayerCommands)
            {
                p = GetPlayerControllerWithId(i.DataId);
                if (p != null)
                {
                    p.SetCommands(i.DataStruct);
                }
            }
        }

        public bool RestoreWorldStateFromTick(int toTick)
        {
            int sIndex = GetSnapshotIndexForTick(toTick);
            if (sIndex >= 0)
            {
                RestoreWorldStateFromSnapshotIndex(sIndex);
                return true;
            }
            return false;
        }

        public void RestoreWorldStateFromSnapshotIndex(int index)
        {
            _simulationController.RestoreStateFromSnapshot(ref SnapshotsRingBuffer[index].WorldSnapshot);
        }

        private CMD GetPlayerCommandsFromTick(int playerId, int tick)
        {
            int index = GetSnapshotIndexForTick(tick);
            if(index >= 0)
            {
                foreach(var pi in SnapshotsRingBuffer[index].CommandsSnapshot.PlayerCommands)
                {
                    if(pi.DataId == playerId)
                    {
                        return pi.DataStruct;
                    }
                }
            }

            return default(CMD);
        }

        public void OnConnect(int connectionId)
        {
            _simulationController.OnConnect(connectionId);
        }

        public void OnDisconnect(int connectionId)
        {
            _simulationController.OnDisconnect(connectionId);
        }

        public void OnData(int connectionId, int channelId, int receivedSize)
        {
            GameStatics.OnlineSession.NetBuffer.SeekZero();
            int msgId = GameStatics.OnlineSession.NetBuffer.ReadInt();

            // Messages with an ID under zero are reserved framework messages
            if (msgId >= 0)
            {
                _simulationController.OnData(connectionId, channelId, receivedSize, msgId);
            }
            else
            {
                FrameworkNetworkMessageID fMsgId = (FrameworkNetworkMessageID)msgId;
                switch (fMsgId)
                {
                    case (FrameworkNetworkMessageID.PlayerCommands):
                        if (GameStatics.OnlineSession.IsServer())
                        {
                            CMD newCommands = new CMD();

                            int receivedTick;
                            int receivedPlayerId;
                            DeserializePlayerCommands(OnlineSession.NetBuffer, ref newCommands, out receivedTick, out receivedPlayerId);

                            if (IsPlayerIdValidForConnection(receivedPlayerId, connectionId))
                            {
                                // See if we missed any commands and ask for resend
                                int lastTickForPlayer;
                                if (_lastReceivedCommandTick.TryGetValue(receivedPlayerId, out lastTickForPlayer))
                                {
                                    if (receivedTick > lastTickForPlayer)
                                    {
                                        // remember latest received tick
                                        _lastReceivedCommandTick[receivedPlayerId] = receivedTick;

                                        for (int i = lastTickForPlayer + 1; i < receivedTick; i++)
                                        {
                                            CommandRequestMsg crmsg = new CommandRequestMsg();
                                            crmsg.ForPlayerId = receivedPlayerId;
                                            crmsg.ForTick = i;
                                            crmsg.Serialize(GameStatics.OnlineSession.NetBuffer);
                                            GameStatics.OnlineSession.SendBufferToServer(OnlineSession.ReliableSequencedChannelId);
                                        }
                                    }
                                }
                                else
                                {
                                    // remember latest received tick
                                    _lastReceivedCommandTick[receivedPlayerId] = receivedTick;
                                }

                                // Order them as we're adding them to the list
                                if (ReceivedPlayerCommandsBuffer.Count == 0)
                                {
                                    ReceivedPlayerCommandsBuffer.Add(new TimestampedIdentifiedConnectionData<CMD>(receivedPlayerId, receivedTick, connectionId, newCommands));
                                }
                                else
                                {
                                    bool added = false;
                                    for (int i = 0; i < ReceivedPlayerCommandsBuffer.Count; i++)
                                    {
                                        if (receivedTick < ReceivedPlayerCommandsBuffer[i].DataTimestamp)
                                        {
                                            ReceivedPlayerCommandsBuffer.Insert(i, new TimestampedIdentifiedConnectionData<CMD>(receivedPlayerId, receivedTick, connectionId, newCommands));
                                            added = true;
                                            break;
                                        }
                                    }

                                    if (!added)
                                    {
                                        ReceivedPlayerCommandsBuffer.Add(new TimestampedIdentifiedConnectionData<CMD>(receivedPlayerId, receivedTick, connectionId, newCommands));
                                    }
                                }
                            }
                        }
                        break;
                    case (FrameworkNetworkMessageID.GlobalState):
                        if (GameStatics.OnlineSession.IsClient())
                        {
                            int receivedTick;
                            _simulationController.OnPreSnapshot();
                            DeserializeGlobalSnapshot(OnlineSession.NetBuffer, out receivedTick);

                            if (receivedTick > _tickOfLatestReceivedGlobalSnapshot)
                            {
                                _tickOfLatestReceivedGlobalSnapshot = receivedTick;
                            }
                        }
                        break;
                    case (FrameworkNetworkMessageID.CommandRequest):
                        CommandRequestMsg crm = new CommandRequestMsg();
                        crm.Deserialize(GameStatics.OnlineSession.NetBuffer);

                        List <IdentifiedData<CMD>> commandsForTick = SnapshotsRingBuffer[GetSnapshotIndexForTick(crm.ForTick)].CommandsSnapshot.PlayerCommands;
                        foreach(IdentifiedData<CMD> c in commandsForTick)
                        {
                            if(c.DataId == crm.ForPlayerId)
                            {
                                OnlineSession.NetBuffer.SeekZero();
                                OnlineSession.NetBuffer.WriteInt((int)FrameworkNetworkMessageID.PlayerCommands);

                                OnlineSession.NetBuffer.WriteInt(crm.ForTick);
                                OnlineSession.NetBuffer.WriteInt(c.DataId);

                                c.DataStruct.WriteToBuffer(OnlineSession.NetBuffer, true);

                                OnlineSession.SendBufferToServer(OnlineSession.UnreliableChannelId);
                                break;
                            }
                        }
                        break;
                }
            }
        }

        public void AddSimulationEvent(ISimulationEvent simulationEvent, int atTick)
        {
            SnapshotsRingBuffer[GetSnapshotIndexForTick(atTick)].SimulationEvents.Add(simulationEvent);
        }

        private void SerializePlayerCommands(SerializationBuffer buf, PC forPlayer, int forTick, bool mustBeFullState)
        {
            buf.SeekZero();
            buf.WriteInt((int)FrameworkNetworkMessageID.PlayerCommands);

            buf.WriteInt(forTick);
            buf.WriteInt(forPlayer.GetId());

            CMD currentCommands = forPlayer.GetCommands();
            currentCommands.WriteToBuffer(buf, mustBeFullState);
        }

        private void DeserializePlayerCommands(SerializationBuffer buf, ref CMD intoStruct, out int receivedTick, out int receivedPlayerId)
        {
            receivedTick = buf.ReadInt();
            receivedPlayerId = buf.ReadInt();

            intoStruct.ReadFromBuffer(buf);
        }

        private void SerializeGlobalSnapshot(SerializationBuffer buf, ref GlobalSnapshot<CMD, WS> forSnapshot, int forTick, bool mustBeFullState)
        {
            buf.SeekZero();
            buf.WriteInt((int)FrameworkNetworkMessageID.GlobalState);

            buf.WriteInt(forTick);

            // Commands snapshot
            int commandsCount = forSnapshot.CommandsSnapshot.PlayerCommands.Count;
            buf.WriteInt(commandsCount);
            for (int i = 0; i < commandsCount; i++)
            {
                buf.WriteInt(forSnapshot.CommandsSnapshot.PlayerCommands[i].DataId);
                forSnapshot.CommandsSnapshot.PlayerCommands[i].DataStruct.WriteToBuffer(buf, mustBeFullState);
            }

            // World snapshot
            forSnapshot.WorldSnapshot.WriteToBuffer(buf, mustBeFullState);
        }

        private void DeserializeGlobalSnapshot(SerializationBuffer buf, out int receivedTick)
        {
            receivedTick = buf.ReadInt();
            int indexForReceivedTick = GetSnapshotIndexForTick(receivedTick);
            
            // Commands snapshot
            int commandsCount = buf.ReadInt();
            for (int i = 0; i < commandsCount; i++)
            {
                int commandsPlayerId = buf.ReadInt();

                // Don't override commands from local players
                IdentifiedData<CMD> tmpPlayerCommands = new IdentifiedData<CMD>();
                tmpPlayerCommands.DataId = commandsPlayerId;
                tmpPlayerCommands.DataStruct.ReadFromBuffer(buf);

                for (int j = 0; j < SnapshotsRingBuffer[indexForReceivedTick].CommandsSnapshot.PlayerCommands.Count; j++)
                {
                    if (SnapshotsRingBuffer[indexForReceivedTick].CommandsSnapshot.PlayerCommands[j].DataId == commandsPlayerId)
                    {
                        SnapshotsRingBuffer[indexForReceivedTick].CommandsSnapshot.PlayerCommands[j] = tmpPlayerCommands;
                        break;
                    }
                }
            }

            // World snapshot
            SnapshotsRingBuffer[indexForReceivedTick].WorldSnapshot.ReadFromBuffer(buf);
        }
    }
}