using System;

namespace CCEngine
{
	/// <summary>
	/// Map position.
	/// </summary>
	public struct MPos : IEquatable<MPos>
	{
		private readonly int x, y, z;

		public int X { get { return x; } }
		public int Y { get { return y; } }
		public int Z { get { return z; } }

		public int XProj2D { get { return x; } }
		public int YProj2D { get { return y - z; } }

		public MPos(ushort cellId, bool centered = false)
		{
			this.x = (cellId % Constants.MapSize) * Constants.TileSize;
			this.y = (cellId / Constants.MapSize) * Constants.TileSize;
			this.z = 0;

			if(centered)
			{
				this.x += Constants.TileSize / 2;
				this.y += Constants.TileSize / 2;
			}
		}

		public MPos(CPos cpos, bool centered = false)
		{
			this.x = cpos.X * Constants.TileSize;
			this.y = cpos.Y * Constants.TileSize;
			this.z = 0;

			if (centered)
			{
				this.x += Constants.TileSize / 2;
				this.y += Constants.TileSize / 2;
			}
		}

		public MPos(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public CPos ToCPos()
		{
			return new CPos(
				this.x / Constants.TileSize,
				this.y / Constants.TileSize
			);
		}

		public MPos Translate(int x, int y, int z)
		{
			return new MPos(this.x + x, this.y + y, this.z + z);
		}

		public MPos Difference(MPos rhs)
		{
			return new MPos(this.x - rhs.x, this.y - rhs.y, this.z - rhs.z);
		}

		public int LengthSquared()
		{
			return x * x + y * y + z * z;
		}

		public static MPos Difference(MPos lhs, MPos rhs)
		{
			return new MPos(
				lhs.x - rhs.x,
				lhs.y - rhs.y,
				lhs.z - rhs.z
			);
		}

		public static int DistanceSquared(MPos lhs, MPos rhs)
		{
			var dx = lhs.x - rhs.x;
			var dy = lhs.y - rhs.y;
			var dz = lhs.z - rhs.z;
			return dx * dx + dy * dy + dz * dz;
		}

		public override string ToString()
		{
			return "({0}, {1}, {2})".F(x, y, z);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = (int)2166136261;
				hash = (16777619 * hash) ^ x.GetHashCode();
				hash = (16777619 * hash) ^ y.GetHashCode();
				hash = (16777619 * hash) ^ z.GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object rhs)
		{
			return this == ((MPos)rhs);
		}

		bool IEquatable<MPos>.Equals(MPos rhs)
		{
			return this == rhs;
		}

		public static bool operator==(MPos lhs, MPos rhs)
		{
			return !(lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z);
		}

		public static bool operator!=(MPos lhs, MPos rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
		}
	}
}
