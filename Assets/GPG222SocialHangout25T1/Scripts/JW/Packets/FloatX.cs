using System.IO;
using Dana.Shared.Packets;
using Dana.Shared.PlayerInformation;

namespace Dana.Shared.Packets
{
    /// <summary>
    /// This packet will contain 3 float values
    /// </summary>
    public class FloatX : BasePacket
    {
        public float[] data;

        public FloatX(float[] data) : base(PacketTypes.FloatX)
        {
            this.data = data;
        }

        public byte[] Serialize()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(memoryStream);
            
            bw.Write(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                bw.Write(data[i]);
            }
            
            return memoryStream.ToArray();
        }

        protected override void WriteData(BinaryWriter writer)
        {
            // Serialize the actual data
            writer.Write(this.data.Length);
            for (int i = 0; i < this.data.Length; i++)
            {
                writer.Write(this.data[i]);
            }
        }

        public static FloatX Deserialize(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader binaryReader = new BinaryReader(stream);
            
            binaryReader.ReadInt32(); // Read the packet type
            int length = binaryReader.ReadInt32();
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = binaryReader.ReadSingle();
            }
            
            return new FloatX(data);
        }
    }
}