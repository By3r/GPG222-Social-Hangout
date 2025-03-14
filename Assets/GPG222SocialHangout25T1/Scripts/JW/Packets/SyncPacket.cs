using System.IO;
using Dana.Shared.Packets;
using JW.Shared.Packets;

namespace JW.Syncing
{
    /// <summary>
    /// BasePacket, ObjectCount, OwnerIDs[], ObjectIDs[], FloatXs[]
    /// </summary>
    public class SyncPacket : BasePacket
    {
        public int ObjectCount; // number of objects to sync
        public static int[] ObjectIDs;
        public static int[] OwnerIDs;
        public static FloatX[] FloatXs;
        
        public int[] SyncIDs { get { return ObjectIDs; } set { ObjectIDs = value; } }
        public FloatX[] SyncFloatXs { get { return FloatXs; } set { FloatXs = value; } }
        public int[]  SyncOwnerIDs { get { return OwnerIDs; } set { OwnerIDs = value; } }

        public SyncPacket(int objectCount, int[] OwnerIDS, int[] IDs, FloatX[] floatXes) : base(PacketTypes.SyncPacket)
        {
            this.ObjectCount = objectCount;
            this.SyncOwnerIDs = OwnerIDS;
            ObjectIDs = IDs;
            FloatXs = floatXes;
        }


        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(this.ObjectCount);
            for (int i = 0; i < this.ObjectCount; i++)
            {
                writer.Write(SyncOwnerIDs[i]);
                writer.Write(ObjectIDs[i]);
                writer.Write(FloatXs[i].Serialize());
            }
        }

        public static SyncPacket Deserialize(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            
            // Get the packet type
            br.ReadInt32();
            
            // Get the number of objects to sync and set the arrays to be this size
            int objectCount = br.ReadInt32();
            OwnerIDs = new int[objectCount];
            ObjectIDs = new int[objectCount];
            FloatXs = new FloatX[objectCount];
            
            // For each synced object, get the sync ID and FloatX packet
            for (int i = 0; i < objectCount; i++)
            {
                OwnerIDs[i] = br.ReadInt32();
                int objectID = br.ReadInt32();
                ObjectIDs[i] = objectID;
                
                int dataCount = br.ReadInt32(); // How many floats are coming next
                float[] floats = new float[dataCount];
                for (int j = 0; j < dataCount; j++)
                {
                    floats[j] = br.ReadSingle();
                }
                
                FloatX floatX = new FloatX(floats);
                FloatXs[i] = floatX;
            }
            return new SyncPacket(objectCount, OwnerIDs, ObjectIDs, FloatXs);
        }
    }
}