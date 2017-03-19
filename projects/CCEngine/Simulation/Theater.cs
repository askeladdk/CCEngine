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
		private readonly Dictionary<string, TerrainObject> terrains;

		public Palette Palette { get { return palette; } }
		public string Name { get { return name; } }

		public TmpFile GetTemplate(ushort tmpidx)
		{
			return templates.GetOrDefault(tmpidx, null);
		}

		public TerrainObject GetTerrainObject(string id)
		{
			return terrains.GetOrDefault(id);
		}

		public Theater(string name, string extension, Palette palette,
			Dictionary<ushort, TmpFile> templates,
			Dictionary<string, TerrainObject> terrains)
		{
			this.name = name;
			this.extension = extension;
			this.palette = palette;
			this.templates = templates;
			this.terrains = terrains;
		}
	}
}
