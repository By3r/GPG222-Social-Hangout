using Networking.Core;

namespace Networking.Packets
{
    public class DestroyPacket : BasePacket
    {
        public int OwnerID { get; private set; }
        public string ObjectID { get; private set; }

        public DestroyPacket() : base(PacketType.Destroy)
        {
            OwnerID = -1;
            ObjectID = "";
        }

        public DestroyPacket(PlayerData playerData, string objectID) : base(PacketType.Destroy, playerData)
        {
            OwnerID = playerData.DuckID;
            ObjectID = objectID;
        }

        public byte[] Serialize()
        {
            BeginSerialize();

            _writer.Write(OwnerID);
            _writer.Write(ObjectID);
            
            return EndSerialize();
        }

        public new DestroyPacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            
            OwnerID = _reader.ReadInt32();
            ObjectID = _reader.ReadString();
            Size += sizeof(int);
            Size += ObjectID.Length + 1;

            bufferSize -= Size;
            offset += Size;
            
            return this;
        }
    }
}