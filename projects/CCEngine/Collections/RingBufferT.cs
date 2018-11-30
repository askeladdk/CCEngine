using System;
using System.Diagnostics;
using System.IO;

namespace CCEngine.Collections
{
	/// Generic ring buffer capable of storing objects.
	public class RingBuffer<T> : ICopyable
	{
		private T[] buffer;
		private int head;
		private int tail;

		public int Capacity { get => buffer.Length; }

		public int Count { get => head - tail; }

		/// Creates a new ring buffer.
		/// The capacity must be a power of two.
		public RingBuffer(int capacity)
		{
			Debug.Assert( (capacity >= 1) && ((capacity & (capacity - 1)) == 0) );
			this.buffer = new T[capacity];
			this.head = 0;
			this.tail = 0;
		}

		public void Clear()
		{
			for(int i = 0; i < buffer.Length; i++)
				buffer[i] = default(T);
			this.head = 0;
			this.tail = 0;
		}

		public void Enqueue(T item)
		{
			var overflow = 1 > (Capacity - Count);
			buffer[ (head++) & (Capacity - 1) ] = item;
			if(overflow)
				tail = head - Capacity;
		}

		public bool TryDequeue(out T item)
		{
			var underflow = (Count == 0);
			item = underflow ? default(T) : buffer[ (tail++) & (Capacity - 1) ];
			return !underflow;
		}

		public T Dequeue()
		{
			T item;
			if(!TryDequeue(out item))
				throw new ArgumentException();
			return item;
		}

		#region ICopyable

		public void CopyTo(object dst)
		{
			var that = (RingBuffer<T>)dst;
			Debug.Assert(this.buffer.Length == that.buffer.Length);
			Array.Copy(this.buffer, that.buffer, this.buffer.Length);
			that.head = this.head;
			that.tail = this.tail;
		}

		#endregion
	}
}
