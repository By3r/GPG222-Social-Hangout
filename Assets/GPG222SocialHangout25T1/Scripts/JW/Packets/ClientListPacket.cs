using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Dana.Shared.Packets;
using UnityEngine;

public class ClientListPacket : BasePacket
{
    public List<int> ClientDuckIDs;
    public List<string> ClientUsernames;

    public ClientListPacket(List<int> duckIDs, List<string> usernames) : base(PacketTypes.ClientListPackets)
    {
        ClientDuckIDs = duckIDs;
        ClientUsernames = usernames;
    }

    protected override void WriteData(BinaryWriter writer)
    {
        writer.Write(ClientDuckIDs.Count);
        for (int i = 0; i < ClientDuckIDs.Count; ++i)
        {
            writer.Write(ClientDuckIDs[i]);
        }

        writer.Write(ClientUsernames.Count);
        for (int j = 0;  j < ClientUsernames.Count; ++j)
        {
            writer.Write(ClientUsernames[j]);
        }
    }

    public static ClientListPacket Deserialize(byte[] buffer)
    {
        Debug.LogWarning("Client List Packet Deserialize");
        using MemoryStream stream = new MemoryStream(buffer);
        using BinaryReader reader = new BinaryReader(stream);

        reader.ReadInt32(); // Packet type

        int duckIDcount = reader.ReadInt32();
        List<int> duckIDs = new List<int>();
        for (int i = 0; i < duckIDcount; ++i)
        {
            duckIDs.Add(reader.ReadInt32());
        }

        int usernamesCount = reader.ReadInt32();
        List<string> usernames = new List<string>();
        for (int i = 0;i < usernamesCount; ++i)
        {
            usernames.Add(reader.ReadString());
        }

        ClientListPacket packet = new ClientListPacket(duckIDs, usernames);
        return packet;
    }
}
