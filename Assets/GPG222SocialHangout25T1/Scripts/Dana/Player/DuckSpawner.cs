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
        #endregion

        private void Start()
        {
            NetworkManager.instance.Client.OnPacketReceived += OnPacketReceived;
            if (NetworkManager.instance != null && NetworkManager.instance.playerData != null)
            {
                SpawnPlayer(
                    NetworkManager.instance.playerData.Name,
                    NetworkManager.instance.playerData.DuckID,
                    isLocal: true
                );
            }
            else
            {
                Debug.LogError("Player data isntt available, can't spawn duck.");
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.instance != null)
                NetworkManager.instance.Client.OnPacketReceived -= OnPacketReceived;
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
                if (NetworkManager.instance.playerData != null &&
                    joinPacket.username == NetworkManager.instance.playerData.Name)
                {
                    return;
                }

                if (!spawnedPlayers.ContainsKey(joinPacket.username))
                {
                    SpawnPlayer(joinPacket.username, joinPacket.duckID, isLocal: false);
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
            }
            if (!isLocal)
            {
                spawnedPlayers.Add(username, duckInstance);
            }

            Debug.Log($"Spawned {(isLocal ? "local" : "remote")} duck for {username} with duck ID {duckID}");
        }
        #endregion
    }
}