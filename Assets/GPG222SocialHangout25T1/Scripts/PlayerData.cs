using UnityEngine;

namespace Networking.Core
{
    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] public int DuckID { get; private set; }
        [SerializeField] public string Username { get; private set; }
        public float WaveOffset { get; private set; }

        public PlayerData(int duckID, string username, float offset)
        {
            DuckID = duckID;
            Username = username;
            WaveOffset = offset;
        }

        public PlayerData()
        {
            DuckID = -1;
            Username = string.Empty;
            WaveOffset = 0;
        }

        public void SetWaveOffset(float offset)
        {
            WaveOffset = offset;
        }
    }
}
