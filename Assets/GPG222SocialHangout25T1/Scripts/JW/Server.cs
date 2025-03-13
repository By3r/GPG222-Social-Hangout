using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace JW.Dana.Server
{
    public class Server : MonoBehaviour
    {
        [SerializeField] string ipAddress = "127.0.0.1"; // this is local network ip
        [SerializeField] int port = 5500; // general unity based networking port according to google to avoid failed binding
        private Socket serverSocket;
        private List<Socket> clients = new List<Socket>();
        private Dictionary<int, int> characterOwnership = new(); 

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
                            string message = Encoding.Unicode.GetString(buffer, 0, received).Trim();

                            string[] parts = message.Split(':');
                            if (parts.Length < 2)
                            {
                                continue;
                            }
                            if (parts[0] == "CHECK" && parts.Length >= 2)
                            {
                                int characterID;
                                if (!int.TryParse(parts[1], out characterID))
                                {
                                    return;
                                }

                                bool isTaken = characterOwnership.ContainsKey(characterID);
                                string response = $"CHARACTER_STATUS:{characterID}:{(isTaken ? "1" : "0")}";

                                client.Send(Encoding.Unicode.GetBytes(response));
                                return;
                            }
                            if (parts[0] == "CHARACTER_SELECT" && parts.Length >= 2)
                            {
                                int characterID = int.Parse(parts[1]);
                                string username = "UnknownPlayer";

                                if (clients.Count > 0)
                                {
                                    username = $"Player{clients.Count}";
                                }

                                string response = $"CHARACTER_ACCEPTED:{username}:{characterID}";
                                client.Send(Encoding.Unicode.GetBytes(response));
                            }

                            else if (parts[0] == "JOIN" && parts.Length >= 5)
                            {
                                string username = parts[1];
                                int tag = int.Parse(parts[2]);
                                string color = parts[3];
                                int characterID = int.Parse(parts[4]);

                                if (characterOwnership.ContainsKey(characterID))
                                {
                                    client.Send(Encoding.Unicode.GetBytes("CHARACTER_TAKEN"));
                                    continue;
                                }
                                characterOwnership[characterID] = tag;
                                BroadcastAllConnectedClients(message);
                                client.Send(Encoding.Unicode.GetBytes("JOIN:" + username + ":" + tag + ":" + color + ":" + characterID + "\n"));
                                client.Send(Encoding.Unicode.GetBytes("JOIN_SUCCESS\n")); // ----- into a new line so that it can be recognised as separate than the previous join messsage
                            }
                            else if (parts[0] == "CHAT" && parts.Length >= 5)
                            {
                                BroadcastAllConnectedClients(message);
                            }
                        }
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                    {
                        Debug.LogError($"Server receive error: {e}");
                    }
                }
            }
        }

        private void BroadcastAllConnectedClients(string message)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            Debug.Log($"📡 Broadcasting: '{message}' to {clients.Count} clients");
            foreach (Socket otherClient in clients)
            {
                if (otherClient.Connected)
                {
                    otherClient.Send(buffer);
                }
            }
        }

        /// <summary>
        /// Checks whether a given socket is still connected or not.
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
