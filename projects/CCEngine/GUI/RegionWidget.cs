using System;
using System.Drawing;
using System.Collections.Generic;

namespace CCEngine.GUI
{
	/// Interaction event arguments.
	public struct InteractionEventArgs
	{
		private IWidget widget;
		private Interaction interaction;

		public IWidget Widget { get => widget; }
		public Interaction Interaction { get => interaction; }

		public InteractionEventArgs(IWidget widget, Interaction interaction)
		{
			this.widget = widget;
			this.interaction = interaction;
		}
	}

	/// Basic widget that describes a screen region and responds to interactions
	/// via an event handler.
	public class RegionWidget : IWidget
	{
		public event Action<object, InteractionEventArgs> OnInteraction;

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

		public void Interact(GUI gui, Interaction interaction)
		{
			if(OnInteraction != null)
				OnInteraction.Invoke(gui, new InteractionEventArgs(this, interaction));
		}
	}
}