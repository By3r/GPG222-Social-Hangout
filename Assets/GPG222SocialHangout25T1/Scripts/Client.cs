using System;
using System.Collections;
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

            if (PlayerData == null)
            {
                PlayerData = new PlayerData();
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
                                JoinPacket jp = new JoinPacket().Deserialize(buffer, ref bufferSize,  ref offset);
                                if (!_playersInLobby.Contains(jp.PlayerData))
                                {
                                    _playersInLobby.Add(jp.PlayerData);
                                }
                                Debug.LogError($"Added {jp.PlayerData.Username} to {PlayerData.Username}'s lobby list client side");
                                break;
                            case BasePacket.PacketType.ClientList:
                                // TODO: Add a client list packet to send a list of PlayerData for all the clients in the lobby or server
                                break;
                            case BasePacket.PacketType.Message:
                                MessagePacket mp = new MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
                                // TODO: Add a Message packet to send and receive messages
                                Debug.LogError($"{mp.PlayerData.Username}: {mp.Message}");
                                ChatMessageReceivedEvent(mp.PlayerData, mp.Message);
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
            SceneManager.LoadScene(1);
        }

        public void SendChatMessage(string message)
        {
            byte[] buffer = new MessagePacket(PlayerData, message).Serialize();
            _clientSocket.Send(buffer);
            ChatMessageSentEvent(PlayerData, message);
        }

        // Sends the packet to the server to be distributed as needed
        public void SendPacket(BasePacket packet)
        {
            switch (packet.Type)
            {
                case BasePacket.PacketType.None:
                    break;
                case BasePacket.PacketType.Join:
                    JoinPacket jp = (JoinPacket)packet;
                    _clientSocket.Send(jp.Serialize());
                    break;
                case BasePacket.PacketType.Message:
                    MessagePacket mp = (MessagePacket)packet;
                    _clientSocket.Send(mp.Serialize());
                    // TODO: Implement sending a chat Message packet
                    break;
                default:
                    break;
            }
        }
    }
}
