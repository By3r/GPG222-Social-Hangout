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
        [Header("Server Connection info")] [SerializeField]
        private string _ipAddress = "127.0.0.1";

        [SerializeField] private int _port = 5500;

        [Header("Debug Info")] [SerializeField]
        private TMP_Text _feedbackText;

        [SerializeField] private TMP_Text _serverCountText;
        [SerializeField] private Button _clearButton;

        // Client info
        private List<Socket> _clientsInServer = new List<Socket>();

        [Tooltip("To track each player's readiness")]
        private Dictionary<int, bool> _playerReadyStatus = new Dictionary<int, bool>();

        private List<PlayerData> _playersInLobby = new List<PlayerData>();
        private Socket server;

        private int _feedbackLineCount = 0;

        private void Start()
        {
            // Spin up the server
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
            server.Listen(4);
            server.Blocking = false;
            Debug.LogError("Server Started Up!");
            Log("Server Started Up Successfully!\n");

            _clearButton.onClick.AddListener(ClearFeedbackText);
            _serverCountText.text = "Server Count: 0";
        }

        private void Update()
        {
            try // Try to have a client connect
            {
                Socket newClient = server.Accept();
                newClient.Blocking = false;
                Debug.LogError("Client socket connected");
                Log("Client socket connected\n");
                // TODO: Send packet to the new client with a list of all the current clients on the server so we can check duck availability

                // Add the client to the list of clients on the server if they aren't already
                _clientsInServer.Add(newClient);
                // Log($"Client with socket {newClient} has been added to the list of clients in the server\n";
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

            if (_clientsInServer.Count == 0) // Only bother checking client stuff if there are clients
            {
                return;
            }

            for (int i = 0; i < _clientsInServer.Count; i++) // Go through each client in the server
            {
                Socket client = _clientsInServer[i];
                if (client.Available > 0) // If the client has a packet we check to see if it should be broadcast
                {
                    Log("=====\n");
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
                        Log($"Packet type: {bp.Type} | Size: {bp.Size} | Offset: {offset}\n");

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

                                var clientList = _playersInLobby.FindAll(
                                    p => !(p.DuckID == newPlayer.DuckID && p.Username == newPlayer.Username));
                                PlayerDataListPacket pdlp = new PlayerDataListPacket(clientList);
                                _clientsInServer[i].Send(pdlp.Serialize());
                                
                                SceneChangePacket hostSetPacket = new SceneChangePacket(_playersInLobby[0], -1);
                                BroadcastToAllPlayersInLobby(hostSetPacket.Serialize(), -1);
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

                                Log($"  Prefab Name: {ip.PrefabName}\n");
                                Log($"  Owner ID: {ip.PlayerData.DuckID}\n");

                                break;

                            case BasePacket.PacketType.Position:
                                PositionPacket pp = new PositionPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                BroadcastToAllPlayersInLobby(pp.Serialize(), i);
                                /*
                                Log($"  Owner ID: {pp.OwnerID} | Object ID: {pp.ObjectID}\n";
                                Log($"  Position: {pp.Position} | Rotation: {pp.Rotation}\n";
                                */
                                break;

                            case BasePacket.PacketType.Destroy:
                                DestroyPacket dp = new DestroyPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                BroadcastToAllPlayersInLobby(dp.Serialize(), i);

                                Log($"  Owner ID: {dp.PlayerData.DuckID}\n");
                                Log($"  Position: {dp.ObjectID}\n");

                                break;

                            case BasePacket.PacketType.ReadyStatus:
                                ReadinessPacket readyPacket = new ReadinessPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                _playerReadyStatus[readyPacket.PlayerData.DuckID] = readyPacket.IsReady;

                                Log($"  {readyPacket.PlayerData.Username}'s readiness status is: {readyPacket.IsReady}\n");

                                #region Checks if all players are ready

                                if (_playerReadyStatus.Count == _playersInLobby.Count && !_playerReadyStatus.ContainsValue(false))
                                {
                                    if (_playersInLobby.Count < 2) return;

                                    int chosenScene = 3;
                                    Log($"[TEST MODE] All players ready — loading Scene {chosenScene}");

                                    SceneChangePacket scp = new SceneChangePacket(_playersInLobby[0], chosenScene);
                                    BroadcastToAllPlayersInLobby(scp.Serialize(), -1);

                                    Log("== All players are READY! ==\n");
                                }


                                #endregion

                                break;
                            
                            case BasePacket.PacketType.SceneChange:
                                SceneChangePacket sceneChangePacket = new SceneChangePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                break;

                            default:
                                stopPacketSpliting = true;
                                break;
                        }

                        if (stopPacketSpliting) { break; }
                    }
                }
            }
        }

        public void ResetServer()
        {
            _clientsInServer.Clear();
            _playersInLobby.Clear();
            _playerReadyStatus.Clear();

            _feedbackText.text = "";
            _serverCountText.text = "Server Count: 0";
        }

        private void BroadcastToAllPlayersInLobby(byte[] buffer, int sender)
        {
            if (sender >= 0)
            {
                for (int i = 0; i < _clientsInServer.Count; i++)
                {
                    if (i == sender) continue;

                    _clientsInServer[i].Send(buffer);
                }
            }
            else
            {
                for (int i = 0; i < _playersInLobby.Count; i++)
                {
                    _clientsInServer[i].Send(buffer);
                }
            }
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
    }
}