namespace JW.Dana.PlayerInformation
{
    public class PlayerData
    {
        public string Name { get; private set; }
        public int Tag { get; private set; }
        public string Color { get; private set; }  // ----------------- D 
        public int CharacterID { get; private set; }  // --------------------------- D

        public PlayerData(string name, int tag, string color, int duckID)
        {
            Name = name;
            Tag = tag;
            Color = color; // ------------------------------ D
            CharacterID = duckID; // ------------------ D
        }
    }
}