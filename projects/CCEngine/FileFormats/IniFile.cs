using System;
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
		public static IniDictionary Read(Stream stream)
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
					// the game always choses the first duplicate key entry
					if(!section.Contains(key))
						section[key] = val;
				}
			}

			return sections;
		}
	}

	public static class IniFileExtensions
	{
		public static string GetString(this IniSection section, string key, string def = null)
		{
			string value;
			return section.TryGetValue(key, out value) ? value : def;
		}

		public static int GetInt(this IniSection section, string key, int otherwise = 0)
		{
			int n;
			return int.TryParse(section.GetString(key), out n) ? n : otherwise;
		}

		public static uint GetUint(this IniSection section, string key, uint otherwise = 0)
		{
			uint n;
			return uint.TryParse(section.GetString(key), out n) ? n : otherwise;
		}

		public static float GetFloat(this IniSection section, string key, float otherwise = 0.0f)
		{
			float n;
			return float.TryParse(section.GetString(key), out n) ? n : otherwise;
		}

		public static bool GetBool(this IniSection section, string key, bool otherwise = false)
		{
			string value = section.GetString(key);
			return value != null ? "yYtT".IndexOf(value[0]) >= 0 : otherwise;
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
