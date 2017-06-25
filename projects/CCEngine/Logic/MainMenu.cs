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
		bool initialized;

		private void Initialize()
		{
			initialized = true;
			Game.Instance.LoadMap("scg01ea.ini");
		}

		public void HandleMessage(IMessage message)
		{
			var game = Game.Instance;
			MsgKeyDown msgk;
			MsgMouseMove mouseMove;

			if(message.Is<MsgKeyDown>(out msgk))
			{
				if(msgk.e.Key == OpenTK.Input.Key.Escape)
					game.SetState(0);
				else if (msgk.e.Key == OpenTK.Input.Key.Right)
					game.Camera.Pan(8, 0);
				else if (msgk.e.Key == OpenTK.Input.Key.Left)
					game.Camera.Pan(-8, 0);
				else if (msgk.e.Key == OpenTK.Input.Key.Up)
					game.Camera.Pan(0, -8);
				else if (msgk.e.Key == OpenTK.Input.Key.Down)
					game.Camera.Pan(0, 8);
			}
			else if(message.Is<MsgMouseMove>(out mouseMove))
			{
				var mousecell = game.Camera.ScreenToMapCoord(mouseMove.e.Position).ToCPos();
				var map = game.Map;
				map.CellHighLight = mousecell;

				if (map.IsTilePassable(mousecell))
				{
					var path = PathFinding.AStar(Game.Instance.Map, new CPos(62, 50), mousecell).ToArray();
					map.PathHighLight = path;
				}
				else
				{
					map.PathHighLight = null;
				}
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
