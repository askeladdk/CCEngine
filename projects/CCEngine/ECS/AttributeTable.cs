using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CCEngine.ECS
{
	public interface IAttributeTable
	{
		bool Contains(string name);
		T Get<T>(string name);
		T Get<T>(string name, T otherwise);

		StringBuilder GetRepresentation(StringBuilder sb);
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
				throw new ArgumentException("Attribute [{0}] Not Found".F(name));
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

		public StringBuilder GetRepresentation(StringBuilder sb)
		{
			foreach (var kv in this)
				sb.AppendLine("{0}={1}".F(kv.Key, kv.Value));
			return sb;
		}

		public override string ToString()
		{
			return GetRepresentation(new StringBuilder()).ToString();
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
				throw new ArgumentException("Attribute [{0}] Not Found".F(name));
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

		public StringBuilder GetRepresentation(StringBuilder sb)
		{
			foreach(var tab in this)
				sb = tab.GetRepresentation(sb);
			return sb;
		}

		public override string ToString()
		{
			return GetRepresentation(new StringBuilder()).ToString();
		}
	}
}
