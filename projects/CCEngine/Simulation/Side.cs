using System.Collections.Generic;

namespace CCEngine.Simulation
{
	public class Side
	{
		private string id;
		private House[] houses;

		public string ID { get => id; }
		public IReadOnlyList<House> Houses { get => houses; }

		public Side(string id, string[] houseids, IDictionary<string, House> houses)
		{
			var hs = new List<House>();
			foreach(var hid in houseids)
			{
				if(houses.ContainsKey(hid))
					hs.Add(houses[hid]);
			}
			this.id = id;
			this.houses = hs.ToArray();
		}
	}
}