namespace CCEngine.Simulation
{
	public class House
	{
		private string id;
		private bool multiplay;
		private bool multiplayPassive;
		private int colorScheme;

		public string ID { get => id; }
		public bool Multiplay { get => multiplay; }
		public bool MultiplayPassive { get => multiplayPassive; }
		public int ColorScheme { get => colorScheme; }

		public House(string id, IConfiguration rules)
		{
			this.id = id;
			this.multiplay = rules.GetBool(id, "Multiplay");
			this.multiplayPassive = rules.GetBool(id, "MultiplayPassive");
			this.colorScheme = rules.GetInt(id, "ColorScheme");
		}
	}
}
