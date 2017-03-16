using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.Collections;

namespace CCEngine.Simulation
{
	public class Theater
	{
		private readonly string name;
		private readonly string extension;
		private readonly Palette palette;
		private readonly Dictionary<ushort, TmpFile> templates;

		public int Count { get { return 65536; } }
		public Palette Palette { get { return palette; } }
		public string Name { get { return name; } }

		public TmpFile this[ushort tmpidx]
		{
			get
			{
				return templates.GetOrDefault(tmpidx, null);
			}
		}

		public Theater(string name, string extension, Palette palette, Dictionary<ushort, TmpFile> templates)
		{
			this.name = name;
			this.extension = extension;
			this.palette = palette;
			this.templates = templates;
		}
	}
}
