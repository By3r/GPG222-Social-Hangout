using Networking.Core;
using UnityEngine;

namespace Networking.Packets
{
    public class PositionPacket : BasePacket
    {
        public int OwnerID { get; private set; }
        public string ObjectID { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public PositionPacket() : base(PacketType.Position)
        {
            OwnerID = -1;
            ObjectID = "";
            Position = new Vector3(0, 0, 0);
            Rotation = Quaternion.identity;
        }

        public PositionPacket(PlayerData playerData, string objectID, Vector3 position, Quaternion rotation) : base(PacketType.Position,
            playerData)
        {
            OwnerID = playerData.DuckID;
            ObjectID = objectID;
            Position = position;
            Rotation = rotation;
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            _writer.Write(OwnerID);
            _writer.Write(ObjectID);
            _writer.Write(Position.x);
            _writer.Write(Position.y);
            _writer.Write(Position.z);
            _writer.Write(Rotation.x);
            _writer.Write(Rotation.y);
            _writer.Write(Rotation.z);
            _writer.Write(Rotation.w);
            return EndSerialize();
        }

        public new PositionPacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            
            OwnerID = _reader.ReadInt32();
            ObjectID = _reader.ReadString();
            Size += sizeof(int);
            Size += ObjectID.Length + 1;
            
            Position = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
            Rotation = new Quaternion(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
            Size += sizeof(float) * 7;
            
            return this;
        }
    }
}