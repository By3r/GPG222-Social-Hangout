using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Dana.Shared.Packets;
using JW.Syncing;
using UnityEngine;

namespace Dana.Shared.Server
{
    public class Server : MonoBehaviour
    {
        #region Variables
        [SerializeField] string ipAddress = "127.0.0.1"; // this is local network ip
        [SerializeField] int port = 5500; // general unity based networking port according to google to avoid failed binding
        private Socket serverSocket;
        private List<Socket> clients = new List<Socket>();
        private Dictionary<int, int> characterOwnership = new();
        private List<int> ducksChosen = new();
        private List<string> duckNames = new();
        #endregion

        void Start()
        {
            // Spinning up the server
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            serverSocket.Listen(1000);
            Debug.LogError("Waiting For Connection...");
            serverSocket.Blocking = false;
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
            ProcessClientMessages();
        }


        private void ProcessClientMessages()
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                Socket client = clients[i];

                if (!IsSocketConnected(client))
                {
                    Debug.LogError($"Client has disconnected");
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
                            HandlePacket(client, buffer);
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

        private void HandlePacket(Socket client, byte[] buffer)
        {
            IPacket packet = PacketHandler.DeserializePacket(buffer);

            switch (packet)
            {
                case JoinPacket joinPacket:
                    Debug.LogWarning("Received JoinPacket");
                    ducksChosen.Add(joinPacket.duckID);
                    duckNames.Add(joinPacket.username);
                    BroadcastToAllClients(packet);
                    break;

                case ChatPacket chatPacket:
                    Debug.LogWarning($"Received ChatPacket from '{chatPacket.senderUsername}', color: {chatPacket.senderColor}, message: '{chatPacket.message}'");
                    BroadcastToAllClients(chatPacket);
                    break;

                case DuckSelectPacket characterSelect:
                    bool isTaken = characterOwnership.ContainsKey(characterSelect.characterID);
                    DuckOwnershipPacket responsePacket = new DuckOwnershipPacket(characterSelect.characterID, isTaken);
                    client.Send(responsePacket.SerializePacket());

                    if (!isTaken)
                    {
                        characterOwnership[characterSelect.characterID] = clients.IndexOf(client);
                        BroadcastToAllClients(characterSelect);
                    }
                    break;
                
                case SyncPacket syncPacket:
                    BroadcastToAllClients(syncPacket);
                    break;

                case ClientListPacket clientListPacket:
                    Debug.LogWarning("ClientListPacket Recieved");
                    BroadcastToAllClients(clientListPacket);
                    break;

                case ClientCountPacket clientCountPacket:
                    SendToClient(client, clientCountPacket);
                    break;
            }
        }


        private IEnumerator BroadcastToAllClients(IPacket packet, float delay = 0f)
        {
            byte[] buffer = packet.SerializePacket();

            if (packet is ChatPacket chatPacket)
            {
                Debug.LogWarning($"[Server] Broadcasting ChatPacket from '{chatPacket.senderUsername}', color: {chatPacket.senderColor}");
            }

            Debug.LogWarning($"📡 Broadcasting Packet: {packet.PacketType} to {clients.Count} clients");

            foreach (Socket client in clients)
            {
                if (client.Connected)
                {
                    yield return new WaitForSeconds(delay);
                    client.Send(buffer);
                }
            }
        }

        IEnumerator SendDelayed(Socket client, IPacket packet, float delay)
        {
            yield return new WaitForSeconds(delay);
            client.Send(packet.SerializePacket());
        }

        private void SendToClient(Socket client, IPacket packet)
        {
            if (!client.Connected)
            {
                return;
            }

            client.Send(packet.SerializePacket());
        }

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
