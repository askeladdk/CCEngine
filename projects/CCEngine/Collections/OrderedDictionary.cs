using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;

namespace CCEngine.Collections
{
	/// <summary>
	/// Represents a generic indexed collection of key/value pairs.
	/// </summary>
	public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		int IndexOf(TKey key);
		void Insert(int index, TKey key, TValue value);
		KeyValuePair<TKey, TValue> GetAt(int index);
		void SetAt(int index, KeyValuePair<TKey, TValue> value);
	}

	/// <summary>
	/// Generic indexed collection of key/value pairs.
	/// </summary>
	public class OrderedDictionary<TKey, TValue> :
		KeyedCollection<TKey, KeyValuePair<TKey, TValue>>,
		IOrderedDictionary<TKey, TValue>,
		IReadOnlyDictionary<TKey, TValue>
	{
		protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
		{
			return item.Key;
		}

		public OrderedDictionary()
			: base()
		{ }

		#region IOrderedDictionary<TKey, TValue>

		public int IndexOf(TKey key)
		{
			if(base.Contains(key))
				return base.IndexOf(base[key]);
			return -1;
		}

		public KeyValuePair<TKey, TValue> GetAt(int index)
		{
			return base[index];
		}

		public void SetAt(int index, KeyValuePair<TKey, TValue> item)
		{
			base[index] = item;
		}

		public void Insert(int index, TKey key, TValue value)
		{
			base.Insert(index, KeyValuePair.Create(key, value));
		}

		#endregion

		#region IDictionary<TKey, TValue>

		public new TValue this[TKey key]
		{
			get => base[key].Value;
			set => Add(key, value);
		}

		public ICollection<TKey> Keys
		{
			get => Dictionary.Keys.ToList();
		}

		public ICollection<TValue> Values
		{
			get => Dictionary.Values.Select(x => x.Value).ToList();
		}

		public bool ContainsKey(TKey key)
		{
			return Dictionary.ContainsKey(key);
		}

		public void Add(TKey key, TValue value)
		{
			var kv = KeyValuePair.Create(key, value);
			var index = IndexOf(key);
			if(index != -1)
				base[index] = kv;
			else
				base.Add(kv);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			KeyValuePair<TKey, TValue> kvp;
			if(Dictionary.TryGetValue(key, out kvp))
			{
				value = kvp.Value;
				return true;
			}
			value = default(TValue);
			return false;
		}

		#endregion

		#region IReadOnlyDictionary<TKey, TValue>

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get => Dictionary.Keys;
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get => Dictionary.Values.Select(x => x.Value);
		}

		#endregion
	}
}
