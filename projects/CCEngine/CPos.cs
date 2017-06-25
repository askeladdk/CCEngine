using System;
using System.Drawing;

namespace CCEngine
{
	/// <summary>
	/// Cell position.
	/// </summary>
	public struct CPos
	{
		private int x, y;

		public int X { get { return x; } }
		public int Y { get { return y; } }

		public CPos(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public CPos(ushort cellId)
		{
			this.x = cellId % Constants.MapSize;
			this.y = cellId / Constants.MapSize;
		}

		public MPos ToMPos()
		{
			return new MPos(
				this.x * Constants.TileSize,
				this.y * Constants.TileSize,
				0
			);
		}

		public int CellId
		{
			get { return y * Constants.MapSize + x; }
		}

		public CPos Translate(int dx, int dy)
		{
			return new CPos(x + dx, y + dy);
		}

		public float Distance(CPos other)
		{
			var dx = this.x - other.x;
			var dy = this.y - other.y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

		public override string ToString()
		{
			return "({0}, {1})".F(x, y);
		}

		public override bool Equals(object obj)
		{
			return this.CellId == ((CPos)obj).CellId;
		}

		public override int GetHashCode()
		{
			return this.CellId;
		}
	}

	public static class RectangleExt
	{
		public static bool Contains(this Rectangle rect, CPos cpos)
		{
			return rect.Contains(cpos.X, cpos.Y);
		}

		public static bool Contains(this Rectangle rect, MPos mpos)
		{
			return rect.Contains(mpos.XProj2D, mpos.YProj2D);
		}
	}
}
