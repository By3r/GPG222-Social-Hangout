namespace Mustafa
{
    public class ColorPacket : BasePacket
    {
        public int ColorIndex { get; private set; }

        public ColorPacket() :
            base(PacketType.None, null)
        {
            ColorIndex = 0;
        }

        public ColorPacket(PlayerData playerData, int colorIndex) :
            base(PacketType.Color, playerData)
        {
            this.playerData = playerData;
            ColorIndex = colorIndex;
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(ColorIndex);
            return EndSerialize();
        }

        public new ColorPacket Deserialize(byte[] buffer)
        {
            base.Deserialize(buffer);
            ColorIndex = br.ReadInt32();
            return this;
        }
    } 
}