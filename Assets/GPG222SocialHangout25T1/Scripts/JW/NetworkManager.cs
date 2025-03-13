using JW.Dana.PlayerInformation;
using System.Text;
using Dana.JW.Client;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

namespace JW.Dana.BaseNetwork
{
    public class NetworkManager : NetworkEvents
    {
        [Header("Networking Variables")]
        [Tooltip("Make sure it's the same as the server's ip address.")]
        [SerializeField] private string ipAddress = "192.168.1.135"; // -------- My local ip.
        [Tooltip("Make sure it is the same as the server's port.")]
        [SerializeField] private int port = 5500;
        [Tooltip("Assign this space with the client script.")]
        private Client client;
        public Client Client => client;

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

        #region Public Functions
        public void RequestCharacterSelection(string username, int characterID)
        {
            client.Connect(ipAddress, port);
            string checkCharacterMsg = $"CHECK:{characterID}";
            client.Send(checkCharacterMsg, Encoding.Unicode);
        }

        /// <summary>
        /// Connects to the server using the username players have entered.
        /// creates a random tag and color and assigns it to the player.
        /// Sends a player has joined message.
        /// </summary>
        public void ConnectToServer(string username, int characterID)
        {
            int randomTag = UnityEngine.Random.Range(0, 999);
            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(randomColor);

            playerData = new PlayerData(username, randomTag, hexColor, characterID);

            string joinMsg = $"JOIN:{playerData.Name}:{playerData.Tag}:{playerData.Color}:{playerData.CharacterID}";

            client.Send(joinMsg, Encoding.Unicode);
        }
        #endregion

        #region Private Functions
        private void HandleMessageReceived(string message)
        {
            Debug.Log($"📩 Client received raw message: '{message}' (Length: {message.Length})");

            if (string.IsNullOrEmpty(message))
            {
                Debug.LogError("Received empty message, ignoring...");
                return;
            }

            string[] parts = message.Split(':');

            if (parts[0] == "CHARACTER_STATUS" && parts.Length >= 3)
            {
                int characterID = int.Parse(parts[1]);
                bool isTaken = parts[2] == "1";

                Debug.Log($" [Step 7] Client received CHARACTER_STATUS: {characterID} {(isTaken ? "TAKEN" : "AVAILABLE")}");
                InvokeCharacterAvailabilityEvent(characterID, isTaken);

                if (!isTaken)
                {
                    Debug.Log($" [Step 8] Sending CHARACTER_SELECT request for Character ID: {characterID}");
                    client.Send($"CHARACTER_SELECT:{characterID}", Encoding.Unicode);
                }
            }
            else if (parts[0] == "CHARACTER_ACCEPTED")
            {
                if (parts.Length < 3)
                {
                    Debug.LogError("❌ [Step none] CHARACTER_ACCEPTED message is incomplete.");
                    return;
                }

                string username = parts[1];
                int characterID = int.Parse(parts[2]);

                ConnectToServer(username, characterID);
            }
            else if (message.Trim() == "JOIN_SUCCESS")
            {
                SceneManager.LoadScene(1);
            }
            else if (parts[0] == "CHARACTER_TAKEN")
            {
                Debug.LogError("Character is already taken...."); // ------ Will debug to feedbacktext later
            }
        }

        private void HandleError(string error)
        {
            Debug.LogError(error);
        }
        #endregion
    }
}
