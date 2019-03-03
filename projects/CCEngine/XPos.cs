using System;

namespace CCEngine
{
	public struct XPos : IEquatable<XPos>
	{
		private static XPos[] adjacent = {
			new XPos(0xFF000000), // N  (-256,     0)
			new XPos(0xFF000100), // NE (-256,  +256)
			new XPos(0x00000100), // E  (   0,  +256)
			new XPos(0x01000100), // SE (+256,  +256)
			new XPos(0x01000000), // S  (+256,     0)
			new XPos(0x0100FF00), // SW (+256,  -256)
			new XPos(0x0000FF00), // W  (   0,  -256)
			new XPos(0xFF00FF00), // SW (-256,  -256)
		};

		private uint value;

		public short LeptonsX { get => (short)(value & 0xFFFF); }
		public short LeptonsY { get => (short)((value >> 16) & 0xFFFF); }
		public sbyte CellX { get => (sbyte)((value & 0x00007F00) >> 8); }
		public sbyte CellY { get => (sbyte)((value & 0x7F000000) >> 24); }
		public byte SubCellX { get => (byte)((value & 0xFF)); }
		public byte SubCellY { get => (byte)((value >> 16) & 0xFF); }
		public XPos Center { get => new XPos(value & 0xFF80FF80); }
		public XPos TopLeft { get => new XPos(value & 0xFF00FF00); }
		public bool IsNegative { get => (value & 0x80008000) != 0; }
		public CPos CPos { get => new CPos(CellX, CellY); }

		private XPos(uint value)
		{
			this.value = value;
		}

		public static XPos FromCellId(ushort cellId)
		{
			var x = (uint)cellId % Constants.MapSize;
			var y = (uint)cellId / Constants.MapSize;
			return new XPos(0x00800080 | (x << 8) | (y << 24));
		}

		public static XPos FromCell(CPos cell)
		{
			return XPos.FromCellId(cell.CellId);
		}

		public static XPos FromLeptons(short leptonsX, short leptonsY)
		{
			var x = (uint)( (ushort)leptonsX | ((ushort)leptonsY << 16) );
			return new XPos(x);
		}

		public XPos AdjacentCell(CardinalDirection facing)
		{
			return XPos.Add(this, XPos.adjacent[(int)facing]).Center;
		}

		public static XPos Add(XPos a, XPos b)
		{
			var leptonsX = (short)(a.LeptonsX + b.LeptonsX);
			var leptonsY = (short)(a.LeptonsY + b.LeptonsY);
			return XPos.FromLeptons(leptonsX, leptonsY);
		}

		public static XPos Sub(XPos a, XPos b)
		{
			return XPos.FromLeptons(
				(short)(a.LeptonsX - b.LeptonsX),
				(short)(a.LeptonsY - b.LeptonsY)
			);
		}

		public override string ToString()
		{
			return "({0}.{1}, {2}.{3})".F(CellX, SubCellX, CellY, SubCellY);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
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
			return lhs.value == rhs.value;
		}

		public static bool operator!=(XPos lhs, XPos rhs)
		{
			return lhs.value != rhs.value;
		}

		public static float Distance(XPos x0, XPos x1)
		{
			var dx = x1.LeptonsX - x0.LeptonsX;
			var dy = x1.LeptonsY - x0.LeptonsY;
			return MathF.Sqrt(dx * dx + dy * dy);
		}

		public static XPos Lerp(float alpha, XPos p0, XPos p1)
		{
			var dx = p1.LeptonsX - p0.LeptonsX;
			var dy = p1.LeptonsY - p0.LeptonsY;
			return XPos.FromLeptons(
				(short)(p0.LeptonsX + (dx * alpha)),
				(short)(p0.LeptonsY + (dy * alpha))
			);
		}
	}
}
