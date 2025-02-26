using System.IO;

namespace JW
{
	public class BasePacket
	{
		#region Variables
		protected MemoryStream writeMemoryStream;
		protected BinaryWriter binaryWriter;

		protected MemoryStream readMemoryStream;
		protected BinaryReader binaryReader;

		public enum PacketType
		{
			None,
			Message,
			Color,
			PlayersColorData
		}
		public PacketType packetType;
		public PlayerData playerData;
		#endregion

		#region Constructors
		public BasePacket()
		{
			packetType = PacketType.None;
			playerData = null;
		}

		public BasePacket(PacketType packetType, PlayerData playerData)
		{
			this.packetType = packetType;
			this.playerData = playerData;
		} 
		#endregion

		/// <summary>
		/// Begins the serialization process. Order: [(int)packetType, (string)playerData.Name, (int)playerData.Tag]
		/// </summary>
		protected void BeginSerialize()
		{
			// Set up memory and writer
			writeMemoryStream = new MemoryStream();
			binaryWriter = new BinaryWriter(writeMemoryStream);

			// Serialize Data
			binaryWriter.Write((int)packetType);
			binaryWriter.Write(playerData.Name);
			binaryWriter.Write(playerData.Tag);
		}

		protected byte[] EndSerialize()
		{
			return writeMemoryStream.ToArray();
		}

		public void Deserialize(byte[] buffer)
		{
			// Starts up memory stream and binary reader
			readMemoryStream = new MemoryStream(buffer);
			binaryReader = new BinaryReader(readMemoryStream);

			// Decodes the packet data and playerData
			packetType = (PacketType)binaryReader.ReadInt32();
			playerData = new PlayerData(binaryReader.ReadString(), binaryReader.ReadInt32());
		}
	} 
}
