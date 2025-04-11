using System.Collections.Generic;
using System.Net.Sockets;
using Networking.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Core
{
    public class Client : NetworkEvents
    {
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 5500;
        private Socket _clientSocket;

        private PlayerData _playerData;
        public List<PlayerData> _playersInLobby = new List<PlayerData>();

        public List<PlayerData> PlayersInLobby { get { return _playersInLobby; } }
        public PlayerData PlayerData
        {
            get { return _playerData; }
        }

        public static Client Instance;

        private void Awake()
        {
            // Set up and maintain the client as a singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }

            ServerConnectEvent += ServerConnectEvent;
        }

        private void OnDestroy()
        {
            ServerConnectEvent -= ServerConnectEvent;
        }

        private void Start()
        {
            try // Attempt to connect to the server
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (SocketException e) // We could not connect so print out the error
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        public void ConnectToServer(string ipAddress, int duckChosen, string playerName)
        {
            _ipAddress = ipAddress;
            _clientSocket.Connect(_ipAddress, _port);
            _clientSocket.Blocking = false;
            Debug.LogError("Client socket connected");
            JoinLobby(duckChosen, playerName);
        }

        private void Update()
        {
            if (_clientSocket.Available > 0)
            {
                try
                {
                    // Get all the data from the buffer of data we have received
                    byte[] buffer = new byte[_clientSocket.Available];
                    _clientSocket.Receive(buffer);

                    int bufferSize = buffer.Length;
                    int offset = 0;

                    while (bufferSize > 0) // Process all the packets in the buffer until the buffer is empty
                    {
                        // Deserialize the base of the packet to expose its packet type
                        BasePacket basePacket = new BasePacket();
                        basePacket.Deserialize(buffer, ref bufferSize, ref offset);
                        switch (basePacket.Type)
                        {
                            case BasePacket.PacketType.None:
                                Debug.LogError("Packet type is None");
                                break;

                            case BasePacket.PacketType.Join:
                                JoinPacket jp = new JoinPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                bool isInLobby = _playersInLobby.Exists(p => p.Username == jp.PlayerData.Username && p.DuckID == jp.PlayerData.DuckID); // -delete later- determined by checking the user and duck id to confirm presence in lobby. ///
                                if (!isInLobby)
                                {
                                    _playersInLobby.Add(jp.PlayerData);
                                    PlayerConnectedEvent?.Invoke(jp.PlayerData);
                                }
                                break;

                            case BasePacket.PacketType.ClientList:
                                Debug.LogError("Client list received");
                                PlayerDataListPacket pdlp = new PlayerDataListPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                foreach (PlayerData pd in pdlp.Players)
                                {
                                    // Don't include our data (current player data)
                                    if (pd.Username == _playerData.Username)
                                        continue;
                                    bool exists = _playersInLobby.Exists(
                                        p => p.Username == pd.Username && p.DuckID == pd.DuckID);
                                    if (!exists)
                                    {
                                        _playersInLobby.Add(pd);
                                        PlayerConnectedEvent?.Invoke(pd);
                                    }
                                }
                                break;

                            case BasePacket.PacketType.Message:
                                MessagePacket mp = new MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                ChatMessageReceivedEvent(mp.PlayerData, mp.Message);
                                break;

                            case BasePacket.PacketType.Instantiate:
                                InstantiatePacket ip = new InstantiatePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                InstantiateFromNetwork(ip);
                                break;

                            case BasePacket.PacketType.Position:
                                PositionPacket pp = new PositionPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                PositionPacketReceivedEvent(pp);
                                break;

                            case BasePacket.PacketType.Destroy:
                                DestroyPacket dp = new DestroyPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                DestroyPacketReceivedEvent(dp);
                                break;
                            case BasePacket.PacketType.ReadyStatus:
                                ReadinessPacket rp = new ReadinessPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                PlayerReadinessChangedEvent?.Invoke(rp.PlayerData, rp.IsReady);
                                break;

                            default:
                                break;
                        }
                    }
                }
                catch (SocketException e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        public void JoinLobby(int duckChosen, string username)
        {
            _playerData = new PlayerData(duckChosen, username);

            if (!_playersInLobby.Exists(p => p.Username == username && p.DuckID == duckChosen))
            {
                _playersInLobby.Add(_playerData);
            }
            _clientSocket.Send(new JoinPacket(_playerData).Serialize());
            SceneManager.LoadScene(1);
        }

        public void SendChatMessage(string message)
        {
            byte[] buffer = new MessagePacket(PlayerData, message).Serialize();
            ChatMessageSentEvent(PlayerData, message);
            _clientSocket.Send(buffer);
        }

        public void InstantiateFromNetwork(InstantiatePacket packet)
        {
            GameObject prefab = Resources.Load<GameObject>(packet.PrefabName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, packet.Position, packet.Rotation);
                var nc = go.GetComponent<NetworkComponent>();
                if (nc != null)
                {
                    nc.SetObjectData(packet.ObjectID, packet.PlayerData.DuckID);
                }
            }
        }


        public void InstantiateOverNetwork(string prefabName, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, position, rotation);

                string objectID = System.Guid.NewGuid().ToString();
                NetworkComponent nc = go.GetComponent<NetworkComponent>();
                if (nc != null)
                {
                    nc.SetObjectData(objectID, _playerData.DuckID);
                }

                InstantiatePacket ip = new InstantiatePacket(_playerData, objectID, prefabName, position, rotation);
                _clientSocket.Send(ip.Serialize());
            }
        }

        public void SendPositionPacket(PositionPacket packet)
        {
            _clientSocket.Send(packet.Serialize());
        }

        public void SendDestroyPacket(DestroyPacket packet)
        {
            _clientSocket.Send(packet.Serialize());
        }

        public void SendReadyStatus(bool isReady)
        {
            var readyPacket = new ReadinessPacket(_playerData, isReady);
            _clientSocket.Send(readyPacket.Serialize());
        }
    }
}