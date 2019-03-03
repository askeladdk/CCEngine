using System;
using System.Diagnostics;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Land type specifies what kind of land a tile is.
	/// </summary>
	public enum LandType : byte
	{
		// The order is important, do not change
		Clear,
		Beach,
		Rock,
		Road,
		Water,
		River,
		Rough,
		Resource,
		Wall,
	}

	/// <summary>
	/// Land specifies which tiles are passable by which movement zones.
	/// </summary>
	public struct Land
	{
		public static int Count = Enum.GetNames(typeof(LandType)).Length;

		private int[] speedMultipliers;

		public bool IsBuildable { get; private set; }

		public Land(bool buildable, float[] speedMultipliers)
		{
			Debug.Assert(speedMultipliers.Length == Simulation.Speed.Count);
			this.IsBuildable = buildable;
			this.speedMultipliers = new int[Simulation.Speed.Count];
			for(var i = 0; i < Simulation.Speed.Count; i++)
				this.speedMultipliers[i] = (int)(100 * (1 / speedMultipliers[i]));
		}

		public int Speed(SpeedType spdt, int baseSpeed)
		{
			var mult = this.speedMultipliers[(int)spdt];
			return mult > 0 ? (100 * baseSpeed) / mult : 0;
		}

		public bool IsPassable(SpeedType spdt)
		{
			return this.speedMultipliers[(int)spdt] > 0;
		}
	}
}
