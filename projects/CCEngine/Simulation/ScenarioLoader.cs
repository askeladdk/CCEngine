using System;
using System.Collections.Generic;
using System.Drawing;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	public struct ScenarioCell
	{
		public const byte NoOverlay = 255;

		public ushort templateId;
		public byte templateIndex;
		public byte overlayId;
	}

	public class ScenarioStructure
	{
		public string country;
		public string technoId;
		public int health;
		public CPos cell;
		public BinaryAngle facing;
		public string triggerId;
		public bool sellable;
		public bool repairable;
	}

	public class ScenarioUnit
	{
		public string country;
		public string technoId;
		public int health;
		public CPos cell;
		public int subcell;
		public BinaryAngle facing;
		public string mission;
		public string triggerId;
	}

	public class ScenarioTerrain
	{
		public CPos cell;
		public string terrainId;
	}

	public interface IScenarioLoader
	{
		IConfiguration GetConfiguration();
		IEnumerable<string> GetMiscSections();
		Theater GetTheater();
		Rectangle GetBounds();
		ScenarioCell[] GetCells();
		List<ScenarioUnit> GetUnits();
		List<ScenarioUnit> GetInfantry();
		List<ScenarioStructure> GetStructures();
		List<ScenarioTerrain> GetTerrains();
	}
}