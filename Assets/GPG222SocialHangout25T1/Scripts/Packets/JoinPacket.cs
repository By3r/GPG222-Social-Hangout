using System.Runtime.CompilerServices;
using Networking.Core;

namespace Networking.Packets
{
    /// <summary>
    /// This packet will send the PlayerData of a client who has just joined the lobby
    /// </summary>
    public class JoinPacket : BasePacket
    {
        public JoinPacket()
        {
            Type = PacketType.Join;
        }
        public JoinPacket(PlayerData playerData) : base(BasePacket.PacketType.Join, playerData)
        {
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            return EndSerialize();
        }

        public new JoinPacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            
            // Update the buffer size and offset from the calculated size
            bufferSize -= Size;
            offset += Size;
            
            return this;
        }
    }
}