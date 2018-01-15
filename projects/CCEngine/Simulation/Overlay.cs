using CCEngine.Rendering;

namespace CCEngine.Simulation
{
	/// Overlay type determines special game logic.
	public enum OverlayType : byte
	{
		/// No overlay.
		Empty = 0,
		/// Cannot be crossed.
		Obstacle = 32,
		/// Apply wall logic.
		Wall = 64,
		/// Apply CTF logic.
		Flag = 96,
		/// Apply crate logic.
		CrateWood = 128,
		/// Apply crate logic.
		CrateSteel = 160,
		/// Harvestable material.
		ResourceCommon = 192,
		/// Harvestable material, but more valuable.
		ResourcePrecious = 224,
	}

	/// Overlay.
	public class Overlay
	{
		private Sprite art;
		private OverlayType type;

		public Sprite Art { get => art; }
		public OverlayType Type { get => type; }

		public Overlay(string type, Sprite art)
		{
			this.type = GetType(type);
			this.art = art;
		}

		private static OverlayType GetType(string typestr)
		{
			switch(typestr)
			{
				case "Obstacle": return OverlayType.Obstacle;
				case "Wall": return OverlayType.Wall;
				case "Flag": return OverlayType.Flag;
				case "CrateWood": return OverlayType.CrateWood;
				case "CrateSteel": return OverlayType.CrateSteel;
				case "ResourceCommon": return OverlayType.ResourceCommon;
				case "ResourcePrecious": return OverlayType.ResourcePrecious;
				default: return OverlayType.Empty;
			}
		}
	}
}