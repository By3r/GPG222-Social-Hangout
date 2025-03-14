using UnityEngine;
using JW.Dana.BaseNetwork;
using Dana.Shared.PlayerInformation;
using JW.Syncing;

namespace Dana.Duck.Spawn
{
/// <summary>
/// 
/// </summary>

public class DuckSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] duckPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (NetworkManager.instance != null && NetworkManager.instance.playerData != null)
        {
            SpawnDuck(NetworkManager.instance.playerData);
        }
        else
        {
            Debug.LogError("Player data is not available for duck spawning.");
        }
    }

    /// <summary>
    /// Spawns the duck prefab corresponding to the player's duck selection at its designated spawn point.
    /// </summary>
    /// <param name="playerData">The player data containing the duck selection.</param>
    private void SpawnDuck(PlayerData playerData)
    {
        int duckID = playerData.DuckID;

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

        duckInstance.name = playerData.Name;

        ObjectSyncer syncer = duckInstance.GetComponent<ObjectSyncer>();
        if (syncer != null)
        {
            syncer.OwnerID = playerData.Tag;
        }
    }
}
}