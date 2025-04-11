using System.Collections.Generic;
using UnityEngine;

namespace Networking.Core.Lobby
{
    public class DuckSpawner : MonoBehaviour
    {
        private Client _client;

        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        private Dictionary<int, string> _prefabNames = new Dictionary<int, string>();
        [SerializeField] private string _prefabBaseName;

        private HashSet<string> _spawnedKeys = new HashSet<string>();

        private void Start()
        {
            _client = Client.Instance;
            _client.PlayerConnectedEvent += OnPlayerConnected;

            for (int i = 0; i < 4; i++)
            {
                _prefabNames[i] = $"{_prefabBaseName}/Duck{i}";
            }

            // Spawn any ducks that were already known in the lobby list
            foreach (var existingPlayer in _client.PlayersInLobby)
            {
                OnPlayerConnected(existingPlayer);
            }
        }

        private void OnPlayerConnected(PlayerData player)
        {
            string key = $"{player.DuckID}_{player.Username}";

            if (_spawnedKeys.Contains(key))
            {
                return; // Already spawned
            }

            if (!_prefabNames.ContainsKey(player.DuckID))
            {
                Debug.LogWarning($"[DuckSpawner] Unknown DuckID {player.DuckID}");
                return;
            }

            if (player.DuckID >= _spawnPoints.Count)
            {
                Debug.LogWarning($"[DuckSpawner] No spawn point for DuckID {player.DuckID}");
                return;
            }

            Debug.Log($"[DuckSpawner] Instantiating network duck for {player.Username}");

            if (player.Username == _client.PlayerData.Username)
            {
                // Only the *owner* spawns their duck over the network
                _client.InstantiateOverNetwork(
                    _prefabNames[player.DuckID],
                    _spawnPoints[player.DuckID].position,
                    _spawnPoints[player.DuckID].rotation
                );
            }

            _spawnedKeys.Add(key); // Mark as spawned regardless of who instantiated it
        }
    }
}
