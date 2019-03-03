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

		public static void ReadStructs<T>(this Stream s, T[] buf, int count) where T : struct
		{
			for(var i = 0; i < count; i++)
				buf[i] = s.ReadStruct<T>();
		}

		public static string F(this string fmt, params object[] args)
		{
			return string.Format(fmt, args);
		}
	}


	public static class IListExtensions
	{
		public static int BinarySearch<T, TKey>(this IList<T> list, int start, int length, TKey value, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			int lo = start;
			int hi = start + length - 1;

			while (lo <= hi)
			{
				int median = lo + (hi - lo >> 1);
				var num = comparer.Compare(selector(list[median]), value);
				if (num == 0)
					return median;
				if (num  < 0)
					lo = median + 1;
				else
					hi = median - 1;
			}

			return ~lo;
		}

		public static int BinarySearch<T>(this IList<T> list, int start, int length, T value, IComparer<T> comparer)
		{
			return list.BinarySearch(start, length, value, x => x, comparer);
		}

		public static int BinarySearch<T>(this IList<T> list, int start, int length, T value)
		{
			return list.BinarySearch(start, length, value, Comparer<T>.Default);
		}

		public static int BinarySearch<T>(this IList<T> list, T value)
		{
			return list.BinarySearch(0, list.Count, value);
		}

		public static void Swap<T>(this IList<T> list, int idx0, int idx1)
		{
			T tmp = list[idx0];
			list[idx0] = list[idx1];
			list[idx1] = tmp;
		}
	}
}
