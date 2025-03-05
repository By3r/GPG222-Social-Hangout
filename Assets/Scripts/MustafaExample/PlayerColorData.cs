namespace Mustafa
{
    public class PlayerColorData
    {
        public PlayerData playerData;
        public int colorIndex;

        public PlayerColorData(PlayerData playerData, int colorIndex)
        {
            this.playerData = playerData;
            this.colorIndex = colorIndex;
        }
    } 
}