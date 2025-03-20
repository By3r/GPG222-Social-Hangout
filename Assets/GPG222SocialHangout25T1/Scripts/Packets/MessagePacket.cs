using Networking.Core;

namespace Networking.Packets
{
    public class MessagePacket : BasePacket
    {
        public string Message;

        public MessagePacket()
        {
            Message = "";
        }

        public MessagePacket(PlayerData playerData, string message) : base(PacketType.Message, playerData)
        {
            this.Message = message;
        }
        
        public byte[] Serialize()
        {
            BeginSerialize();
            _writer.Write(Message);
            return EndSerialize();
        }

        public new MessagePacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            
            Message = _reader.ReadString();
            Size += Message.Length + 1; // Size in bytes of the message string
            
            // Update the buffer size and offset from the calculated size
            bufferSize -= Size;
            offset += Size;
            
            return this;
        }
    }
}