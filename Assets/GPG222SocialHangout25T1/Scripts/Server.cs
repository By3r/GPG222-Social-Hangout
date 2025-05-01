using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Networking.Packets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Networking.Packets.BasePacket;

namespace Networking.Core
{
    public class Server : MonoBehaviour
    {
        #region Variables
        [Header("Server Connection info")]
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 5500;

        [Header("Debug Info")]
        [SerializeField] private TMP_Text _feedbackText;
        [SerializeField] private TMP_Text _serverCountText;
        [SerializeField] private Button _clearButton;

        // Client info
        private List<Socket> _clientsInServer = new List<Socket>();

        [Tooltip("To track each player's readiness")]
        private Dictionary<int, bool> _playerReadyStatus = new Dictionary<int, bool>();

        private List<PlayerData> _playersInLobby = new List<PlayerData>();

        private Socket server;

        private int _feedbackLineCount = 0;

        private Dictionary<Socket, byte[]> _receiveBuffers = new Dictionary<Socket, byte[]>();
        private Dictionary<Socket, int> _bufferCounts = new Dictionary<Socket, int>();
        private Dictionary<Socket, DateTime> _lastHeartbeatTimes = new();
        private Dictionary<Socket, PlayerData> _socketToPlayer = new(); // this maps each client to their playerdata //

        private readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(5);

        #endregion

        private void Start()
        {
            // Spin up the server
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
            server.Listen(4);
            server.Blocking = false;

            Log("Server Started Up Successfully!\n");

            _clearButton.onClick.AddListener(ClearFeedbackText);
            _serverCountText.text = "Server Count: 0";
        }

