namespace Networking.Core
{
    public class PlayerData
    {
        public int DuckID { get; private set; }
        public string Username { get; private set; }

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
