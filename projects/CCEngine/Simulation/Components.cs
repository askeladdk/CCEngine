using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using CCEngine.Rendering;
using CCEngine.ECS;
using CCEngine.Collections;
using CCEngine.Algorithms;

namespace CCEngine.Simulation
{
	public class ComponentBuilderArgs
	{
		public World World { get; private set; }
		public IAttributeTable AttributeTable { get; private set; }

		public ComponentBuilderArgs(World world, IAttributeTable attributeTable)
		{
			this.World = world;
			this.AttributeTable = attributeTable;
		}
	}

	public static class ComponentBuilder
	{
		private delegate object BuilderFunc(ComponentBuilderArgs args);

		private static IReadOnlyDictionary<Type, BuilderFunc> builders =
			new Dictionary<Type, BuilderFunc>
		{
			{typeof(PoseComponent), PoseComponent.Build},
			{typeof(PlacementComponent), PlacementComponent.Build},
			{typeof(AnimationComponent), AnimationComponent.Build},
			{typeof(RepresentationComponent), RepresentationComponent.Build},
			{typeof(DriveComponent), DriveComponent.Build},
		};

		public static Type[] Types
		{
			get => builders.Keys.ToArray();
		}

		public static object Build(Type componentType, ComponentBuilderArgs args)
		{
			return builders[componentType](args);
		}
	}

	public struct PoseComponent
	{
		public Interpolated<XPos> Position { get; private set; }
		public BinaryAngle Facing { get; private set; }

		public PoseComponent(Interpolated<XPos> position, BinaryAngle facing)
		{
			this.Position = position;
			this.Facing = facing;
		}

		public static object Build(ComponentBuilderArgs args)
		{
			var pos = args.AttributeTable.Get<XPos>("Locomotion.Position");
			var position = Interpolated.Create(pos);
			var facing = args.AttributeTable.Get<BinaryAngle>("Locomotion.Facing", new BinaryAngle(0));
			return new PoseComponent(position, facing);
		}
	}

	public struct PlacementComponent
	{
		public Foundation Foundation { get; private set; }
		public StructureGrid OccupyGrid { get; private set; }
		public StructureGrid OverlapGrid { get; private set; }

		public PlacementComponent(Foundation foundation, StructureGrid occupyGrid, StructureGrid overlapGrid)
		{
			this.Foundation = foundation;
			this.OccupyGrid = occupyGrid;
			this.OverlapGrid = overlapGrid;
		}

		public static object Build(ComponentBuilderArgs args)
		{
			var attributeTable = args.AttributeTable;
			var objectStore = args.World.ObjectStore;
			var foundationId = attributeTable.Get<string>("Placement.Foundation");
			var occupyGridId = attributeTable.Get<string>("Placement.Occupy");
			var overlapGridId = attributeTable.Get<string>("Placement.Overlap");
			var foundation = objectStore.GetFoundation(foundationId);
			var occupyGrid = objectStore.GetGrid(occupyGridId);
			var overlapGrid = objectStore.GetGrid(overlapGridId);
			return new PlacementComponent(foundation, occupyGrid, overlapGrid);
		}
	}

	public struct AnimationComponent
	{
		public Sequence Sequence { get; private set; }
		public SequenceType SequenceType { get; private set; }
		public int Frame { get; private set; }

		public AnimationComponent(Sequence sequence, SequenceType sequenceType, int frame)
		{
			this.Sequence = sequence;
			this.SequenceType = sequenceType;
			this.Frame = frame;
		}

		public static object Build(ComponentBuilderArgs args)
		{
			var attributeTable = args.AttributeTable;
			var sequenceId = attributeTable.Get<string>("Animation.Sequence");
			var sequenceType = attributeTable.Get<SequenceType>("Animation.Initial", SequenceType.Idle);
			var frame = attributeTable.Get<int>("Animation.StartFrame", 0);
			var sequence = args.World.ObjectStore.GetSequence(sequenceId);
			return new AnimationComponent(sequence, sequenceType, frame);
		}
	}

	public struct RepresentationComponent
	{
		public Sprite Sprite { get; private set; }
		public Rectangle AABB { get; private set; }
		public Vector2I DrawOffset { get; private set; }

		public RepresentationComponent(Sprite sprite, Rectangle aabb, Vector2I drawOffset)
		{
			this.Sprite = sprite;
			this.AABB = aabb;
			this.DrawOffset = drawOffset;
		}

		public static object Build(ComponentBuilderArgs args)
		{
			var attributeTable = args.AttributeTable;
			var spriteId = attributeTable.Get<string>("Animation.Sprite");
			var sprite = args.World.AssetManager.Load<Sprite>(spriteId);

			var sz = sprite.Size;
			var aabb = new Rectangle(
				sz.Width / -2,
				sz.Height / -2,
				sz.Width,
				sz.Height
			);

			var drawOffset = new Vector2I(
				attributeTable.Get<int>("Animation.DrawOffsetX", 0),
				attributeTable.Get<int>("Animation.DrawOffsetY", 0)
			);

			return new RepresentationComponent(sprite, aabb, drawOffset);
		}
	}



	// public class CLocomotion : IComponent<CLocomotion>
	// {
	// 	private Locomotor locomotor;

