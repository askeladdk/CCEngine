using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;

namespace CCEngine.Collections
{
	/// <summary>
	/// Generic implementation of IOrderedDictionary.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	public class OrderedDictionary<TKey, TValue> :
		KeyedCollection<TKey, KeyValuePair<TKey, TValue>>,
		IDictionary<TKey, TValue>,
		IOrderedDictionary
	{
		protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
		{
			return item.Key;
		}

		public OrderedDictionary()
			: base()
		{ }

		/// <summary>
		/// Set item by key.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Set(TKey key, TValue value)
		{
			Add(key, value);
		}

		/// <summary>
		/// Get item by key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue Get(TKey key)
		{
			return base[key].Value;
		}

		/// <summary>
		/// Set item by index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public void SetAt(int index, TValue value)
		{
			var kv = base[index];
			base[index] = new KeyValuePair<TKey, TValue>(kv.Key, value);
		}

		/// <summary>
		/// Get item by index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TValue GetAt(int index)
		{
			return base[index].Value;
		}

		public void Add(TKey key, TValue value)
		{
			var kv = new KeyValuePair<TKey, TValue>(key, value);
			if(base.Contains(key))
			{
				int index = base.IndexOf(base[key]);
				base[index] = kv;
			}
			else
			{
				base.Add(kv);
			}
		}

		public new TValue this[TKey key]
		{
			get
			{
				return base[key].Value;
			}

			set
			{
				Add(key, value);
			}
		}

		public new TValue this[int index]
		{
			get
			{
				return base[index].Value;
			}

			set
			{
				var kv = base[index];
				base[index] = new KeyValuePair<TKey, TValue>(kv.Key, value);
			}
		}

		#region IOrderedDictionary

		IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
		{
			return (IDictionaryEnumerator)this.GetEnumerator();
		}

		void IOrderedDictionary.Insert(int index, object key, object value)
		{
			base.Insert(index, new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value));
		}

		object IOrderedDictionary.this[int index]
		{
			get
			{
				return base[index].Value;
			}

			set
			{
				var kv = base[index];
				base[index] = new KeyValuePair<TKey, TValue>(kv.Key, (TValue)value);
			}
		}

		#endregion

		#region IDictionary

		void IDictionary.Add(object key, object value)
		{
			this.Add((TKey)key, (TValue)value);
		}

		void IDictionary.Remove(object key)
		{
			Remove((TKey)key);
		}

		bool IDictionary.Contains(object key)
		{
			return Contains((TKey)key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return (IDictionaryEnumerator)this.GetEnumerator();
		}

		bool IDictionary.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		bool IDictionary.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		ICollection IDictionary.Keys
		{
			get
			{
				return (ICollection)Dictionary.Keys;
			}
		}

		ICollection IDictionary.Values
		{
			get
			{
				return Dictionary.Values.Select(x => x.Value).ToList();
			}
		}

		public object this[object key]
		{
			get
			{
				return base[(TKey)key].Value;
			}

			set
			{
				Add((TKey)key, (TValue)value);
			}
		}

		#endregion

		#region IDictionary<TKey, TValue>

		bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
		{
			return Dictionary.ContainsKey(key);
		}

		public ICollection<TKey> Keys
		{
			get
			{
				return Dictionary.Keys;
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				return Dictionary.Values.Select(x => x.Value).ToList();
			}
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
	}
}
