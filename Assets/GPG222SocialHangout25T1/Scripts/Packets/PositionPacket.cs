using Networking.Core;
using System.IO;
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

            // ADD THIS:
            if (offset + sizeof(int) > buffer.Length)
                throw new EndOfStreamException("Not enough data to read OwnerID");

            OwnerID = _reader.ReadInt32();

            // Strings are length-prefixed, so it's tricky — safest option:
            if (offset + sizeof(int) > buffer.Length)
                throw new EndOfStreamException("Not enough data to read ObjectID length");

            int stringLength = System.BitConverter.ToInt32(buffer, offset);
            if (offset + sizeof(int) + stringLength > buffer.Length)
                throw new EndOfStreamException("Not enough data to read full ObjectID");

            ObjectID = _reader.ReadString();

            // Do the same kind of check for all Vector3 and Quaternion values:
            int vector3Size = sizeof(float) * 3;
            int quaternionSize = sizeof(float) * 4;

            if (offset + vector3Size > buffer.Length)
                throw new EndOfStreamException("Not enough data to read Position");

            Position = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());

            if (offset + quaternionSize > buffer.Length)
                throw new EndOfStreamException("Not enough data to read Rotation");

            Rotation = new Quaternion(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());

            Size += sizeof(int); // for OwnerID
            Size += System.Text.Encoding.UTF8.GetByteCount(ObjectID) + sizeof(int); // Add sizeof(int) for the string length
            Size += sizeof(float) * 7;

            bufferSize -= Size;
            offset += Size;

            return this;
        }

    }
}