using UnityEngine;
using JW.Dana.BaseNetwork;
using Dana.Shared.Packets;
using System.Collections.Generic;
using JW.Syncing;

namespace Dana.Duck.Spawn
{
    /// <summary>
    /// 
    /// </summary>
   
    public class DuckSpawner : MonoBehaviour
    {
        #region Variables
        [Tooltip("Drag all duck 3d models into this array.")]
        [SerializeField] private GameObject[] duckPrefabs;

        [Tooltip("Drag all spawnpoint gameobjects (Empty objects you place wherever you want) into this array.")]
        [SerializeField] private Transform[] spawnPoints;

        [Tooltip("Tracks other players that are spawned into the scene!")]
        private Dictionary<string, GameObject> spawnedPlayers = new Dictionary<string, GameObject>();
        
        public NetworkManager networkManager;
        #endregion

        private void Start()
        {
            if (NetworkManager.instance != null && NetworkManager.instance.playerData != null)
            {
                networkManager = NetworkManager.instance;
            }
            else
            {
                Debug.LogError("Player data isn't available, can't spawn duck.");
                return;
            }

            NetworkManager.instance.Client.OnPacketReceived += OnPacketReceived;

            SpawnPlayer(networkManager.playerData.Name, networkManager.playerData.DuckID, true);
        }

        private void OnDestroy()
        {
            if (NetworkManager.instance != null)
                NetworkManager.instance.Client.OnPacketReceived -= OnPacketReceived;
        }

        public void RequestClientSync()
        {
            networkManager.Client.SendPacket(new ClientRequestPacket(PacketTypes.ClientListPackets));
            while (networkManager.IsRequestFulfilled)
            {
                Debug.Log("Waiting for client list request");
            }
            networkManager.IsRequestFulfilled = false;
        }

        #region Private Functions

        /// <summary>
        /// Handle incoming packets and spawn remote players based on JoinPackets.
        /// </summary>
        /// <param name="packet">The received packet.</param>
        private void OnPacketReceived(IPacket packet)
        {
            if (packet is JoinPacket joinPacket)
            {
                if (NetworkManager.instance != null)
                {
                    Debug.Log("Duck Spawner has no network manager");
                    return;
                }
                
                SpawnPlayer(joinPacket.username, joinPacket.duckID, isLocal: NetworkManager.instance.playerData.Name == joinPacket.username);
            }
            else if (packet is ClientListPacket clientListPacket)
            {
                foreach (var item in clientListPacket.ClientUsernames)
                {
                    if (!spawnedPlayers.ContainsKey(item))
                    {
                        SpawnDuck(item, networkManager.ClientsDuckIDs[networkManager.ClientUsernames.IndexOf(item)], false);
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a duck for a given player.
        /// </summary>
        /// <param name="username">The player's username.</param>
        /// <param name="duckID">The duck selection ID.</param>
        /// <param name="isLocal">Whether this is the local player.</param>
        private void SpawnPlayer(string username, int duckID, bool isLocal)
        {
            if (duckID < 0 || duckID >= duckPrefabs.Length || duckID >= spawnPoints.Length)
            {
                Debug.LogError("Invalid duck ID: " + duckID);
                return;
            }

            string _name = username;
            int _duckID = duckID;
            bool _isLocal = isLocal;
            if (!spawnedPlayers.ContainsKey(_name))
            {
                SpawnDuck(_name, _duckID, _isLocal); // This spawns your local duck
            }
            
            if (NetworkManager.instance.ClientsDuckIDs == null) // Only continue spawning other ducks if other clients have connected
            {
                return;
            }
            else if (NetworkManager.instance.ClientsDuckIDs.Count == 1)
            {
                return;
            }
            
            for (int i = 1; i < NetworkManager.instance.ClientsDuckIDs.Count; i++)
            {
                _name = NetworkManager.instance.ClientUsernames[i];
                _duckID = NetworkManager.instance.ClientsDuckIDs[i];
                if (!spawnedPlayers.ContainsKey(_name))
                {
                    SpawnDuck(_name, _duckID, false);
                }
            }
        }

        private void SpawnDuck(string username, int duckID, bool isLocal)
        {
            GameObject duckInstance = Instantiate(
                duckPrefabs[duckID],
                spawnPoints[duckID].position,
                spawnPoints[duckID].rotation
            );
            duckInstance.name = username;

            ObjectSyncer syncer = duckInstance.GetComponent<ObjectSyncer>();
            if (syncer != null)
            {
                // Jan you can configure object syncer here
                syncer.OwnerID = duckID;
            }
            spawnedPlayers.Add(username, duckInstance);
            Debug.Log($"Spawned {(isLocal ? "local" : "remote")} duck for {username} with duck ID {duckID}");
        }
        #endregion
    }
}