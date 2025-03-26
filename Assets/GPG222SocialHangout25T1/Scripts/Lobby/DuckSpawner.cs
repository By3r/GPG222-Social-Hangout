using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.Core.Lobby
{
    public class DuckSpawner : MonoBehaviour
    {
        private Client _client;
        
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        public Dictionary<int, string> _prefabNames = new Dictionary<int, string>();
        public List<int> _playersInLobby = new();
        [SerializeField] private string _prefabBaseName;

        private void Start()
        {
            _client = Client.Instance;
            
            _client.PlayerConnectedEvent += SpawnPlayer;
            
            for (int i = 0; i < 3; i++)
            {
                _prefabNames.Add(i, $"{_prefabBaseName}/Duck{i}");
            }
            
            SpawnPlayer(_client.PlayerData);
        }

        public void SpawnPlayer(PlayerData player)
        {
            if (_playersInLobby.Contains(player.DuckID))
            {
                return; // The player is already spawned in, then don't spawn it again
            }
            else
            {
                _playersInLobby.Add(player.DuckID);
                _client.InstantiateOverNetwork(_prefabNames[_client.PlayerData.DuckID], _spawnPoints[_client.PlayerData.DuckID].position, _spawnPoints[_client.PlayerData.DuckID].rotation);
            }
        }
    }
}