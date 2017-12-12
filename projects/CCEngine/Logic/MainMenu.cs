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

		private void Initialize()
		{
			initialized = true;
			Game.Instance.LoadMap("scg01ea.ini");
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
		}

		public void Render(float dt)
		{
			var batch = Game.Instance.SpriteBatch;

			batch.Begin();
			Game.Instance.Map.Render(dt);
			batch.End();
		}
	}
}
