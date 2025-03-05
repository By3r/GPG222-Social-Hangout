using System.Collections.Generic;

namespace Mustafa
{
    public class PlayersColorDataPacket : BasePacket
    {
        public List<PlayerColorData> PlayerColorData { get; private set; }

        public PlayersColorDataPacket() :
            base(PacketType.None, null)
        {
            PlayerColorData = new List<PlayerColorData>();
        }

        public PlayersColorDataPacket(PlayerData playerData, List<PlayerColorData> playerColorData) :
            base(PacketType.PlayersColorData, playerData)
        {
            this.playerData = playerData;
            PlayerColorData = playerColorData;
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(PlayerColorData.Count);

            for (int i = 0; i < PlayerColorData.Count; i++)
            {
                bw.Write(PlayerColorData[i].playerData.name);
                bw.Write(PlayerColorData[i].playerData.tag);
                bw.Write(PlayerColorData[i].colorIndex);
            }
            return EndSerialize();
        }

        public new PlayersColorDataPacket Deserialize(byte[] buffer)
        {
            base.Deserialize(buffer);
            int playerColorDataListCount = br.ReadInt32();

            PlayerColorData = new List<PlayerColorData>(playerColorDataListCount);

            for (int i = 0; i < playerColorDataListCount; i++)
            {
                PlayerData pd = new PlayerData(br.ReadString(), br.ReadInt32());
                PlayerColorData pcd = new PlayerColorData(pd, br.ReadInt32());
                PlayerColorData.Add(pcd);
            }

            return this;
        }
    } 
}