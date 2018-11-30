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
	{
		private const int MinCapacity = 8;

		private T[] dense;
		private int[] reverse;
		private int[] sparse;
		private int count;

		public PackedArray() : this(MinCapacity)
		{
		}

		public PackedArray(int capacity)
		{
			Debug.Assert(Helpers.IsPowerOfTwo(capacity));
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
			if(i < 0)
				throw new ArgumentOutOfRangeException();
			else if(i >= Capacity)
				SizeUpTo(i);
			else if(ContainsKey(i))
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
			for(int i = 0; i < dense.Length; i++)
				dense[i] = default(T);
			count = 0;
		}

		private void Resize(int newcap)
		{
			var cap = Capacity;
			var newdense = new T[newcap];
			var newsparse = new int[newcap];
			var newreverse = new int[newcap];
			Array.Copy(dense, newdense, cap);
			Array.Copy(sparse, newsparse, cap);
			Array.Copy(reverse, newreverse, cap);
			dense = newdense;
			sparse = newsparse;
			reverse = newreverse;
		}

		private void SizeUpTo(int i)
		{
			var newcap = Helpers.NextPowerOfTwo(i);
			if(newcap <= Capacity)
				return;
			Resize(newcap);
		}

		public void TrimExcess()
		{
			if(count > (Capacity / 2))
				return;
			var newcap = count > MinCapacity ? Helpers.NextPowerOfTwo(count) : MinCapacity;
			Resize(newcap);
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

		#region ICopyable

		public void CopyTo(object dst)
		{
			var thiscap = this.Capacity;
			var that = (PackedArray<T>)dst;
			if(that.Capacity < thiscap)
			{
				that.dense = new T[thiscap];
				that.sparse = new int[thiscap];
				that.reverse = new int[thiscap];
			}
			Array.Copy(this.dense, that.dense, thiscap);
			Array.Copy(this.sparse, that.sparse, thiscap);
			Array.Copy(this.reverse, that.reverse, thiscap);
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
