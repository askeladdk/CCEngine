using System;
using System.Drawing;
using CCEngine.Rendering;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public class CPose : IComponent
	{
		private MPos location;
		private int facing;
		private bool centered;
		private int centerOffsetX;
		private int centerOffsetY;

		public MPos Location
		{
			get { return this.location; }
			set { this.location = value; }
		}

		public MPos CenterLocation
		{
			get { return this.location.Translate(centerOffsetX, centerOffsetY, 0); }
		}

		public CPos CellLocation
		{
			get { return this.location.ToCPos(); }
		}

		public int Facing
		{
			get { return this.facing; }
			set { this.facing = CCEngine.Facing.FromInt(value); }
		}

		public bool Centered
		{
			get { return this.centered; }
		}

		public void Initialise(IAttributeTable table)
		{
			this.location = table.Get<MPos>("Pose.Location");
			this.centered = table.Get<bool>("Pose.Centered", false);
			this.facing = table.Get<int>("Pose.Facing", 0);

			this.centerOffsetX = table.Get<int>("Pose.CenterOffsetX", 0);
			this.centerOffsetY = table.Get<int>("Pose.CenterOffsetY", 0);
		}
	}

	public class CAnimation : IComponent
	{
		private SpriteArt art;
		private string artId;
		private string animId;
		private bool reset;
		private int frame;
		private Rectangle aabb;

		public void Initialise(IAttributeTable table)
		{
			artId = table.Get<string>("Animation.Art");
			animId = table.Get<string>("Animation.Sequence", "Idle");
			frame = table.Get<int>("Animation.StartFrame", 0);
			reset = table.Get<bool>("Animation.Reset", false);
			art = Game.Instance.GetArt(artId);
			var drawOffsetX = table.Get<int>("Animation.DrawOffsetX", 0);
			var drawOffsetY = table.Get<int>("Animation.DrawOffsetY", 0);
			this.aabb = new Rectangle(
				drawOffsetX,
				drawOffsetY,
				(int)art.Sprite.FramePixels.X,
				(int)art.Sprite.FramePixels.Y
			);
		}

		public string Sequence
		{
			get
			{
				return animId;
			}

			set
			{
				animId = value;
				reset = true;
				frame = 0;
			}
		}

		public int Frame
		{
			get { return frame; }
		}

		public bool Reset
		{
			get { return reset; }
		}

		public Sprite Sprite
		{
			get { return art.Sprite; }
		}

		public Rectangle AABB
		{
			get { return this.aabb; }
		}

		public int NextFrame(int globalClock, int facing)
		{
			var nextFrame = art.GetNextFrame(globalClock, facing, animId);
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
			this.foundation = Game.Instance.GetFoundation(foundationId);
			this.occupyGrid = Game.Instance.GetGrid(occupyGridId);
			this.overlapGrid = Game.Instance.GetGrid(overlapGridId);
		}
	}

	class CBoundingBox : IComponent
	{
		// measured in pixels
		private Rectangle aabb;

		public Rectangle AABB { get { return this.aabb; } }

		public void Initialise(IAttributeTable table)
		{
			var offsetX = table.Get<int>("BoundingBox.OffsetX", 0);
			var offsetY = table.Get<int>("BoundingBox.OffsetY", 0);
			var width   = table.Get<int>("BoundingBox.Width", 0);
			var height  = table.Get<int>("BoundingBox.Height", 0);

			this.aabb = new Rectangle(-offsetX, -offsetY, width, height);
		}
	}
}
