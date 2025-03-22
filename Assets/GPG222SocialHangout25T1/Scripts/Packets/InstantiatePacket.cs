using Networking.Core;
using UnityEngine;

namespace Networking.Packets
{
    public class InstantiatePacket : BasePacket
    {
        public string ObjectID { get; private set; }
        public string PrefabName { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public InstantiatePacket() : base(PacketType.Instantiate)
        {
            ObjectID = "";
            PrefabName = "";
            Position = new Vector3(0, 0, 0);
            Rotation = new Quaternion(0, 0, 0, 0);
        }

        public InstantiatePacket(
            PlayerData playerData,
            string objectID, 
            string prefabName, 
            Vector3 position, 
            Quaternion rotation) : base(PacketType.Instantiate, playerData)
        {
            this.ObjectID = objectID;
            this.PrefabName = prefabName;
            this.Position = position;
            this.Rotation = rotation;
        }

        public byte[] Serialize()
        {
            BeginSerialize();

            _writer.Write(ObjectID);
            _writer.Write(PrefabName);
            _writer.Write(Position.x);
            _writer.Write(Position.y);
            _writer.Write(Position.z);
            _writer.Write(Rotation.x);
            _writer.Write(Rotation.y);
            _writer.Write(Rotation.z);
            _writer.Write(Rotation.w);
            
            return EndSerialize();
        }

        public new InstantiatePacket Deserialize(byte[] bytes, ref int bufferSize, ref int offset)
        {
            base.Deserialize(bytes, ref bufferSize, ref offset);
            
            ObjectID = _reader.ReadString();
            PrefabName = _reader.ReadString();
            Size += ObjectID.Length + 1;
            Size += PrefabName.Length + 1;
            
            Position = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
            Rotation = new Quaternion(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
            Size += sizeof(float) * 7;

            bufferSize -= Size;
            offset += Size;
            
            return this;
        }
    }
}