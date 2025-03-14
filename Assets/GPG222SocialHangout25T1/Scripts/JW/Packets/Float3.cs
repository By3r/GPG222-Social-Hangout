//using Dana.Shared.PlayerInformation;

//namespace JW
//{
//    /// <summary>
//    /// This packet will contain 3 float values
//    /// </summary>
//    public class Float3 : BasePacket
//    {
//        public float[] data;

//        public Float3() : base(PacketType.None, null)
//        {
//            data = new float[3];
//        }

//        public Float3(PlayerData playerData, float x, float y, float z) : base(PacketType.Float3, playerData)
//        {
//            this.playerData = playerData;
//            data = new float[3] { x, y, z };
//        }

//        public byte[] Serialize()
//        {
//            BeginSerialize();

//            for (int i = 0; i < data.Length; i++)
//            {
//                binaryWriter.Write(data[i]);
//            }
            
//            return EndSerialize();
//        }

//        public new Float3 Deserialize(byte[] buffer)
//        {
//            base.Deserialize(buffer);
            
//            data = new float[3];
//            for (int i = 0; i < data.Length; i++)
//            {
//                data[i] = binaryReader.ReadSingle();
//            }
//            return this;
//        }
//    }
//}