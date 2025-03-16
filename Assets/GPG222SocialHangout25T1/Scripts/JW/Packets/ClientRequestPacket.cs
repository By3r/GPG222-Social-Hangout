using Dana.Shared.Packets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ClientRequestPacket : BasePacket
{
    public int packetID;

    public ClientRequestPacket(PacketTypes packetType) : base(PacketTypes.ClientRequestPacket)
    {
        packetID = (int)packetType;
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(packetID);
    }

    public static ClientRequestPacket Deserialize(byte[] buffer)
    {
        using MemoryStream stream = new MemoryStream(buffer);
        using BinaryReader reader = new BinaryReader(stream);

        reader.ReadInt32(); // Origonal packet type

        int id = reader.ReadInt32();

        return new ClientRequestPacket((PacketTypes)id);
    }
}
