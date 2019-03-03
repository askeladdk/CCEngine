using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using CCEngine.Collections;

namespace CCEngine.FileFormats
{
	using IniDict = OrderedDictionary<string, string>;
	using SectionDict = Dictionary<string, OrderedDictionary<string, string>>;

	public class IniFile : AbstractConfiguration
	{
		private readonly SectionDict sections;

		private IniFile(SectionDict sections)
		{
			this.sections = sections;
		}

		public override bool Contains(string section)
		{
			return sections.ContainsKey(section);
		}

		public override bool Contains(string section, string key)
		{
			IniDict sec;
			return sections.TryGetValue(section, out sec)
				? sec.Contains(key)
				: false;
		}

		public override IEnumerable<KeyValuePair<string, string>> Enumerate(string section)
		{
			IniDict sec;
			return sections.TryGetValue(section, out sec)
				? sec
				: Enumerable.Empty<KeyValuePair<string, string>>();
		}

		public override string GetString(string section, string key, string otherwise = null)
		{
			IniDict sec;
			if (sections.TryGetValue(section, out sec))
			{
				string value;
				return sec.TryGetValue(key, out value) ? value : otherwise;
			}
			return null;
		}

		public override IEnumerable<string> EnumerateSections()
		{
			return this.sections.Keys;
		}

		public static IniFile Read(Stream stream)
		{
			StreamReader reader = new StreamReader(stream);
			SectionDict sections = new SectionDict();
			IniDict section = null;
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
