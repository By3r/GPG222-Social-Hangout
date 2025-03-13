using System.IO;
using System.Text;

namespace Dana.Shared.Packets
{
    public class JoinPacket : IPacket
    {
        #region Variabels
        public PacketTypes PacketType => PacketTypes.Join;
        public string username { get; }
        public int duckID { get; }
        #endregion

        public JoinPacket(string username, int characterID)
        {
            this.username = username;
            this.duckID = characterID;
        }

        public byte[] SerializeChatPackets()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode);
            writer.Write((int)PacketType);
            writer.Write(username);
            writer.Write(duckID);
            return stream.ToArray();
        }

        public static JoinPacket Deserialize(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);
            reader.ReadInt32(); 
            string username = reader.ReadString();
            int characterID = reader.ReadInt32();
            return new JoinPacket(username, characterID);
        }
    }
}
