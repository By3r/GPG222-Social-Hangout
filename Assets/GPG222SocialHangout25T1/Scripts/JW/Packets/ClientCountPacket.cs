using Dana.Shared.Packets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ClientCountPacket : BasePacket
{
    public int ClientCount = 0;

    public ClientCountPacket() : base(PacketTypes.CLientCountPacket)
    {
        ClientCount = 0;
    }

    public ClientCountPacket(int count) : base(PacketTypes.CLientCountPacket)
    {
        ClientCount = count;
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(ClientCount);
    }

    public static ClientCountPacket Deserialize(byte[] buffer)
    {
        using MemoryStream memoryStream = new MemoryStream(buffer);
        using BinaryReader reader = new BinaryReader(memoryStream);

        reader.ReadInt32(); // packet type

        int packetCount = reader.ReadInt32();
        return new ClientCountPacket(packetCount);
    }
}