        private void Update()
        {
            #region new heartbeat integration
            DateTime now = DateTime.UtcNow;
            List<Socket> toRemove = new List<Socket>();

            foreach (var kvp in _lastHeartbeatTimes)
            {
                if ((now - kvp.Value) > HeartbeatTimeout)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (Socket deadClient in toRemove)
            {
                Log($"<color=red>💀 No heartbeat from {(_socketToPlayer.ContainsKey(deadClient) ? _socketToPlayer[deadClient].Username : "Unknown")} — Disconnecting.</color>\n");
                DisconnectClient(deadClient);
            }
            #endregion

            // Accept new clients
            try
            {
                Socket newClient = server.Accept();
                newClient.Blocking = false;
                Debug.LogError("Client socket connected");
                Log("Client socket connected\n");

                // Add the client to the list of clients on the server if they aren't already
                _clientsInServer.Add(newClient);
                _receiveBuffers[newClient] = new byte[8192];
                _bufferCounts[newClient] = 0;

                Log($"Clients connected to server: {+_clientsInServer.Count}\n");
                _serverCountText.text = $"Server Count: {_clientsInServer.Count}\n";
            }
            catch (SocketException e) // Otherwise print out the error Message
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }

            for (int i = 0; i < _clientsInServer.Count; i++)
            {
                Socket client = _clientsInServer[i];

                if (client.Available > 0)
                {
                    try
                    {
                        byte[] buffer = _receiveBuffers[client];
                        int bufferCount = _bufferCounts[client];

                        int bytesReceived = client.Receive(buffer, bufferCount, buffer.Length - bufferCount, SocketFlags.None);
                        bufferCount += bytesReceived;

                        while (bufferCount >= 4)
                        {
                            int packetLength = System.BitConverter.ToInt32(buffer, 0);

                            if (bufferCount >= packetLength + 4)
                            {
                                byte[] packetBytes = new byte[packetLength];
                                System.Buffer.BlockCopy(buffer, 4, packetBytes, 0, packetLength);

                                ProcessPacket(packetBytes, client, i);

                                System.Buffer.BlockCopy(buffer, packetLength + 4, buffer, 0, bufferCount - (packetLength + 4));
                                bufferCount -= (packetLength + 4);
                            }
                            else
                            {
                                break;
                            }
                        }

                        _bufferCounts[client] = bufferCount;
                    }
                    catch (SocketException e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }
            }
        }

        #region Public Functions
        public void ResetServer()
        {
            _clientsInServer.Clear();
            _playersInLobby.Clear();
            _playerReadyStatus.Clear();
            _receiveBuffers.Clear();
            _bufferCounts.Clear();

            _feedbackText.text = "";
            _serverCountText.text = "Server Count: 0";
        }
        #endregion

        #region Private Functions
        private void ProcessPacket(byte[] buffer, Socket client, int clientIndex)
        {
            PacketType type = (PacketType)BitConverter.ToInt32(buffer, 0);

            if (type == PacketType.Heartbeat)
            {
                _lastHeartbeatTimes[client] = DateTime.UtcNow;
                if (_socketToPlayer.TryGetValue(client, out var s))
                    Log($"<color=green>Heartbeat from {s.Username}</color>\n");
                else
                    Log("<color=yellow>Heartbeat  an unknown client</color>\n");
                return;
            }

            BasePacket bp = new BasePacket();
            int bufferSize = buffer.Length;
            int offset = 0;
            bp.Deserialize(buffer, ref bufferSize, ref offset);

            Log($"Packet type: {bp.Type} | Size: {bp.Size} | Offset: {offset}\n");

            #region Packets Switch case
            switch (bp.Type)
            {
                case BasePacket.PacketType.None:
                    {
                        break;
                    }

                case BasePacket.PacketType.Join:
                    {
                        JoinPacket jp = new JoinPacket().Deserialize(buffer, ref bufferSize, ref offset);
                        PlayerData newPlayer = jp.PlayerData;

                        bool alreadyInLobby = _playersInLobby.Exists(p => p.DuckID == newPlayer.DuckID && p.Username == newPlayer.Username);
                        if (!alreadyInLobby)
                        {
                            _playersInLobby.Add(newPlayer);
                            _socketToPlayer[client] = newPlayer;

                            for (int existingIndex = 0; existingIndex < _clientsInServer.Count; existingIndex++)
                            {
                                if (existingIndex == clientIndex) continue;
                                SendPacket(_clientsInServer[existingIndex], jp.Serialize());
                            }
                        }
                        else
                        {
                            Debug.Log($" !!Server cs line 120!!: There is a duplicate of duck ID {newPlayer.DuckID} and username {newPlayer.Username}");
                        }

                        var clientList = _playersInLobby.FindAll(p => !(p.DuckID == newPlayer.DuckID && p.Username == newPlayer.Username));
                        PlayerDataListPacket pdlp = new PlayerDataListPacket(clientList);
                        SendPacket(client, pdlp.Serialize());

                        SceneChangePacket hostSetPacket = new SceneChangePacket(_playersInLobby[0], -1);
                        BroadcastToAllPlayersInLobby(hostSetPacket.Serialize(), -1);
                        break;
                    }

                case BasePacket.PacketType.ClientList:
                    {
                        break;
                    }

                case BasePacket.PacketType.Message:
                    {
                        MessagePacket mp = new MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
                        byte[] mpb = mp.Serialize();
                        BroadcastToAllPlayersInLobby(mpb, clientIndex);
                        break;
                    }

                case BasePacket.PacketType.Instantiate:
                    {
                        InstantiatePacket ip = new InstantiatePacket().Deserialize(buffer, ref bufferSize, ref offset);
                        BroadcastToAllPlayersInLobby(ip.Serialize(), clientIndex);

                        Log($"  Prefab Name: {ip.PrefabName}\n");
                        Log($"  Owner ID: {ip.PlayerData.DuckID}\n");

                        break;
                    }

                case BasePacket.PacketType.Position:
                    {
                        PositionPacket pp = new PositionPacket().Deserialize(buffer, ref bufferSize, ref offset);
                        BroadcastToAllPlayersInLobby(pp.Serialize(), clientIndex);
                        break;
                    }

                case BasePacket.PacketType.Destroy:
                    {
                        DestroyPacket dp = new DestroyPacket().Deserialize(buffer, ref bufferSize, ref offset);
                        BroadcastToAllPlayersInLobby(dp.Serialize(), clientIndex);

                        Log($"  Owner ID: {dp.PlayerData.DuckID}\n");
                        Log($"  Object ID: {dp.ObjectID}\n");
                        break;
                    }

                case BasePacket.PacketType.ReadyStatus:
                    {
                        ReadinessPacket readyPacket = new ReadinessPacket().Deserialize(buffer, ref bufferSize, ref offset);
                        _playerReadyStatus[readyPacket.PlayerData.DuckID] = readyPacket.IsReady;

                        Log($"  {readyPacket.PlayerData.Username}'s readiness status is: {readyPacket.IsReady}\n");
                        #region Checks if all players are ready

                        if (_playerReadyStatus.Count == _playersInLobby.Count && !_playerReadyStatus.ContainsValue(false))
                        {
                            if (_playersInLobby.Count < 2) return;

                            int chosenScene = UnityEngine.Random.Range(2, 4);
                            Log($"All players ready — loading Scene {chosenScene}");

                            SceneChangePacket scp = new SceneChangePacket(_playersInLobby[0], chosenScene);
                            BroadcastToAllPlayersInLobby(scp.Serialize(), -1);

                            Log("== All players are READY! ==\n");
                        }

                        #endregion

                        break;
                    }

                case BasePacket.PacketType.SceneChange:
                    {
                        SceneChangePacket sceneChangePacket = new SceneChangePacket().Deserialize(buffer, ref bufferSize, ref offset);
                        break;
                    }

                case BasePacket.PacketType.Heartbeat:
                    {
                        var hb = new HeartbeatPacket().Deserialize(buffer, ref bufferSize, ref offset);
                        _lastHeartbeatTimes[client] = DateTime.UtcNow;
                        if (_socketToPlayer.TryGetValue(client, out var sender))
                            Log($"<color=green>Heartbeat from {sender.Username}</color>\n");
                        else
                            Log("<color=yellow>Heartbeat from unknown client</color>\n");
                        break;
                    }

                default:
                    {
                        break;
                    }

                    #endregion
            }
        }

        private void BroadcastToAllPlayersInLobby(byte[] buffer, int sender)
        {
            if (sender >= 0)
            {
                for (int i = 0; i < _clientsInServer.Count; i++)
                {
                    if (i == sender) continue;
                    SendPacket(_clientsInServer[i], buffer);
                }
            }
            else
            {
                for (int i = 0; i < _clientsInServer.Count; i++)
                {
                    SendPacket(_clientsInServer[i], buffer);
                }
            }
        }

        private void SendPacket(Socket client, byte[] packetData)
        {
            int packetLength = packetData.Length;
            byte[] lengthPrefix = System.BitConverter.GetBytes(packetLength);

            byte[] finalPacket = new byte[lengthPrefix.Length + packetData.Length];
            System.Buffer.BlockCopy(lengthPrefix, 0, finalPacket, 0, lengthPrefix.Length);
            System.Buffer.BlockCopy(packetData, 0, finalPacket, lengthPrefix.Length, packetData.Length);

            client.Send(finalPacket);
        }

        private void Log(string message)
        {
            if (_feedbackLineCount >= 25)
            {
                _feedbackText.text = "";
                _feedbackLineCount = 0;
            }

            _feedbackLineCount++;
            _feedbackText.text += message;
        }

        private void ClearFeedbackText()
        {
            _feedbackText.text = "";
        }
        private void DisconnectClient(Socket client)
        {
            _lastHeartbeatTimes.Remove(client);
            if (!_socketToPlayer.TryGetValue(client, out var gone))
            {
                gone = null;
            }
            _socketToPlayer.Remove(client);

            _clientsInServer.Remove(client);
            _receiveBuffers.Remove(client);
            _bufferCounts.Remove(client);
            client.Close();

            if (gone != null)
            {
                _playersInLobby.RemoveAll(p => p.DuckID == gone.DuckID && p.Username == gone.Username);
                _playerReadyStatus.Remove(gone.DuckID);

                var destroy = new DestroyPacket(gone, "Duck" + gone.DuckID);
                BroadcastToAllPlayersInLobby(destroy.Serialize(), -1);
            }

            var listPkt = new PlayerDataListPacket(_playersInLobby);
            BroadcastToAllPlayersInLobby(listPkt.Serialize(), -1);

            if (_playersInLobby.Count > 0)
            {
                var newHost = _playersInLobby[0];
                var hostPkt = new SceneChangePacket(newHost, -1);
                BroadcastToAllPlayersInLobby(hostPkt.Serialize(), -1);
            }

            Log($"<color=red>Client {gone?.Username ?? "Unknown"} disconnected</color>\n");
        }

        #endregion
    }
}