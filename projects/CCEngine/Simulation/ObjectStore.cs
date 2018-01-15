using System;
using System.Collections.Generic;
using System.Linq;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public class ObjectStore
	{
		private Dictionary<string, Theater> theaters;
		private Dictionary<string, Sequence> sequences;
		private Dictionary<string, Foundation> foundations;
		private Dictionary<string, StructureGrid> grids;
		private Dictionary<string, Blueprint> technoTypes;
		private Land[] lands;

		private IConfiguration artcfg;

		public ObjectStore(IConfiguration artcfg)
		{
			var g = Game.Instance;
			this.artcfg = artcfg;
			this.sequences = LoadSequences(artcfg);
			this.foundations = LoadFoundations(artcfg);
			this.grids = LoadGrids(artcfg);
			this.theaters = LoadTheaters();
		}

		public Theater GetTheater(string id)
		{
			return theaters[id];
		}

		public Land GetLand(LandType landType)
		{
			return lands[(int)landType];
		}

		public Sequence GetSequence(string id)
		{
			return sequences[id];
		}

		public StructureGrid GetGrid(string id)
		{
			return grids[id];
		}

		public Foundation GetFoundation(string id)
		{
			return foundations[id];
		}

		public Blueprint GetTechnoType(string id)
		{
			return technoTypes[id];
		}

		public void LoadRules(IConfiguration rules)
		{
			lands = LoadLands(rules);
			technoTypes = new Dictionary<string, Blueprint>();
			LoadUnitTypes(technoTypes, rules, artcfg);
		}

		private static void LoadUnitTypes(Dictionary<string, Blueprint> technoTypes,
			IConfiguration rules, IConfiguration art)
		{
			foreach(var kv in rules.Enumerate("VehicleTypes"))
			{
				var id = kv.Value;
				if(!rules.Contains(id))
					continue;

				var artId = rules.GetString(id, "Image", id);
				var sequenceId = art.GetString(artId, "Sequence", "DefaultSequence");
				var mz = rules.GetBool(id, "Tracked", false) ? MovementZone.Track : MovementZone.Wheel;

				var config = new AttributeTable
				{
					{"Type", TechnoType.Vehicle},
					{"Animation.Sprite", "{0}.SHP".F(artId)},
					{"Animation.Sequence", sequenceId},
					{"Locomotion.Speed", rules.GetInt(id, "Speed", 1)},
					{"Locomotion.MovementZone", mz},
					{"Locomotion.Locomotor", "Drive"},
				};

				technoTypes[id] = new Blueprint(config,
					typeof(CLocomotion),
					typeof(CAnimation),
					typeof(CRadio)
				);
			}
		}

		private static Land[] LoadLands(IConfiguration cfg)
		{
			var lands = new Land[(int)LandType.Count];
			foreach(var kv in Land.Lands)
				lands[(int)kv.Key] = new Land(cfg, kv.Value, kv.Key);
			return lands;
		}


		private static Dictionary<string, Sequence> LoadSequences(IConfiguration cfg)
		{
			var sequences = new Dictionary<string, Sequence>();
			foreach(var kv in cfg.Enumerate("Sequences"))
			{
				var id = kv.Value;
				var seq = new Sequence();
				foreach (var entry in cfg.Enumerate(id))
				{
					var a = cfg.GetIntArray(id, entry.Key, 5);
					seq.Add(entry.Key, a[0], a[1], a[2], a[3], a[4]);
				}
				sequences[id] = seq;
			}
			return sequences;
		}

		private static Dictionary<string, Foundation> LoadFoundations(IConfiguration cfg)
		{
			var foundations = new Dictionary<string, Foundation>();

			foreach(var kv in cfg.Enumerate("Foundations"))
			{
				var id = kv.Value;
				if (cfg.Contains(id))
					foundations[id] = new Foundation(cfg, id);;
			}
			return foundations;
		}

		private static Dictionary<string, StructureGrid> LoadGrids(IConfiguration cfg)
		{
			var grids = new Dictionary<string, StructureGrid>();
			foreach(var kv in cfg.Enumerate("Grids"))
			{
				var id = kv.Value;
				if (cfg.Contains(id))
					grids[id] = new StructureGrid(cfg, id);
			}
			return grids;
		}

		/// Loads overlays for a theater.
		private static Overlay[] LoadTheaterOverlays(IConfiguration cfg, string theaterext)
		{
			var g = Game.Instance;
			var overlays = new Overlay[256];
			foreach(var kv in cfg.Enumerate("Overlays"))
			{
				var idx = int.Parse(kv.Key);
				var overlayid = kv.Value;
				var type = cfg.GetString(overlayid, "Type");
				var ext = cfg.GetBool(overlayid, "Theater") ? theaterext : "SHP";
				var art = g.LoadAsset<Sprite>("{0}.{1}".F(overlayid, ext));
				overlays[idx] = new Overlay(type, art);
			}
			return overlays;
		}

		/// Read terrains for theater.
		private static Dictionary<string, Blueprint> LoadTheaterTerrains(
			IConfiguration cfg, string theaterext)
		{
			var g = Game.Instance;
			var terrains = new Dictionary<string, Blueprint>();

			foreach(var kv in cfg.Enumerate("Terrains"))
			{
				var id = kv.Value;
				var spriteId = "{0}.{1}".F(id, theaterext);
				var spr = g.LoadAsset<Sprite>(spriteId);

				if(spr == null)
					continue;

				var seqId = cfg.GetString(id, "Sequence", "DefaultSequence");
				var foundationId = cfg.GetString(id, "Foundation", "1x1");
				var occupyGridId = cfg.GetString(id, "Occupy", "DefaultGrid");
				var overlapGridId = cfg.GetString(id, "Overlap", "DefaultGrid");

				var config = new AttributeTable
				{
					{"Type", TechnoType.Terrain},
					{"Animation.Sprite", spriteId},
					{"Animation.Sequence", seqId},
					{"Animation.DrawOffsetX", (spr.Size.Width - Constants.TileSize) / 2},
					{"Animation.DrawOffsetY", (spr.Size.Height - Constants.TileSize) / 2},
					{"Placement.Foundation", foundationId},
					{"Placement.Occupy", occupyGridId},
					{"Placement.Overlap", overlapGridId},
					{"Locomotion.Locomotor", "Immobile"},
				};

				terrains[id] = new Blueprint(config,
					typeof(CLocomotion),
					typeof(CAnimation),
					typeof(CPlacement)
				);
			}

			return terrains;
		}

		/// Read theaters.ini into memory.
		/// This only needs to be done once.
		private static Dictionary<string, Theater> LoadTheaters()
		{
			var g = Game.Instance;
			var theatercfg = g.LoadAsset<IniFile>("theaters.ini", false);
			var overlaycfg = g.LoadAsset<IniFile>("overlays.ini", false);
			var terraincfg = g.LoadAsset<IniFile>("terrains.ini", false);
			var theaters = new Dictionary<string, Theater>();

			// List all possible templates.
			var tmplist = theatercfg
				.Enumerate("Templates")
				.Select(x => new KeyValuePair<ushort, string>(
					ushort.Parse(x.Key),
					x.Value.ToUpperInvariant()
				))
				.ToArray();

			// Needed to make the theater palettes remappable.
			var palette_cps = g.LoadAsset<BinFile>("PALETTE.CPS", false);

			foreach (var entry in theatercfg.Enumerate("Theaters"))
			{
				var id = entry.Value;
				var name = theatercfg.GetString(id, "Name");
				var extension = theatercfg.GetString(id, "Extension");
				var palname = theatercfg.GetString(id, "Palette");
				var templates = new TmpFile[1024];

				// Every theater has its own palette.
				var palette = g.LoadAsset<Palette>("{0}.PAL".F(palname), false, new PaletteParameters{
					shift=2,
					hasShadow=true,
					cycles=true,
					cpsRemap=palette_cps.Bytes,
				});

				// Load template files for this theater into memory.
				// There may be gaps because not every template is available for every theater.
				foreach (var kv2 in tmplist)
				{
					var tmp = g.LoadAsset<TmpFile>("{0}.{1}".F(kv2.Value, extension), false);
					if (tmp != null)
						templates[kv2.Key] = tmp;
				}

				var overlays = LoadTheaterOverlays(overlaycfg, extension);
				var terrains = LoadTheaterTerrains(terraincfg, extension);

				theaters[id] = new Theater(name, extension, palette, templates, overlays, terrains);
			}

			return theaters;
		}

	}
}