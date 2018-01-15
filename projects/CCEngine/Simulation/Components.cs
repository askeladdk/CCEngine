using System;
using System.Drawing;
using CCEngine.Rendering;
using CCEngine.ECS;
using CCEngine.Collections;
using CCEngine.Algorithms;

namespace CCEngine.Simulation
{
	public class CLocomotion : IComponent
	{
		private Locomotor locomotor;

		public XPos Position
		{
			get => this.locomotor.Position;
		}

		public BinaryAngle Facing
		{
			get => this.locomotor.Facing;
		}

		public XPos InterpolatedPosition(float alpha)
		{
			return locomotor.InterpolatedPosition(alpha);
		}

		public void MoveTo(IFlowField field)
		{
			this.locomotor.MoveTo(field);
		}

		public void Process()
		{
			this.locomotor.Process();
		}

		public void Initialise(IAttributeTable table)
		{
			var pos = table.Get<XPos>("Locomotion.Position");
			var fac = table.Get<BinaryAngle>("Locomotion.Facing", new BinaryAngle(0));
			var spd = table.Get<int>("Locomotion.Speed", 1);
			var mz = table.Get<MovementZone>("Locomotion.MovementZone", MovementZone.Track);
			var loc = table.Get<string>("Locomotion.Locomotor", "Immobile");
			switch(loc)
			{
				case "Drive":
					this.locomotor = new DriveLocomotor(pos, fac, spd, mz);
					break;
				default:
					this.locomotor = new ImmobileLocomotor(pos, fac);
					break;				
			}
		}
	}

	public class CAnimation : IComponent
	{
		private Sprite sprite;
		private Sequence sequence;
		private string animId;
		private bool reset;
		private int frame;
		private Rectangle aabb;
		private Vector2I drawOffset;

		public int Frame { get => frame; }

		public bool Reset { get => reset; }

		public Sprite Sprite { get => sprite; }

		public Rectangle AABB { get => aabb; }
		public Vector2I DrawOffset { get => drawOffset; }

		public string Sequence
		{
			get => animId;
			set
			{
				animId = value;
				reset = true;
				frame = 0;
			}
		}

		public void Initialise(IAttributeTable table)
		{
			var spriteId = table.Get<string>("Animation.Sprite");
			var seqId = table.Get<string>("Animation.Sequence");

			var g = Game.Instance;
			sprite = g.LoadAsset<Sprite>(spriteId);
			sequence = g.ObjectStore.GetSequence(seqId);

			animId = table.Get<string>("Animation.Initial", "Idle");
			frame = table.Get<int>("Animation.StartFrame", 0);
			reset = table.Get<bool>("Animation.Reset", false);
			drawOffset = new Vector2I(
				table.Get<int>("Animation.DrawOffsetX", 0),
				table.Get<int>("Animation.DrawOffsetY", 0)
			);
			var sz = sprite.Size;
			this.aabb = new Rectangle(
				sz.Width / -2,
				sz.Height / -2,
				sz.Width,
				sz.Height
			);
		}

		public int NextFrame(int globalClock, BinaryAngle facing)
		{
			var nextFrame = sequence.GetNextFrame(globalClock, facing, animId);
			reset = nextFrame < frame;
			frame = nextFrame;
			return frame;
		}
	}

	class CPlacement : IComponent
	{
		private Foundation foundation;
		private StructureGrid occupyGrid;
		private StructureGrid overlapGrid;

		public Foundation Foundation
		{
			get { return this.foundation; }
		}

		public StructureGrid OccupyGrid { get { return occupyGrid; } }
		public StructureGrid OverlapGrid { get { return overlapGrid; } }
		
		public void Initialise(IAttributeTable table)
		{
			var foundationId = table.Get<string>("Placement.Foundation");
			var occupyGridId = table.Get<string>("Placement.Occupy");
			var overlapGridId = table.Get<string>("Placement.Overlap");
			var objectStore = Game.Instance.ObjectStore;
			this.foundation = objectStore.GetFoundation(foundationId);
			this.occupyGrid = objectStore.GetGrid(occupyGridId);
			this.overlapGrid = objectStore.GetGrid(overlapGridId);
		}
	}
}
