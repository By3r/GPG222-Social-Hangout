using System;
using System.IO;
using System.Text;
using JW.Shared.Packets;
using JW.Syncing;

namespace Dana.Shared.Packets
{
    #region Enum
    public enum PacketTypes
    {
        None = 0,
        Join = 1,
        Chat = 2,
        CharacterSelect = 3,
        CharacterStatus = 4,
        FloatX = 5,
        SyncPacket = 6,
    }
    #endregion

    #region Interface
    public interface IPacket
    {
        PacketTypes PacketType { get; }
        byte[] SerializePacket();
    }
    #endregion

    #region - - - - - - Base Packet
    public abstract class BasePacket : IPacket
    {
        public PacketTypes PacketType { get; }

        protected BasePacket(PacketTypes packetType)
        {
            PacketType = packetType;
        }

        public byte[] SerializePacket()
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode);
            writer.Write((int)PacketType);
            WriteData(writer);
            return stream.ToArray();
        }

        protected abstract void WriteData(BinaryWriter writer);
    }
    #endregion

    #region Packet Handler
    public static class PacketHandler
    {
        public static IPacket DeserializePacket(byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode);

            PacketTypes packetType = (PacketTypes)reader.ReadInt32();
            stream.Position = 0;

            return packetType switch
            {
                PacketTypes.Join => JoinPacket.Deserialize(buffer),
                PacketTypes.Chat => ChatPacket.Deserialize(buffer),
                PacketTypes.CharacterSelect => DuckSelectPacket.Deserialize(buffer),
                PacketTypes.CharacterStatus => DuckOwnershipPacket.Deserialize(buffer),
                PacketTypes.FloatX => FloatX.Deserialize(buffer),
                PacketTypes.SyncPacket => SyncPacket.Deserialize(buffer),
                _ => throw new Exception($"Unknown packet type: {packetType}")
            };
        }
    }
    #endregion
}
