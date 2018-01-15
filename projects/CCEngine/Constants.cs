using System;

namespace CCEngine
{
	public class Constants
	{
		/// Updates per second.
		public const int FrameRate = 15;

		/// Map width and height in cells.
		public const int MapSize = 128;
		
		/// Total number of cells in the map.
		public const int MapCellCount = MapSize * MapSize;
	
		/// Map tile width and height in pixels.
		public const int TileSize = 24;

		/// Half the map tile width and height in pixels.
		public const int TileSizeHalf = TileSize / 2;

		/// Maximum number of houses.
		public const int MaxHouses = 32;

		/// Number of house color remaps in palette.cps.
		public const int HouseColors = 8;

		/// Height of the HUD top bar in pixels.
		public const int HUDTopBarHeight = 16;

		/// Width of the HUD sidebar in pixels.
		public const int HUDSideBarWidth = 160;
	}
}
