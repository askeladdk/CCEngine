using System;

namespace CCEngine
{
	public struct BinaryAngle : IEquatable<BinaryAngle>
	{
		private static CardinalDirection[] cardinals = {
			CardinalDirection.N,
			CardinalDirection.NE,
			CardinalDirection.E,
			CardinalDirection.SE,
			CardinalDirection.S,
			CardinalDirection.SW,
			CardinalDirection.W,
			CardinalDirection.NW,
		};

		private const int Maximum = 256;
		private const float TAU   = MathF.PI * 2.0f;
		private const float HPI   = MathF.PI * 0.5f;

		private byte angle;

		public BinaryAngle(int facing)
		{
			this.angle = (byte)facing;
		}

		public BinaryAngle(CardinalDirection cardinal)
		{
			this.angle = (byte)(32 * (int)cardinal);
		}

		public int Integer
		{
			get => angle;
		}

		public CardinalDirection CardinalDirection
		{
			get => cardinals[angle / 32];
		}

		public float Radians
		{
			get => HPI + TAU * ((float)angle / (float)Maximum);
		}

		public BinaryAngle(float radians)
		{
			var a = (int)(Maximum * MathF.Abs(radians - HPI) / TAU);
			this.angle = (byte)((a + 1) & ~1);
		}

		public BinaryAngle Add(BinaryAngle b)
		{
			return new BinaryAngle(this.angle + b.angle);
		}

		public BinaryAngle Sub(BinaryAngle b)
		{
			return new BinaryAngle(this.angle - b.angle);
		}

		public int Scale(int scale)
		{
			return (angle * scale) / Maximum;
		}

		public static BinaryAngle Between(int x0, int y0, int x1, int y1)
		{
			var radians = TAU + MathF.Atan2(y0 - y1, x0 - x1);
			return new BinaryAngle(radians);
		}

		public override string ToString()
		{
			return "{0}{1}".F(angle, this.CardinalDirection);
		}

		public override int GetHashCode()
		{
			return angle.GetHashCode();
		}

		public override bool Equals(object rhs)
		{
			return this == ((BinaryAngle)rhs);
		}

		bool IEquatable<BinaryAngle>.Equals(BinaryAngle rhs)
		{
			return this == rhs;
		}

		public static bool operator==(BinaryAngle lhs, BinaryAngle rhs)
		{
			return lhs.angle == rhs.angle;
		}

		public static bool operator!=(BinaryAngle lhs, BinaryAngle rhs)
		{
			return lhs.angle != rhs.angle;
		}

		public static BinaryAngle Lerp(float alpha, BinaryAngle a0, BinaryAngle a1)
		{
			var da = a0.angle - a1.angle;
			return new BinaryAngle(a0.angle + (int)(alpha * da));
		}
	}
}
