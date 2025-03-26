namespace Networking.Packets
{
    public class AnimationPacket <T> : BasePacket
    {
        public string Name { get; private set; }
        public int DataTypeID { get; private set; }
        public T Data { get; private set; }
        
        public AnimationPacket(string name, T data)
        {
            Name = name;
            Data = data;

            if (typeof(T) == typeof(bool))
            {
                DataTypeID = 0;
            }
        }

        public byte[] Serialize()
        {
            BeginSerialize();
            
            _writer.Write(Name);
            _writer.Write(DataTypeID);
            
            return EndSerialize();
        }
    }
}