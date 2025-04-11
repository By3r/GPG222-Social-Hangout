using System.Collections.Generic;
using Networking.Packets;
using UnityEngine;

namespace Networking.Core.Lobby
{
    public class DuckSpawner : MonoBehaviour
    {
        private Client _client;

        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        private Dictionary<int, string> _prefabNames = new Dictionary<int, string>();
        [SerializeField] private string _prefabBaseName;

        private void Start()
        {
            _client = Client.Instance;
            _client.PlayerConnectedEvent += OnPlayerConnected;

            for (int i = 0; i < 4; i++) // change the 4 to a higher num if you decided to add more than 4 ducks later lol.
            {
                _prefabNames[i] = $"{_prefabBaseName}/Duck{i}";
                // Debug.Log($"Adding a prefab for duck {i} {_prefabNames[i]}");
            }
            
            SpawnPlayer(_client.PlayerData);
        }

        private void OnPlayerConnected(PlayerData newPlayer)
        {
            if (_client.SceneHost.DuckID != _client.PlayerData.DuckID) return; // We only want the host to send the spawn packet for the ducks
            
            // We have to send the packet for spawning all ducks in the Host's list whenever another one joins
            // I feel comfortable doing it this way as it will only be instantiated if it is not in the scene yet when the packet is received 
            foreach (PlayerData playerData in _client.PlayersInLobby) 
            {
                SpawnPlayer(playerData);
            }
        }

        public void SpawnPlayer(PlayerData player)
        {
            if (_client.SceneHost.DuckID == _client.PlayerData.DuckID) // If we are the host, then we need to spawn the players
            {
                Debug.Log($"[DuckSpawner] Instantiating prefab {_prefabNames[player.DuckID]} at spawn point index {player.DuckID}");
                _client.InstantiateOverNetwork(_prefabNames[player.DuckID], _spawnPoints[player.DuckID].position, _spawnPoints[player.DuckID].rotation);
            }
        }
    }
}