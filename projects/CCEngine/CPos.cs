using System;
using System.Drawing;
using System.Linq;

namespace CCEngine
{
	/// <summary>
	/// Cell position.
	/// </summary>
	public struct CPos : IEquatable<CPos>, IComparable<CPos>
	{
		private ushort cellId;

		public int X { get => cellId % Constants.MapSize; }
		public int Y { get => cellId / Constants.MapSize; }
		public ushort CellId { get => cellId; }
		public XPos XPos { get => new XPos(cellId); }

		public CPos(int cx, int cy)
		{
			this.cellId = MakeCellId(cx, cy);
		}

		public CPos(ushort cellId)
		{
			this.cellId = cellId;
		}

		public CPos Translate(int dx, int dy)
		{
			return new CPos((ushort)(this.cellId + dx + dy * Constants.MapSize));
		}

		public override string ToString()
		{
			return "({0}, {1})".F(this.X, this.Y);
		}

		public override int GetHashCode()
		{
			return cellId.GetHashCode();
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
			return lhs.cellId == rhs.cellId;
		}

		public static bool operator!=(CPos lhs, CPos rhs)
		{
			return lhs.cellId != rhs.cellId;
		}

		public static ushort MakeCellId(int cx, int cy)
		{
			return (ushort)(cy * Constants.MapSize + cx);
		}

		int IComparable<CPos>.CompareTo(CPos other)
		{
			return this.cellId - other.cellId;
		}
	}
}
