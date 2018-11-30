using System;

namespace CCEngine
{
	public enum CardinalDirection : byte
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW
	}

	public static class CardinalDirectionExt
	{
		private static Vector2I[] vectors = new Vector2I[8]
		{
			new Vector2I( 0, -1),
			new Vector2I( 1, -1),
			new Vector2I( 1,  0),
			new Vector2I( 1,  1),
			new Vector2I( 0,  1),
			new Vector2I(-1,  1),
			new Vector2I(-1,  0),
			new Vector2I(-1, -1),
		};

		public static Vector2I ToVector(this CardinalDirection cardinal)
		{
			return vectors[(int)cardinal];
		}
	}
}
