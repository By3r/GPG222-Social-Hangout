using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dana.Shared.Packets
{
    public class JoinPacket : IPacket
    {
        #region Variabels
        public PacketTypes PacketType => PacketTypes.Join;
        public string username { get; }
        public int duckID { get; }
        public List<int> DucksIDs { get; }
        public List<string> ClientUsernames { get; }
        #endregion

        public JoinPacket(string username, int characterID, List<int> ducksIDs, List<string> usernames)
        {
            this.username = username;
            this.duckID = characterID;
            this.DucksIDs = ducksIDs;
            this.ClientUsernames = usernames;
        }
        public JoinPacket(string username, int characterID) 
        {
            this.username = username;
            this.duckID = characterID;
            DucksIDs = new List<int>();
            DucksIDs.Add(characterID);

            ClientUsernames = new List<string>();
            ClientUsernames.Add(username);
        }

        public byte[] SerializePacket()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode);
            writer.Write((int)PacketType);
            writer.Write(username);
            writer.Write(duckID);

            writer.Write(DucksIDs.Count);
            for (int i = 0; i < DucksIDs.Count; i++)
            {
                writer.Write(DucksIDs[i]);
            }

            writer.Write(ClientUsernames.Count);
            for (int i = 0; i < ClientUsernames.Count; i++)
            {
                writer.Write(ClientUsernames[i]);
            }

            return stream.ToArray();
        }

        public static JoinPacket Deserialize(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);
            reader.ReadInt32(); 
            string username = reader.ReadString();
            int characterID = reader.ReadInt32();

            int duckIDsCount = reader.ReadInt32();
            List<int> duckIDs = new List<int>();
            for (int i = 0; i < duckIDsCount; i++)
            {
                duckIDs.Add(reader.ReadInt32());
            }

            int usernameCount = reader.ReadInt32();
            List<string> usernames = new List<string>();
            for (int j = 0; j < usernameCount; j++)
            {
                usernames.Add(reader.ReadString());
            }

            return new JoinPacket(username, characterID, duckIDs, usernames);
        }
    }
}
