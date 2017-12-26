using System;
using System.Linq;
using System.Collections.Generic;
using CCEngine.Simulation;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.ECS;

namespace CCEngine
{
	public partial class Game
	{
		private Dictionary<string, Theater> theaters = new Dictionary<string, Theater>();
		private Dictionary<string, SpriteArt> spriteArts = new Dictionary<string, SpriteArt>();
		private Dictionary<string, Sequence> sequences = new Dictionary<string, Sequence>();
		private Dictionary<string, Blueprint> blueprints = new Dictionary<string, Blueprint>();
		private Dictionary<string, Foundation> foundations = new Dictionary<string, Foundation>();
		private Dictionary<string, StructureGrid> grids = new Dictionary<string, StructureGrid>();
		private Dictionary<LandType, Land> lands = new Dictionary<LandType, Land>();
		private IConfiguration theaterscfg;
		private IConfiguration artcfg;
		private IConfiguration rulescfg;

		public Theater GetTheater(string id)
		{
			return theaters[id];
		}

		public SpriteArt GetArt(string id)
		{
			return spriteArts[id];
		}

		public Sequence GetSequence(string id)
		{
			Sequence seq;
			if (sequences.TryGetValue(id, out seq))
				return seq;

			seq = new Sequence();
			foreach (var entry in artcfg.Enumerate(id))
			{
				var a = artcfg.GetIntArray(id, entry.Key, 5);
				seq.Add(entry.Key, a[0], a[1], a[2], a[3], a[4]);
			}
			sequences[id] = seq;
			return seq;
		}

		public Foundation GetFoundation(string id)
		{
			Foundation foundation;
			if (foundations.TryGetValue(id, out foundation))
				return foundation;
			if (!artcfg.Contains(id))
				throw new Exception("Foundation {0} not found.".F(id));
			foundation = new Foundation(artcfg, id);
			foundations[id] = foundation;
			return foundation;
		}

		public StructureGrid GetGrid(string id)
		{
			StructureGrid grid;
			if (grids.TryGetValue(id, out grid))
				return grid;
			if (!artcfg.Contains(id))
				throw new Exception("Grid {0} not found.".F(id));
			grid = new StructureGrid(artcfg, id);
			grids[id] = grid;
			return grid;
		}

		public Blueprint GetTerrainType(string id, Theater theater)
		{
			// Check cache
			var artId = "{0}.{1}".F(id, theater.Extension);
			Blueprint bp;
			if (blueprints.TryGetValue(artId, out bp))
				return bp;

			// Read art.
			var spr = this.LoadAsset<Sprite>(artId, false);
			if (spr == null)
				return null;

			var seq = GetSequence(artcfg.GetString(id, "Sequence", "DefaultSequence"));
			spriteArts[artId] = new SpriteArt(spr, seq);

			var foundationId = artcfg.GetString(id, "Foundation", "1x1");
			var occupyGridId = artcfg.GetString(id, "Occupy", "DefaultGrid");
			var overlapGridId = artcfg.GetString(id, "Overlap", "DefaultGrid");

			// Create blueprint.
			var config = new AttributeTable
			{
				{"Animation.Art", artId},
				{"Placement.Foundation", foundationId},
				{"Placement.Occupy", occupyGridId},
				{"Placement.Overlap", overlapGridId},
				{"Animation.DrawOffsetX", (spr.FrameSize.Width - Constants.TileSize) / 2},
				{"Animation.DrawOffsetY", (spr.FrameSize.Height - Constants.TileSize) / 2},
			};

			bp = new Blueprint(config,
				typeof(CLocomotion),
				typeof(CAnimation),
				typeof(CPlacement)
			);
			blueprints[artId] = bp;
			this.Log("BLUEPRINT {0}\n{1}", id, bp.Configuration);
			return bp;
		}

		public Blueprint GetUnitType(string id)
		{
			Blueprint bp;
			if (blueprints.TryGetValue(id, out bp))
				return bp;

			if (!rulescfg.Contains(id))
				return null;

			// Art entry id
			var artId = rulescfg.GetString(id, "Image", id);
			if (!spriteArts.ContainsKey(artId))
			{
				// Read art.
				var spr = assets.Load<Sprite>("{0}.SHP".F(artId));
				if (spr == null)
					return null;

				var seq = GetSequence(artcfg.GetString(artId, "Sequence", "DefaultSequence"));
				spriteArts[artId] = new SpriteArt(spr, seq);
			}

			var mz = rulescfg.GetBool(id, "Tracked", false) ? MovementZone.Track : MovementZone.Wheel;

			var config = new AttributeTable
			{
				{"Animation.Art", artId},
				{"Name", rulescfg.GetString(id, "Name", id)},
				{"Health.Strength", rulescfg.GetInt(id, "Strength", 1)},
				{"Health.Armor", rulescfg.GetString(id, "Armor", "none")},
				{"Locomotion.Speed", rulescfg.GetInt(id, "Speed", 1)},
				{"Locomotion.MovementZone", mz},
				{"Locomotion.Locomotor", "Drive"},
			};

			Log("{0}:\n{1}", id, config);

			bp = new Blueprint(config,
				typeof(CLocomotion),
				typeof(CAnimation),
				typeof(CRadio)
			);

			blueprints[id] = bp;
			return bp;
		}

		public Land GetLand(LandType id)
		{
			Land land;
			if (lands.TryGetValue(id, out land))
				return land;
			land = new Land(rulescfg, Land.Lands[id], id);
			lands[id] = land;
			return land;
		}

		private void ReadTheaters()
		{
			var theatercfg = this.theaterscfg;

			var tmplist = theatercfg
				.Enumerate("Templates")
				.Select(x => new KeyValuePair<ushort, string>(
					ushort.Parse(x.Key),
					x.Value.ToUpperInvariant()
				))
				.ToArray();

			var trnlist = theatercfg.Enumerate("Terrains").Select(x => x.Value).ToArray();

			var palette_cps = this.LoadAsset<Sprite>("palette.cps", false);

			foreach (var entry in theatercfg.Enumerate("Theaters"))
			{
				var id = entry.Value;
				var name = theatercfg.GetString(id, "Name");
				var extension = theatercfg.GetString(id, "Extension");
				var palname = theatercfg.GetString(id, "Palette");
				var templates = new Dictionary<ushort, TmpFile>();

				var palette = this.LoadAsset<Palette>("{0}.PAL".F(palname), true, new PaletteParameters{
					shift=2,
					fix_special=true
				});
				palette.MakeRemappable(palette_cps);

				// Load templates
				foreach (var kv2 in tmplist)
				{
					var tmp = this.LoadAsset<TmpFile>("{0}.{1}".F(kv2.Value, extension), false);
					if (tmp != null)
						templates.Add(kv2.Key, tmp);
				}

				this.theaters[id] = new Theater(name, extension, palette, templates);
			}
		}

		public void SetRules()
		{
			//this.defaultSequence.Add("Idle", 0, 0, 10);

			this.theaterscfg = this.LoadAsset<IniFile>("theaters.ini");
			this.artcfg = this.LoadAsset<IniFile>("art.ini");
			this.rulescfg = this.LoadAsset<IniFile>("rules.ini");
		}

		public void LoadWorldData()
		{
			ReadTheaters();
		}
	}
}
