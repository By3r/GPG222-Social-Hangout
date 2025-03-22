using System.Collections.Generic;
using Networking.Core;

namespace Networking.Packets
{
    public class PlayerDataListPacket : BasePacket
    {
        public int ListCount;
        public List<PlayerData> Players;

        public PlayerDataListPacket() : base(PacketType.ClientList)
        {
            ListCount = 0;
            Players = new List<PlayerData>();
        }

        public PlayerDataListPacket(List<PlayerData> players) : base(PacketType.ClientList)
        {
            ListCount = players.Count;
            Players = players;
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            
            // Write how many PlayerDatas are in the list
            _writer.Write(ListCount);
            
            // Write each PlayerData into the memory stream
            foreach (var player in Players)
            {
                _writer.Write(player.Username);
                _writer.Write(player.DuckID);
            }
            
            return EndSerialize();
        }

        public PlayerDataListPacket Deserialize(byte[] buffer, ref int bufferSize, ref int offset)
        {
            base.Deserialize(buffer, ref bufferSize, ref offset);
            
            ListCount = _reader.ReadInt32();
            Size += sizeof(int);

            Players = new List<PlayerData>();
            for (int i = 0; i < ListCount; i++)
            {
                string username = _reader.ReadString();
                Size += username.Length + 1;
                int  duckID = _reader.ReadInt32();
                Size += sizeof(int);
                
                PlayerData playerData = new PlayerData(duckID, username);
                Players.Add(playerData);
            }

            bufferSize -= Size;
            offset += Size;
            
            return this;
        }
    }
}