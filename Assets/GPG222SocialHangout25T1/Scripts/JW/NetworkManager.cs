using Dana.Shared.PlayerInformation;
using Dana.Shared.Packets;
using Dana.JW.Client;
using Dana.ChatSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
            client.OnPacketReceived += HandlePacketReceived;
            client.OnError += Debug.LogError;
            client.Connect(ipAddress, port);
        }

        private void Update()
        {
            client?.ProcessIncomingData();
        }

        #region Public Functions
        /// <summary>
        /// Connects to the server using the username players have entered.
        /// Assigns a colour to the player.
        /// Sends a player has joined message.
        /// </summary>
        public void ConnectToServer(string username, int characterID)
        {
            if (playerData == null)
            {
                int randomTag = UnityEngine.Random.Range(0, 999);
                // Map duck selection (characterID) to a specific colour:
                string selectedColor = characterID switch
                {
                    0 => "#FF0000",   // Hot Red
                    1 => "#0000FF",   // Bluee Bluee
                    2 => "#800080",   // Just Purpoe
                    3 => "#008000",   // Vert
                    _ => "#FFFFFF"    // Whites === _ as in default
                };
                Debug.Log($"DuckID received: {characterID}, Selected Color: {selectedColor}");

                playerData = new PlayerData(username, randomTag, selectedColor, characterID);
            }

            Debug.Log($"PlayerData Initialised as {playerData.Name}, {playerData.Tag}, {playerData.Color}, {playerData.DuckID}");

            client.SendPacket(new JoinPacket(playerData.Name, playerData.DuckID));
        }

        public void RequestCharacterSelection(string username, int characterID)
        {
            ConnectToServer(username, characterID);
            client.SendPacket(new DuckSelectPacket(characterID));
        }
        #endregion

        #region Private Functions
        private void HandlePacketReceived(IPacket packet)
        {
            switch (packet)
            {
                case DuckOwnershipPacket statusPacket:
                    InvokeCharacterAvailabilityEvent(statusPacket.duckID, statusPacket.isTaken);

                    if (!statusPacket.isTaken)
                    {
                        client.SendPacket(new JoinPacket(playerData.Name, playerData.DuckID));
                    }
                    break;

                case JoinPacket:
                    Debug.Log("Loading game...");
                    StartCoroutine(LoadGameSceneAfterDelay());
                    break;

                case ChatPacket chatPacket:
                    Debug.Log($"Chat message: {chatPacket.message}");
                    break;
            }
        }

        private IEnumerator LoadGameSceneAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(1);
        }
        #endregion
    }
}
