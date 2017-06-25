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

		public MPos Location
		{
			get { return this.location; }
			set { this.location = value; }
		}

		public int Facing
		{
			get { return this.facing; }
			set { this.facing = value % Constants.Facings; }
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
		}
	}

	public class CAnimation : IComponent
	{
		private SpriteArt art;
		private string artId;
		private string animId;
		private bool reset;
		private int frame;

		public void Initialise(IAttributeTable table)
		{
			artId = table.Get<string>("Animation.Art");
			animId = table.Get<string>("Animation.Sequence", "Idle");
			frame = table.Get<int>("Animation.StartFrame", 0);
			reset = table.Get<bool>("Animation.Reset", false);
			art = Game.Instance.GetArt(artId);
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

		public int NextFrame(int globalClock, int facing)
		{
			var nextFrame = art.GetNextFrame(globalClock, facing, animId);
			reset = nextFrame < frame;
			frame = nextFrame;
			return frame;
		}
	}

	class CFoundation : IComponent
	{
		private string foundationId;
		private Foundation foundation;

		public Foundation Foundation
		{
			get { return this.foundation; }
		}
		
		public void Initialise(IAttributeTable table)
		{
			this.foundationId = table.Get<string>("Foundation");
			this.foundation = Game.Instance.GetFoundation(this.foundationId);
		}
	}
}
