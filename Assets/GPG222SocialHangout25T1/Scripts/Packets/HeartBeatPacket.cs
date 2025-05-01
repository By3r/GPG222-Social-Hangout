using Networking.Core;
using System.IO;

namespace Networking.Packets
{
    public class HeartbeatPacket : BasePacket
    {
        public HeartbeatPacket()
        {
            Type = PacketType.Heartbeat;
        }

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((int)Type);
            return ms.ToArray();
        }

        public new HeartbeatPacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            using var ms = new MemoryStream(buffer);
            ms.Seek(offset, SeekOrigin.Begin);
            using var reader = new BinaryReader(ms);

            Type = (PacketType)reader.ReadInt32();
            Size = sizeof(int);

            bufferSize -= Size;
            offset += Size;

            return this;
        }
    }
}