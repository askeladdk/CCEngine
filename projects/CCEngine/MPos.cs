using System;

namespace CCEngine
{
	/// <summary>
	/// Map position.
	/// </summary>
	public struct MPos
	{
		private int x, y, z;

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

		public float Distance(MPos other)
		{
			var dx = this.x - other.x;
			var dy = this.y - other.y;
			var dz = this.z - other.z;
			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public override string ToString()
		{
			return "({0}, {1}, {2})".F(x, y, z);
		}
	}
}
