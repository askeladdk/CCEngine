using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCEngine
{
	public class Constants
	{
		// Updates per second.
		public const float FrameRate = 15.0f;

		// Map width and height in cells.
		public const int MapSize = 128;
		
		// Total cells in map.
		public const int MapCells = MapSize * MapSize;
	
		// Map tile width and height in pixels.
		public const int TileSize = 24;

		// Number of house color remaps in palette.cps.
		public const int HouseColors = 8;
	}
}
