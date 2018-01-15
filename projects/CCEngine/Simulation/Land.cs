using System;
using System.Collections.Generic;

namespace CCEngine.Simulation
{
	/// Land type specifies what kind of land a tile is.
	public enum LandType : byte
	{
		// The order is important
		Clear,
		Beach,
		Rock,
		Road,
		Water,
		River,
		Rough,
		Resource,
		Wall,
		Count,
	}

	/// Land specifies which tiles are passable by which movement zones.
	public class Land
	{
		private static Dictionary<LandType, string> translate = new Dictionary<LandType, string>
		{
			{LandType.Clear, "Clear"},
			{LandType.Beach, "Beach"},
			{LandType.Rock, "Rock"},
			{LandType.Road, "Road"},
			{LandType.Water, "Water"},
			{LandType.River, "River"},
			{LandType.Rough, "Rough"},
			{LandType.Resource, "Ore"},
			{LandType.Wall, "Wall"},
		};

		public static IReadOnlyDictionary<LandType, string> Lands
		{
			get { return translate; }
		}

		private float[] speeds = new float[MovementZoneExt.Zones.Count];
		private bool buildable;
		private LandType type;

		public Land(IConfiguration ini, string section, LandType type)
		{
			this.buildable = ini.GetBool(section, "Buildable");
			this.type = type;

			foreach (var kv in MovementZoneExt.Zones)
			{
				var speed = ini.GetFloat(section, kv.Key);
				this.speeds[(int)kv.Value] = speed;
			}
		}

		public float SpeedMultiplier(MovementZone z)
		{
			switch(z)
			{
				case MovementZone.None:
					return 0.0f;
				case MovementZone.Fly:
					return 1.0f;
				default:
					return this.speeds[(int)z];
			}
		}

		public bool IsPassable(MovementZone z)
		{
			return SpeedMultiplier(z) > 0;
		}

		public bool IsBuildable
		{
			get { return this.buildable; }
		}

		public LandType Type
		{
			get { return this.type; }
		}

		public override string ToString()
		{
			return this.type.ToString();
		}
	}
}
