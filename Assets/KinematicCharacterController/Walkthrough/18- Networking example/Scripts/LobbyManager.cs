using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public class PlayerData
    {
        public int PlayerId; // The unique ID of the player, used for networking
        public int ConnectionId;
        public string Name;
    }

    public class LobbyManager : MonoBehaviour, INetworkEventHandler
    {
        public GameData GameData;

        [Header("UI")]
        public Text LocalHostPortText;
        public InputField LocalHostPortField;
        public Text NameText;
        public InputField NameField;
        public Button DisconnectButton;
        public Button HostButton;
        public Button ConnectButton;
        public Button StartGameButton;
        public InputField ConnectIPField;
        public InputField ConnectPortField;
        public RectTransform JoinedPlayersPanel;
        public LobbyPlayerEntry LobbyPlayerEntryPrefab;

        [NonSerialized]
        public Dictionary<int, PlayerData> JoinedPlayers = new Dictionary<int, PlayerData>();

        private int _playerIdCounter = 0;
        private List<LobbyPlayerEntry> _lobbyPlayerEntries = new List<LobbyPlayerEntry>();

        public void Start()
        {
            StartGameButton.gameObject.SetActive(false);

            HostButton.onClick.AddListener(OnHostButton);
            ConnectButton.onClick.AddListener(OnConnectButton);
            DisconnectButton.onClick.AddListener(OnOfflineButton);
            StartGameButton.onClick.AddListener(OnStartGameButton);

            GameStatics.GameData = GameData;

            Initialize();


            // Cmd args
            OnlineSessionMode startMode = OnlineSessionMode.Offline;
            string[] args = System.Environment.GetCommandLineArgs();
            foreach(string arg in args)
            {
                string[] splitArg = arg.Split(':');
                if(splitArg.Length >= 2)
                {
                    switch(splitArg[0])
                    {
                        case "mode":
                            switch(splitArg[1])
                            {
                                case "server":
                                    startMode = OnlineSessionMode.Server;
                                    break;
                                case "client":
                                    startMode = OnlineSessionMode.Client;
                                    break;
                            }
                            break;
                        case "hostport":
                            LocalHostPortField.text = splitArg[1];
                            break;
                        case "connectip":
                            ConnectIPField.text = splitArg[1];
                            break;
                        case "connectport":
                            ConnectPortField.text = splitArg[1];
                            break;
                        case "playername":
                            NameField.text = splitArg[1];
                            break;
                    }
                }
            }

            if (startMode == OnlineSessionMode.Server)
            {
                OnHostButton();
            }
            else if (startMode == OnlineSessionMode.Client)
            {
                OnConnectButton();
            }
        }

        private void Update()
        {
            // Debug for quickly incrementing local host port
            if(Input.GetKeyDown(KeyCode.Space))
            {
                int hostPort;
                if (int.TryParse(LocalHostPortField.text, out hostPort))
                {
                    LocalHostPortField.text = (hostPort + 1).ToString();
                }
            }
        }

        public void FixedUpdate()
        {
            if(GameStatics.OnlineSession.Mode != OnlineSessionMode.Offline)
            {
                GameStatics.OnlineSession.ReceiveIncomingMessages();
            }
        }

        private void Initialize()
        {
            // Create the online session (as offline)
            GameStatics.CreateOnlineSession();
            GameStatics.OnlineSession.NetworkEventHandler = this;

            JoinedPlayers.Clear();

            // Clear lobby
            for (int i = _lobbyPlayerEntries.Count - 1; i >= 0; i--)
            {
                Destroy(_lobbyPlayerEntries[i].gameObject);
            }
            _lobbyPlayerEntries.Clear();
        }

        public void OnHostButton()
        {
            int hostPort;
            if (int.TryParse(LocalHostPortField.text, out hostPort))
            {
                if (GameStatics.OnlineSession.StartAsHost(hostPort, 100))
                {
                    SetConnectionControlsActive(false);

                    StartGameButton.gameObject.SetActive(true);

                    // Add self to game session
                    if(AddPlayer(_playerIdCounter, true, -1, NameField.text))
                    {
                        _playerIdCounter++;
                    }
                }
            }
        }

        public void OnConnectButton()
        {
            int hostPort;
            if (int.TryParse(LocalHostPortField.text, out hostPort))
            {
                int connectToPort;
                if (int.TryParse(ConnectPortField.text, out connectToPort))
                {
                    if (GameStatics.OnlineSession.StartAsClient(hostPort, ConnectIPField.text, connectToPort))
                    {
                        SetConnectionControlsActive(false);
                    }
                }
            }
        }

        public void OnOfflineButton()
        {
            GameStatics.OnlineSession.TerminateNetworking();

            Initialize();

            SetConnectionControlsActive(true);
        }

        public void OnStartGameButton()
        {
            SceneLoadingMsg lsm = new SceneLoadingMsg();
            lsm.SceneName = GameStatics.GameData.GameSceneName;
            lsm.Serialize(GameStatics.OnlineSession.NetBuffer);
            GameStatics.OnlineSession.SendBufferToAllClients(OnlineSession.ReliableSequencedChannelId);

            SceneManager.LoadScene(GameStatics.GameData.GameSceneName);
        }

        private void SetConnectionControlsActive(bool active)
        {
            LocalHostPortField.gameObject.SetActive(active);
            NameField.gameObject.SetActive(active);
            HostButton.gameObject.SetActive(active);
            ConnectButton.gameObject.SetActive(active);
            ConnectIPField.gameObject.SetActive(active);
            ConnectPortField.gameObject.SetActive(active);
            LocalHostPortText.gameObject.SetActive(active);
            NameText.gameObject.SetActive(active);
        }

        public void OnConnect(int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                // Send msg to register name with server
                PlayerRegistrationMsg msg = new PlayerRegistrationMsg();
                msg.PlayerName = NameField.text;                
                msg.Serialize(GameStatics.OnlineSession.NetBuffer);
                GameStatics.OnlineSession.SendBufferToServer(OnlineSession.ReliableSequencedChannelId);
            }
        }

        public void OnDisconnect(int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server)
            {
                RemovePlayersForConnection(connectionId);
            }
            else
            {
                Initialize();
            }
        }

        public void OnData(int connectionId, int channelId, int receiveSize)
        {
            GameStatics.OnlineSession.NetBuffer.SeekZero();
            NetworkMessageID msgId = (NetworkMessageID)GameStatics.OnlineSession.NetBuffer.ReadInt();
            
            switch (msgId)
            {
                case (NetworkMessageID.PlayerRegistration):
                    PlayerRegistrationMsg prm = new PlayerRegistrationMsg();
                    prm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandlePlayerRegistrationMsg(ref prm, connectionId);
                    break;
                case (NetworkMessageID.PlayerAdd):
                    PlayerAddMsg pam = new PlayerAddMsg();
                    pam.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandlePlayerAddMsg(ref pam, connectionId);
                    break;
                case (NetworkMessageID.SceneLoading):
                    SceneLoadingMsg lsm = new SceneLoadingMsg();
                    lsm.Deserialize(GameStatics.OnlineSession.NetBuffer);
                    HandleLoadSceneMsg(ref lsm);
                    break;
            }
        }

        private void HandlePlayerRegistrationMsg(ref PlayerRegistrationMsg msg, int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Server)
            {
                if (AddPlayer(_playerIdCounter, false, connectionId, msg.PlayerName))
                {
                    _playerIdCounter++;
                }
            }
        }

        private void HandlePlayerAddMsg(ref PlayerAddMsg msg, int connectionId)
        {
            if (GameStatics.OnlineSession.Mode == OnlineSessionMode.Client)
            {
                if (msg.Add)
                {
                    AddLobbyPlayerEntry(msg.PlayerName, msg.PlayerId);
                }
                else
                {
                    RemoveLobbyPlayerEntry(msg.PlayerId);
                }
            }
        }

        private void HandleLoadSceneMsg(ref SceneLoadingMsg msg)
        {
            SceneManager.LoadScene(msg.SceneName);
        }

        public void AddLobbyPlayerEntry(string name, int id)
        {
            LobbyPlayerEntry lpe = Instantiate<LobbyPlayerEntry>(LobbyPlayerEntryPrefab, JoinedPlayersPanel);
            lpe.Init(name, id);
            _lobbyPlayerEntries.Add(lpe);
        }

        public void RemoveLobbyPlayerEntry(int id)
        {
            for(int i = _lobbyPlayerEntries.Count - 1; i >= 0; i--)
            {
                if(_lobbyPlayerEntries[i].EntryId == id)
                {
                    Destroy(_lobbyPlayerEntries[i].gameObject);
                    _lobbyPlayerEntries.RemoveAt(i);
                }
            }
        }

        public void RemovePlayer(int playerId)
        {
            PlayerData removedPlayer = JoinedPlayers[playerId];
            if (JoinedPlayers.Remove(playerId))
            {
                OnPlayerRemovedFromSession(removedPlayer);
            }
        }

        public void RemovePlayersForConnection(int connectionId)
        {
            List<int> playerIdsToRemove = new List<int>();

            // Remove all players from that connection id
            foreach (var pd in JoinedPlayers)
            {
                if (pd.Value.ConnectionId == connectionId)
                {
                    playerIdsToRemove.Add(pd.Value.PlayerId);
                }
            }

            for (int i = playerIdsToRemove.Count - 1; i >= 0; i--)
            {
                RemovePlayer(playerIdsToRemove[i]);
            }
        }

        public bool AddPlayer(int playerId, bool isLocal, int connectionId, string name)
        {
            if (JoinedPlayers.ContainsKey(playerId))
            {
                return false;
            }

            PlayerData newPlayer = new PlayerData();
            newPlayer.PlayerId = playerId;
            newPlayer.ConnectionId = connectionId;
            newPlayer.Name = name;

            JoinedPlayers[playerId] = newPlayer;

            OnPlayerAddedToSession(newPlayer);

            return true;
        }

        public void OnPlayerAddedToSession(PlayerData player)
        {
            AddLobbyPlayerEntry(player.Name, player.PlayerId);

            // Send all players information to this new player
            if (player.ConnectionId >= 0)
            {
                foreach (var pd in JoinedPlayers)
                {
                    PlayerAddMsg newMsg1 = new PlayerAddMsg();
                    newMsg1.Add = true;
                    newMsg1.PlayerId = pd.Value.PlayerId;
                    newMsg1.PlayerName = pd.Value.Name;

                    // For the new added connection, send msg to add as local player
                    if (pd.Value.ConnectionId == player.ConnectionId)
                    {
                        newMsg1.ForLocalPlayer = true;
                        newMsg1.Serialize(GameStatics.OnlineSession.NetBuffer);
                        GameStatics.OnlineSession.SendBufferToConnection(player.ConnectionId, OnlineSession.ReliableSequencedChannelId);
                    }
                    else
                    {
                        newMsg1.ForLocalPlayer = false;
                        newMsg1.Serialize(GameStatics.OnlineSession.NetBuffer);
                        GameStatics.OnlineSession.SendBufferToConnection(player.ConnectionId, OnlineSession.ReliableSequencedChannelId);
                    }
                }
            }

            // Send new player information to other players
            PlayerAddMsg newMsg2 = new PlayerAddMsg();
            newMsg2.Add = true;
            newMsg2.ForLocalPlayer = false;
            newMsg2.PlayerId = player.PlayerId;
            newMsg2.PlayerName = player.Name;
            newMsg2.Serialize(GameStatics.OnlineSession.NetBuffer);
            GameStatics.OnlineSession.SendBufferToAllClientsExcept(player.ConnectionId, OnlineSession.ReliableSequencedChannelId);
        }

        public void OnPlayerRemovedFromSession(PlayerData player)
        {
            // Remove player from gameSession
            RemoveLobbyPlayerEntry(player.PlayerId);

            // Send remove notification of disconnected player to all other players
            PlayerAddMsg msg = new PlayerAddMsg();
            msg.Add = false;
            msg.PlayerId = player.PlayerId;
            msg.Serialize(GameStatics.OnlineSession.NetBuffer);
            GameStatics.OnlineSession.SendBufferToAllClients(OnlineSession.ReliableSequencedChannelId);
        }
    }
}