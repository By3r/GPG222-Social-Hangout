using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using TMPro;

public class Server : MonoBehaviour
{
    [SerializeField] string ipAddress;
    [SerializeField] int port;
    Socket server;

    List<Socket> clients = new List<Socket>();

    void Start()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        server.Listen(1000);
        Debug.LogError("Waiting for connection...");
        server.Blocking = false;
    }

    void Update()
    {
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

        try
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Available > 0)
                {
                    byte[] buffer = new byte[clients[i].Available];
                    clients[i].Receive(buffer);

                    for (int j = 0; j < clients.Count; j++)
                    {
                        if (i == j)
                            continue;

                        clients[j].Send(buffer);
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