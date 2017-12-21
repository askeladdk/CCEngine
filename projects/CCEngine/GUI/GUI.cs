using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Input;

namespace CCEngine.GUI
{
	/// Describes how the user is interacting with a GUI widget.
	public enum Interaction
	{
		/// No interaction.
		Cold,
		/// Mouse is hovering over.
		Hot,
		/// Any mouse button is pressed.
		Enter,
		/// Any mouse button is held down.
		Hold,
		/// Any mouse button is let go.
		Leave,
	}



	/// Widgets receive interactions from the GUI.
	public interface IWidget
	{
		/// Whether or not this widget can be interacted with.
		bool CanInteract { get; }
		/// This widget's interactable screen region.
		Rectangle Region { get; }
		/// Enumerates this widget's children or null.
		IEnumerable<IWidget> Children { get; }
		/// Interact with this widget.
		void OnInteraction(GUI gui, Interaction interaction);
	}



	/// The simplest GUI system that could possibly work, based on the IMGUI design.
	public class GUI
	{
		private object activeWidget;
		private Point mouse;
		private MouseButton mouseButton;
		private bool mouseDown;

		/// If IsMousePressed is true, which mouse button is pressed.
		public MouseButton MouseButton { get => mouseButton; }

		/// Whether any mouse button is pressed.
		public bool IsMousePressed { get => mouseDown; }

		/// The mouse position.
		public Point Mouse { get => mouse; }

		/// Must be called whenever the mouse moves.
		public void MouseMove(int mx, int my)
		{
			this.mouse = new Point(mx, my);
		}

		/// Must be called whenever a mouse button is pressed or let go.
		public void MousePress(MouseButton button, bool mouseDown)
		{
			this.mouseButton = button;
			this.mouseDown = mouseDown;
		}

		/// The heart of the GUI that determines an interaction on a widget.
		private Interaction GetInteraction(object widget, Rectangle region)
		{
			if(region.Contains(mouse))
			{
				if(activeWidget == null && mouseDown)
				{
					activeWidget = widget;
					return Interaction.Enter;
				}
				else if(activeWidget == widget)
				{
					if(mouseDown)
						return Interaction.Hold;
					activeWidget = null;
					return Interaction.Leave;
				}
				return Interaction.Hot;
			}

			return Interaction.Cold;
		}

		/// Interact with a widget.
		public void Interact(IWidget widget)
		{
			var children = widget.Children;
			if(children != null)
				foreach(var c in children)
					Interact(c);
			if(widget.CanInteract)
				widget.OnInteraction(this, GetInteraction(widget, widget.Region));
		}

		/// Must be called after all Interact() calls.
		public void Flip()
		{
			if(!mouseDown)
				activeWidget = null;
		}

	}
}