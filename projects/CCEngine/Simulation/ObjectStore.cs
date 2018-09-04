using System;
using System.Collections.Generic;
using System.Linq;
using CCEngine.Collections;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public class ObjectStore
	{
		private OrderedDictionary<string, Theater> theaters =
			new OrderedDictionary<string, Theater>();
		private OrderedDictionary<string, Sequence> sequences =
			new OrderedDictionary<string, Sequence>();
		private OrderedDictionary<string, Foundation> foundations =
			new OrderedDictionary<string, Foundation>();
		private OrderedDictionary<string, StructureGrid> grids =
			new OrderedDictionary<string, StructureGrid>();
		private OrderedDictionary<string, Blueprint> vehicleTypes =
			new OrderedDictionary<string, Blueprint>();
		private OrderedDictionary<string, House> houses =
			new OrderedDictionary<string, House>();
		private OrderedDictionary<string, Side> sides =
			new OrderedDictionary<string, Side>();
		private Land[] lands = new Land[Land.Lands.Count];

		private AssetManager assetManager;

		public AssetManager AssetManager { get => assetManager; }

		private T Load<T>(string filename, bool cache=true, object parameters=null) where T:class
		{
			return assetManager.Load<T>(filename, cache, parameters);
		}

		public ObjectStore(AssetManager assetManager)
		{
			this.assetManager = assetManager;
			LoadInitialTypes();
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

		public Blueprint GetUnitType(string id)
		{
			return vehicleTypes[id];
		}

		public IEnumerable<House> Houses
		{
			get => houses.Values;
		}

		/// Base types and type lists are loaded once on startup.
		private void LoadInitialTypes()
		{
			var rules = Load<IniFile>("rules.ini");
			var art = Load<IniFile>("art.ini");

			foreach(var kv in rules.Enumerate("Houses"))
				houses[kv.Value] = new House(kv.Value, rules);
			foreach(var kv in rules.Enumerate("Sides"))
				sides[kv.Key] = new Side(kv.Key, kv.Value.Split(","), houses);
			foreach(var kv in Land.Lands)
				lands[(int)kv.Key] = new Land(rules, kv.Value, kv.Key);
			foreach(var kv in rules.Enumerate("VehicleTypes"))
				vehicleTypes[kv.Value] = null;

			LoadSequences(art);
			LoadFoundations(art);
			LoadGrids(art);
			LoadTheaters();
		}

		/// Load types that can change from scenario to scenario.
		public void LoadRules(IConfiguration rules)
		{
			var art = Load<IniFile>("art.ini");
			LoadVehicleTypes(rules, art);
		}

		private void LoadVehicleTypes(IConfiguration rules, IConfiguration art)
		{
			foreach(var id in vehicleTypes.Keys)
			{
				if(!rules.Contains(id))
					throw new Exception("VehicleType {0} not found".F(id));

				var artId = rules.GetString(id, "Image", id);
				var sequenceId = art.GetString(artId, "Sequence", "DefaultSequence");
				var mz = rules.GetBool(id, "Tracked", false) ? MovementZone.Track : MovementZone.Wheel;

				var config = new AttributeTable
				{
					{"Basic.ID", id},
					{"Basic.Type", TechnoType.Vehicle},
					{"Animation.Sprite", "{0}.SHP".F(artId)},
					{"Animation.Sequence", sequenceId},
					{"Locomotion.Speed", rules.GetInt(id, "Speed", 1)},
					{"Locomotion.MovementZone", mz},
					{"Locomotion.Locomotor", "Drive"},
				};

				vehicleTypes[id] = new Blueprint(config,
					typeof(CLocomotion),
					typeof(CAnimation),
					typeof(CRadio)
					// typeof(Locomotor2),
					// typeof(Pose)
				);
			}
		}

		private void LoadSequences(IConfiguration cfg)
		{
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
		}

		private void LoadFoundations(IConfiguration cfg)
		{
			foreach(var kv in cfg.Enumerate("Foundations"))
			{
				var id = kv.Value;
				if (cfg.Contains(id))
					foundations[id] = new Foundation(cfg, id);;
			}
		}

		private void LoadGrids(IConfiguration cfg)
		{
			foreach(var kv in cfg.Enumerate("Grids"))
			{
				var id = kv.Value;
				if (cfg.Contains(id))
					grids[id] = new StructureGrid(cfg, id);
			}
		}

		/// Loads overlays for a theater.
		private Overlay[] LoadTheaterOverlays(IConfiguration cfg, string theaterext)
		{
			var overlays = new Overlay[256];
			foreach(var kv in cfg.Enumerate("Overlays"))
			{
				var idx = int.Parse(kv.Key);
				var overlayid = kv.Value;
				var type = cfg.GetString(overlayid, "Type");
				var ext = cfg.GetBool(overlayid, "Theater") ? theaterext : "SHP";
				var art = Load<Sprite>("{0}.{1}".F(overlayid, ext));
				overlays[idx] = new Overlay(type, art);
			}
			return overlays;
		}

		/// Read terrains for theater.
		private Dictionary<string, Blueprint> LoadTheaterTerrains(
			IConfiguration cfg, string theaterext)
		{
			var terrains = new Dictionary<string, Blueprint>();

			foreach(var kv in cfg.Enumerate("Terrains"))
			{
				var id = kv.Value;
				var spriteId = "{0}.{1}".F(id, theaterext);
				var spr = Load<Sprite>(spriteId);

				if(spr == null)
					continue;

				var seqId = cfg.GetString(id, "Sequence", "DefaultSequence");
				var foundationId = cfg.GetString(id, "Foundation", "1x1");
				var occupyGridId = cfg.GetString(id, "Occupy", "DefaultGrid");
				var overlapGridId = cfg.GetString(id, "Overlap", "DefaultGrid");

				var config = new AttributeTable
				{
					{"Basic.ID", id},
					{"Basic.Type", TechnoType.Terrain},
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
					// typeof(Locomotor2),
					// typeof(Pose),
					// typeof(Placement)
				);
			}

			return terrains;
		}

		/// Read theaters.ini into memory.
		/// This only needs to be done once.
		private void LoadTheaters()
		{
			var theatercfg = Load<IniFile>("theaters.ini", false);
			var overlaycfg = Load<IniFile>("overlays.ini", false);
			var terraincfg = Load<IniFile>("terrains.ini", false);

			// List all possible templates.
			var tmplist = theatercfg
				.Enumerate("Templates")
				.Select(x => new KeyValuePair<ushort, string>(
					ushort.Parse(x.Key),
					x.Value.ToUpperInvariant()
				))
				.ToArray();

			// Needed to make the theater palettes remappable.
			var palette_cps = Load<BinFile>("PALETTE.CPS", false);

			foreach (var entry in theatercfg.Enumerate("Theaters"))
			{
				var id = entry.Value;
				var name = theatercfg.GetString(id, "Name");
				var extension = theatercfg.GetString(id, "Extension");
				var palname = theatercfg.GetString(id, "Palette");
				var templates = new TmpFile[1024];

				// Every theater has its own palette.
				var palette = Load<Palette>("{0}.PAL".F(palname), false, new PaletteParameters{
					shift=2,
					hasShadow=true,
					cycles=true,
					cpsRemap=palette_cps.Bytes,
				});

				// Load template files for this theater into memory.
				// There may be gaps because not every template is available for every theater.
				foreach (var kv2 in tmplist)
				{
					var tmp = Load<TmpFile>("{0}.{1}".F(kv2.Value, extension), false);
					if (tmp != null)
						templates[kv2.Key] = tmp;
				}

				var overlays = LoadTheaterOverlays(overlaycfg, extension);
				var terrains = LoadTheaterTerrains(terraincfg, extension);

				theaters[id] = new Theater(name, extension, palette, templates, overlays, terrains);
			}
		}
	}
}