using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace CCEngine
{
	public interface IConfiguration
	{
		bool Contains(string section);
		bool Contains(string section, string key);
		string GetString(string section, string key, string otherwise = null);
		int GetInt(string section, string key, int otherwise = 0);
		float GetFloat(string section, string key, float otherwise = 0.0f);
		bool GetBool(string section, string key, bool otherwise = false);
		string[] GetStringArray(string section, string key);
		int[] GetIntArray(string section, string key, int nentries);
		IEnumerable<KeyValuePair<string, string>> Enumerate(string section);
	}

	public abstract class AbstractConfiguration : IConfiguration
	{
		public abstract bool Contains(string section);
		public abstract bool Contains(string section, string key);
		public abstract string GetString(string section, string key, string otherwise = null);
		public abstract IEnumerable<KeyValuePair<string, string>> Enumerate(string section);

		public int GetInt(string section, string key, int otherwise = 0)
		{
			int n;
			return int.TryParse(GetString(section, key), out n) ? n : otherwise;
		}

		public float GetFloat(string section, string key, float otherwise = 0.0f)
		{
			float n;
			return float.TryParse(GetString(section, key), out n) ? n : otherwise;
		}

		public bool GetBool(string section, string key, bool otherwise = false)
		{
			string value = GetString(section, key);
			return value != null ? "1yYtT".IndexOf(value[0]) >= 0 : otherwise;
		}

		public string[] GetStringArray(string section, string key)
		{
			var s = GetString(section, key);
			if (s == null)
				return null;
			return s.Split(',');
		}

		public int[] GetIntArray(string section, string key, int nentries)
		{
			var ss = GetStringArray(section, key);
			var ar = ss.Select(x =>
			{
				int n;
				return int.TryParse(x, out n) ? n : 0;
			}).ToArray();
			if (ar.Length < nentries)
				throw new Exception("[{0}>{1}] Array too short");
			return ar;
		}
	}

	public class ConfigurationList : AbstractConfiguration
	{
		private IConfiguration[] configs;

		public ConfigurationList(params IConfiguration[] configs)
		{
			this.configs = configs;
		}

		public override bool Contains(string key)
		{
			foreach (var table in configs)
				if (table.Contains(key))
					return true;
			return false;
		}

		public override bool Contains(string section, string key)
		{
			foreach (var table in configs)
				if (table.Contains(section, key))
					return true;
			return false;
		}

		public override string GetString(string section, string key, string otherwise = null)
		{
			foreach (var table in configs)
				if (table.Contains(section, key))
					return table.GetString(section, key, otherwise);
			return otherwise;
		}

		public override IEnumerable<KeyValuePair<string, string>> Enumerate(string section)
		{
			return configs
				.Select(x => x.Enumerate(section))
				.Aggregate((a, b) => a.Union(b));
		}
	}
}
