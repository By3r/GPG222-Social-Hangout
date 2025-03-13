using System.IO;
using System.Text;

namespace Dana.Shared.Packets
{
    public class DuckOwnershipPacket : BasePacket
    {
        #region Variables
        public int duckID;
        public bool isTaken;
        #endregion

        public DuckOwnershipPacket(int duckID, bool isTaken) : base(PacketTypes.CharacterStatus)
        {
            this.duckID = duckID;
            this.isTaken = isTaken;
        }

        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(duckID);
            writer.Write(isTaken);
        }

        public static DuckOwnershipPacket Deserialize(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);
            reader.ReadInt32();
            int characterID = reader.ReadInt32();
            bool isTaken = reader.ReadBoolean();
            return new DuckOwnershipPacket(characterID, isTaken);
        }
    }
}