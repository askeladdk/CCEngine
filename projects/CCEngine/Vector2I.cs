using System;

namespace CCEngine
{
	public struct Vector2I : IEquatable<Vector2I>
	{
		private int x;
		private int y;

		public int X { get => x; }

		public int Y { get => y; }

		public BinaryAngle BinaryAngle { get => new BinaryAngle(MathF.Atan2(-y, x)); }

		public Vector2I(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2I Multiply(int n)
		{
			return new Vector2I(x * n, y * n);
		}

		public Vector2I Multiply(float n)
		{
			return new Vector2I((int)(x * n), (int)(y * n));
		}


		public override string ToString()
		{
			return "({0},{1})".F(x, y);
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
			return this == ((Vector2I)rhs);
		}

		bool IEquatable<Vector2I>.Equals(Vector2I rhs)
		{
			return this == rhs;
		}

		public static bool operator==(Vector2I lhs, Vector2I rhs)
		{
			return !(lhs.x != rhs.x || lhs.y != rhs.y);
		}

		public static bool operator!=(Vector2I lhs, Vector2I rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y;
		}
	}
}