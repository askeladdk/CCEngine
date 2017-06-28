using System;

namespace CCEngine
{
	/// <summary>
	/// Logical facing.
	/// </summary>
	public static class Facing
	{
		public const int North   = 0;
		public const int East    = 64;
		public const int South   = 128;
		public const int West    = 192;
		public const int Facings = 256;

		private const double PI20d = System.Math.PI * 2.0;
		private const double PI05d = System.Math.PI * 0.5;
		private const float PI20f  = (float)PI20d;
		private const float PI05f  = (float)PI05d;

		/// <summary>
		/// Convert an integer to a logical facing.
		/// </summary>
		public static int FromInt(int n)
		{
			return n % Facings;
		}

		/// <summary>
		/// Convert an angle in radians to a logical facing.
		/// </summary>
		public static int FromRadians(double radians)
		{
			var facings = (int)(Facings * (radians - PI05d) / PI20d);
			return FromInt((facings + 1) & ~1); // boot out odd numbers to fix small rounding errors
		}

		/// <summary>
		/// Convert an angle in radians to a logical facing.
		/// </summary>
		public static int FromRadians(float radians)
		{
			var facings = (int)(Facings * (radians - PI05f) / PI20f);
			return FromInt((facings + 1) & ~1); // boot out odd numbers to fix small rounding errors
		}

		/// <summary>
		/// Convert a logical facing to radians.
		/// </summary>
		public static float ToRadians(int logicalFacing)
		{
			var r = PI05f + PI20f * ((float)logicalFacing / (float)Facings);
			return r > PI20f ? r - PI20f : r;
		}

		/// <summary>
		/// Translate a logical facing to another scale.
		/// </summary>
		public static int Scale(int facing, int toFacings)
		{
			return (facing * toFacings) / Facings;
		}

		/// <summary>
		/// Calculate the angle in logical facings between two map positions.
		/// </summary>
		public static int Between(MPos source, MPos target)
		{
			var radians = PI20d + Math.Atan2(source.Y - target.Y, source.X - target.X);
			return FromRadians(radians);
		}

		/// <summary>
		/// Calculate the angle in logical facings between two cell locations.
		/// </summary>
		public static int Between(CPos source, CPos target)
		{
			var radians = PI20d + Math.Atan2(source.Y - target.Y, source.X - target.X);
			return FromRadians(radians);
		}
	}
}
