using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.Simulation;
using CCEngine.Algorithms;
using CCEngine.GUI;

namespace CCEngine.Logic
{
	class MainMenu : IGameState, IWidget
	{
		public bool CanInteract { get => true; }
		public System.Drawing.Rectangle Region { get => System.Drawing.Rectangle.Empty; }
		public IEnumerable<IWidget> Children
		{
			get
			{
				yield return topBarWidget;
				yield return sideBarWidget;
				yield return mapViewWidget;
			}
		}

		public void OnInteraction(object sender, InteractionEventArgs e)
		{
			var g = Game.Instance;
			switch(e.Interaction)
			{
				case GUI.Interaction.KeyDown:
					var k = ((KeyInteractionEventArgs)e).Args;
					if(k.Key == Key.Escape)
						g.SetState(0);
					else if (k.Key == Key.Right)
						g.Camera.Pan(8, 0);
					else if (k.Key == Key.Left)
						g.Camera.Pan(-8, 0);
					else if (k.Key == Key.Up)
						g.Camera.Pan(0, -8);
					else if (k.Key == Key.Down)
						g.Camera.Pan(0, 8);
					else if(k.Key == Key.P)
						g.PlayRandomMusic();
					break;
				default:
					e.GUI.TakeInput(this, true);
					break;
			}
		}

		public void OnInteraction_Map(object sender, InteractionEventArgs e)
		{
			var g = Game.Instance;
			var pos = e.GUI.Mouse;
			if(e.Interaction != Interaction.Cold)
				g.Map.cellHighlight = g.Camera.ScreenToMapCoord(pos.X, pos.Y).CPos;

			if(e.Interaction == Interaction.Enter)
			{
				g.SendMessage(new MissionMove(28, g.Camera.ScreenToMapCoord(pos.X, pos.Y).CPos));
			}
		}


		private bool initialized;
		private TopBarWidget topBarWidget;
		private SideBarWidget sideBarWidget;
		private MapViewWidget mapViewWidget;

		private void Initialize()
		{
			initialized = true;
			Game.Instance.LoadMap("scg01ea.ini");

			topBarWidget = new TopBarWidget();
			sideBarWidget = new SideBarWidget();
			mapViewWidget = new MapViewWidget();
			mapViewWidget.Interaction += OnInteraction_Map;
			//topBarWidget.SetTabVisibility(TopBarWidget.TabTimer, false);
			sideBarWidget.SetButtonEnabled(SideBarWidget.ButtonMap, false);
		}

		public void HandleMessage(IMessage message)
		{
			var g = Game.Instance;
			g.Map.HandleMessage(message);
		}

		public void Show()
		{
			if (!initialized)
				Initialize();
			Game.Instance.PlayRandomMusic();
		}

		public void Hide()
		{
		}

		public void Update(float dt)
		{
			var g = Game.Instance;
			g.Map.Update(dt);
			g.Interact(this);
		}

		public void Render(float dt)
		{
			var g = Game.Instance;
			var renderer = g.Renderer;

			renderer.Begin();

			renderer.PushPalette(g.Map.Theater.Palette);
			mapViewWidget.Render(renderer, dt);
			topBarWidget.Render(renderer);
			sideBarWidget.Render(renderer);
			renderer.PopPalette();

			var grad6 = g.GetFont("GRAD6FNT");
			renderer.Text(grad6, "Reinforcements have arrived.", 2, 16);

			renderer.End();
		}
	}
}
