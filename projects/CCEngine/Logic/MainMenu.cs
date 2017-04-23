﻿using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.Simulation;

namespace CCEngine.Logic
{
	class MainMenu : IGameState
	{
		bool initialized;
		Sprite sprite;

		private void Initialize()
		{
			initialized = true;
			this.sprite = Game.Instance.LoadAsset<Sprite>("ctnk.shp");
			Game.Instance.LoadMap("scg01ea.ini");
		}

		public void HandleMessage(IMessage message)
		{
			MsgKeyDown msgk;
			MsgMouseMove mouse;
			if(message.Is<MsgKeyDown>(out msgk))
			{
				if(msgk.e.Key == OpenTK.Input.Key.Escape)
				{
					Game.Instance.SetState(0);
				}

				if (msgk.e.Key == OpenTK.Input.Key.Right)
					Game.Instance.Camera.Pan(8, 0);
				else if (msgk.e.Key == OpenTK.Input.Key.Left)
					Game.Instance.Camera.Pan(-8, 0);
				else if (msgk.e.Key == OpenTK.Input.Key.Up)
					Game.Instance.Camera.Pan(0, -8);
				else if (msgk.e.Key == OpenTK.Input.Key.Down)
					Game.Instance.Camera.Pan(0, 8);
			}
			else if(message.Is<MsgMouseMove>(out mouse))
			{
				var mousecell = Game.Instance.Camera.ScreenToMapCoord(mouse.e.Position);
				Game.Instance.Map.CellHighLight = mousecell;
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
			int tick = Game.Instance.GlobalClock;
			//GL.Enable(EnableCap.Blend);
			//GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

			var batch = Game.Instance.SpriteBatch;

			batch.Begin();
			Game.Instance.Map.Render(dt);
			batch.End();

			/*for (int i = 0; i < 10; i++)
			{
				TmpFile tmp = temperat[i + 135];
				if (tmp != null)
				{
					batch.SetSprite(tmp);
					batch.Draw(i * 48, i * 48);
				}
			}*/

			/*
			var tmp = temperat[400];
			int tmpw = (int)tmp.CellSize.X;
			int tmpy = (int)tmp.CellSize.Y;
			int tmpi = 0;
			batch.SetSprite(tmp);
			for (int y = 0; y < tmpy; y++)
			{
				for(int x = 0; x < tmpw; x++)
				{
					batch.Render(tmpi++, 0, x * tmp.FramePixels.X, y * tmp.FramePixels.Y, 0.0f);
				}
			}

			float swh = sprite.FramePixels.X / 2;
			float shh = sprite.FramePixels.Y / 2;
			batch
				.SetSprite(sprite)
				.Render((int)tick, (int)(tick / 30), -swh + 12 + 24, -shh + 12 + 0, 0.0f)
				.Render((int)tick, (int)(tick / 30), -swh + 12 + 48, -shh + 12 + 24, 0.0f)
				.Render((int)tick, (int)(tick / 30), -swh + 12 + 72, -shh + 12 + 48, 1.0f)
				.End();
			*/
		}
	}
}
