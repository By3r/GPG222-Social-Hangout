using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Networking.Core.Lobby;
using Networking.Packets;
using Networking.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Core
{
    public class Client : NetworkEvents
    {
        #region Variables
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 5500;
        private Socket _clientSocket;

        [Tooltip("Player information")]
        private PlayerData _playerData;
        private string _pendingUsername;
        public PlayerData PlayerData { get { return _playerData; } }

        [Tooltip("Track players in lobby/ connected players")]
        private List<PlayerData> _playersInLobby = new List<PlayerData>();
        public List<PlayerData> PlayersInLobby { get { return _playersInLobby; } }

        [Tooltip("'Owner of the lobby'")]
        public PlayerData SceneHost;

        public int SceneIndex = 0;

        public static Client Instance;

        private float _heartbeatTimer = 0f;
        private const float HeartbeatInterval = 1f;

        private byte[] _receiveBuffer = new byte[8192];
        private int _bufferCount = 0;
        #endregion

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

            DestroyPacketReceivedEvent += OnDestroyPacketReceived;
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

        private void Update()
        {
            if (_clientSocket == null || !_clientSocket.Connected)
                return;

            if (_playerData != null)
            {
                _heartbeatTimer += Time.deltaTime;
                if (_heartbeatTimer >= HeartbeatInterval)
                {
                    _heartbeatTimer = 0f;
                    SendHeartbeat();
                }
            }

            try
            {
                if (_clientSocket.Available > 0)
                {
                    int bytesReceived = _clientSocket.Receive(_receiveBuffer, _bufferCount, _receiveBuffer.Length - _bufferCount, SocketFlags.None);
                    _bufferCount += bytesReceived;


                    while (_bufferCount >= 4)
                    {
                        int packetLength = System.BitConverter.ToInt32(_receiveBuffer, 0);

                        if (_bufferCount >= packetLength + 4)

                        {
                            byte[] packetBytes = new byte[packetLength];
                            System.Buffer.BlockCopy(_receiveBuffer, 4, packetBytes, 0, packetLength);

                            ProcessPacket(packetBytes);

                            System.Buffer.BlockCopy(_receiveBuffer, packetLength + 4, _receiveBuffer, 0, _bufferCount - (packetLength + 4));
                            _bufferCount -= (packetLength + 4);
                        }
                        else
                        {
                            break;
                        }

                    }
                }
            }
            catch (SocketException e)
            {
                Debug.LogError(e.ToString());
            }
        }

        #region Public Functions
        public void ConnectToServer(string ipAddress, string playerName)
        {
            _ipAddress = ipAddress;
            _pendingUsername = playerName;

            _clientSocket.Connect(_ipAddress, _port);
            _clientSocket.Blocking = false;

            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.EnableDuckSelection();
            }
        }

        public void ConfirmDuckSelection(int duckChosen)
        {
            _playerData = new PlayerData(duckChosen, _pendingUsername);
            _playersInLobby.Add(_playerData);
            SendPacket(new JoinPacket(_playerData).Serialize());

            SceneManager.LoadScene(1, LoadSceneMode.Single);
            SceneIndex = 1;
        }

        #region Packets helper functions
        public PlayerData GetPlayerData(int duckID)
        {
            PlayerData playerData = PlayersInLobby.First(p => p.DuckID == duckID);
            return playerData;
        }

        #region HeartBeat
        private void SendHeartbeat()
        {
            SendPacket(new HeartbeatPacket().Serialize());
        }
        #endregion
        #region Join
        public void JoinLobby(int duckChosen, string username)
        {
            _playerData = new PlayerData(duckChosen, username);
            _playersInLobby.Add(_playerData);
            SendPacket(new JoinPacket(_playerData).Serialize());
            SceneManager.LoadScene(1, LoadSceneMode.Single);
            SceneIndex = 1;
        }
        #endregion

        #region Chat 
        public void SendChatMessage(string message)
        {
            byte[] buffer = new MessagePacket(PlayerData, message).Serialize();
            ChatMessageSentEvent(PlayerData, message);
            SendPacket(buffer);
        }
        #endregion

        #region Instantiation Logic
        public void InstantiateFromNetwork(InstantiatePacket packet)
        {
            string prefabName = packet.PrefabName;
            if (prefabName.Contains("Prefabs/Ducks")) // We only want to spawn ducks not in the scene yet
            {
                var ncs = FindObjectsOfType<NetworkComponent>();

                foreach (NetworkComponent nc in ncs)
                {
                    if (nc.gameObject.name.Contains($"{packet.PrefabName[^1]}"))
                    {
                        return;
                    }
                }

                GameObject prefab = Resources.Load<GameObject>(prefabName);
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, packet.Position, packet.Rotation);
                    NetworkComponent nc = go.GetComponent<NetworkComponent>();
                    nc.SetObjectData(packet.ObjectID, packet.PlayerData.DuckID, packet.PlayerData.Username);

                    if (SceneManager.GetActiveScene().name.Contains("Minigame_DisappearingTileFloor") && DuckKiller.Instance != null)
                    {
                        DuckKiller.Instance.RegisterDuck(go);
                    }

                    return;
                }
            }
            else
            {
                Debug.LogError($"Instantiating {packet.PrefabName} from network");
                GameObject prefab = Resources.Load<GameObject>(packet.PrefabName);
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, packet.Position, packet.Rotation);
                    NetworkComponent nc = go.GetComponent<NetworkComponent>();

                    nc.SetObjectData(packet.ObjectID, packet.PlayerData.DuckID, packet.PlayerData.Username);
                }
            }
        }

        public void InstantiateOverNetwork(string prefabName, Vector3 position, Quaternion rotation, PlayerData player)
        {
            if (prefabName.Contains("Prefabs/Ducks"))
            {
                var ncs = FindObjectsOfType<NetworkComponent>();

                foreach (NetworkComponent nc in ncs)
                {
                    if (nc.gameObject.name.Contains($"{prefabName[^1]}"))
                    {
                        InstantiatePacket ip = new InstantiatePacket(player, nc.GameObjectID, prefabName, position, rotation);
                        SendPacket(ip.Serialize());
                        return;
                    }
                }
            }

            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, position, rotation);
                NetworkComponent nc = go.GetComponent<NetworkComponent>();
                var objectID = System.Guid.NewGuid();
                nc.SetObjectData(objectID.ToString(), player.DuckID);

                InstantiatePacket ip = new InstantiatePacket(player, objectID.ToString(), prefabName, position, rotation);
                SendPacket(ip.Serialize());
            }
        }
        #endregion

        #region Position 
        public void SendPositionPacket(PositionPacket packet)
        {
            SendPacket(packet.Serialize());
        }
        #endregion

        #region Destroy
        public void SendDestroyPacket(DestroyPacket packet)
        {
            SendPacket(packet.Serialize());
        }
        #endregion

        #region Ready Status
        public void SendReadyStatus(bool isReady)
        {
            var readyPacket = new ReadinessPacket(_playerData, isReady);
            SendPacket(readyPacket.Serialize());
        }
        #endregion

        private void ProcessPacket(byte[] packetBytes)
        {
            int bufferSize = packetBytes.Length;
            int offset = 0;

            BasePacket basePacket = new BasePacket();
            basePacket.Deserialize(packetBytes, ref bufferSize, ref offset);

            #region Packet Switch cases
            switch (basePacket.Type)
            {
                case BasePacket.PacketType.None:
                    Debug.LogError("Packet type is None");
                    break;

                case BasePacket.PacketType.Join:
                    {
                        JoinPacket jp = new JoinPacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        bool isInLobby = _playersInLobby.Exists(p => p.Username == jp.PlayerData.Username && p.DuckID == jp.PlayerData.DuckID);
                        if (!isInLobby)
                        {
                            _playersInLobby.Add(jp.PlayerData);
                            PlayerConnectedEvent?.Invoke(jp.PlayerData);
                        }
                        break;
                    }

                case BasePacket.PacketType.ClientList:
                    {
                        Debug.LogError("Client list received");
                        PlayerDataListPacket pdlp = new PlayerDataListPacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        foreach (PlayerData pd in pdlp.Players)
                        {
                            if (pd.Username == _playerData.Username)
                                continue;

                            bool exists = _playersInLobby.Exists(p => p.Username == pd.Username && p.DuckID == pd.DuckID);
                            if (!exists)
                            {
                                _playersInLobby.Add(pd);
                                PlayerConnectedEvent?.Invoke(pd);
                            }
                        }
                        break;
                    }

                case BasePacket.PacketType.Message:
                    {
                        MessagePacket mp = new MessagePacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        ChatMessageReceivedEvent(mp.PlayerData, mp.Message);
                        break;
                    }

                case BasePacket.PacketType.Instantiate:
                    {
                        InstantiatePacket ip = new InstantiatePacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        InstantiateFromNetwork(ip);
                        break;
                    }

                case BasePacket.PacketType.Position:
                    {
                        PositionPacket pp = new PositionPacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        PositionPacketReceivedEvent(pp);
                        break;
                    }

                case BasePacket.PacketType.Destroy:
                    {
                        var dp = new DestroyPacket()
                            .Deserialize(packetBytes, ref bufferSize, ref offset);

                        if (dp == null || dp.PlayerData == null)
                        {
                            break;
                        }

                        DestroyPacketReceivedEvent?.Invoke(dp);
                        break;
                    }

                case BasePacket.PacketType.ReadyStatus:
                    {
                        ReadinessPacket rp = new ReadinessPacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        PlayerReadinessChangedEvent?.Invoke(rp.PlayerData, rp.IsReady);
                        break;
                    }

                case BasePacket.PacketType.SceneChange:
                    {
                        Debug.LogError("Scene change received");
                        SceneChangePacket scp = new SceneChangePacket().Deserialize(packetBytes, ref bufferSize, ref offset);
                        SceneHost = scp.PlayerData;
                        if (scp.SceneID >= 0)
                        {
                            SceneManager.LoadScene(scp.SceneID);
                            SceneIndex = scp.SceneID;
                        }
                        else
                        {
                            DuckSpawner duckSpawner = FindObjectOfType<DuckSpawner>();
                            if (duckSpawner != null)
                            {
                                duckSpawner.SpawnPlayer(PlayerData);
                            }
                        }
                        break;
                    }

                default:
                    break;
            }
            #endregion
        }
        #endregion

        #region Packet Sending Helper
        public void SendPacket(byte[] packetData)
        {
            int packetLength = packetData.Length;
            byte[] lengthPrefix = System.BitConverter.GetBytes(packetLength);

            byte[] finalPacket = new byte[lengthPrefix.Length + packetData.Length];
            System.Buffer.BlockCopy(lengthPrefix, 0, finalPacket, 0, lengthPrefix.Length);
            System.Buffer.BlockCopy(packetData, 0, finalPacket, lengthPrefix.Length, packetData.Length);

            _clientSocket.Send(finalPacket);
        }
        private void OnDestroyPacketReceived(DestroyPacket dp)
        {
            _playersInLobby.RemoveAll(p => p.DuckID == dp.PlayerData.DuckID);

            var nc = FindObjectsOfType<NetworkComponent>()
                .FirstOrDefault(n => n.OwnerID == dp.PlayerData.DuckID);
            if (nc != null) Destroy(nc.gameObject);
        }

        #endregion 
    }
}
#endregion