using System;
using System.Drawing;
using CCEngine.Rendering;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	class PAnimation : SingleProcessor
	{
		private IFilter filter;

		private PAnimation(Registry registry)
			: base(registry)
		{
			this.filter = registry.Filter
				.All(typeof(CPose), typeof(CAnimation));
		}

		protected override IFilter Filter
		{
			get { return filter; }
		}

		public override bool IsActive
		{
			get { return true; }
		}

		public override bool IsRenderLoop
		{
			get { return false; }
		}

		protected override int Priority
		{
			get { return 0; }
		}

		protected override void Process(float dt, int entityId)
		{
			var game = Game.Instance;
			var pose = Registry.GetComponent<CPose>(entityId);
			pose.Facing = Facing.Between(pose.CenterLocation, game.mousePos);
			var animation = Registry.GetComponent<CAnimation>(entityId);
			animation.NextFrame(game.GlobalClock, pose.Facing);
		}

		public static PAnimation Attach(Registry registry)
		{
			return new PAnimation(registry);
		}
	}



	class PRender : SingleProcessor
	{
		private IFilter filter;

		private PRender(Registry registry)
			: base(registry)
		{
			this.filter = registry.Filter
				.All(typeof(CPose), typeof(CAnimation));
		}

		protected override IFilter Filter
		{
			get { return filter; }
		}

		public override bool IsActive
		{
			get { return true; }
		}

		public override bool IsRenderLoop
		{
			get { return true; }
		}

		protected override int Priority
		{
			get { return 1; }
		}

		protected override void Process(float dt, int entityId)
		{
			var game = Game.Instance;
			var batch = game.SpriteBatch;
			var camera = game.Camera;
			var objectBounds = game.Map.ObjectBounds;

			var pose = Registry.GetComponent<CPose>(entityId);
			var animation = Registry.GetComponent<CAnimation>(entityId);

			var location = pose.Location;
			var bb = animation.AABB.Translate(location);

			if (objectBounds.IntersectsWith(bb))
			{
				var p = camera.MapToScreenCoord(location);
				p.Offset(animation.AABB.Location);
				batch
					.SetSprite(animation.Sprite)
					.Render(animation.Frame, 0, p.X, p.Y);
			}
		}

		public static PRender Attach(Registry registry)
		{
			return new PRender(registry);
		}
	}
}
