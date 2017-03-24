using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using CCEngine.Collections;

namespace CCEngine.FileFormats
{
	using IniSection = OrderedDictionary<string, string>;
	using IniDictionary = Dictionary<string, OrderedDictionary<string, string>>;

	public class IniFile
	{
		private readonly IniDictionary sections;

		private IniFile(IniDictionary sections)
		{
			this.sections = sections;
		}

		public ICollection<string> Sections
		{
			get { return sections.Keys; }
		}

		public bool Contains(string section)
		{
			return sections.ContainsKey(section);
		}

		public bool Contains(string section, string key)
		{
			IniSection sec;
			return sections.TryGetValue(section, out sec)
				? sec.Contains(key)
				: false;
		}

		public int Count(string section)
		{
			IniSection sec;
			return sections.TryGetValue(section, out sec)
				? sec.Count
				: 0;
		}

		public string GetAt(string section, int index, string otherwise = null)
		{
			IniSection sec;
			return sections.TryGetValue(section, out sec)
				? sec.GetAt(index)
				: otherwise;
		}

		public IEnumerable<KeyValuePair<string, string>> EnumerateSection(string section)
		{
			IniSection sec;
			return sections.TryGetValue(section, out sec)
				? sec
				: null;
		}

		public ICollection<string> GetSectionKeys(string section)
		{
			IniSection sec;
			return sections.TryGetValue(section, out sec)
				? sec.Keys
				: null;
		}

		public ICollection<string> GetSectionValues(string section)
		{
			IniSection sec;
			return sections.TryGetValue(section, out sec)
				? sec.Values
				: null;
		}

		public string GetString(string section, string key, string otherwise = null)
		{
			IniSection sec;
			if (sections.TryGetValue(section, out sec))
			{
				string value;
				return sec.TryGetValue(key, out value) ? value : otherwise;
			}
			return null;
		}

		public int GetInt(string section, string key, int otherwise = 0)
		{
			int n;
			return int.TryParse(GetString(section, key), out n) ? n : otherwise;
		}

		public uint GetUint(string section, string key, uint otherwise = 0)
		{
			uint n;
			return uint.TryParse(GetString(section, key), out n) ? n : otherwise;
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

		public static IniFile Read(Stream stream)
		{
			StreamReader reader = new StreamReader(stream);
			IniDictionary sections = new IniDictionary();
			IniSection section = null;
			string key, val, line;
			int index;
			string sectionKey = "";

			while ((line = reader.ReadLine()) != null)
			{
				// strip off comments
				if ((index = line.IndexOf(';')) != -1)
					line = line.Substring(0, index);
				// strip off white spaces
				line = line.Trim();

				// skip empty lines
				if (line.Length == 0)
					continue;

				// start of section
				if (line[0] == '[')
				{
					if ((index = line.IndexOf(']')) == -1)
						continue;
					sectionKey = line.Substring(1, index - 1);
					section = new OrderedDictionary<string, string>();
					if (!sections.ContainsKey(sectionKey))
						sections.Add(sectionKey, section);
				}
				// key=value pair
				else if (section != null)
				{
					if ((index = line.IndexOf('=')) == -1)
						continue;
					key = line.Substring(0, index).Trim();
					val = line.Substring(index + 1).Trim();
					// the game always chooses the first duplicate key entry
					if(!section.Contains(key))
						section[key] = val;
				}
			}

			return new IniFile(sections);
		}
	}

	public class IniLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			using(var stream = handle.Open())
				return IniFile.Read(stream);
		}
	}
}
