using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Script.NoneProject
{

    /// <summary>
    /// Base class notes lol
    /// </summary>

    public class ClassNotes : MonoBehaviour //
    {


        private struct PlayerData
        {
            public int health;
            public string name;
            // public int Tag;
            public int damage;

            public PlayerData(int health, string name, int damage)
            {
                this.health = health;
                this.name = name;
                this.damage = damage;
            }
        }

        private enum Type
        {
            None,
            Message
        }

        private byte[] Serialize(PlayerData playerData) // Make sure you serialize and deserializ in the same order.
        {
            playerData = new PlayerData();
            // BinaryFormatter bf = new BinaryFormatter(); -- Format is insecure and cannot be made secure. When you serialise data and you convert it into bytes,
            // it gets transmitted over the internet.
            // Some can take that data, intercept it and, add malicious code to it.

            // Stream Types: File Stream, Network Stream, Memory Stream +++ etc. Streams are basically pipes that data go through.
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            #region Intitial Code
            //bw.Write(playerData.health); 
            //bw.Write(playerData.health); 
            //bw.Write(playerData.health);
            #endregion

            return ms.ToArray();
        }

    }
    /*private PlayerData Deserialize(byte[] buffer)
    {
        // BinaryFormatter bf = new BinaryFormatter(); -- Format is insecure and cannot be made secure. When you serialise data and you convert it into bytes,
        // it gets transmitted over the internet.
        // Some can take that data, intercept it and, add malicious code to it.

        // Stream Types: File Stream, Network Stream, Memory Stream +++ etc. Streams are basically pipes that data go through.
        MemoryStream ms = new MemoryStream(buffer);
        BinaryWriter bw = new BinaryWriter(ms);

        #region Initial Code
        //bw.Write(playerData.health);
        //bw.Write(playerData.health);
        //bw.Write(playerData.health);
        #endregion

        //   return;
    }*/


    ///<summary>
    ///Create a ms writer
    ///create a binary writer
    ///then create a memory stream reader
    ///binary reader
    ///
    /// create a BasePacket(Type packetType) You need the player Data when sending a byte packet
    /// {
    /// Whoever needs to send a base[acket have to send information about what kind of packettype 
    /// they want to send and who is the one sending it
    /// this.packetType = packetType;
    /// this.playerDAta = playerData;
    /// }
    /// 
    #region Serializing
    /// byte[] Serialize()
    /// {
    /// msw = new MmoryStream();
    /// bw = new BinaryWriter(msw)
    /// bw.Write((int) packetType);
    /// bw.Write(playerData.name);
    /// bw.Write()
    /// 
    /// return msw.ToArray
    /// }
    #endregion
    /// void deserialize(byte[] buffer)
    /// {
    /// 
    /// packetType = (PacketType)br.ReadInt32();
    /// playerDAta = new PLayerData(br.ReadString(), br.ReadInt32);
    /// 
    /// return buffer();
    /// }

    /// 
    /// 
    /// </summary>

}
#region Class Note Stuff
///<summary>
///Class Note times:
///
/// 
/// 
/// 
/// </summary>
#endregion