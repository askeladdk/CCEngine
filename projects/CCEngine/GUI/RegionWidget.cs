using System;
using System.Drawing;
using System.Collections.Generic;

namespace CCEngine.GUI
{
	/// Basic widget that describes a screen region and responds to interactions
	/// via an event handler.
	public class RegionWidget : IWidget
	{
		public event EventHandler<InteractionEventArgs> Interaction;

		private Rectangle region;

		public bool CanInteract { get => true; }
		public Rectangle Region { get => region; }
		public IEnumerable<IWidget> Children { get => null; }

		public RegionWidget(Rectangle bounds)
		{
			this.region = bounds;
		}

		public RegionWidget(int x, int y, int w, int h)
		{
			this.region = new Rectangle(x, y, w, h);
		}

		public void OnInteraction(object sender, InteractionEventArgs e)
		{
			Interaction?.Invoke(sender, e);
		}
	}
}