using System;

namespace CCEngine
{
	public class Constants
	{
		// Updates per second.
		public const int FrameRate = 15;

		// Map width and height in cells.
		public const int MapSize = 128;
		
		// Total cells in map.
		public const int MapCells = MapSize * MapSize;
	
		// Map tile width and height in pixels.
		public const int TileSize = 24;

		// Number of house color remaps in palette.cps.
		public const int HouseColors = 8;

		// HUD constants.
		public const int HUDTopBarHeight = 16;
		public const int HUDSideBarWidth = 160;

		// Logical number of facings a TechnoType has.
		public const int Facings = 256;
	}
}
