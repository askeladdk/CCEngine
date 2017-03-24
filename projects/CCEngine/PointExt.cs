using System;
using System.Drawing;

namespace CCEngine
{
	/// <summary>
	/// Point extensions to deal with cell coordinates.
	/// </summary>
	public static class PointExt
	{
		/// <summary>
		/// Convert cell ID to map coordinate.
		/// </summary>
		/// <param name="cellId"></param>
		/// <returns></returns>
		public static Point FromCellId(int cellId)
		{
			return new Point(
				(cellId % Constants.MapSize) * Constants.TileSize,
				(cellId / Constants.MapSize) * Constants.TileSize
			);
		}

		/// <summary>
		/// Convert (x, y) cell coordinate to map coordinate.
		/// </summary>
		/// <param name="cellX"></param>
		/// <param name="cellY"></param>
		/// <returns></returns>
		public static Point FromCell(int cellX, int cellY)
		{
			return new Point(
				cellX * Constants.TileSize,
				cellY * Constants.TileSize
			);
		}

		/// <summary>
		/// Convert map coordinate to cell coordinate.
		/// </summary>
		/// <param name="mapCoord"></param>
		/// <returns></returns>
		public static Point ToCell(this Point mapCoord)
		{
			return new Point(
				mapCoord.X / Constants.TileSize,
				mapCoord.Y / Constants.TileSize
			);
		}

		/// <summary>
		/// Convert map coordinate to cell ID.
		/// </summary>
		/// <param name="mapCoord"></param>
		/// <returns></returns>
		public static int CellId(this Point mapCoord)
		{
			return (mapCoord.Y * Constants.MapSize + mapCoord.X) / Constants.TileSize;
		}
	}
}
