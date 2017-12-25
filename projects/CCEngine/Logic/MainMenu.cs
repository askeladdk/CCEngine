using System.Diagnostics;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.Simulation;
using CCEngine.Algorithms;

namespace CCEngine.Logic
{
	class MainMenu : IGameState
	{
		private bool initialized;
		private GUI.GUI gui;
		private GUI.HUDWidget hud;

		private void Initialize()
		{
			initialized = true;
			Game.Instance.LoadMap("scg01ea.ini");

			gui = new GUI.GUI();
			hud = new GUI.HUDWidget();
			hud.SetButtonEnabled(GUI.HUDWidget.ButtonMap, false);
			hud.SetTabVisibility(GUI.HUDWidget.TabTimer, false);
		}

		public void HandleMessage(IMessage message)
		{
			var g = Game.Instance;
			MsgKeyDown msgk;
			MsgMouseMove mouseMove;
			MsgMouseButton mouseButton;

			if(message.Is<MsgKeyDown>(out msgk))
			{
				if(msgk.e.Key == OpenTK.Input.Key.Escape)
					g.SetState(0);
				else if (msgk.e.Key == OpenTK.Input.Key.Right)
					g.Camera.Pan(8, 0);
				else if (msgk.e.Key == OpenTK.Input.Key.Left)
					g.Camera.Pan(-8, 0);
				else if (msgk.e.Key == OpenTK.Input.Key.Up)
					g.Camera.Pan(0, -8);
				else if (msgk.e.Key == OpenTK.Input.Key.Down)
					g.Camera.Pan(0, 8);
			}
			else if(message.Is<MsgMouseMove>(out mouseMove))
			{
				OpenTK.Point pos;
				if(g.Display.NormaliseScreenPosition(mouseMove.e.Position, out pos))
				{
					g.Map.cellHighlight = g.Camera.ScreenToMapCoord(pos.X, pos.Y).CPos;
					gui.MouseMove(pos.X, pos.Y);
				}
			}
			else if(message.Is<MsgMouseButton>(out mouseButton))
			{
				if (mouseButton.e.IsPressed)
				{
					OpenTK.Point pos;
					if(g.Display.NormaliseScreenPosition(mouseButton.e.Position, out pos))
					{
						g.SendMessage(new MissionMove(28, g.Camera.ScreenToMapCoord(pos.X, pos.Y).CPos));
					}
				}
				gui.MousePress(mouseButton.e.Button, mouseButton.e.IsPressed);
			}
			else
			{
				g.Map.HandleMessage(message);
			}
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
			Game.Instance.Map.Update(dt);
			gui.Interact(hud);
			gui.Flip();
		}

		public void Render(float dt)
		{
			var g = Game.Instance;
			var renderer = g.Renderer;

			g.Map.Render(dt);
			hud.Render(renderer);

			// renderer.Rectangle(0, 0, 10, 10, -1);
			// renderer.Line(10, 10, 100, 10, -1,3);
			// renderer.Line(100, 10, 100, 100, -1,3);
			// renderer.Line(100, 100, 10, 100, -1,3);
			// renderer.Line(10, 100, 10, 10, -1,3);
			// renderer.Line(10, 10, 100, 100, Color4.BlueViolet.ToArgb());
			// renderer.Flush();
		}
	}
}
