using System;
using System.Collections.Generic;

namespace CCEngine.Collections
{
	public sealed class PriorityQueue<TKey, TValue>
	{
		private readonly List<KeyValuePair<TKey, TValue>> heap = new List<KeyValuePair<TKey, TValue>>();
		private readonly IComparer<TKey> comparer;

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
			var count = this.Count;
			var last = (count - 2) / 2;

			while (idx <= last)
			{
				var left = 2 * idx + 1;
				var right = left + 1;
				var min = idx;
				if (left < count && Compare(left, min) <= 0)
					min = left;
				if (right < count && Compare(right, min) <= 0)
					min = right;
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

		public int Count
		{
			get { return heap.Count; }
		}

		public bool TryPeek(out KeyValuePair<TKey, TValue> kv)
		{
			var ok = (this.Count > 0);
			kv = ok ? heap[0] : default(KeyValuePair<TKey, TValue>);
			return ok;
		}

		public KeyValuePair<TKey, TValue> Peek()
		{
			KeyValuePair<TKey, TValue> kv;
			if(!TryPeek(out kv))
				throw new InvalidOperationException("PriorityQueue is empty");
			return kv;
		}

		public void Enqueue(KeyValuePair<TKey, TValue> kv)
		{
			heap.Add(kv);
			var last = this.Count - 1;
			BubbleUp(last);
		}

		public void Enqueue(TKey priority, TValue value)
		{
			Enqueue(KeyValuePair.Create(priority, value));
		}

		public KeyValuePair<TKey, TValue> Dequeue()
		{
			var kv = Peek();
			var last = this.Count - 1;
			heap[0] = heap[last];
			heap.RemoveAt(last);
			BubbleDown(0);
			return kv;
		}

		public bool TryDequeue(out KeyValuePair<TKey, TValue> kv)
		{
			var ok = this.Count > 0;
			kv = ok ? Dequeue() : default(KeyValuePair<TKey, TValue>);
			return ok;
		}

		public void Clear()
		{
			heap.Clear();
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
