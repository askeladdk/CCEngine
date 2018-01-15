using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using CCEngine.Codecs;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	public class ScenarioLoaderFormat3 : IScenarioLoader
	{
		private static string[] ImportantSections = {
			"Basic",
			"Map",
			"MapPack",
			"OverlayPack",
			"TERRAIN",
			"UNITS",
			"SHIPS",
			"INFANTRY",
			"STRUCTURES",
			"Base",
			"Trigs",
			"TeamTypes",
			"Waypoints",
			"CellTriggers",
			"Digest",
		};

		private IConfiguration config;
		private ObjectStore objectStore;

		public ScenarioLoaderFormat3(IConfiguration config, ObjectStore objectStore)
		{
			this.config = config;
			this.objectStore = objectStore;
		}

		public IConfiguration GetConfiguration()
		{
			return config;
		}

		public IEnumerable<string> GetMiscSections()
		{
			foreach(var section in config.EnumerateSections())
				if(!ImportantSections.Contains(section))
					yield return section;
		}

		public Theater GetTheater()
		{
			var t = config.GetString("Map", "Theater");
			return objectStore.GetTheater(t);
		}

		private void GetUnits(List<ScenarioUnit> units, string section)
		{
			foreach (var kv in config.Enumerate(section))
			{
				var data = kv.Value.Split(',');
				if (data.Length < 7)
					continue;
				var country = data[0];
				var technoId = data[1];
				var health = int.Parse(data[2]);
				var cell = new CPos(ushort.Parse(data[3]));
				var facing = new BinaryAngle(int.Parse(data[4]));
				var mission = data[5];
				var triggerId = data[6];
				units.Add(new ScenarioUnit{
					country = country,
					technoId = technoId,
					health = health,
					cell = cell,
					subcell = -1,
					facing = facing,
					mission = mission,
					triggerId = triggerId,
				});
			}
		}

		// [UNITS]
		// num=country,type,health,cell,facing,mission,trigger
		// [SHIPS]
		// num=country,type,health,cell,facing,mission,trigger
		public List<ScenarioUnit> GetUnits()
		{
			var units = new List<ScenarioUnit>();
			GetUnits(units, "UNITS");
			GetUnits(units, "SHIPS");
			return units;
		}

		// [INFANTRY]
		// num=country,type,health,cell,sub_cell,action,facing,trig
		public List<ScenarioUnit> GetInfantry()
		{
			var units = new List<ScenarioUnit>();
			foreach (var kv in config.Enumerate("INFANTRY"))
			{
				var data = kv.Value.Split(',');
				if (data.Length < 8)
					continue;
				var country = data[0];
				var technoId = data[1];
				var health = int.Parse(data[2]);
				var cell = new CPos(ushort.Parse(data[3]));
				var subcell = int.Parse(data[4]);
				var mission = data[5];
				var facing = new BinaryAngle(int.Parse(data[6]));
				var triggerId = data[7];
				units.Add(new ScenarioUnit{
					country = country,
					technoId = technoId,
					health = health,
					cell = cell,
					subcell = subcell,
					facing = facing,
					mission = mission,
					triggerId = triggerId,
				});
			}
			return units;
		}

		// [STRUCTURES]
		// num=country,type,health,cell,facing,trig,sellabe,repairable
		public List<ScenarioStructure> GetStructures()
		{
			var structures = new List<ScenarioStructure>();
			foreach (var kv in config.Enumerate("STRUCTURES"))
			{
				var data = kv.Value.Split(',');
				if (data.Length < 8)
					continue;
				var country = data[0];
				var technoId = data[1];
				var health = int.Parse(data[2]);
				var cell = new CPos(ushort.Parse(data[3]));
				var facing = new BinaryAngle(int.Parse(data[4]));
				var triggerId = data[5];
				var sellable = data[6] == "1";
				var repairable = data[7] == "1";
				structures.Add(new ScenarioStructure{
					country = country,
					technoId = technoId,
					health = health,
					cell = cell,
					facing = facing,
					triggerId = triggerId,
					sellable = sellable,
					repairable = repairable,
				});
			}
			return structures;
		}

		// [TERRAINS]
		// cell=terrainId
		public List<ScenarioTerrain> GetTerrains()
		{
			var terrains = new List<ScenarioTerrain>();
			foreach(var kv in config.Enumerate("TERRAIN"))
			{
				var cell = new CPos(ushort.Parse(kv.Key));
				terrains.Add(new ScenarioTerrain{
					cell = cell,
					terrainId = kv.Value,
				});
			}
			return terrains;
		}

		public Rectangle GetBounds()
		{
			var mapx = config.GetInt("Map", "X");
			var mapy = config.GetInt("Map", "Y");
			var mapw = config.GetInt("Map", "Width");
			var maph = config.GetInt("Map", "Height");
			return new Rectangle(mapx, mapy, mapw, maph);
		}

		private void UnpackSection(string section, byte[] buffer, int ofs, int len)
		{
			if (!config.Contains(section))
				throw new Exception("[{0}] missing".F(section));
			var sb = string.Join(string.Empty, config.Enumerate(section).Select(x => x.Value));
			var bin = Convert.FromBase64String(sb);

			using (var br = new BinaryReader(new MemoryStream(bin)))
			{
				var max = ofs + len;
				while (ofs < max)
				{
					var cmpsz = br.ReadUInt16(); // compressed size
					var ucmpsz = br.ReadUInt16(); // uncompressed size
					var chunk = br.ReadBytes(cmpsz);
					Format80.Decode(chunk, 0, buffer, ofs, cmpsz);
					ofs += ucmpsz;
				}
			}
		}

		public ScenarioCell[] GetCells()
		{
			var theater = GetTheater();
			var mapdata = new byte[4 * Constants.MapCellCount];

			UnpackSection("MapPack", mapdata, 0, 3 * Constants.MapCellCount);
			UnpackSection("OverlayPack", mapdata, 3 * Constants.MapCellCount, Constants.MapCellCount);

			var rng = new Random(0);
			var cells = new ScenarioCell[Constants.MapCellCount];

			for (var i = 0; i < cells.Length; i++)
			{
				var x = i % Constants.MapSize;
				var y = i / Constants.MapSize;
				var tmpid0  = mapdata[2 * i + 0];
				var tmpid1  = mapdata[2 * i + 1];
				var tmpid = (ushort)(tmpid1 << 8 | tmpid0);
				var tmpidx  = mapdata[2 * Constants.MapCellCount + i];
				var ovrid   = mapdata[3 * Constants.MapCellCount + i];
				var tmp = theater.GetTemplate(tmpid);
				// Replace invalid tiles with the clear tile.
				//var altclear = (tmpid == 255 || tmpid == 65535) && (tmp != null);
				//if (!altclear || tmp == null || tmpidx >= tmp.FrameCount)
				if(tmpid == 255 || tmpid == 65535 || tmp == null)
				{
					tmpid = 0;
					tmp = theater.GetTemplate(0);
				}
				if (tmpid == 0)
					tmpidx = (byte)rng.Next(tmp.FrameCount);
				cells[i] = new ScenarioCell{
					templateId = tmpid,
					templateIndex = tmpidx,
					overlayId = ovrid,
				};
			}

			return cells;
		}
	}
}