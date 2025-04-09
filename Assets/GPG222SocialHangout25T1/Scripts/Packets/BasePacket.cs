using System.Drawing;
using System.IO;
using Networking.Core;

namespace Networking.Packets
{
    public class BasePacket
    {
        protected MemoryStream  _readerStream;
        protected MemoryStream  _writerStream;
        protected BinaryReader _reader;
        protected BinaryWriter _writer;
        public int Size { get; protected set; }

        public enum PacketType
        {
            None = 0,
            Join = 1,
            ClientList = 2,
            Message = 3,
            Instantiate = 4,
            Position = 5,
            Destroy = 6,
            ReadyStatus = 7
        }
        public PacketType Type { get; set; }
        public PlayerData PlayerData { get; set; }

        public BasePacket()
        {
            Type = PacketType.None;
            PlayerData = new PlayerData();
        }

        public BasePacket(PacketType type)
        {
            Type = type;
            PlayerData = new PlayerData();
        }

        public BasePacket(PacketType type, PlayerData playerData)
        {
            Type = type;
            PlayerData = playerData;
        }

        protected void BeginSerialize()
        {
            _writerStream = new MemoryStream();
            _writer = new BinaryWriter(_writerStream);
            
            _writer.Write((int)Type);
            _writer.Write(PlayerData.DuckID);
            _writer.Write(PlayerData.Username);
        }

        protected byte[] EndSerialize()
        {
            return _writerStream.ToArray();
        }

        public void Deserialize(byte[] buffer, ref int count, ref int offset)
        {
            _readerStream = new MemoryStream(buffer);
            _readerStream.Seek(offset, SeekOrigin.Begin);
            _reader = new BinaryReader(_readerStream);
            Size = 0; // The size of the packet so far being deserialized
            
            Type = (PacketType)_reader.ReadInt32();
            Size += sizeof(int); // PacketType size
            
            PlayerData = new PlayerData(_reader.ReadInt32(), _reader.ReadString());
            Size += PlayerData.Username.Length + 1; // Full size of the string including null terminator
            Size += sizeof(int); // DuckID size
        }
    }
}