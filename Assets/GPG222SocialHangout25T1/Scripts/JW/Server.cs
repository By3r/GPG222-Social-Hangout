using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace JW
{
    public class Server : MonoBehaviour
    {
        [SerializeField] string ipAddress = "127.0.0.1"; // this is local network ip
        [SerializeField] int port = 7777; // general unity based networking port according to google to avoid failed binding
        Socket server;

        List<Socket> clients = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // Spinning up the server
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            server.Blocking = false; // CContinue on with your life my guy!
            server.Listen(1000);
            Debug.Log("Waiting For Connection...");
            
        }

        // Update is called once per frame
        void Update()
        {
            // Attempt to catch a client
            try
            {
                clients.Add(server.Accept());
                Debug.LogError("Client connected!");
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }

            // Check for package delivery
            try
            {
                for (int i = 0; i < clients.Count; i++) // go tghrough everyone and look for a package delivery
                {
                    if (clients[i].Available > 0) // Package delivery
                    {
                        byte[] buffer = new byte[clients[i].Available];
                        clients[i].Receive(buffer);

                        for (int j = 0; j < clients.Count; j++) // Send the same package to everyone else
                        {
                            if (i == j) // Skip yourself
                                continue;

                            clients[j].Send(buffer);
                        }
                    }
                }
            }
            catch (SocketException e) // No users joined yet ;(
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
    }
}
