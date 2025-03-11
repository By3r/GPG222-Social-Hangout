using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace JW
{
    public class Server : MonoBehaviour
    {
        [SerializeField] string ipAddress = "127.0.0.1"; // this is local network ip
        [SerializeField] int port = 5500; // general unity based networking port according to google to avoid failed binding
        private Socket serverSocket;
        private List<Socket> clients = new List<Socket>();

        void Start()
        {
            // Spinning up the server
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            serverSocket.Blocking = false;
            serverSocket.Listen(1000);
            Debug.Log("Waiting For Connection...");

        }

        void Update()
        {
            // Attempt to catch a client
            try
            {
                Socket clientSocket = serverSocket.Accept();
                clientSocket.Blocking = false;
                clients.Add(clientSocket);
                Debug.LogError("Client connected!");
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                Socket client = clients[i];

                if (!IsSocketConnected(client))
                {
                    Debug.Log($"Client has disconnected");
                    client.Close();
                    clients.RemoveAt(i);
                    continue;
                }

                try
                {
                    if (client.Available > 0)
                    {
                        byte[] buffer = new byte[client.Available];
                        int received = client.Receive(buffer);
                        if (received > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, received);
                            Debug.Log("Received: " + message);


                            foreach (Socket otherClient in clients)
                            {
                                if (otherClient.Connected)
                                {
                                    otherClient.Send(buffer);
                                }
                            }
                        }
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

        /// <summary>
        /// Checks whether a given socket is still connected or nott.
        /// </summary>
        private bool IsSocketConnected(Socket s)
        {
            try
            {
                return !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
