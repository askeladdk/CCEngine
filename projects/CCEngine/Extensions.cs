using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace CCEngine
{
	public static class Extensions
	{
		public static byte[] ReadBytes(this Stream s, int nbytes)
		{
			byte[] bytes = new byte[nbytes];
			s.Read(bytes, 0, nbytes);
			return bytes;
		}

		public static T TypeCastByteBuffer<T>(this byte[] buffer)
		{
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			T p = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return p;
		}

		public static T ReadStruct<T>(this Stream s) where T : struct
		{
			return s.ReadBytes(Helpers.SizeOf<T>()).TypeCastByteBuffer<T>();
		}

		public static int BinarySearch<T, TKey>(this IList<T> tf, TKey target, Func<T, TKey> selector)
		{
			int lo = 0;
			int hi = (int)tf.Count - 1;
			var comp = Comparer<TKey>.Default;

			while (lo <= hi)
			{
				int median = lo + (hi - lo >> 1);
				var num = comp.Compare(selector(tf[median]), target);
				if (num == 0) return median;
				if (num  < 0) lo = median + 1;
				else          hi = median - 1;
			}

			return ~lo;
		}

		public static string F(this string fmt, params object[] args)
		{
			return string.Format(fmt, args);
		}

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict,
			TKey key, TValue def=default(TValue))
		{
			TValue value;
			if (dict.TryGetValue(key, out value))
				return value;
			return def;
		}
	}
}
