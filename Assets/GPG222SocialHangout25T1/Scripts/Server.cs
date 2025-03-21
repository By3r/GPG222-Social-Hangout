using System;
using System.Collections;
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
        // Server Connection info
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 5500;
        private Socket server;

        // Client info
        private List<Socket> _clientsInLobby = new List<Socket>();
        private List<Socket> _clientsInServer = new List<Socket>();
        private List<PlayerData> _playersInLobby = new List<PlayerData>();
        public List<PlayerData> PlayersInLobby { get; }
        
        // Debug Info
        [SerializeField] private TMP_Text _feedbackText;
        [SerializeField] private TMP_Text _serverCountText;
        [SerializeField] private TMP_Text _lobbyCountText;
        [SerializeField] private Button _clearButton;
        
        // Start is called before the first frame update
        void Start()
        {
            // Spin up the server
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
            server.Listen(100);
            server.Blocking = false;
            Debug.LogError("Server Started Up!");
            _feedbackText.text = "Server Started Up Successfully!\n";
            
            _clearButton.onClick.AddListener(ClearFeedbackText);

            _serverCountText.text = "Server Count: 0";
            _lobbyCountText.text = "Lobby Count: 0";
        }

        // Update is called once per frame
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
                _feedbackText.text += $"Clients connected to server: {+_clientsInServer.Count} | Clients in lobby: {_clientsInLobby.Count}\n";
                _serverCountText.text = $"Server Count: {_clientsInServer.Count}\n";
                _lobbyCountText.text = $"Lobby Count: {_clientsInLobby.Count}\n";
                if (!_clientsInServer.Contains(newClient))
                {
                }
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
                    bp.Deserialize(buffer,ref bufferSize, ref offset);
                    
                    while (bufferSize > 0)
                    {
                            // bp.Deserialize(buffer, ref bufferSize, ref offset);
                        Debug.LogError($"Packet Type: {bp.Type}");
                        Debug.LogError(
                            $"Buffer Size: {bp.Size} | Packet Size: {bp.Size} | Offset: {offset}");
                        _feedbackText.text += $"Packet type: {bp.Type} | Size: {bp.Size} | Offset: {offset}\n";

                        switch (bp.Type)
                        {
                            case BasePacket.PacketType.None:
                                break;
                            
                            case BasePacket.PacketType.Join:
                                JoinPacket jp = new JoinPacket().Deserialize(buffer, ref bufferSize, ref offset);
                                byte[] jpb = jp.Serialize();
                                BroadcastToAllPlayersInLobby(jpb, i);
                                break;
                                
                            case BasePacket.PacketType.ClientList:
                                Debug.LogError("Server received Client List Packet");
                                break;
                            
                            case BasePacket.PacketType.Message:
                                MessagePacket mp = new MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                byte[] mpb = mp.Serialize();
                                BroadcastToAllPlayersInLobby(mpb, i);
                                Debug.LogError("Server received Message Packet");
                                break;
                            
                            default:
                                stopPacketSpliting = true;
                                break;
                        }
                        
                        if  (stopPacketSpliting) {break;}
                    }
                    
                    _feedbackText.text += "=====";
                }
            }
        }

        public void ResetServer()
        {
            _clientsInLobby.Clear();
            _clientsInServer.Clear();
            _playersInLobby.Clear();
            
            _feedbackText.text = "";
            _serverCountText.text = "Server Count: 0";
            _lobbyCountText.text = "Lobby Count: 0";
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
