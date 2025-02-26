namespace JW
{
	public class PlayerData
	{
		public string Name { get; private set; }
		public int Tag {  get; private set; }

		public PlayerData(string name, int tag)
		{
			this.Name = name;
			this.Tag = tag;
		}
	} 
}