﻿using System.Diagnostics;
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
		private CPos pathGoal;
		private CPos pathStart;

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
			MsgMouseButton mouseButton;

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
				var mouseCell = game.Camera.ScreenToMapCoord(mouseMove.e.Position).ToCPos();
				if (mouseCell != pathGoal)
				{
					this.pathGoal = mouseCell;
					var map = game.Map;
					map.CellHighLight = pathGoal;

					if (map.IsTilePassable(pathGoal))
					{
						var watch = new Stopwatch();
						watch.Start();
						var path = PathFinding.AStar(Game.Instance.Map, pathStart, pathGoal).ToArray();
						watch.Stop();
						game.Log("Path finding time: {0}".F(watch.Elapsed));
						map.PathHighLight = path;
					}
					else
					{
						map.PathHighLight = null;
					}
				}
			}
			else if(message.Is<MsgMouseButton>(out mouseButton))
			{
				if(mouseButton.e.IsPressed)
					this.pathStart = game.Camera.ScreenToMapCoord(mouseButton.e.Position).ToCPos();
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
