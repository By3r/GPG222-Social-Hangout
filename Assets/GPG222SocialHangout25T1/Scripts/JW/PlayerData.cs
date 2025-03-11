namespace JW.Dana.PlayerInformation
{
    public class PlayerData
    {
        public string Name { get; private set; }
        public int Tag { get; private set; }
        public string Color { get; private set; }  // ----------------- D Added for the player's chat colour.

        public PlayerData(string name, int tag, string color)
        {
            Name = name;
            Tag = tag;
            Color = color; // ------------------------------ D
        }
    }
}