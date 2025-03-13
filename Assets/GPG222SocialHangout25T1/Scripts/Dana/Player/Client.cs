using System;
using System.Net.Sockets;
using Dana.Shared.Packets;

namespace Dana.JW.Client
{
    public class Client
    {
        #region Variables
        public Socket Socket { get; private set; }
        public bool Connected => Socket != null && Socket.Connected;

        public event Action<IPacket> OnPacketReceived;
        public event Action<string> OnError;
        #endregion

        #region Public Functions
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

        public void SendPacket(IPacket packet)
        {
            if (Connected)
            {
                byte[] bytes = packet.SerializeChatPackets();
                Socket.Send(bytes);
            }
        }

        public void ProcessIncomingData()
        {
            if (Connected && Socket.Available > 0)
            {
                try
                {
                    byte[] buffer = new byte[Socket.Available];
                    int received = Socket.Receive(buffer);

                    if (received > 0)
                    {
                        IPacket packet = PacketHandler.DeserializePacket(buffer);
                        OnPacketReceived?.Invoke(packet);
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
