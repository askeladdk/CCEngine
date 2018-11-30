using System;
using System.Collections;
using System.Collections.Generic;

namespace CCEngine.Collections
{
	/// <summary>
	/// This is a specialised list that maintains a gap within its buffer to
	/// improve insertion/removal performance of items in the middle of the list.
	///
	/// https://www.codeproject.com/Articles/20910/Generic-Gap-Buffer
	/// </summary>
	public class GapBuffer<T>
		: IList
		, IList<T>
		, IReadOnlyList<T>
		, ICopyable
	{
		private T[] buffer;
		private int gapLo;
		private int gapHi;

		private int GapSize { get => this.gapHi - this.gapLo; }

		private int ToInternalIndex(int externalIndex)
		{
			if(externalIndex < 0 || externalIndex >= Count)
				throw new ArgumentOutOfRangeException();
			return externalIndex >= this.gapLo
				? externalIndex + this.GapSize
				: externalIndex;
		}

		private void PlaceGapStart(int index)
		{
			// Are we already there?
			if(index == this.gapLo)
				return;

			// Is there even a gap?
			if(this.GapSize == 0)
			{
				this.gapLo = index;
				this.gapHi = index;
				return;
			}

			// Move the gap near
			// (by copying the items at the beginning of the gap to the end)
			if(index < this.gapLo)
			{
				var delta = this.gapLo - index;
				var deltaIndex = Math.Min(this.GapSize, delta);
				Array.Copy(this.buffer, index, this.buffer, this.gapHi - delta, delta);
				this.gapLo -= delta;
				this.gapHi -= delta;

				// Clear the contents of the gap
				Array.Clear(this.buffer, index, deltaIndex);
			}
			// Move the gap far
			// (by copying the items at the end of the gap to the beginning)
			else
			{
				var delta = index - this.gapLo;
				var deltaIndex = Math.Max(index, this.gapHi);
				Array.Copy(this.buffer, this.gapHi, this.buffer, this.gapLo, delta);
				this.gapLo += delta;
				this.gapHi += delta;

				// Clear the contents of the gap
				Array.Clear(this.buffer, deltaIndex, this.gapHi - deltaIndex);
			}
		}

		private void Resize(int newcap)
		{
			if(newcap == this.Capacity)
				return;
			if(newcap < this.Count)
				throw new ArgumentOutOfRangeException();
			var newbuf = new T[newcap];
			var newgaphi = newbuf.Length - (this.Capacity - this.gapHi);
			Array.Copy(this.buffer, 0, newbuf, 0, this.gapLo);
			Array.Copy(this.buffer, this.gapHi, newbuf, newgaphi, this.Capacity - this.gapHi);
			this.buffer = newbuf;
			this.gapHi = newgaphi;
		}

		private void EnsureGapCapacity(int required)
		{
			if(this.GapSize >= required)
				return;
			var newcap = Math.Max(2 * (this.Count + required), 4);
			Resize(newcap);
		}

		public int Capacity
		{
			get => this.buffer.Length;
		}

		public int Count
		{
			get => this.Capacity - this.GapSize;
		}

		public GapBuffer(int capacity)
		{
			if(capacity < 0)
				throw new ArgumentOutOfRangeException();
			this.buffer = new T[capacity];
			this.gapLo = 0;
			this.gapHi = capacity;
		}

		public GapBuffer() : this(4)
		{
			// empty
		}

		public T this[int index]
		{
			get => this.buffer[ToInternalIndex(index)];
			set => this.buffer[ToInternalIndex(index)] = value;
		}

		public void Insert(int index, T value)
		{
			if(index < 0 || index > this.Count)
				throw new ArgumentOutOfRangeException();
			PlaceGapStart(index);
			EnsureGapCapacity(1);
			this.buffer[index] = value;
			this.gapLo++;
		}

		public void RemoveAt(int index)
		{
			if(index < 0 || index >= this.Count)
				throw new ArgumentOutOfRangeException();
			PlaceGapStart(index);
			this.buffer[this.gapHi] = default(T);
			this.gapHi++;
		}

		public void Clear()
		{
			Array.Clear(this.buffer, 0, this.Capacity);
			this.gapLo = 0;
			this.gapHi = this.Capacity;
		}

		public int IndexOf(T value)
		{
			var index = Array.IndexOf(this.buffer, value, 0, this.gapLo);
			if(index >= 0)
				return index;
			index = Array.IndexOf(this.buffer, value, this.gapHi,
				this.Capacity - this.gapHi);
			if(index >= 0)
				return index - this.GapSize;
			return -1;
		}

		public int BinarySearch(int index, int length, T value, IComparer<T> comparer)
		{
			if(index + length > this.Count)
				throw new ArgumentOutOfRangeException();

			var lo = index;
			var hi = index + length - 1;

			while (lo <= hi)
			{
				var med = lo + (hi - lo >> 1);
				var cmp = comparer.Compare(this.buffer[ToInternalIndex(med)], value);
				if (cmp == 0)
					return med;
				if (cmp  < 0)
					lo = med + 1;
				else
					hi = med - 1;
			}

			return ~lo;
		}

		public int BinarySearch(T value, IComparer<T> comparer)
		{
			return this.BinarySearch(0, this.Count, value, comparer);
		}

		public int BinarySearch(T value)
		{
			return this.BinarySearch(0, this.Count, value, Comparer<T>.Default);
		}

		public void Sort(int index, int length, IComparer<T> comparer)
		{
			if(index + length >= this.Count)
				throw new ArgumentOutOfRangeException();
			PlaceGapStart(index + length);
			Array.Sort(this.buffer, index, index + length, comparer);
		}

		public void Sort(IComparer<T> comparer)
		{
			PlaceGapStart(this.Count);
			Array.Sort(this.buffer, 0, this.Count, comparer);
		}

		public void Sort()
		{
			PlaceGapStart(this.Count);
			Array.Sort(this.buffer, 0, this.Count);
		}

		public void TrimExcess()
		{
			var threshold = (int)(this.Capacity * 0.9);
			if(this.Count < threshold)
				Resize(this.Count);
		}

		#region ICollection<T>

		public bool IsReadOnly { get => false; }

		public void Add(T value)
		{
			Insert(this.Count, value);
		}

		public bool Remove(T value)
		{
			var index = this.IndexOf(value);
			if(index < 0)
				return false;
			this.RemoveAt(index);
			return true;
		}

		public bool Contains(T value)
		{
			return this.IndexOf(value) >= 0;
		}

		public void CopyTo(T[] array, int offset)
		{
			if(array == null)
				throw new ArgumentNullException();
			if(offset < 0 || offset >= array.Length)
				throw new ArgumentOutOfRangeException();
			if(array.Length - offset < this.Count)
				throw new ArgumentException();
			for(var i = 0; i < this.gapLo; i++)
				array[i] = this.buffer[offset + i];
			for(var i = this.gapHi; i < this.buffer.Length; i++)
				array[offset + i - this.gapHi] = this.buffer[i];
		}

		#endregion

		#region IEnumerator, IEnumerator<T>

		public IEnumerator<T> GetEnumerator()
		{
			for(var i = 0; i < this.gapLo; i++)
				yield return this.buffer[i];
			for(var i = this.gapHi; i < this.Capacity; i++)
				yield return this.buffer[i];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		#region ICollection

		object ICollection.SyncRoot { get => throw new NotImplementedException(); }

		bool ICollection.IsSynchronized { get => false; }

		void ICollection.CopyTo(Array array, int offset)
		{
			this.CopyTo((T[])array, offset);
		}

		#endregion

		#region IList

		private bool IsCompatibleObject(object value)
		{
			return (value is T) || (!typeof(T).IsValueType && (value == null));
		}

		bool IList.IsFixedSize {get => false; }

		object IList.this[int index]
		{
			get => this[index];
			set
			{
				if(!IsCompatibleObject(value))
					throw new ArgumentException();
				this[index] = (T)value;
			}
		}

		void IList.Insert(int index, object value)
		{
			if(!IsCompatibleObject(value))
				throw new ArgumentException();
			this.Insert(index, (T)value);
		}

		int IList.Add(object value)
		{
			if(!IsCompatibleObject(value))
				throw new ArgumentException();
			this.Add((T)value);
			return this.Count - 1;
		}

		void IList.Remove(object value)
		{
			if(IsCompatibleObject(value))
				this.Remove((T)value);
		}

		int IList.IndexOf(object value)
		{
			if(IsCompatibleObject(value))
				return this.IndexOf((T)value);
			return -1;
		}

		bool IList.Contains(object value)
		{
			if(IsCompatibleObject(value))
				return this.Contains((T)value);
			return false;
		}

		#endregion

		#region ICopyable

		public void CopyTo(object dst)
		{
			if(dst == null)
				throw new ArgumentNullException();

			var that = (GapBuffer<T>)dst;

			if(that.Capacity > this.Capacity)
			{
				var thatGapHi = that.Capacity - (this.Capacity - this.gapHi);
				Array.Copy(this.buffer, 0, that.buffer, 0, this.gapLo);
				Array.Copy(this.buffer, this.gapHi, that.buffer,
					thatGapHi, this.Capacity - this.gapHi);
				that.gapLo = this.gapLo;
				that.gapHi = thatGapHi;
				Array.Clear(that.buffer, that.gapLo, that.GapSize);
			}
			else
			{
				if(that.Capacity < this.Capacity)
					that.buffer = new T[this.Capacity];
				Array.Copy(this.buffer, 0, that.buffer, 0, this.gapLo);
				Array.Copy(this.buffer, this.gapHi, that.buffer, this.gapHi,
					this.Capacity - this.gapHi);
				that.gapLo = this.gapLo;
				that.gapHi = this.gapHi;
			}
		}

		#endregion
	}
}
