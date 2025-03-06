using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Mustafa;

namespace Mustafa
{
#if Mustafa
    public class NetworkManager : NetworkEvents
    {
        [SerializeField] string ipAddress;
        [SerializeField] int port;
        Socket socket;

        public PlayerData playerData { get; private set; }

        public static NetworkManager instance { get; private set; }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ConnectToServer(string username)
        {
            try
            {
                playerData = new PlayerData(username, Random.Range(0, 9999));

                socket.Connect(ipAddress, port);
                socket.Blocking = false;
                Debug.LogError("Connected to server!");
                ServerConnectEvent();
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        void Start()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Debug.LogError("Connecting to server...");
        }

        public void SendColor(int colorIndex)
        {
            byte[] buffer = new ColorPacket(playerData, colorIndex).Serialize();
            socket.Send(buffer);
            //playerColors.Add(playerData, colorIndex);
            ColorSentEvent(playerData, colorIndex);
        }

        public void SendChatMessage(string message)
        {
            byte[] buffer = new MessagePacket(playerData, message).Serialize();
            socket.Send(buffer);
            ChatMessageSentEvent(playerData, message);
        }

        void Update()
        {
            if (socket.Available > 0)
            {
                try
                {
                    byte[] buffer = new byte[socket.Available];
                    socket.Receive(buffer);

                    BasePacket bp = new BasePacket();
                    bp.Deserialize(buffer);

                    switch (bp.packetType)
                    {
                        case BasePacket.PacketType.None:
                            break;
                        case BasePacket.PacketType.Color:
                            ColorPacket cp = new ColorPacket().Deserialize(buffer);
                            //playerColors.Add(playerData, cp.ColorIndex);
                            ColorReceivedEvent(cp.playerData, cp.ColorIndex);
                            break;
                        case BasePacket.PacketType.Message:
                            MessagePacket mp = new MessagePacket().Deserialize(buffer);
                            ChatMessageReceivedEvent(mp.playerData, mp.Message);
                            break;
                        default:
                            break;
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                    {
                        Debug.LogError(e.ToString());
                    }
                }
            }
        }
    } 
#endif
}