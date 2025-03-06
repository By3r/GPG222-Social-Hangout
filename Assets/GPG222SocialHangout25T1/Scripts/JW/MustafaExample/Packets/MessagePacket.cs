namespace Mustafa
{
#if MUSTAFA
    public class MessagePacket : BasePacket
    {
        public string Message { get; private set; }

        public MessagePacket() :
            base(PacketType.None, null)
        {
            Message = "";
        }

        public MessagePacket(PlayerData playerData, string message) :
            base(PacketType.Message, playerData)
        {
            this.playerData = playerData;
            Message = message;
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(Message);
            return EndSerialize();
        }

        public new MessagePacket Deserialize(byte[] buffer)
        {
            base.Deserialize(buffer);
            Message = br.ReadString();
            return this;
        }
    } 
#endif
}