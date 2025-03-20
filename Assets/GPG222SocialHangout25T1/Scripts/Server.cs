using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Networking.Packets;
using UnityEngine;

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
        
        // Start is called before the first frame update
        void Start()
        {
            // Spin up the server
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
            server.Listen(100);
            server.Blocking = false;
            Debug.LogError("Server Started Up!");
        }

        // Update is called once per frame
        void Update()
        {
            try // Try to have a client connect
            {
                Socket newClient = server.Accept();
                newClient.Blocking = false;
                Debug.LogError("Client socket connected");
                // TODO: Send packet to the new client with a list of all the current clients on the server so we can check duck availability

                // Add the client to the list of clients on the server if they aren't already
                if (!_clientsInServer.Contains(newClient))
                {
                    _clientsInServer.Add(newClient);
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
                    // Get the packets in the client's buffer
                    byte[] buffer = new byte[client.Available];
                    int bufferSize = buffer.Length;
                    int offset = 0;
                    bool stopPacketSpliting = false;
                    
                    // Get the packet's type
                    BasePacket basePacket = new BasePacket();
                    basePacket.Deserialize(buffer,ref bufferSize, ref offset);
                    Debug.LogError($"Buffer size: {bufferSize} bytes, offset: {offset}");
                    
                    while (bufferSize > 0)
                    {
                        Debug.LogError($"Server buffer size: {bufferSize}");
                        switch (basePacket.Type)
                        {
                            case BasePacket.PacketType.None:
                                break;
                            case BasePacket.PacketType.Join: // the client has joined the lobby so needs to be sent to other clients
                                Debug.LogError("Server received Join Packet");
                                _clientsInLobby.Add(client);
                                JoinPacket jp = new JoinPacket().Deserialize(buffer, ref bufferSize,  ref offset);
                                BroadcastToAllPlayersInLobby(jp.Serialize());
                                break;
                            case BasePacket.PacketType.ClientList:
                                Debug.LogError("Server received Client List Packet");
                                break;
                            case BasePacket.PacketType.Message:
                                Debug.LogError("Server received Message Packet");
                                MessagePacket mp = new  MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                BroadcastToAllPlayersInLobby(mp.Serialize());
                                break;
                            default:
                                stopPacketSpliting = true;
                                break;
                        }
                        
                        if  (stopPacketSpliting) {break;}
                    }
                }
            }
        }

        private void BroadcastToAllPlayersInLobby(byte[] buffer)
        {
            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                _clientsInLobby[i].Send(buffer);
            }
        }

        private void BroadcastToAllPlayersInServer(BasePacket packet)
        {
            foreach (var client in _clientsInServer)
            {
                if (packet.Type == BasePacket.PacketType.Join)
                {
                    
                }
            }
        }
    }
}
