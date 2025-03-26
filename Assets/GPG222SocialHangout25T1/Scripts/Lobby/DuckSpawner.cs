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

        private Dictionary<string, GameObject> _spawnedDucks = new Dictionary<string, GameObject>();

        private void Start()
        {
            _client = Client.Instance;
            _client.PlayerConnectedEvent += OnPlayerConnected;

            for (int i = 0; i < 4; i++) // change the 4 to a higher num if you decided to add more than 4 ducks later lol.
            {
                _prefabNames[i] = $"{_prefabBaseName}/Duck{i}";
                // Debug.Log($"Adding a prefab for duck {i} {_prefabNames[i]}");
            }

            foreach (var existingPlayer in _client.PlayersInLobby)
            {
                SpawnPlayer(existingPlayer);
            }
        }

        private void OnPlayerConnected(PlayerData newPlayer)
        {
            SpawnPlayer(newPlayer);
        }

        private void SpawnPlayer(PlayerData player)
        {
            string existingPlayerKey = $"{player.DuckID}_{player.Username}";

            if (_spawnedDucks.ContainsKey(existingPlayerKey))
            {
                // Duck/player already exists
                return;
            }

            if (!_prefabNames.ContainsKey(player.DuckID))
            {
                // The quack prefab doesnt exist
                return;
            }

            if (player.DuckID >= _spawnPoints.Count)
            {
                //Spawn point doesnt exist for the corresponding duck youre trying to spawn
                return;
            }

            Debug.Log($"[DuckSpawner] Instantiating prefab {_prefabNames[player.DuckID]} at spawn point index {player.DuckID}");
            GameObject duck = Instantiate(Resources.Load<GameObject>(_prefabNames[player.DuckID]),
                _spawnPoints[player.DuckID].position,
                _spawnPoints[player.DuckID].rotation);
            _spawnedDucks.Add(existingPlayerKey, duck);
        }
    }
}