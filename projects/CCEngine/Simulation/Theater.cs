using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.Collections;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public class Theater
	{
		private string name;
		private string extension;
		private Palette palette;
		private TmpFile[] templates;
		private Overlay[] overlays;
		private Dictionary<string, Blueprint> terrains;

		public Palette Palette { get => palette; }
		public string Name { get => name; }
		public string Extension { get => extension; }

		public TmpFile GetTemplate(ushort tmpidx)
		{
			if(tmpidx >= templates.Length)
				return null;
			return templates[tmpidx];
		}

		public Overlay GetOverlay(byte overlay)
		{
			return overlays[overlay];
		}

		public Blueprint GetTerrain(string id)
		{
			return terrains[id];
		}

		public Theater(string name, string extension, Palette palette,
			TmpFile[] templates, Overlay[] overlays, Dictionary<string, Blueprint> terrains)
		{
			this.name = name;
			this.extension = extension;
			this.palette = palette;
			this.templates = templates;
			this.overlays = overlays;
			this.terrains = terrains;
		}

		private static Overlay[] LoadOverlays(IConfiguration config,
			AssetManager assets, string theaterExt)
		{
			var overlays = new Overlay[256];
			foreach(var kv in config.Enumerate("Overlays"))
			{
				var idx = int.Parse(kv.Key);
				var overlayid = kv.Value;
				var type = config.GetString(overlayid, "Type");
				var ext = config.GetBool(overlayid, "Theater") ? theaterExt : "SHP";
				var art = assets.Load<Sprite>("{0}.{1}".F(overlayid, ext));
				overlays[idx] = new Overlay(type, art);
			}
			return overlays;
		}

		public static Theater Load(IConfiguration cfg, AssetManager assets)
		{
			return null;
		}
	}
}
