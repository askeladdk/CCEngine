using System;
using System.Drawing;
using CCEngine.Rendering;

namespace CCEngine.Simulation
{
	public class WorldView
	{
		private Map map;
		private Rectangle view; // viewable area in screen space
		private Point topLeft;  // top left in map space

		public WorldView(Map map, int vieww, int viewh)
		{
			this.map = map;
			this.view = new Rectangle(0, 0, vieww, viewh);
			this.SetTopLeft(map.Bounds.Location);
		}

		public void SetTopLeft(int mapX, int mapY)
		{
			var ts = Constants.TileSize;
			topLeft.X = Helpers.Clamp(ts * mapX, ts * map.Bounds.Left, ts * map.Bounds.Right - view.Width);
			topLeft.Y = Helpers.Clamp(ts * mapY, ts * map.Bounds.Top, ts * map.Bounds.Bottom - view.Height);
		}

		public void SetTopLeft(Point mapcoord)
		{
			SetTopLeft(mapcoord.X, mapcoord.Y);
		}

		public void CenterAt(Point mapcoord)
		{
			var ts = Constants.TileSize;
			var w = view.Width / ts;
			var h = view.Height / ts;
			SetTopLeft(mapcoord.X - w / 2, mapcoord.Y - h / 2);
		}

		public SpriteBatch Render(SpriteBatch batch)
		{
			int mapX = topLeft.X / Constants.TileSize;
			int mapY = topLeft.Y / Constants.TileSize;
			int mapXOffset = topLeft.X % Constants.TileSize;
			int mapYOffset = topLeft.Y % Constants.TileSize;
			int xMax = (view.Width / Constants.TileSize) + 1;// (mapXOffset != 0 ? 1 : 0);
			int yMax = (view.Height / Constants.TileSize) + 1;// (mapYOffset != 0 ? 1 : 0);
			int mapHeight = map.Bounds.Height;

			int screenY = view.Y - mapYOffset;

			batch.SetBlending(false);

			for(int y = 0; y < yMax; y++)
			{
				int screenX = view.X - mapXOffset;
				for(int x = 0; x < xMax; x++)
				{
					var tile = map.GetTile(mapX + x, mapY + y);
					batch
						.SetSprite(map.Theater[tile.TmpId])
						.Render(tile.TmpIndex, 0, screenX, screenY, 0.0f);
					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}

			return batch;
		}
	}
}
