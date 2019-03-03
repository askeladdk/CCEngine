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
	}
}
