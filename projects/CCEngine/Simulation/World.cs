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
		private EntityComponentRegistry ecregistry =
			new EntityComponentRegistry(1024, ComponentBuilder.Types);
		private Queue<IMission> radio = new Queue<IMission>();
		private ObjectStore objectStore;
		private Map map;

		private DriveSystem driveSystem;

		public int Clock { get; private set; }
		public ObjectStore ObjectStore { get => objectStore; }
		public AssetManager AssetManager { get => objectStore.AssetManager; }
		public EntityComponentRegistry Registry { get => ecregistry; }
		public Map Map { get => map; }

		public World(ObjectStore objectStore)
		{
			this.objectStore = objectStore;
			this.map = new Map(objectStore);
			this.driveSystem = new DriveSystem(this);
		}

		private Entity Assemble(Blueprint bp, IAttributeTable attributes)
		{
			var entity = ecregistry.Create();
			var table = new AttributeTableList{attributes, bp.Configuration};
			var args = new ComponentBuilderArgs(this, table);
			foreach(var type in bp.ComponentTypes)
			{
				var component = ComponentBuilder.Build(type, args);
				ecregistry.Set(type, entity, component);
			}
			return entity;
		}

		private void ProcessAnimations()
		{
			var g = Game.Instance;
			var clock = g.GlobalClock;
			foreach(var entity in ecregistry.View<PoseComponent, AnimationComponent>())
			{
				var pose = ecregistry.Get<PoseComponent>(entity);
				var anim = ecregistry.Get<AnimationComponent>(entity);
				var nextFrame = anim.Sequence.GetFrame(anim.SequenceType, clock, pose.Facing);
				// var reset = nextFrame < anim.Frame;
				anim = new AnimationComponent(anim.Sequence, anim.SequenceType, nextFrame);
				ecregistry.Set(entity, anim);
			}
		}

		private void ProcessRadio()
		{
			// var g = Game.Instance;

			// while(radio.Count != 0)
			// {
			// 	var mission = radio.Dequeue();
			// 	var entity = mission.Entity;
			// 	CRadio cradio;
			// 	if(Registry.TryGet<CRadio>(entity, out cradio))
			// 	{
			// 		cradio.Push(mission);
			// 	}
			// }

			// foreach(var entity in ecregistry.View<CRadio>())
			// {
			// 	ecregistry.Get<CRadio>(entity).Process();
			// }

			IMission mission;
			while(this.radio.TryDequeue(out mission))
			{
				MissionMove move;
				if(mission.Is<MissionMove>(out move))
				{
					if(this.Registry.Has<DriveComponent, PoseComponent>(move.Entity))
					{
						DriveComponent drive = this.Registry.Get<DriveComponent>(move.Entity);
						PoseComponent pose = this.Registry.Get<PoseComponent>(move.Entity);
						// TODO: FlowField should be not directly attached to component.
						var flowField = new Algorithms.SingularFlowField<SpeedType>(
							this.map,
							CPos.FromXPos(pose.Position.V0),
							move.Destination,
							drive.SpeedType);
						drive = new DriveComponent(
							drive.SpeedType,
							drive.Speed,
							DriveState.PreMove,
							flowField,
							new Vector2I(),
							new XPos()
						);
						this.Registry.Set(move.Entity, drive);
					}
				}
			}
		}

		protected void RenderObjects(RenderArgs args)
		{
			var g = Game.Instance;
			var camera = g.Camera;
			var renderer = args.renderer;
			var objectBounds = args.bounds.ObjectBounds;
			var alpha = args.alpha;

			// var t = this.Clock;

			foreach(var entity in ecregistry.View<PoseComponent, AnimationComponent, RepresentationComponent>())
			{
				var pose = ecregistry.Get<PoseComponent>(entity);
				var anim = ecregistry.Get<AnimationComponent>(entity);
				var repr = ecregistry.Get<RepresentationComponent>(entity);
				var pos = pose.Position;
				// var pos0 = pos.Lerp(pos.Alpha(t));
				// var pos1 = pos.Lerp(pos.Alpha(t+1));
				// var finalPos = XPos.Lerp(alpha, pos0, pos1);
				var finalPos = pos.Lerp(args.alpha);
				var pixelx = Lepton.ToPixel(finalPos.LeptonsX);
				var pixely = Lepton.ToPixel(finalPos.LeptonsY);
				var bb = repr.AABB.Translate(pixelx, pixely);
				if (objectBounds.IntersectsWith(bb))
				{
					var p = camera.MapToScreenCoord(pixelx, pixely);
					renderer.Blit(repr.Sprite, anim.Frame,
						p.X + repr.DrawOffset.X, p.Y + repr.DrawOffset.Y);
				}
			}
		}

		public void LoadScenario(string scname)
		{
			var g = Game.Instance;
			var rulesini = g.LoadAsset<IniFile>("rules.ini");
			var scenario = Scenario.Load(scname, objectStore);
			var rules = new ConfigurationList(scenario.Configuration, rulesini);

			ecregistry = new EntityComponentRegistry(1024, ComponentBuilder.Types);
			objectStore.LoadRules(rules);
			map.Load(scenario);

			foreach(var terrain in scenario.Terrains)
			{
				var attrs = new AttributeTable
				{
					{"Locomotion.Position", XPos.FromCell(terrain.cell)},
				};
				var msg = new MsgSpawnEntity(terrain.terrainId, TechnoType.Terrain, attrs);
				g.SendMessage(msg);
			}

			foreach(var unit in scenario.Units)
			{
				var attrs = new AttributeTable
				{
					{"Basic.Owner", unit.house},
					{"Locomotion.Position", XPos.FromCell(unit.cell)},
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
			PlacementComponent placement;
			if(!ecregistry.TryGet<PlacementComponent>(entity, out placement))
				return false;

			var pose = ecregistry.Get<PoseComponent>(entity);
			var center = CPos.FromXPos(pose.Position.V0);

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
			ProcessAnimations();
			ProcessRadio();
			this.driveSystem.Process();
			this.Clock += 1;
		}

		public void Render(RenderArgs args)
		{
			Map.Render(args);
			RenderObjects(args);
		}
	}
}