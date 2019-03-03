using System;

namespace CCEngine
{
	public static class Lepton
	{
		public static short FromPixel(int p)
		{
			return (short)( (p * 256 + Constants.TileSizeHalf) / Constants.TileSize );
		}

		public static int ToPixel(short e)
		{
			return (e * Constants.TileSize + 128) / 256;
		}

		public static sbyte GetCell(short e)
		{
			return (sbyte)((e & 0x7F00) >> 8);
		}

		public static byte GetSubCell(short e)
		{
			return (byte)(e & 0xFF);
		}
	}
}
