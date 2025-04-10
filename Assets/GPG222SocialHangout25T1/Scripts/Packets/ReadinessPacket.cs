using Networking.Core;

namespace Networking.Packets
{
    public class ReadinessPacket : BasePacket
    {
        public bool IsReady;

        public ReadinessPacket() : base(PacketType.ReadyStatus) { }

        public ReadinessPacket(PlayerData playerData, bool isReady) : base(PacketType.ReadyStatus, playerData)
        {
            IsReady = isReady;
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            _writer.Write(IsReady);
            return EndSerialize();
        }

        public new ReadinessPacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            IsReady = _reader.ReadBoolean();
            Size += sizeof(bool);
            bufferSize -= Size;
            offset += Size;
            return this;
        }
    }
}
