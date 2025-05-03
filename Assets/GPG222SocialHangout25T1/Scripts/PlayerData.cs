using UnityEngine;

namespace Networking.Core
{
    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] public int DuckID { get; private set; }
        [SerializeField] public string Username { get; private set; }

        public PlayerData(int duckID, string username)
        {
            DuckID = duckID;
            Username = username;
        }

        public PlayerData()
        {
            DuckID = -1;
            Username = string.Empty;
        }
    }
}
