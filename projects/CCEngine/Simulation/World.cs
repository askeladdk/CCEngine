using System;
using System.Collections.Generic;
using System.Linq;
using CCEngine.ECS;
using CCEngine.FileFormats;
using CCEngine.Rendering;

namespace CCEngine.Simulation
{
	/// World holds the map, all game objects, and the houses.
	/// There is only one World in the game.
	public partial class World
	{
		private Registry registry;
		private PRadioReceiver pradiorec;
		private ObjectStore objectStore;
		private Map map;

		public ObjectStore ObjectStore { get => objectStore; }
		public Registry Registry { get => registry; }
		public Map Map { get => map; }

		public World(ObjectStore objectStore)
		{
			this.objectStore = objectStore;
			this.map = new Map(objectStore);
			CreateRegistry();
		}

		private void CreateRegistry()
		{
			registry = new Registry();
			PAnimation.Attach(registry);
			PRender.Attach(registry);
			pradiorec = PRadioReceiver.Attach(registry);
			PRadio.Attach(registry);
		}

		public void LoadScenario(string scname)
		{
			var g = Game.Instance;
			var rulesini = g.LoadAsset<IniFile>("rules.ini");
			var scenario = Scenario.Load(scname, objectStore);
			var rules = new ConfigurationList(scenario.Configuration, rulesini);
			objectStore.LoadRules(rules);
			map.Load(scenario);

			foreach(var terrain in scenario.Terrains)
			{
				var attrs = new AttributeTable
				{
					{"Locomotion.Position", new XPos(terrain.cell)},
				};
				var msg = new MsgSpawnEntity(terrain.terrainId, TechnoType.Terrain, attrs);
				g.SendMessage(msg);
			}

			foreach(var unit in scenario.Units)
			{
				var attrs = new AttributeTable
				{
					{"Locomotion.Position", unit.cell.XPos},
					{"Locomotion.Facing", unit.facing},
				};
				var msg = new MsgSpawnEntity(unit.technoId, TechnoType.Vehicle, attrs);
				g.SendMessage(msg);
			}
		}

		private void SpawnTechno(string id, IAttributeTable table)
		{
			var g = Game.Instance;
			int eid = 0;
			var bp = objectStore.GetTechnoType(id);

			switch(bp.Configuration.Get<TechnoType>("Type"))
			{
				case TechnoType.Vehicle:
					eid = registry.Spawn(bp, table);
					break;
				default:
					throw new Exception("Not implemented");
			}

			g.Log("Spawned {0} ({1})\n{2}", id, eid, table);
		}

		private bool TryPlace(int entityId)
		{
			CPlacement placement;
			if(!registry.TryGetComponent<CPlacement>(entityId, out placement))
				return false;

			var pose = registry.GetComponent<CLocomotion>(entityId);
			var center = pose.Position.CPos;

			var occupy = placement.OccupyGrid;
			map.Place(occupy, center, entityId);
			return true;
		}

		private void SpawnTerrain(string id, IAttributeTable table)
		{
			var g = Game.Instance;
			var bp = map.Theater.GetTerrain(id);
			var eid = registry.Spawn(bp, table);
			TryPlace(eid);
			g.Log("Spawned {0} ({1})\n{2}", id, eid, table);
		}

		public void HandleMessage(IMessage msg)
		{
			MsgSpawnEntity spawn;

			if(msg.Is<MsgSpawnEntity>(out spawn))
			{
				switch(spawn.TechnoType)
				{
					case TechnoType.Terrain:
						SpawnTerrain(spawn.ID, spawn.Table);
						break;
					default:
						SpawnTechno(spawn.ID, spawn.Table);
						break;
				}
			}

			pradiorec.OnMessage(msg);
		}

		public void Update(float dt)
		{
			Map.Update();
			registry.Update(dt);
		}

		public void Render(RenderArgs args)
		{
			Map.Render(args);
			registry.Render(args);
		}
	}
}