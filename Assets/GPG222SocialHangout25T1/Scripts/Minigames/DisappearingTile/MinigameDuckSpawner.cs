using System.Collections.Generic;
using UnityEngine;
using Networking.Core;

namespace Networking.Minigames
{
    public class MinigameDuckSpawner : MonoBehaviour
    {
        #region Variables
        private Client _client;

        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        [SerializeField] private string _prefabBaseName = "Prefabs/Ducks";

        private Dictionary<int, string> _prefabNames = new Dictionary<int, string>();
        #endregion

        private void Start()
        {
            _client = Client.Instance;

            for (int i = 0; i < 4; i++)
            {
                _prefabNames[i] = $"{_prefabBaseName}/Duck{i}";
            }

            _client.PlayerConnectedEvent += OnDuckConnected;

            if (_client.SceneHost.DuckID == _client.PlayerData.DuckID)
            {
                foreach (var player in _client.PlayersInLobby)
                {
                    SpawnPlayer(player);
                }
            }
        }

        #region Private Functions
        private void OnDuckConnected(PlayerData newPlayer)
        {
            if (_client.SceneHost.DuckID != _client.PlayerData.DuckID) return;

            SpawnPlayer(newPlayer);
        }

        private void SpawnPlayer(PlayerData player)
        {
            if (!_prefabNames.ContainsKey(player.DuckID) || player.DuckID >= _spawnPoints.Count)
            {
                Debug.LogWarning($"Invalid DuckID or missing spawn point for {player.Username}");
                return;
            }

            string prefabName = _prefabNames[player.DuckID];
            Vector3 position = _spawnPoints[player.DuckID].position;
            Quaternion rotation = _spawnPoints[player.DuckID].rotation;

            _client.InstantiateOverNetwork(prefabName, position, rotation, player);
        }
        #endregion
    }
}
