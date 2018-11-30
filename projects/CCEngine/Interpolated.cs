using System;
using System.Diagnostics;

namespace CCEngine
{
	/// <summary>
	/// A value interpolated between two moments in time: current and previous.
	/// </summary>
	public struct Interpolated<T>
	{
		private T v0;
		private T v1;

		public T V0 { get => v0; }
		public T V1 { get => v1; }

		public Interpolated(T v0, T v1)
		{
			this.v0 = v0;
			this.v1 = v1;
		}

		/// Advance from [v0, v1] to [v1, v].
		public Interpolated<T> Advance(T v)
		{
			return new Interpolated<T>(v1, v);
		}
	}

	public static class Interpolated
	{
		/// Create initial interpolation.
		public static Interpolated<T> Create<T>(T v0, T v1)
		{
			return new Interpolated<T>(v0, v1);
		}

		/// Create initial interpolation.
		public static Interpolated<T> Create<T>(T v)
		{
			return new Interpolated<T>(v, v);
		}

		/// Linear interpolation of float.
		public static float Lerp(this Interpolated<float> lerp, float alpha)
		{
			return lerp.V0 * (1 - alpha) + lerp.V1 * alpha;
		}

		/// Linear interpolation of XPos.
		public static XPos Lerp(this Interpolated<XPos> lerp, float alpha)
		{
			// return XPos.Lerp(alpha, lerp.V0, lerp.V1);
			var dx = lerp.V1.X - lerp.V0.X;
			var dy = lerp.V1.Y - lerp.V0.Y;
			return new XPos(
				0, 0,
				lerp.V0.X + (int)(dx * alpha),
				lerp.V0.Y + (int)(dy * alpha)
			);
		}
	}
}