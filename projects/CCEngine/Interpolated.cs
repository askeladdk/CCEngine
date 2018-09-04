using System;
using System.Diagnostics;

namespace CCEngine
{
	/// Time-varying value that is interpolated between ticks t0 and t1.
	/// There is no concept of time besides the current (t0) and future (t1) moment.
	/// Use Lerp.Advance to advance to the next moment in time.
	public struct Interpolated<T>
	{
		private T v0;
		private T v1;
		private int t0;
		private int t1;

		public int T0 { get => t0; }
		public int T1 { get => t1; }
		public T V0 { get => v0; }
		public T V1 { get => v1; }

		public Interpolated(int t0, T v0, int t1, T v1)
		{
			Debug.Assert(t0 <= t1);
			this.v0 = v0;
			this.v1 = v1;
			this.t0 = t0;
			this.t1 = t1;
		}

		public float Alpha(float t)
		{
			return Helpers.Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
		}

		/// Advance from [t0, t1] to [t1, t1 + dt].
		public Interpolated<T> Advance(int dt, T v)
		{
			return new Interpolated<T>(t1, v1, t1 + dt, v);
		}

		/// Adjust from [t0, t1] to [t0, t0 + dt].
		public Interpolated<T> Adjust(int dt, T v)
		{
			return new Interpolated<T>(t0, v0, t0 + dt, v);
		}
	}

	public static class Interpolated
	{
		/// Create initial interpolation.
		public static Interpolated<T> Create<T>(int t0, T v0, int t1, T v1)
		{
			return new Interpolated<T>(t0, v0, t1, v1);
		}

		/// Create initial interpolation.
		public static Interpolated<T> Create<T>(int t, T v)
		{
			return new Interpolated<T>(t, v, t, v);
		}

		/// Interpolate at tick t.
		public static float Lerp(this Interpolated<float> lerp, float t)
		{
			var alpha = lerp.Alpha(t);
			return lerp.V0 * (1 - alpha) + lerp.V1 * alpha;
		}

		/// Interpolate at tick t.
		public static XPos Lerp(this Interpolated<XPos> lerp, float t)
		{
			var alpha = lerp.Alpha(t);
			return XPos.Lerp(alpha, lerp.V0, lerp.V1);
		}
	}
}