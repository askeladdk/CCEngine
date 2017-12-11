using System;

namespace CCEngine
{
	public struct XPos : IEquatable<XPos>
	{
		private short x, y;

		public int X { get => x; }
		public int Y { get => y; }
		public int CellX { get => x / Constants.TileSize; }
		public int CellY { get => y / Constants.TileSize; }
		public int OffsetX { get => x % Constants.TileSize; }
		public int OffsetY { get => y % Constants.TileSize; }
		public ushort CellId { get => CPos.MakeCellId(CellX, CellY); }
		public CPos CPos { get => new CPos(CellX, CellY); }

		public XPos(ushort cellId)
		{
			this.x = (short)((cellId % Constants.MapSize) * Constants.TileSize + Constants.TileSizeHalf);
			this.y = (short)((cellId / Constants.MapSize) * Constants.TileSize + Constants.TileSizeHalf);
		}

		public XPos(CPos cpos)
		{
			this.x = (short)(cpos.X * Constants.TileSize + Constants.TileSizeHalf);
			this.y = (short)(cpos.Y * Constants.TileSize + Constants.TileSizeHalf);
		}

		public XPos(int cx, int cy, int ox, int oy)
		{
			this.x = (short)(cx * Constants.TileSize + ox);
			this.y = (short)(cy * Constants.TileSize + oy);
		}

		public XPos Translate(int cx, int cy, int ox, int oy)
		{
			return new XPos(
				0,
				0,
				this.x + cx * Constants.TileSize + ox,
				this.y + cy * Constants.TileSize + oy
			);
		}

		public XPos Difference(XPos rhs)
		{
			return new XPos(0, 0, rhs.x - this.x, rhs.y - this.y);
		}

		public override string ToString()
		{
			return "({0}.{1}, {2}.{3})".F(CellX, OffsetX, CellY, OffsetY);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = (int)2166136261;
				hash = (16777619 * hash) ^ x.GetHashCode();
				hash = (16777619 * hash) ^ y.GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object rhs)
		{
			return this == ((XPos)rhs);
		}

		bool IEquatable<XPos>.Equals(XPos rhs)
		{
			return this == rhs;
		}

		public static bool operator==(XPos lhs, XPos rhs)
		{
			return lhs.x == rhs.x && lhs.y != rhs.y;
		}

		public static bool operator!=(XPos lhs, XPos rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y;
		}
	}
}
