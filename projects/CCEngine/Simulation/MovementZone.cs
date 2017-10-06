using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Movement zone specifies on what land mobile entities can move.
	/// </summary>
	public enum MovementZone : int
	{
		Foot,
		Wheel,
		Track,
		Float,
		Amphibious,

		// Special cases must be listed last
		None,
		Fly
	}

	public static class MovementZoneExt
	{
		private static Dictionary<string, MovementZone> translate = new Dictionary<string, MovementZone> {
			{"Foot", MovementZone.Foot},
			{"Wheel", MovementZone.Wheel},
			{"Track", MovementZone.Track},
			{"Float", MovementZone.Float},
			{"Amphibious", MovementZone.Amphibious},
		};

		public static IReadOnlyDictionary<string, MovementZone> Zones
		{
			get { return translate; }
		}

		public static MovementZone Parse(string s)
		{
			MovementZone z;
			return translate.TryGetValue(s, out z) ? z : MovementZone.None;
		}
	}
}
