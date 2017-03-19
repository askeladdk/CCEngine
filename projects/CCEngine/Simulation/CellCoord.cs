using System;
using System.Drawing;

namespace CCEngine.Simulation
{
	public struct CellCoord
	{
		private ushort x;
		private ushort y;

		public ushort X { get { return x; } }
		public ushort Y { get { return y; } }

		public ushort CellId { get { return (ushort)(y * Constants.MapSize + x); } }

		public CellCoord(ushort x, ushort y)
		{
			this.x = x;
			this.y = y;
		}

		public CellCoord(int x, int y)
		{
			this.x = (ushort)x;
			this.y = (ushort)y;
		}

		public CellCoord(ushort cell)
		{
			this.x = (ushort)(cell % Constants.MapSize);
			this.y = (ushort)(cell / Constants.MapSize);
		}

		public Point ToPoint()
		{
			return new Point(x * Constants.TileSize, y * Constants.TileSize);
		}
	}
}
