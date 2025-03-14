namespace Dana.Shared.PlayerInformation
{
    public class PlayerData
    {
        public string Name { get; private set; }
        public int Tag { get; private set; }
        public string Color { get; private set; }
        public int DuckID { get; private set; }

        public PlayerData(string name, int tag, string color, int duckID)
        {
            Name = name;
            Tag = tag;
            Color = color;
            DuckID = duckID;
        }
    }
}