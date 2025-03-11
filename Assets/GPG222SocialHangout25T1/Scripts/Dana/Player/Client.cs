using System;
using System.Net.Sockets;
using System.Text;

namespace Dana.JW.Client // --------------------------------------- I moved client related script from the network manager to the client script. You indirectly worked on this script lmao.
{
    public class Client
    {
        #region Variables
        public Socket Socket { get; private set; }
        public bool Connected => Socket != null && Socket.Connected;

        // Events for incoming messages and errors.
        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;
        #endregion

        #region Public Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void Connect(string ipAddress, int port)
        {
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(ipAddress, port);
                Socket.Blocking = false;
            }
            catch (SocketException e)
            {
                OnError?.Invoke(e.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="encoding"></param>
        public void Send(string message, Encoding encoding)
        {
            if (Connected)
            {
                byte[] bytes = encoding.GetBytes(message);
                Socket.Send(bytes);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoding"></param>
        public void EncodeMessage(Encoding encoding)
        {
            if (Connected && Socket.Available > 0)
            {
                try
                {
                    byte[] buffer = new byte[Socket.Available];
                    int received = Socket.Receive(buffer);
                    if (received > 0)
                    {
                        string message = encoding.GetString(buffer, 0, received);
                        OnMessageReceived?.Invoke(message);
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                    {
                        OnError?.Invoke(e.ToString());
                    }
                }
            }
        }
        #endregion
    }
}
