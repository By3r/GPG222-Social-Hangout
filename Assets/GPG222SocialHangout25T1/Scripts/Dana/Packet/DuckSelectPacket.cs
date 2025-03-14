using System.IO;
using System.Text;

namespace Dana.Shared.Packets
{
    public class DuckSelectPacket : IPacket
    {
        public PacketTypes PacketType => PacketTypes.CharacterSelect;
        public int characterID { get; }

        public DuckSelectPacket(int characterID)
        {
            this.characterID = characterID;
        }

        public byte[] SerializePacket()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode);
            writer.Write((int)PacketType);
            writer.Write(characterID);
            return stream.ToArray();
        }

        public static DuckSelectPacket Deserialize(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);
            reader.ReadInt32();
            int characterID = reader.ReadInt32();
            return new DuckSelectPacket(characterID);
        }
    }
}