﻿using System;
using System.Drawing;
using System.Linq;
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
				.All(typeof(CLocomotion), typeof(CAnimation));
		}

		protected override IFilter Filter { get => filter; }

		public override bool IsActive { get => true; }

		public override bool IsRenderLoop {get => false; }

		protected override void Process(object e, int entityId)
		{
			var g = Game.Instance;
			var loco = Registry.GetComponent<CLocomotion>(entityId);
			var anim = Registry.GetComponent<CAnimation>(entityId);
			loco.Process();
			anim.NextFrame(g.GlobalClock, loco.Facing);
		}

		public static PAnimation Attach(Registry registry)
		{
			return new PAnimation(registry);
		}
	}


	class RenderArgs
	{
		public float alpha;
		public Rectangle objectBounds;
	}

	class PRender : SingleProcessor
	{
		private IFilter filter;

		private PRender(Registry registry)
			: base(registry)
		{
			this.filter = registry.Filter
				.All(typeof(CLocomotion), typeof(CAnimation));
		}

		protected override IFilter Filter { get => filter; }

		public override bool IsActive { get => true; }

		public override bool IsRenderLoop { get => true; }

		protected override void Process(object e, int entityId)
		{
			var g = Game.Instance;
			var renderer = g.Renderer;
			var camera = g.Camera;
			var args = e as RenderArgs;
			var objectBounds = args.objectBounds;

			var loco = Registry.GetComponent<CLocomotion>(entityId);
			var anim = Registry.GetComponent<CAnimation>(entityId);

			var pos = loco.InterpolatedPosition(args.alpha);
			var bb = anim.AABB.Translate(pos.X, pos.Y);

			if (objectBounds.IntersectsWith(bb))
			{
				var p = camera.MapToScreenCoord(pos.X, pos.Y);
				renderer.Blit(anim.Sprite, anim.Frame,
					p.X + anim.DrawOffset.X, p.Y + anim.DrawOffset.Y);
			}
		}

		public static PRender Attach(Registry registry)
		{
			return new PRender(registry);
		}
	}
}
