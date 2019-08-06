using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public interface INetworkEventHandler
    {
        void OnConnect(int connectionId);
        void OnDisconnect(int connectionId);
        void OnData(int connectionId, int channelId, int receivedSize);
    }

    public enum OnlineSessionMode
    {
        Offline,
        Server,
        Client
    }

    public class ClientConnection
    {
        public int ConnectionId = -1;

        public ClientConnection(int connectionId)
        {
            ConnectionId = connectionId;
        }
    }

    public class OnlineSession : MonoBehaviour
    {
        [Header("Network Config")]
        public GlobalConfig GlobalConfig;
        public ConnectionConfig ConnectionConfig;

        [NonSerialized]
        public OnlineSessionMode Mode = OnlineSessionMode.Offline;
        [NonSerialized]
        public int HostId;
        [NonSerialized]
        public INetworkEventHandler NetworkEventHandler;
        [NonSerialized]
        public SerializationBuffer NetBuffer;
        [NonSerialized]
        public int ServerConnection = -1;
        [NonSerialized]
        public List<ClientConnection> ClientConnections = new List<ClientConnection>();

        public const int ReliableSequencedChannelId = 0;
        public const int UnreliableFragmentedChannelId = 1;
        public const int UnreliableChannelId = 2;

        private void Awake()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        public bool IsOnline()
        {
            return Mode != OnlineSessionMode.Offline;
        }

        public bool IsServer()
        {
            return Mode == OnlineSessionMode.Server;
        }

        public bool IsClient()
        {
            return Mode == OnlineSessionMode.Client;
        }

        public bool StartAsHost(int hostPort, int maxConnections)
        {
            InitializeNetworking();

            HostTopology topology = new HostTopology(ConnectionConfig, maxConnections);

            bool success = false;
            int tmpHostId = -1;
            tmpHostId = NetworkTransport.AddHost(topology, hostPort);
            if (tmpHostId >= 0)
            {
                Mode = OnlineSessionMode.Server;
                HostId = tmpHostId;

                success = true;
            }

            if (!success)
            {
                Debug.LogError("StartAsHost failed");
            }

            return success;
        }

        public bool StartAsClient(int hostPort, string connectToIP, int connectToPort)
        {
            InitializeNetworking();

            HostTopology topology = new HostTopology(ConnectionConfig, 1);

            bool success = false;
            int tmpHostId = -1;
            tmpHostId = NetworkTransport.AddHost(topology, hostPort);
            if (tmpHostId >= 0)
            {
                byte networkError;
                int connectionId = NetworkTransport.Connect(tmpHostId, connectToIP, connectToPort, 0, out networkError);
                if ((NetworkError)networkError == NetworkError.Ok)
                {
                    Mode = OnlineSessionMode.Client;
                    HostId = tmpHostId;

                    success = true;
                }
            }

            if (!success)
            {
                Debug.LogError("StartAsClient failed");
            }

            return success;
        }

        private void InitializeNetworking()
        {
            NetworkTransport.Shutdown();
            NetworkTransport.Init(GlobalConfig);

            Application.runInBackground = true;

            // Network buffer for messages
            NetBuffer = new SerializationBuffer(GlobalConfig.MaxPacketSize, 1.5f);

            // Message channels
            ConnectionConfig.Channels.Clear();
            ConnectionConfig.AddChannel(QosType.ReliableSequenced);
            ConnectionConfig.AddChannel(QosType.UnreliableFragmented);
            ConnectionConfig.AddChannel(QosType.Unreliable);
        }

        public void TerminateNetworking()
        {
            NetworkTransport.Shutdown();
            Mode = OnlineSessionMode.Offline;
        }

        public void ReceiveIncomingMessages()
        {
            byte networkError;
            int connectionId;
            int channelId;
            int receivedSize;
            NetworkEventType networkEvent;

            if (HostId >= 0 && NetworkEventHandler != null)
            {
                do
                {
                    networkEvent = NetworkTransport.ReceiveFromHost(HostId, out connectionId, out channelId, NetBuffer.InternalBuffer, NetBuffer.InternalBuffer.Length, out receivedSize, out networkError);
                    switch (networkEvent)
                    {
                        case NetworkEventType.ConnectEvent:
                            if ((NetworkError)networkError == NetworkError.Ok)
                            {
                                if (Mode == OnlineSessionMode.Server)
                                {
                                    ClientConnections.Add(new ClientConnection(connectionId));
                                }
                                else if (Mode == OnlineSessionMode.Client)
                                {
                                    ServerConnection = connectionId;
                                }
                                NetworkEventHandler.OnConnect(connectionId);
                            }
                            break;
                        case NetworkEventType.DisconnectEvent:
                            if (Mode == OnlineSessionMode.Server)
                            {
                                for (int i = ClientConnections.Count - 1; i >= 0; i--)
                                {
                                    if (ClientConnections[i].ConnectionId == connectionId)
                                    {
                                        ClientConnections.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            else if (Mode == OnlineSessionMode.Client)
                            {
                                ServerConnection = -1;
                            }

                            NetworkEventHandler.OnDisconnect(connectionId);

                            if (Mode == OnlineSessionMode.Client)
                            {
                                TerminateNetworking();
                            }
                            break;
                        case NetworkEventType.DataEvent:
                            if ((NetworkError)networkError == NetworkError.Ok)
                            {
                                NetworkEventHandler.OnData(connectionId, channelId, receivedSize);
                            }
                            break;
                    }
                }
                while (networkEvent != NetworkEventType.Nothing);
            }
        }

        public ClientConnection GetClientConnectionWithId(int connectionId)
        {
            for (int i = ClientConnections.Count - 1; i >= 0; i--)
            {
                if (ClientConnections[i].ConnectionId == connectionId)
                {
                    return ClientConnections[i];
                }
            }

            return null;
        }

        public void SendBufferToServer(int channelId)
        {
            SendBufferToConnection(ServerConnection, channelId);
        }

        public void SendBufferToAllClients(int channelId)
        {
            for (int i = 0; i < ClientConnections.Count; i++)
            {
                SendBufferToConnection(ClientConnections[i].ConnectionId, channelId);
            }
        }

        public void SendBufferToAllClientsExcept(int exceptConnectionId, int channelId)
        {
            for (int i = 0; i < ClientConnections.Count; i++)
            {
                if (ClientConnections[i].ConnectionId != exceptConnectionId)
                {
                    SendBufferToConnection(ClientConnections[i].ConnectionId, channelId);
                }
            }
        }

        public void SendBufferToConnections(List<int> connections, int channelId)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                SendBufferToConnection(connections[i], channelId);
            }
        }

        public void SendBufferToConnection(int connectionId, int channelId)
        {
            byte netError;
            NetworkTransport.Send(HostId, connectionId, channelId, NetBuffer.InternalBuffer, NetBuffer.IndexPosition, out netError);
            if ((NetworkError)netError != NetworkError.Ok)
            {
                Debug.Log("Network Send Error " + (NetworkError)netError + " Chan: " + channelId);
            }
        }

        private void OnDestroy()
        {
            TerminateNetworking();
        }

        public int GetTimestamp()
        {
            return NetworkTransport.GetNetworkTimestamp();
        }

        public float GetRemoteDelayTimeMS(int connectionId, int remoteTimestamp)
        {
            byte netError;
            return NetworkTransport.GetRemoteDelayTimeMS(HostId, connectionId, remoteTimestamp, out netError);
        }

        public void Disconnect(int connectionId)
        {
            byte netError;
            NetworkTransport.Disconnect(HostId, connectionId, out netError);
        }
    }
}
