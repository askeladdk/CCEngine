using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CCEngine.Codecs;
using CCEngine.FileFormats;
using CCEngine.Collections;

namespace CCEngine.Simulation
{
	public struct MapTile
	{
		public readonly ushort TmpId;
		public readonly byte TmpIndex;
		public readonly byte OverlayId;

		public MapTile(ushort tmpid, byte tmpidx, byte ovrid)
		{
			this.TmpId = tmpid;
			this.TmpIndex = tmpidx;
			this.OverlayId = ovrid;
		}

		public override string ToString()
		{
			return "{0}.{1}.{2}".F(TmpId, TmpIndex, OverlayId);
		}
	}

	public class Map
	{
		private readonly MapTile[,] tiles;
		private readonly Rectangle bounds;
		private readonly Theater theater;

		public Rectangle Bounds { get { return bounds; } }
		public Theater Theater { get { return theater; } }
		
		
		public MapTile GetTile(int x, int y)
		{
			return tiles[y, x];
		}

		private Map(MapTile[,] tiles, Rectangle bounds, Theater theater)
		{
			this.tiles = tiles;
			this.bounds = bounds;
			this.theater = theater;
		}

		private static void UnpackSection(OrderedDictionary<string, string> section, byte[] buffer, int ofs, int len)
		{
			var sb = string.Join(string.Empty, section.Values);
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

		private static MapTile[,] ReadTiles(Dictionary<string, OrderedDictionary<string, string>> ini,
			Theater theater)
		{
			OrderedDictionary<string, string> mappack, ovrpack;

			if (!ini.TryGetValue("MapPack", out mappack))
				throw new Exception("[MapPack] missing");
			if (!ini.TryGetValue("OverlayPack", out ovrpack))
				throw new Exception("[OverlayPack] missing");

			byte[] mapdata = new byte[4 * Constants.MapCells];

			UnpackSection(mappack, mapdata, 0, 3 * Constants.MapCells);
			UnpackSection(ovrpack, mapdata, 3 * Constants.MapCells, Constants.MapCells);

			var terrain = new MapTile[Constants.MapSize, Constants.MapSize];

			for (int i = 0; i < Constants.MapCells; i++)
			{
				int x = i % Constants.MapSize;
				int y = i / Constants.MapSize;
				byte tmpid0  = mapdata[2 * i + 0];
				byte tmpid1  = mapdata[2 * i + 1];
				ushort tmpid = (ushort)(tmpid1 << 8 | tmpid0);
				byte tmpidx  = mapdata[2 * Constants.MapCells + i];
				byte ovridx  = mapdata[3 * Constants.MapCells + i];
				terrain[y,x] = new MapTile(tmpid, tmpidx, ovridx);
			}
			return terrain;
		}

		public static Map Read(Stream stream, AssetManager assets, MapParameters parameters)
		{
			var ini = IniFile.Read(stream);
			OrderedDictionary<string, string> section;

			// Read Map section.
			if (!ini.TryGetValue("Map", out section))
				return null;
			int mapx = section.GetInt("X");
			int mapy = section.GetInt("Y");
			int mapw = section.GetInt("Width");
			int maph = section.GetInt("Height");
			Rectangle bounds = new Rectangle(mapx, mapy, mapw, maph);

			var theater_name = section.GetString("Theater");
			var theater = parameters.world.GetTheater(theater_name);
			if (theater == null)
				throw new Exception("Invalid theater {0}".F(theater_name));

			var tiles = ReadTiles(ini, theater);

			return new Map(tiles, bounds, theater);
		}
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
