using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCEngine.ECS;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.Collections;

namespace CCEngine.Simulation
{
	/// World holds the map, all game objects, and the houses.
	/// There is only one World in the game.
	public partial class World
	{
		public delegate object Builder(IAttributeTable table);

		private const int MaxEntities = 1024;

		private static IReadOnlyDictionary<Type, Builder> componentBuilders =
			new Dictionary<Type, Builder> {
				{typeof(CLocomotion), CLocomotion.Create},
				{typeof(CAnimation), CAnimation.Create},
				{typeof(CPlacement), CPlacement.Create},
				{typeof(CRadio), CRadio.Create},
			};

		private Registry registry = new Registry(MaxEntities);
		private Queue<IMission> radio = new Queue<IMission>();
		private ObjectStore objectStore;
		private Map map;

		public ObjectStore ObjectStore { get => objectStore; }
		public Registry Registry { get => registry; }
		public Map Map { get => map; }

		public World(ObjectStore objectStore)
		{
			this.objectStore = objectStore;
			this.map = new Map(objectStore);

			registry.RegisterType<CAnimation>();
			registry.RegisterType<CLocomotion>();
			registry.RegisterType<CRadio>();
			registry.RegisterType<CPlacement>();
		}

		private Entity Assemble(Blueprint bp, IAttributeTable attributes)
		{
			var entity = registry.Create();
			var table = new AttributeTableList{attributes, bp.Configuration};
			foreach(var type in bp.ComponentTypes)
				registry.Attach(type, entity, componentBuilders[type](table));
			return entity;
		}

		private void ProcessAnimations()
		{
			var g = Game.Instance;
			var clock = g.GlobalClock;
			foreach(var entity in registry.View<CAnimation, CLocomotion>())
			{
				var loco = registry.Get<CLocomotion>(entity);
				var anim = registry.Get<CAnimation>(entity);
				loco.Process();
				anim.NextFrame(clock, loco.Facing);
				//map.Unplace(loco.LastPosition.CPos);
				//map.Place(loco.Position.CPos, entity);
			}
		}

		private void ProcessRadio()
		{
			var g = Game.Instance;

			while(radio.Count != 0)
			{
				var mission = radio.Dequeue();
				var entity = mission.Entity;
				CRadio cradio;
				if(Registry.TryGet<CRadio>(entity, out cradio))
				{
					cradio.Push(mission);
				}
			}

			foreach(var entity in registry.View<CRadio>())
			{
				registry.Get<CRadio>(entity).Process();
			}
		}

		protected void RenderObjects(RenderArgs args)
		{
			var g = Game.Instance;
			var camera = g.Camera;
			var renderer = args.renderer;
			var objectBounds = args.bounds.ObjectBounds;
			var alpha = args.alpha;

			foreach(var entity in registry.View<CAnimation, CLocomotion>())
			{
				var loco = registry.Get<CLocomotion>(entity);
				var anim = registry.Get<CAnimation>(entity);
				var pos = loco.InterpolatedPosition(alpha);
				var bb = anim.AABB.Translate(pos.X, pos.Y);
				if (objectBounds.IntersectsWith(bb))
				{
					var p = camera.MapToScreenCoord(pos.X, pos.Y);
					renderer.Blit(anim.Sprite, anim.Frame,
						p.X + anim.DrawOffset.X, p.Y + anim.DrawOffset.Y);
				}
			}
		}

		public void LoadScenario(string scname)
		{
			var g = Game.Instance;
			var rulesini = g.LoadAsset<IniFile>("rules.ini");
			var scenario = Scenario.Load(scname, objectStore);
			var rules = new ConfigurationList(scenario.Configuration, rulesini);

			registry.Clear();
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
					{"Basic.Owner", unit.house},
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
			var bp = objectStore.GetUnitType(id);
			Entity eid;

			if(bp == null)
				throw new Exception("TechnoType {0} does not exist.".F(id));

			switch(bp.Configuration.Get<TechnoType>("Basic.Type"))
			{
				case TechnoType.Vehicle:
					eid = Assemble(bp, table);
					break;
				default:
					throw new Exception("Not implemented");
			}

			g.Log("Spawned {0} ({1})\n{2}", id, eid, table);
		}

		private bool TryPlace(Entity entity)
		{
			CPlacement placement;
			if(!registry.TryGet<CPlacement>(entity, out placement))
				return false;

			var pose = registry.Get<CLocomotion>(entity);
			var center = pose.Position.CPos;

			var occupy = placement.OccupyGrid;
			map.Place(center, entity, occupy);
			return true;
		}

		private void SpawnTerrain(string id, IAttributeTable table)
		{
			var g = Game.Instance;
			var bp = map.Theater.GetTerrain(id);
			var eid = Assemble(bp, table);
			TryPlace(eid);
			g.Log("Spawned {0} ({1})\n{2}", id, eid, table);
		}

		public void HandleMessage(IMessage msg)
		{
			MsgSpawnEntity spawn;
			IMission mission;

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
			else if(msg.Is<IMission>(out mission))
			{
				radio.Enqueue(mission);
			}
		}

		public void Update(float dt)
		{
			Map.Update();
			//registry.Update(dt);
			ProcessAnimations();
			ProcessRadio();
		}

		public void Render(RenderArgs args)
		{
			Map.Render(args);
			RenderObjects(args);
			//registry.Render(args);
		}

		private static void ProcessAnimations(World src, World dst)
		{
			var g = Game.Instance;
			var clock = g.GlobalClock;

			foreach(var entity in src.registry.View<CAnimation, CLocomotion>())
			{
				var loco = src.registry.Get<CLocomotion>(entity);
				var anim = src.registry.Get<CAnimation>(entity);
				loco.Process();
				anim.NextFrame(clock, loco.Facing);
			}
		}
	}
}