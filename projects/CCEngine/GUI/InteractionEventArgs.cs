using System;
using OpenTK.Input;

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

	public class KeyInteractionEventArgs : InteractionEventArgs
	{
		private KeyboardKeyEventArgs e;

		public KeyboardKeyEventArgs Args { get => e; }

		public KeyInteractionEventArgs(GUI gui, Interaction interaction, KeyboardKeyEventArgs e)
			: base(gui, interaction)
		{
			this.e = new KeyboardKeyEventArgs(e);
		}
	}
}