using Networking.Core;

namespace Networking.Packets
{
    public class SceneChangePacket : BasePacket
    {
        public int SceneID { get; private set; }
        public float WaveOffset { get; private set; }

        public SceneChangePacket() : base(PacketType.SceneChange)
        {
            SceneID = -1;
            WaveOffset = 0;
        }
        
        public SceneChangePacket(PlayerData playerData, int sceneID, float offset) : base(PacketType.SceneChange, playerData)
        {
            SceneID = sceneID;
            WaveOffset = offset;
        }
        
        public byte[] Serialize()
        {
            BeginSerialize();
            _writer.Write(SceneID);
            _writer.Write(WaveOffset);
            return EndSerialize();
        }

        public new SceneChangePacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            
            SceneID = _reader.ReadInt32();
            Size += sizeof(int);
            
            WaveOffset = _reader.ReadSingle();
            Size += sizeof(float);
            
            bufferSize -= Size;
            offset += Size;
            return this;
        }
    }
}