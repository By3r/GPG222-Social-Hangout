using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Networking.Packets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.Core
{
    public class Server : MonoBehaviour
    {
        [Header("Server Connection info")]
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 5500;
        private Socket server;

        // Client info
        private List<Socket> _clientsInServer = new List<Socket>();
        private List<PlayerData> _playersInLobby = new List<PlayerData>();
        [Tooltip("To track each player's readiness")]
        private Dictionary<int, bool> _playerReadyStatus = new Dictionary<int, bool>();

        [Header("Debug Info")]
        [SerializeField] private TMP_Text _feedbackText;
        [SerializeField] private TMP_Text _serverCountText;
        [SerializeField] private Button _clearButton;

        private const int MinPlayersToStart = 2;

        void Start()
        {
            // Spin up the server
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
            server.Listen(4);
            server.Blocking = false;
            Debug.LogError("Server Started Up!");
            _feedbackText.text = "Server Started Up Successfully!\n";

            _clearButton.onClick.AddListener(ClearFeedbackText);
            _serverCountText.text = "Server Count: 0";
        }

        void Update()
        {
            try // Try to have a client connect
            {
                Socket newClient = server.Accept();
                newClient.Blocking = false;
                Debug.LogError("Client socket connected");
                _feedbackText.text += "Client socket connected\n";
                // TODO: Send packet to the new client with a list of all the current clients on the server so we can check duck availability

                // Add the client to the list of clients on the server if they aren't already
                _clientsInServer.Add(newClient);
                // _feedbackText.text += $"Client with socket {newClient} has been added to the list of clients in the server\n";
                _feedbackText.text += $"Clients connected to server: {+_clientsInServer.Count}\n";
                _serverCountText.text = $"Server Count: {_clientsInServer.Count}\n";
            }
            catch (SocketException e) // Otherwise print out the error Message
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }

            }

            if (_clientsInServer.Count == 0) // Only bother checking client stuff if there are clients
            {
                return;
            }

            for (int i = 0; i < _clientsInServer.Count; i++) // Go through each client in the server
            {
                Socket client = _clientsInServer[i];
                if (client.Available > 0) // If the client has a packet we check to see if it should be broadcast
                {
                    _feedbackText.text += "=====\n";
                    // Get the packets in the client's buffer
                    byte[] buffer = new byte[client.Available];
                    client.Receive(buffer); // Actually get the stuff in the client's buffer

                    // Set up buffer tracking for packet splitting
                    int bufferSize = buffer.Length;
                    int offset = 0;
                    bool stopPacketSpliting = false;

                    // Get the packet's type
                    BasePacket bp = new BasePacket();
                    bp.Deserialize(buffer, ref bufferSize, ref offset);

                    while (bufferSize > 0)
                    {
                        _feedbackText.text += $"Packet type: {bp.Type} | Size: {bp.Size} | Offset: {offset}\n";

                        switch (bp.Type)
                        {
                            case BasePacket.PacketType.None:
                                break;

                            case BasePacket.PacketType.Join:
                                JoinPacket jp = new JoinPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                PlayerData newPlayer = jp.PlayerData;

                                // Using duck id and the player's username we only add them to existing player list if they exist
                                bool alreadyInLobby = _playersInLobby.Exists(
                                    p => p.DuckID == newPlayer.DuckID && p.Username == newPlayer.Username);
                                if (!alreadyInLobby)
                                {
                                    _playersInLobby.Add(newPlayer);

                                    for (int existingIndex = 0; existingIndex < _clientsInServer.Count; existingIndex++)
                                    {
                                        if (existingIndex == i) continue;
                                        _clientsInServer[existingIndex].Send(jp.Serialize());
                                    }
                                }
                                else
                                {
                                    Debug.Log($" !!Server cs line 120!!: There is a duplicate of duck ID {newPlayer.DuckID} and username {newPlayer.Username}");
                                }

                                List<PlayerData> clientList = _playersInLobby.FindAll(
                                    p => !(p.DuckID == newPlayer.DuckID && p.Username == newPlayer.Username));
                                PlayerDataListPacket pdlp = new PlayerDataListPacket(clientList);
                                _clientsInServer[i].Send(pdlp.Serialize());
                                break;

                            case BasePacket.PacketType.ClientList:
                                break;

                            case BasePacket.PacketType.Message:
                                MessagePacket mp = new MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                byte[] mpb = mp.Serialize();
                                BroadcastToAllPlayersInLobby(mpb, i);
                                break;

                            case BasePacket.PacketType.Instantiate:
                                InstantiatePacket ip =
                                    new InstantiatePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                BroadcastToAllPlayersInLobby(ip.Serialize(), i);
                                break;

                            case BasePacket.PacketType.Position:
                                PositionPacket pp = new PositionPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                BroadcastToAllPlayersInLobby(pp.Serialize(), i);
                                break;

                            case BasePacket.PacketType.Destroy:
                                DestroyPacket dp = new DestroyPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                BroadcastToAllPlayersInLobby(dp.Serialize(), i);
                                break;
                            case BasePacket.PacketType.ReadyStatus:
                                {
                                    ReadinessPacket rp = new ReadinessPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                    _playerReadyStatus[rp.PlayerData.DuckID] = rp.IsReady;

                                    _feedbackText.text += $"{rp.PlayerData.Username}'s readiness status is: {rp.IsReady}\n";

                                    #region Check if there are enough players to start (readiness ignored till the min requirement is met)
                                    bool hasEnoughPlayers = _playersInLobby.Count >= MinPlayersToStart;

                                    bool allReady = hasEnoughPlayers && _playerReadyStatus.Count == _playersInLobby.Count && !_playerReadyStatus.ContainsValue(false);

                                    if (allReady)
                                    {
                                        _feedbackText.text += "== All players are READY! ==\n";
                                        // TODO: Hide the readiness button
                                        // TODO: Enable minigame voting buttons (aka two for now)
                                        _feedbackText.text += "Voting commences now!";
                                    }
                                    #endregion
                                    else if (hasEnoughPlayers)
                                    {
                                        _feedbackText.text += "Waiting for all players to be ready.\n";
                                    }
                                    else
                                    {
                                        _feedbackText.text += "Need at least 2 players to start.\n";
                                    }
                                    break;
                                }



                            default:
                                stopPacketSpliting = true;
                                break;
                        }

                        if (stopPacketSpliting) { break; }
                    }

                    _feedbackText.text += "=====\n";
                }
            }
        }

        public void ResetServer()
        {
            _clientsInServer.Clear();
            _playersInLobby.Clear();

            _feedbackText.text = "";
            _serverCountText.text = "Server Count: 0";
        }

        private void BroadcastToAllPlayersInLobby(byte[] buffer, int sender)
        {
            for (int i = 0; i < _clientsInServer.Count; i++)
            {
                if (i == sender) continue;

                _clientsInServer[i].Send(buffer);
            }
        }

        private void ClearFeedbackText()
        {
            _feedbackText.text = "";
        }
    }
}
