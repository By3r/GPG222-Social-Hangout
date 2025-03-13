using System.IO;
using System.Text;

namespace Dana.Shared.Packets
{
    public class ChatPacket : IPacket
    {
        public PacketTypes PacketType => PacketTypes.Chat;
        public string senderUsername;
        public string message;
        public string senderColor;  

        public ChatPacket(string senderUsername, string message, string senderColor)
        {
            this.senderUsername = senderUsername;
            this.message = message;
            this.senderColor = senderColor;
        }

        public byte[] SerializeChatPackets()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode);
            writer.Write((int)PacketType);
            writer.Write(senderUsername);
            writer.Write(message);
            writer.Write(senderColor); 
            return stream.ToArray();
        }

        public static ChatPacket Deserialize(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);
            reader.ReadInt32();
            string sender = reader.ReadString();
            string message = reader.ReadString();
            string color = "#FFFFFF";
            if (stream.Position < stream.Length)
            {
                color = reader.ReadString();
            }
            return new ChatPacket(sender, message, color);
        }

    }
}
