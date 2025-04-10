using Networking.Core;

namespace Networking.Packets
{
    public class SceneChangePacket : BasePacket
    {
        public int SceneID { get; private set; }

        public SceneChangePacket() : base(PacketType.SceneChange)
        {
            
        }
        
        public SceneChangePacket(PlayerData playerData, int sceneID) : base(PacketType.SceneChange, playerData)
        {
            SceneID = sceneID;
        }
        
        public byte[] Serialize()
        {
            BeginSerialize();
            _writer.Write(SceneID);
            return EndSerialize();
        }

        public new SceneChangePacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            SceneID = _reader.ReadInt32();
            Size += sizeof(int);
            bufferSize -= Size;
            offset += Size;
            return this;
        }
    }
}