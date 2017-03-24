using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using CCEngine.Codecs;
using CCEngine.FileFormats;
using CCEngine.Collections;
using CCEngine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Simulation
{

	public struct MapTile
	{
		public readonly ushort TmpId;
		public readonly byte TmpIndex;
		public readonly TileTypes TerrainType;

		public MapTile(ushort tmpid, byte tmpidx, TileTypes type)
		{
			this.TmpId = tmpid;
			this.TmpIndex = tmpidx;
			this.TerrainType = type;
		}

		public override string ToString()
		{
			return "{0}.{1}.{2}".F(TmpId, TmpIndex, TerrainType);
		}
	}

	public class Map
	{
		private struct MapTerrainObject
		{
			public Point position;
			public TerrainObject obj;

			public MapTerrainObject(ushort cell, TerrainObject obj)
			{
				this.position = PointExt.FromCellId(cell);
				this.obj = obj;
			}
		}

		private readonly MapTile[] tiles;
		private readonly Rectangle bounds;
		private readonly Theater theater;
		private MapTerrainObject[] terrains;

		private Point cellHighlight = new Point(0, 0);

		public Rectangle Bounds { get { return bounds; } }
		public Theater Theater { get { return theater; } }
		public Point CellHighLight { set { this.cellHighlight = value.ToCell(); } }
		
		public MapTile GetTile(int x, int y)
		{
			return tiles[y * Constants.MapSize + x];
		}

		private Map(MapTile[] tiles, Rectangle bounds, Theater theater, MapTerrainObject[] terrains)
		{
			this.tiles = tiles;
			this.bounds = bounds;
			this.theater = theater;
			this.terrains = terrains;
		}

		#region Rendering

		public void RenderGround(SpriteBatch batch, Rectangle cellBounds, Point screenTopLeft)
		{
			batch.SetBlending(false);
			int screenY = screenTopLeft.Y;
			for (int y = cellBounds.Top; y < cellBounds.Bottom; y++)
			{
				int screenX = screenTopLeft.X;
				for (int x = cellBounds.Left; x < cellBounds.Right; x++)
				{
					var tile = this.GetTile(x, y);
					bool highlight = (x == this.cellHighlight.X && y == this.cellHighlight.Y);
					if (highlight)
						batch.SetColor(OpenTK.Graphics.Color4.Red);
					batch
						.SetSprite(this.theater.GetTemplate(tile.TmpId))
						.Render(tile.TmpIndex, 0, screenX, screenY);
					if (highlight)
						batch.SetColor();
					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}
		}

		public void RenderTerrain(SpriteBatch batch, Rectangle objectBounds, Camera camera)
		{
			var terrains =
				from t in this.terrains
				where objectBounds.Contains(t.position.ToCell())
				select t;

			foreach(var t in terrains)
			{
				var p = camera.MapToScreenCoord(t.position);
				batch
					.SetSprite(t.obj.Sprite)
					.Render(0, 0, p.X, p.Y);
			}
		}

		#endregion

		#region Map Loading

		private static void UnpackSection(IniFile ini, string section, byte[] buffer, int ofs, int len)
		{
			if (!ini.Contains(section))
				throw new Exception("[{0}] missing".F(section));
			var sb = string.Join(string.Empty, ini.GetSectionValues(section));
			var bin = Convert.FromBase64String(sb);

			using (var br = new BinaryReader(new MemoryStream(bin)))
			{
				int max = ofs + len;
				while (ofs < max)
				{
					int cmpsz = br.ReadUInt16(); // compressed size
					int ucmpsz = br.ReadUInt16(); // uncompressed size
					var chunk = br.ReadBytes(cmpsz);
					Format80.Decode(chunk, 0, buffer, ofs, cmpsz);
					ofs += ucmpsz;
				}
			}
		}

		private static MapTile[] ReadTiles(IniFile ini, Theater theater)
		{
			byte[] mapdata = new byte[4 * Constants.MapCells];

			UnpackSection(ini, "MapPack", mapdata, 0, 3 * Constants.MapCells);
			UnpackSection(ini, "OverlayPack", mapdata, 3 * Constants.MapCells, Constants.MapCells);

			var rng = new Random(0);
			var cells = new MapTile[Constants.MapCells];

			for (int i = 0; i < cells.Length; i++)
			{
				int x = i % Constants.MapSize;
				int y = i / Constants.MapSize;
				byte tmpid0  = mapdata[2 * i + 0];
				byte tmpid1  = mapdata[2 * i + 1];
				ushort tmpid = (ushort)(tmpid1 << 8 | tmpid0);
				byte tmpidx  = mapdata[2 * Constants.MapCells + i];
				//byte ovridx  = mapdata[3 * Constants.MapCells + i];
				var tmp = theater.GetTemplate(tmpid);
				// Replace invalid tiles with the clear tile.
				if (tmpid == 0 || tmp == null || tmpidx >= tmp.FrameCount)
				{
					tmpid = 255;
					tmp = theater.GetTemplate(tmpid);
				}
				if (tmpid == 255)
					tmpidx = (byte)rng.Next(tmp.FrameCount);
				var type = tmp.GetTerrainType(tmpidx);
				cells[i] = new MapTile(tmpid, tmpidx, type);
			}

			return cells;
		}

		private static MapTerrainObject[] ReadTerrains(World w, IniFile ini, Theater theater)
		{
			var terrains = new List<MapTerrainObject>();
			foreach(var kv in ini.EnumerateSection("TERRAIN"))
			{
				var cell = ushort.Parse(kv.Key);
				var obj = theater.GetTerrainObject(kv.Value);
				if(obj != null)
					terrains.Add(new MapTerrainObject(cell, obj));
			}
			var arr = terrains.ToArray();
			Array.Sort(arr, (a, b) => a.position.CellId() - b.position.CellId());
			return arr;
		}

		public static Map Read(Stream stream, AssetManager assets, MapParameters parameters)
		{
			var w = parameters.world;
			var ini = IniFile.Read(stream);

			// Read Map section.
			int mapx = ini.GetInt("Map", "X");
			int mapy = ini.GetInt("Map", "Y");
			int mapw = ini.GetInt("Map", "Width");
			int maph = ini.GetInt("Map", "Height");
			Rectangle bounds = new Rectangle(mapx, mapy, mapw, maph);

			var theater_name = ini.GetString("Map", "Theater");
			var theater = w.GetTheater(theater_name);
			if (theater == null)
				throw new Exception("Invalid theater {0}".F(theater_name));

			var tiles = ReadTiles(ini, theater);
			var terrains = ReadTerrains(w, ini, theater);

			return new Map(tiles, bounds, theater, terrains);
		}

		#endregion
	}

	public class MapParameters
	{
		public World world;
	}

	public class MapLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			using(var stream = handle.Open())
				return Map.Read(stream, assets, (MapParameters)parameters);
		}
	}
}
