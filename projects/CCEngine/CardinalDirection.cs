using System;

namespace CCEngine
{
	public enum CardinalDirection
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
		public static Vector2I ToVector(this CardinalDirection cardinal)
		{
			switch(cardinal)
			{
				case CardinalDirection.N:
					return new Vector2I(0, -1);
				case CardinalDirection.NE:
					return new Vector2I(1, -1);
				case CardinalDirection.E:
					return new Vector2I(1, 0);
				case CardinalDirection.SE:
					return new Vector2I(1, 1);
				case CardinalDirection.S:
					return new Vector2I(0, 1);
				case CardinalDirection.SW:
					return new Vector2I(-1, 1);
				case CardinalDirection.W:
					return new Vector2I(-1, 0);
				case CardinalDirection.NW:
					return new Vector2I(-1, -1);
				default:
					throw new Exception("Invalid cardinal");
			}
		}
	}
}