	// 	public XPos LastPosition
	// 	{
	// 		get => this.locomotor.LastPosition;
	// 	}

	// 	public XPos Position
	// 	{
	// 		get => this.locomotor.Position;
	// 	}

	// 	public BinaryAngle Facing
	// 	{
	// 		get => this.locomotor.Facing;
	// 	}

	// 	public XPos InterpolatedPosition(float alpha)
	// 	{
	// 		return locomotor.InterpolatedPosition(alpha);
	// 	}

	// 	public void MoveTo(IFlowField field)
	// 	{
	// 		this.locomotor.MoveTo(field);
	// 	}

	// 	public void Process()
	// 	{
	// 		this.locomotor.Process();
	// 	}

	// 	public void Initialise(IAttributeTable table)
	// 	{
	// 		var pos = table.Get<XPos>("Locomotion.Position");
	// 		var fac = table.Get<BinaryAngle>("Locomotion.Facing", new BinaryAngle(0));
	// 		var spd = table.Get<int>("Locomotion.Speed", 1);
	// 		var mz = table.Get<MovementZone>("Locomotion.MovementZone", MovementZone.Track);
	// 		var loc = table.Get<string>("Locomotion.Locomotor", "Immobile");
	// 		switch(loc)
	// 		{
	// 			case "Drive":
	// 				this.locomotor = new DriveLocomotor(pos, fac, spd, mz);
	// 				break;
	// 			default:
	// 				this.locomotor = new ImmobileLocomotor(pos, fac);
	// 				break;				
	// 		}
	// 	}

	// 	public static object Create(IAttributeTable table)
	// 	{
	// 		var loco = new CLocomotion();
	// 		loco.Initialise(table);
	// 		return loco;
	// 	}
	// }

	// public class CAnimation : IComponent<CAnimation>
	// {
	// 	private Sprite sprite;
	// 	private Sequence sequence;
	// 	private string animId;
	// 	private bool reset;
	// 	private int frame;
	// 	private Rectangle aabb;
	// 	private Vector2I drawOffset;

	// 	public int Frame { get => frame; }

	// 	public bool Reset { get => reset; }

	// 	public Sprite Sprite { get => sprite; }

	// 	public Rectangle AABB { get => aabb; }
	// 	public Vector2I DrawOffset { get => drawOffset; }

	// 	public string Sequence
	// 	{
	// 		get => animId;
	// 		set
	// 		{
	// 			animId = value;
	// 			reset = true;
	// 			frame = 0;
	// 		}
	// 	}

	// 	public void Initialise(IAttributeTable table)
	// 	{
	// 		var spriteId = table.Get<string>("Animation.Sprite");
	// 		var seqId = table.Get<string>("Animation.Sequence");

	// 		var g = Game.Instance;
	// 		sprite = g.LoadAsset<Sprite>(spriteId);
	// 		sequence = g.ObjectStore.GetSequence(seqId);

	// 		animId = table.Get<string>("Animation.Initial", "Idle");
	// 		frame = table.Get<int>("Animation.StartFrame", 0);
	// 		reset = table.Get<bool>("Animation.Reset", false);
	// 		drawOffset = new Vector2I(
	// 			table.Get<int>("Animation.DrawOffsetX", 0),
	// 			table.Get<int>("Animation.DrawOffsetY", 0)
	// 		);
	// 		var sz = sprite.Size;
	// 		this.aabb = new Rectangle(
	// 			sz.Width / -2,
	// 			sz.Height / -2,
	// 			sz.Width,
	// 			sz.Height
	// 		);
	// 	}

	// 	public int NextFrame(int globalClock, BinaryAngle facing)
	// 	{
	// 		var nextFrame = sequence.GetNextFrame(globalClock, facing, animId);
	// 		reset = nextFrame < frame;
	// 		frame = nextFrame;
	// 		return frame;
	// 	}

	// 	public static object Create(IAttributeTable table)
	// 	{
	// 		var anim = new CAnimation();
	// 		anim.Initialise(table);
	// 		return anim;
	// 	}
	// }

	// class CPlacement : IComponent<CPlacement>
	// {
	// 	private Foundation foundation;
	// 	private StructureGrid occupyGrid;
	// 	private StructureGrid overlapGrid;

	// 	public Foundation Foundation
	// 	{
	// 		get { return this.foundation; }
	// 	}

	// 	public StructureGrid OccupyGrid { get { return occupyGrid; } }
	// 	public StructureGrid OverlapGrid { get { return overlapGrid; } }
		
	// 	public void Initialise(IAttributeTable table)
	// 	{
	// 		var foundationId = table.Get<string>("Placement.Foundation");
	// 		var occupyGridId = table.Get<string>("Placement.Occupy");
	// 		var overlapGridId = table.Get<string>("Placement.Overlap");
	// 		var objectStore = Game.Instance.ObjectStore;
	// 		this.foundation = objectStore.GetFoundation(foundationId);
	// 		this.occupyGrid = objectStore.GetGrid(occupyGridId);
	// 		this.overlapGrid = objectStore.GetGrid(overlapGridId);
	// 	}

	// 	public static object Create(IAttributeTable table)
	// 	{
	// 		var plac = new CPlacement();
	// 		plac.Initialise(table);
	// 		return plac;
	// 	}
	// }
}
