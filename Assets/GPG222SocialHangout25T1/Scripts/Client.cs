using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Networking.Packets;
using UnityEngine;

namespace Networking.Core
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _port = 5500;
        private Socket _clientSocket;

        private PlayerData _playerData;
        private List<PlayerData> _playersInLobby = new List<PlayerData>();
        
        public List<PlayerData> PlayersInLobby { get { return _playersInLobby; } }
        public PlayerData PlayerData { get; set; }

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
        }

        private void Start()
        {
            try // Attempt to connect to the server
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(_ipAddress, _port);
                _clientSocket.Blocking = false;
                Debug.LogError("Client socket connected");
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
            if (_clientSocket.Available > 0)
            {
                try
                {
                    // Get all the data from the buffer of data we have recieved
                    byte[] buffer = new byte[_clientSocket.Available];
                    _clientSocket.Receive(buffer);
                    
                    int bufferSize = buffer.Length;
                    int offset = 0;

                    while (bufferSize > 0)
                    {
                        // Deserialize the base of the packet to expose its packet type
                        BasePacket basePacket = new BasePacket();
                        basePacket.Deserialize(buffer, ref bufferSize, ref offset);

                        switch (basePacket.Type)
                        {
                            case BasePacket.PacketType.None:
                                break;
                            case BasePacket.PacketType.Join:
                                JoinPacket joinPacket = new JoinPacket().Deserialize(buffer, ref bufferSize,  ref offset);
                                break;
                            case BasePacket.PacketType.ClientList:
                                // TODO: Add a client list packet to send a list of PlayerData for all the clients in the lobby or server
                                break;
                            case BasePacket.PacketType.Message:
                                // TODO: Add a message packet to send and recieve messages
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

        public void JoinLobby(PlayerData playerData)
        {
            _playerData = playerData;
            _clientSocket.Send(new JoinPacket(playerData).Serialize());
        }

        // Sends the packet to the server to be distributed as needed
        public void SendPacket(BasePacket packet)
        {
            switch (packet.Type)
            {
                case BasePacket.PacketType.None:
                    break;
                case BasePacket.PacketType.Join:
                    byte[] buffer = new JoinPacket(packet.PlayerData).Serialize();
                    _clientSocket.Send(buffer);
                    break;
                case BasePacket.PacketType.Message:
                    // TODO: Implement sending a chat message packet
                    break;
                default:
                    break;
            }
        }
    }
}
