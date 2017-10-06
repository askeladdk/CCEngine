using System;
using System.Drawing;

namespace CCEngine
{
	/// <summary>
	/// Cell position.
	/// </summary>
	public struct CPos : IEquatable<CPos>
	{
		private readonly short x, y;

		public short X { get { return x; } }
		public short Y { get { return y; } }

		public CPos(int x, int y)
		{
			this.x = (short)x;
			this.y = (short)y;
		}

		public CPos(ushort cellId)
		{
			this.x = (short)(cellId % Constants.MapSize);
			this.y = (short)(cellId / Constants.MapSize);
		}

		public MPos ToMPos()
		{
			return new MPos(
				this.x * Constants.TileSize,
				this.y * Constants.TileSize,
				0
			);
		}

		public ushort CellId
		{
			get { return CPos.MakeCellId(x, y); }
		}

		public CPos Translate(int dx, int dy)
		{
			return new CPos(x + dx, y + dy);
		}

		public static int DistanceSquared(CPos lhs, CPos rhs)
		{
			var dx = lhs.x - rhs.x;
			var dy = lhs.y - rhs.y;
			return dx * dx + dy * dy;
		}

		public static ushort MakeCellId(int x, int y)
		{
			return (ushort)(y * Constants.MapSize + x);
		}

		public override string ToString()
		{
			return "({0}, {1})".F(x, y);
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
			return this == ((CPos)rhs);
		}

		bool IEquatable<CPos>.Equals(CPos rhs)
		{
			return this == rhs;
		}

		public static bool operator==(CPos lhs, CPos rhs)
		{
			return !(lhs.x != rhs.x || lhs.y != rhs.y);
		}

		public static bool operator!=(CPos lhs, CPos rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y;
		}
	}
}
