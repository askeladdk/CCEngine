using System;
using System.Collections;
using System.Collections.Generic;

namespace CCEngine.Collections
{
	public sealed class PriorityQueue<TKey, TValue>
		: IEnumerable, IEnumerable<KeyValuePair<TKey, TValue>>
	{
		private readonly List<KeyValuePair<TKey, TValue>> heap = new List<KeyValuePair<TKey, TValue>>();
		private readonly IComparer<TKey> comparer;

		/// <returns></returns>
		private int Compare(int idx0, int idx1)
		{
			return comparer.Compare(heap[idx0].Key, heap[idx1].Key);
		}

		private void Swap(int idx0, int idx1)
		{
			heap.Swap(idx0, idx1);
		}

		private void BubbleUp(int idx)
		{
			while (idx > 0)
			{
				int pidx = (idx - 1) / 2;
				if (Compare(idx, pidx) >= 0)
					break;
				Swap(idx, pidx);
				idx = pidx;
			}
		}

		private void BubbleDown(int idx)
		{
			var count = heap.Count;
			var last = (count - 2) / 2;

			while (idx <= last)
			{
				var left = 2 * idx + 1;
				var right = left + 1;
				var min = idx;
				if (left < count && Compare(left, min) <= 0) min = left;
				if (right < count && Compare(right, min) <= 0) min = right;
				if (min == idx)
					break;
				Swap(min, idx);
				idx = min;
			}
		}

		public PriorityQueue(IComparer<TKey> comparer)
		{
			this.comparer = comparer;
		}

		public void Enqueue(KeyValuePair<TKey, TValue> item)
		{
			heap.Add(item);
			var last = heap.Count - 1;
			BubbleUp(last);
		}

		public void Enqueue(TKey priority, TValue value)
		{
			Enqueue(new KeyValuePair<TKey, TValue>(priority, value));
		}

		public TValue Dequeue()
		{
			if (heap.Count == 0)
				throw new InvalidOperationException("PriorityQueue is empty");
			var value = heap[0].Value;
			var last = heap.Count - 1;
			heap[0] = heap[last];
			heap.RemoveAt(last);
			BubbleDown(0);
			return value;
		}

		public void Clear()
		{
			heap.Clear();
		}

		public int Count
		{
			get { return heap.Count; }
		}

		public TValue Peek()
		{
			if (heap.Count == 0)
				throw new InvalidOperationException("PriorityQueue is empty");
			return heap[0].Value;
		}

		public bool TryDequeue(out TValue value)
		{
			var ok = heap.Count > 0;
			value = ok ? Dequeue() : default(TValue);
			return ok;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return heap.GetEnumerator();
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return heap.GetEnumerator();
		}
	}

	public static class PriorityQueue
	{
		public static PriorityQueue<TKey, TValue> Min<TKey, TValue>()
			where TKey : IComparable<TKey>
		{
			var comparer = Comparer<TKey>.Create((a, b) => a.CompareTo(b));
			return new PriorityQueue<TKey, TValue>(comparer);
		}

		public static PriorityQueue<TKey, TValue> Max<TKey, TValue>()
			where TKey : IComparable<TKey>
		{
			var comparer = Comparer<TKey>.Create((a, b) => b.CompareTo(a));
			return new PriorityQueue<TKey, TValue>(comparer);
		}
	}
}
