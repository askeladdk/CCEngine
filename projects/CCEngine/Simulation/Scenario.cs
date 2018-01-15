using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	public class Scenario
	{
		private IConfiguration config;
		private ScenarioCell[] cells;
		private Rectangle bounds;
		private Theater theater;
		private ScenarioUnit[] units;
		private ScenarioUnit[] infantry;
		private ScenarioStructure[] structures;
		private ScenarioTerrain[] terrains;

		public IConfiguration Configuration { get => config; }
		public ScenarioCell[] Cells { get => cells; }
		public Rectangle Bounds { get => bounds; }
		public Theater Theater { get => theater; }
		public ScenarioUnit[] Units { get => units; }
		public ScenarioUnit[] Infantry { get => infantry; }
		public ScenarioStructure[] Structures { get => structures; }
		public ScenarioTerrain[] Terrains { get => terrains; }

		private Scenario(IScenarioLoader loader)
		{
			config = loader.GetConfiguration();
			cells = loader.GetCells();
			bounds = loader.GetBounds();
			theater = loader.GetTheater();
			units = loader.GetUnits().ToArray();
			infantry = loader.GetInfantry().ToArray();
			structures = loader.GetStructures().ToArray();
			terrains = loader.GetTerrains().ToArray();
		}

		public static Scenario Load(string scenario, ObjectStore objectStore)
		{
			var g = Game.Instance;
			var ini = g.LoadAsset<IniFile>("{0}.INI".F(scenario), false);
			if(ini == null)
				throw new Exception("Scenario {0} not found.".F(scenario));
			var format = ini.GetString("Basic", "NewINIFormat", "0");
			IScenarioLoader loader;
			switch(format)
			{
				case "3":
					loader = new ScenarioLoaderFormat3(ini, objectStore);
					break;
				default:
					throw new Exception("Scenario {0} has invalid INI format: {1}.".F(scenario, format));
			}

			return new Scenario(loader);
		}
	}
}
