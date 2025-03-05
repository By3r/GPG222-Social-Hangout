using System.IO;

namespace Mustafa
{
    public class BasePacket
    {
        protected MemoryStream msw;
        protected BinaryWriter bw;

        protected MemoryStream msr;
        protected BinaryReader br;

        public enum PacketType
        {
            None,
            Message,
            Color,
            PlayersColorData
        }
        public PacketType packetType;
        public PlayerData playerData;

        public BasePacket()
        {
            packetType = PacketType.None;
            playerData = null;
        }

        public BasePacket(PacketType packetType, PlayerData playerData)
        {
            this.packetType = packetType;
            this.playerData = playerData;
        }

        protected void BeginSerialize()
        {
            msw = new MemoryStream();
            bw = new BinaryWriter(msw);
            bw.Write((int)packetType);
            bw.Write(playerData.name);
            bw.Write(playerData.tag);
        }

        protected byte[] EndSerialize()
        {
            return msw.ToArray();
        }

        public void Deserialize(byte[] buffer)
        {
            msr = new MemoryStream(buffer);
            br = new BinaryReader(msr);

            packetType = (PacketType)br.ReadInt32();
            playerData = new PlayerData(br.ReadString(), br.ReadInt32());
        }
    } 
}