using System;

namespace CCEngine
{
	public class Constants
	{
		// Updates per second.
		public const int FrameRate = 15;

		// Map width/height in cells.
		public const int MapSize = 128;
		
		// Total number of cells in map.
		public const int MapCellCount = MapSize * MapSize;
	
		// Map tile width and height in pixels.
		public const int TileSize = 24;
		public const int TileSizeHalf = TileSize / 2;

		// Number of house color remaps in palette.cps.
		public const int HouseColors = 8;

		// HUD constants.
		public const int HUDTopBarHeight = 16;
		public const int HUDSideBarWidth = 160;
	}
}
