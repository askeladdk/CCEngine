using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using CCEngine.FileFormats;
using CCEngine.Collections;
using CCEngine.Rendering;

namespace CCEngine.Simulation
{
	public class World
	{
		private readonly AssetManager assets;
		private readonly Dictionary<string, Theater> theaters;
		private Map map;

		public Map Map { get { return this.map; } }

		private World(AssetManager assets, Dictionary<string, Theater> theaters)
		{
			this.assets = assets;
			this.theaters = theaters;
		}

		public Theater GetTheater(string id)
		{
			return theaters.GetOrDefault(id);
		}


		public Map LoadMap(string mapfile)
		{
			this.map = assets.Load<Map>(mapfile, false, new MapParameters {
				world = this,
			});
			return this.map;
		}

		public void Render(float dt)
		{
			var view = new WorldView(this.map, 640, 480);
			var batch = Game.Instance.SpriteBatch;
			batch.SetPalette(map.Theater.Palette);
			view.Render(batch);
		}


		public static World Read(Stream stream, AssetManager assets)
		{
			var theaters = new Dictionary<string, Theater>();
			OrderedDictionary<string, string> section;

			var ini = IniFile.Read(stream);

			var tmplist = ini["Templates"].ToDictionary(
				x => ushort.Parse(x.Key),
				x => x.Value.ToUpperInvariant()
			);

			var palette_cps = assets.Load<Sprite>("palette.cps", false);

			if (ini.TryGetValue("Theaters", out section))
			{
				foreach (var kv in section)
				{
					var id = kv.Value;
					var settings = ini[id];

					var name = settings["Name"];
					var extension = settings["Extension"];
					Palette palette = assets.Load<Palette>("{0}.PAL".F(id), false).MakeRemappable(palette_cps);
					var templates = new Dictionary<ushort, TmpFile>();

					foreach (var kv2 in tmplist)
					{
						var tmp = assets.Load<TmpFile>("{0}.{1}".F(kv2.Value, extension), false);
						if (tmp != null)
							templates.Add(kv2.Key, tmp);
					}

					theaters[kv.Value] = new Theater(name, extension, palette, templates);
				}
			}

			return new World(assets, theaters);
		}
	}

	public class WorldLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			using(var stream = handle.Open())
				return World.Read(stream, assets);
		}
	}
}
