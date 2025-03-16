using Dana.Shared.PlayerInformation;
using Dana.Shared.Packets;
using Dana.JW.Client;
using JW.Shared.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine.TextCore.Text;
using JW.Syncing;

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
        public int clientCount = 0;
        public bool IsRequestFulfilled = false;

        public List<int> ClientsDuckIDs = new List<int>();
        public List<string> ClientUsernames = new List<string>();
        [Header("Duck Spawning")]
        [Tooltip("Assign duck prefabs into this array, make sure they match the order of ducks you placed in game scene..")]
        [SerializeField] private GameObject[] duckPrefabs;

        // Network Manager Singleton
        public PlayerData playerData { get; private set; }
        public static NetworkManager instance { get; private set; }

        // Scene switching
        public float sceneChangeTimer = 0f;
        public bool SceneShouldChange = false;
        public int SceneNumber = 0;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                ClientUsernames = new List<string>();
                ClientsDuckIDs = new List<int>();
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

            if (SceneShouldChange && SceneNumber == 0)
            {
                sceneChangeTimer += Time.deltaTime;
                if (sceneChangeTimer > 1f)
                {
                    SceneManager.LoadScene(1);
                    SceneNumber = 1;
                    client.SendPacket(new JoinPacket(playerData.Name, playerData.DuckID));
                }
            }
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
                Debug.Log($"PlayerData Initialised as {playerData.Name}, {playerData.Tag}, {playerData.Color}, {playerData.DuckID}");
                ClientsDuckIDs.Add(characterID);
                ClientUsernames.Add(username);
                SceneShouldChange = true;
            }


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
                case ClientRequestPacket request:
                    if (request.packetID == (int)PacketTypes.ClientListPackets)
                    {
                        client.SendPacket(new ClientListPacket(ClientsDuckIDs, ClientUsernames));
                    }
                    break;

                case DuckOwnershipPacket statusPacket:
                    InvokeCharacterAvailabilityEvent(statusPacket.duckID, statusPacket.isTaken);

                    if (!statusPacket.isTaken)
                    {
                        if (ClientsDuckIDs.Count > 1)
                        {
                            client.SendPacket(new JoinPacket(playerData.Name, playerData.DuckID, ClientsDuckIDs, ClientUsernames));
                        }
                        else
                        {
                            client.SendPacket(new JoinPacket(playerData.Name, playerData.DuckID));
                        }
                    }
                    break;

                case JoinPacket:
                    Debug.Log("Loading game...");

                    JoinPacket joinPacket = (JoinPacket)packet;
                    Debug.Log($"DuckID received: {playerData.DuckID}");
                    if (joinPacket.username != playerData.Name)
                    {
                        if (!ClientUsernames.Contains(joinPacket.username))
                        {
                            ClientUsernames.Add(joinPacket.username);
                            SpawnRemoteDuck(joinPacket);
                        }
                        if (!ClientsDuckIDs.Contains(joinPacket.duckID))
                        {
                            ClientsDuckIDs.Add(joinPacket.duckID);
                        }
                    }

                    break;

                case ChatPacket chatPacket:
                    Debug.Log($"Chat message: {chatPacket.message}");
                    break;

                case FloatX:
                    Debug.Log("FloatX recieved");
                    break;

                case ClientListPacket listPacket:
                    ClientsDuckIDs = listPacket.ClientDuckIDs;
                    ClientUsernames = listPacket.ClientUsernames;
                    IsRequestFulfilled = true;
                    break;

                case ClientCountPacket countPacket:
                    clientCount = countPacket.ClientCount;
                    IsRequestFulfilled = true;
                    break;
            }
        }

        /// <summary>
        /// Spawns a duck for a remote player based on the received JoinPacket. (The list's indexing matters)
        /// </summary>
        private void SpawnRemoteDuck(JoinPacket joinPacket)
        {
            int duckID = joinPacket.duckID;
            if (duckID < 0 || duckID >= duckPrefabs.Length)
            {
                Debug.LogError($"Invalid duckID received: {duckID}");
                return;
            }

            GameObject duckInstance = Instantiate(duckPrefabs[duckID], transform.position, Quaternion.Euler(-90, 0, 0)); // --------------- Spawns them at whatever the prefab's pos was
            duckInstance.name = joinPacket.username;
            Debug.Log($"Spawned remote duck for {joinPacket.username} with duckID {duckID}");
        }
        #endregion

        public IEnumerator LoadGameSceneAfterDelay()
        {
            Debug.Log("Loading next scene");
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(1);
        }

    }
}
