using System;
using System.Runtime.InteropServices;

namespace CCEngine
{
	public static class Helpers
	{
		public const float PI05 = 0.5f * (float)Math.PI;
		public const float PI20 = 2.0f * (float)Math.PI;

		public static int SizeOf<T>() where T : struct
		{
			return Marshal.SizeOf(typeof(T));
		}

		public static bool IsPowerOfTwo(int n)
		{
			return (n != 0) && ((n & (n - 1)) == 0);
		}

		/// <summary>
		/// Calculate nearest power of two >= n.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int NextPowerOfTwo(int n)
		{
			n--;
			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			return n + 1;
		}

		/// <summary>
		/// n = a * b where n, a, b are powers of two and a >= b.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static void FactorPowerOfTwo(int n, out int a, out int b)
		{
			int x = (int)Math.Ceiling(Math.Log((double)n, 2));
			a = 1 << (x - x / 2);
			b = 1 << (x / 2);
		}

		public static int Clamp(int value, int min, int max)
		{
			return Math.Min(Math.Max(value, min), max);
		}

		public static float Clamp(float value, float min, float max)
		{
			return Math.Min(Math.Max(value, min), max);
		}

		public static int Lerp(int v0, int v1, float alpha)
		{
			var a = (int)(alpha * 256);
			return v0 + ((v1 - v0) * a) / 256;
		}
	}
}
