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
using CCEngine.ECS;

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
						g.Jukebox.PlayRandom();
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
			{
				highlight = g.Camera.ScreenToMapCoord(pos.X, pos.Y);
				g.World.Map.Highlight = highlight;
			}

			if(e.Interaction == Interaction.Enter)
			{
				g.SendMessage(new MissionMove(Entity.Create(28, 0), g.Camera.ScreenToMapCoord(pos.X, pos.Y)));
			}
		}


		private bool initialized;
		private TopBarWidget topBarWidget;
		private SideBarWidget sideBarWidget;
		private MapViewWidget mapViewWidget;
		private CPos highlight;

		private void Initialize()
		{
			initialized = true;
			Game.Instance.LoadMap("scg01ea");

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
			g.World.HandleMessage(message);
		}

		public void Show()
		{
			if (!initialized)
				Initialize();
		}

		public void Hide()
		{
		}

		public void Update(float dt)
		{
			var g = Game.Instance;
			g.World.Update(dt);
			g.Interact(this);
		}

		public void Render(float dt)
		{
			var g = Game.Instance;
			var renderer = g.Renderer;

			renderer.Begin();

			renderer.PushPalette(g.World.Map.Theater.Palette);
			mapViewWidget.Render(renderer, dt);
			topBarWidget.Render(renderer);
			sideBarWidget.Render(renderer);
			renderer.PopPalette();

			var fnt = g.GetFont("6POINT");
			var landtype = g.World.Map.GetLandType(highlight);
			renderer.Text(fnt, "Cell {0}: {1}".F(highlight, landtype), 2, 400 - 64);
			renderer.Text(fnt, "FPS: {0}".F((int)g.RenderFrequency), 2, 400 - 48);
			renderer.Text(fnt, "UPS: {0}".F((int)g.UpdateFrequency), 2, 400 - 32);
			renderer.Text(fnt, "Clock: {0}".F(g.GlobalClock), 2, 400 - 16);

			renderer.End();
		}
	}
}
