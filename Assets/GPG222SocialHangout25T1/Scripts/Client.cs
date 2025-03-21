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
        public List<PlayerData> _playersInLobby = new List<PlayerData>();
        
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
            }
            catch (SocketException e) // We could not connect so print out the error
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        public void ConnectToServer(string ipAddress)
        {
            _ipAddress = ipAddress;
            _clientSocket.Connect(_ipAddress, _port);
            _clientSocket.Blocking = false;
            Debug.LogError("Client socket connected");
            JoinLobby(PlayerData);
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
                                bool isInLobby = false;
                                foreach (PlayerData playerData in _playersInLobby)
                                {
                                    if (jp.PlayerData.Username == playerData.Username &&
                                        jp.PlayerData.DuckID == playerData.DuckID)
                                    {
                                        isInLobby = true;
                                        break;
                                    }
                                }

                                if (!isInLobby)
                                {
                                    _playersInLobby.Add(jp.PlayerData);
                                }
                                break;
                            
                            case BasePacket.PacketType.ClientList:
                                // TODO: Add a client list packet to send a list of PlayerData for all the clients in the lobby or server
                                break;
                            
                            case BasePacket.PacketType.Message:
                                MessagePacket mp = new MessagePacket().Deserialize(buffer, ref bufferSize, ref offset);
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
            _playerData = new PlayerData(playerData.DuckID, playerData.Username);
            _clientSocket.Send(new JoinPacket(_playerData).Serialize());
            SceneManager.LoadScene(1);
        }

        public void SendChatMessage(string message)
        {
            byte[] buffer = new MessagePacket(PlayerData, message).Serialize();
            ChatMessageSentEvent(PlayerData, message);
            _clientSocket.Send(buffer);
        }
    }
}
