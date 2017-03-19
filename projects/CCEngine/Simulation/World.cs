using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CCEngine.FileFormats;
using CCEngine.Collections;
using CCEngine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Simulation
{
	public class TerrainObject
	{
		public readonly Sprite Sprite;
		public readonly Point Cells;

		public TerrainObject(Sprite sprite)
		{
			this.Sprite = sprite;
			this.Cells = new Point(
				(int)sprite.FramePixels.X / Constants.TileSize,
				(int)sprite.FramePixels.Y / Constants.TileSize
			);
		}
	}

	public class World
	{
		private readonly AssetManager assets;
		private readonly Dictionary<string, Theater> theaters;
		private readonly ShpFile[] overlays = new ShpFile[256];
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
			Game.Instance.Camera.SetBounds(this.map);
			return this.map;
		}

		public void Update(float dt)
		{
			Game.Instance.Camera.Pan(2, 2);
		}

		public void Render(float dt)
		{
			var batch = Game.Instance.SpriteBatch;
			batch.SetPalette(map.Theater.Palette);

			// Calculate viewable map area.
			Point screenTopLeft;
			Rectangle cellBounds;
			Game.Instance.Camera.GetRenderArea(out screenTopLeft, out cellBounds);

			// Only render objects that are within this cell rectangle.
			var objectBounds = new Rectangle(
				Math.Max(0, cellBounds.X - 1),
				Math.Max(0, cellBounds.Y - 1),
				Math.Min(cellBounds.Width + 1, Constants.MapSize),
				Math.Min(cellBounds.Height + 1, Constants.MapSize)
			);

			// Crop rendering to HUD camera area.
			Game.Instance.ScissorCamera();
			GL.Enable(EnableCap.ScissorTest);

			// Render ground layer.
			batch.SetBlending(false);
			map.RenderGround(batch, cellBounds, screenTopLeft);

			batch.SetBlending(true);
			// TODO: Render overlays.
			// TODO: Render units.
			// TODO: Render buildings.
			// Render terrain objects (trees).
			map.RenderTerrain(batch, objectBounds, Game.Instance.Camera);
			// TODO: Render aircraft.

			batch.Flush();
			batch.SetBlending(false);
			GL.Disable(EnableCap.ScissorTest);
		}

		private static Dictionary<string, Theater> ReadTheaters(IniFile ini, AssetManager assets)
		{
			var theaters = new Dictionary<string, Theater>();
			
			var tmplist = ini.GetSection("Templates").ToDictionary(
				x => ushort.Parse(x.Key),
				x => x.Value.ToUpperInvariant()
			);

			var trnlist = ini.GetSectionValues("Terrains").ToArray();

			var palette_cps = assets.Load<Sprite>("palette.cps", false);
			foreach (var id in ini.GetSectionValues("Theaters"))
			{
				var name = ini.GetString(id, "Name");
				var extension = ini.GetString(id, "Extension");
				Palette palette = assets.Load<Palette>("{0}.PAL".F(id), false).MakeRemappable(palette_cps);
				var templates = new Dictionary<ushort, TmpFile>();

				// Load templates
				foreach (var kv2 in tmplist)
				{
					var tmp = assets.Load<TmpFile>("{0}.{1}".F(kv2.Value, extension), false);
					if (tmp != null)
						templates.Add(kv2.Key, tmp);
				}

				// Load terrains
				var terrains = new Dictionary<string, TerrainObject>();
				foreach(var trnid in trnlist)
				{
					var spr = assets.Load<Sprite>("{0}.{1}".F(trnid, extension), false);
					if (spr == null)
						continue;
					terrains[trnid] = new TerrainObject(spr);
				}

				theaters[id] = new Theater(name, extension, palette, templates, terrains);
			}

			return theaters;
		}

		public static World Read(Stream stream, AssetManager assets)
		{
			var ini = IniFile.Read(stream);
			var theaters = ReadTheaters(ini, assets);


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
