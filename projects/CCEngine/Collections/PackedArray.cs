using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CCEngine.Collections
{
	/// Unordered array in which all items are stored in contiguous memory
	/// no matter the order of their insertion or deletion.
	public class PackedArray<T>
		: IDictionary
		, IDictionary<int, T>
		, IReadOnlyDictionary<int, T>
		, IReadOnlyList<T>
		, ICopyable
		, ICloneable
	{
		private T[] dense;
		private int[] reverse;
		private int[] sparse;
		private int count;

		public PackedArray(int capacity)
		{
			dense = new T[capacity];
			reverse = new int[capacity];
			sparse = new int[capacity];
		}

		public int Count
		{
			get => count;
		}

		public int Capacity
		{
			get => sparse.Length;
		}

		public T this[int i]
		{
			get
			{
				int j;
				if(GetDenseIndex(i, out j))
					return dense[j];
				throw new KeyNotFoundException();
			}

			set
			{
				int j;
				if(GetDenseIndex(i, out j))
					dense[j] = value;
				else
					Add(i, value);
			}
		}

		public ICollection<int> Keys
		{
			get => GetKeys().ToList();
		}

		public ICollection<T> Values
		{
			get => GetValues().ToList();
		}

		public bool ContainsKey(int i)
		{
			return GetDenseIndex(i, out i);
		}

		public bool TryGetValue(int i, out T item)
		{
			int j;
			var contains = GetDenseIndex(i, out j);
			item = contains ? dense[j] : default(T);
			return contains;
		}

		public void Add(int i, T item)
		{
			if(i < 0 || i >= Capacity)
				throw new ArgumentOutOfRangeException();
			if(ContainsKey(i))
				throw new ArgumentException();
			var n      = count++;
			dense[n]   = item;
			reverse[n] = i;
			sparse[i]  = n;
		}

		public bool Remove(int i)
		{
			if(!ContainsKey(i))
				return false;
			var n      = --count;
			var j      = reverse[n];
			var k      = sparse[i];
			dense[k]   = dense[n];
			reverse[k] = j;
			sparse[j]  = k;
			return true;
		}

		public void Clear()
		{
			count = 0;
		}

		public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
		{
			for(var i = 0; i < count; i++)
				yield return KeyValuePair.Create(reverse[i], dense[i]);
		}

		private IEnumerable<int> GetKeys()
		{
			for(var i = 0; i < count; i++)
				yield return reverse[i];
		}

		private IEnumerable<T> GetValues()
		{
			for(var i = 0; i < count; i++)
				yield return dense[i];
		}

		private bool GetDenseIndex(int i, out int j)
		{
			if(i >= 0 && i < Capacity)
			{
				j = sparse[i];
				return j < count && reverse[j] == i;
			}
			j = -1;
			return false;
		}

		#region ICloneable

		public object Clone()
		{
			var that = new PackedArray<T>(Capacity);
			this.CopyTo(that);
			return that;
		}

		#endregion

		#region ICopyable

		public void CopyTo(object dst)
		{
			var that = (PackedArray<T>)dst;
			Debug.Assert(this.Capacity == that.Capacity);
			Array.Copy(this.dense, that.dense, this.Capacity);
			Array.Copy(this.sparse, that.sparse, this.Capacity);
			Array.Copy(this.reverse, that.reverse, this.Capacity);
			that.count = this.count;
		}

		#endregion

		#region IDictionary

		bool IDictionary.Contains(object key)
		{
			return ContainsKey((int)key);
		}

		void IDictionary.Remove(object key)
		{
			Remove((int)key);
		}

		void IDictionary.Add(object key, object value)
		{
			Add((int)key, (T)value);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return (IDictionaryEnumerator)GetEnumerator();
		}

		object IDictionary.this[object key]
		{
			get => (object)this[(int)key];
			set => this[(int)key] = (T)value;
		}

		ICollection IDictionary.Keys
		{
			get => (ICollection)Keys;
		}

		ICollection IDictionary.Values
		{
			get => (ICollection)Values;
		}

		bool IDictionary.IsFixedSize
		{
			get => true;
		}

		bool IDictionary.IsReadOnly
		{
			get => false;
		}

		bool ICollection.IsSynchronized
		{
			get => false;
		}

		object ICollection.SyncRoot
		{
			get => throw new NotSupportedException();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection<KeyValuePair<int, T>>)this).CopyTo( (KeyValuePair<int, T>[])array, index);
		}

		#endregion

		#region IEnumerable<T>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			var remaining = count;
			for(var i = 0; i < sparse.Length && remaining >= 0; i++)
			{
				int j;
				if(GetDenseIndex(i, out j))
				{
					remaining--;
					yield return dense[j];
				}
			}
		}

		#endregion

		#region IReadOnlyDictionary<int, T>

		IEnumerable<int> IReadOnlyDictionary<int, T>.Keys
		{
			get => GetKeys();
		}

		IEnumerable<T> IReadOnlyDictionary<int, T>.Values
		{
			get => GetValues();
		}

		#endregion

		#region ICollection<KeyValuePair<int, T>>

		bool ICollection<KeyValuePair<int, T>>.IsReadOnly
		{
			get { return false; }
		}

		void ICollection<KeyValuePair<int, T>>.Add(KeyValuePair<int, T> pair)
		{
			Add(pair.Key, pair.Value);
		}

		bool ICollection<KeyValuePair<int, T>>.Remove(KeyValuePair<int, T> pair)
		{
			int j;
			if(GetDenseIndex(pair.Key, out j))
				if(dense[j].Equals(pair.Value))
					return Remove(pair.Key);
			return false;
		}

		bool ICollection<KeyValuePair<int, T>>.Contains(KeyValuePair<int, T> pair)
		{
			int j;
			if(GetDenseIndex(pair.Key, out j))
				return dense[j].Equals(pair.Value);
			return false;
		}

		void ICollection<KeyValuePair<int, T>>.CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
		{
			for(var i = 0; i < count; i++)
				array[arrayIndex + i] = KeyValuePair.Create(reverse[i], dense[i]);
		}

		#endregion
	}
}
