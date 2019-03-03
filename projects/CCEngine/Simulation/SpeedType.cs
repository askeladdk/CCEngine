using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Speed type specifies on what land mobile entities can move.
	/// </summary>
	public enum SpeedType : byte
	{
		Foot,
		Wheel,
		Track,
		Float,
		Amphibious,
		Winged
	}

	public static class Speed
	{
		public static int Count = Enum.GetNames(typeof(SpeedType)).Length;
	}
}
