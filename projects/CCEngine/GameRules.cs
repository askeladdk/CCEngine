﻿using System;
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

		private Sequence GetSequence(string id)
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

			var seqId = artcfg.GetString(id, "Sequence", "DefaultSequence");
			var seq = GetSequence(seqId);
			spriteArts[artId] = new SpriteArt(spr, seq);

			// Create blueprint.
			var config = new AttributeTable
			{
				{"Animation.Art", artId},
			};

			var blueprint = new Blueprint(config,
				typeof(CPose),
				typeof(CAnimation)
			);
			blueprints[artId] = blueprint;
			return blueprint;
		}

		private void ReadTheaters()
		{
			var theatercfg = this.theaterscfg;
			var theaters = new Dictionary<string, Theater>();

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
				Palette palette = this.LoadAsset<Palette>("{0}.PAL".F(id), false).MakeRemappable(palette_cps);
				var templates = new Dictionary<ushort, TmpFile>();

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


#if false
		private SpriteArt ReadTechnoTypeArt(string section)
		{
			SpriteArt art;

			if (!this.spriteArt.TryGetValue(section, out art))
			{
				var cameoshp = artcfg.GetString(section, "Cameo", "XXICON");
				var sprite = LoadAsset<Sprite>("{0}.SHP".F(section));
				//var cameo = LoadAsset<Sprite>("{0}.SHP".F(cameoshp));
				var facings = artcfg.GetInt(section, "Facings", 1);

				var seqid = artcfg.GetString(section, "Sequence");
				Sequence seq;
				if (!sequences.TryGetValue(seqid, out seq))
				{
					seq = ReadSequence(seqid);
					sequences[seqid] = seq;
				}

				this.spriteArt[section] = art = new SpriteArt(sprite, seq, facings);
			}

			return art;
		}

		private TechnoType ReadTechnoType(string id)
		{
			var art = ReadTechnoTypeArt(id);
			//var strength = rulescfg.GetInt(id, "Strength", 1);
			var techno = new TechnoType(TechnoTypes.Vehicle, art);
			return techno;
		}
#endif

#if false
		private Blueprint GetVehicleType(string id)
		{
			var artId = rulescfg.GetString(id, "Image", id);
		}

		private void LoadVehicleTypes()
		{
			foreach(var entry in rulescfg.Enumerate("VehicleTypes"))
			{
				var id = entry.Value;
				var artId = rulescfg.GetString(id, "Image", id);
				this.technoTypes[id] = ReadTechnoType(id);
			}
		}
#endif

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
