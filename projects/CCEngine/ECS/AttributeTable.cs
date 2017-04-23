using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CCEngine.ECS
{
	public interface IAttributeTable
	{
		bool Contains(string name);
		T Get<T>(string name);
		T Get<T>(string name, T otherwise);
	}

	public class AttributeTable
		: IAttributeTable, IEnumerable<KeyValuePair<string, object>>
	{
		private Dictionary<string, object> attrs =
			new Dictionary<string, object>();

		public void Add(string key, object value)
		{
			attrs.Add(key, value);
		}

		public bool Contains(string name)
		{
			return attrs.ContainsKey(name);
		}

		public T Get<T>(string name)
		{
			object value;
			if (!attrs.TryGetValue(name, out value))
				throw new NotFound("Attribute [{0}] Not Found".F(name));
			return (T)value;
		}

		public T Get<T>(string name, T otherwise)
		{
			object value;
			return attrs.TryGetValue(name, out value) ? (T)value : otherwise;
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return attrs.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class AttributeTableList
		: IAttributeTable, IEnumerable<IAttributeTable>
	{
		private List<IAttributeTable> tables =
			new List<IAttributeTable>();

		public void Add(IAttributeTable table)
		{
			tables.Add(table);
		}

		public bool Contains(string name)
		{
			return tables.Any(x => x.Contains(name));
		}

		public T Get<T>(string name)
		{
			var config = tables.FirstOrDefault(x => x.Contains(name));
			if (config == null)
				throw new NotFound("Attribute [{0}] Not Found".F(name));
			return config.Get<T>(name);
		}

		public T Get<T>(string name, T otherwise)
		{
			var config = tables.FirstOrDefault(x => x.Contains(name));
			return config != null ? config.Get<T>(name, otherwise) : otherwise;
		}

		public IEnumerator<IAttributeTable> GetEnumerator()
		{
			return tables.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
