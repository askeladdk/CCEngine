using System;

namespace CCEngine.GUI
{
	/// Interaction event arguments.
	public class InteractionEventArgs : EventArgs
	{
		private GUI gui;
		private Interaction interaction;

		public GUI GUI { get => gui; }
		public Interaction Interaction { get => interaction; }

		public InteractionEventArgs(GUI gui, Interaction interaction)
		{
			this.gui = gui;
			this.interaction = interaction;
		}
	}
}