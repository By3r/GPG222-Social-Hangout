using JW.Dana.PlayerInformation;
using System.Text;
using Dana.JW.Client;
using UnityEngine;

namespace JW
{
    public class NetworkManager : NetworkEvents
    {
        [Header("Networking Variables")]
        [Tooltip("Make sure it's the same as the server's ip address.")]
        [SerializeField] private string ipAddress;
        [Tooltip("Make sure it is the same as the server's port.")]
        [SerializeField] private int port = 5500;
        [Tooltip("Assign this space with the client script.")]
        private Client client;
        public Client Client => client; // ------------------- D exposed client for other scripts to subscribe

        // Network Manager Singleton
        public PlayerData playerData { get; private set; }
        public static NetworkManager instance { get; private set; }

        private void Awake()
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

        private void Start()
        {
            client = new Client();
            client.OnMessageReceived += HandleMessageReceived;
            client.OnError += HandleError;
        }

        private void Update()
        {
            client?.EncodeMessage(Encoding.Unicode);
        }

        /// <summary>
        /// Connects to the server using the username players have entered.
        /// creates a random tag and color and assigns it to the player.
        /// Sends a player has joined message.
        /// </summary>
        public void ConnectToServer(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                Debug.LogError($"player is trying to connect without writing anything");
                return;
            }

            int randomTag = Random.Range(0, 999);
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(randomColor);

            playerData = new PlayerData(username, randomTag, hexColor);
            client.Connect(ipAddress, port);

            string joinMsg = $"JOIN:{playerData.Name}:{playerData.Tag}:{playerData.Color}";
            client.Send(joinMsg, Encoding.Unicode);

            ServerConnectEvent?.Invoke();
            Debug.Log($"{username} has just connected to the server...");
        }

        private void HandleMessageReceived(string message)
        {
            // It is here for the potential chat system we'll have lol -------------------- D
        }

        private void HandleError(string error)
        {
            Debug.LogError(error);
        }
    }
}
