using System.Net.Sockets;
using UnityEngine;

namespace JW
{
    public class NetworkManager : NetworkEvents
    {
        // Networking Variables
        [SerializeField] private string ipAddress;
        [SerializeField] private int port;
        Socket socket;

        // Network Manager Singleton
        public PlayerData playerData { get; private set; }
        public static NetworkManager instance { get; private set; }

        private void Awake()
        {
            // Set up and maintain singleton
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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ConnectToServer(string username)
        {
            try
            {
                playerData = new PlayerData(username, Random.Range(0, 999));

                socket.Connect(ipAddress, port);
                socket.Blocking = false;
                ServerConnectEvent();
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Debug.LogError(e.ToString());
                }
                throw;
            }
        }
    } 
}
