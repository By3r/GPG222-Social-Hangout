using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

namespace Moustaffa
{
    public class NetworkManager : NetworkEvents
    {
        [SerializeField] string ipAddress;
        [SerializeField] int port;
        Socket socket;

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

        public void ConnectToServer()
        {
            try
            {
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

        public void SendChatMessage(string message)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            socket.Send(buffer);
            ChatMessageSentEvent(message);
        }

        void Update()
        {
            if (socket.Available > 0)
            {
                try
                {
                    byte[] buffer = new byte[socket.Available];
                    socket.Receive(buffer);
                    ChatMessageReceivedEvent(Encoding.Unicode.GetString(buffer));
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
}