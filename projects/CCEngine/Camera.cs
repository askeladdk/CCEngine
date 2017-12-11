using System;
using System.Drawing;
using CCEngine.Simulation;

namespace CCEngine
{
	public class Camera
	{
		private Rectangle viewPort = new Rectangle(); // viewable area in screen space
		private Point topLeft = new Point();  // top left in map space
		private Rectangle mapBounds = new Rectangle();

		public Point TopLeft { get { return topLeft; } }

		public Rectangle ViewPort
		{
			get
			{
				return viewPort;
			}

			set
			{
				viewPort = value;
			}
		}

		public void SetTopLeft(int mapX, int mapY)
		{
			topLeft = new Point(
				Helpers.Clamp(mapX, mapBounds.Left, mapBounds.Right - viewPort.Width),
				Helpers.Clamp(mapY, mapBounds.Top, mapBounds.Bottom - viewPort.Height + viewPort.Top)
			);
		}

		/// <summary>
		/// Center the camera at map coordinate.
		/// </summary>
		/// <param name="mpos"></param>
		public void CenterAt(XPos mpos)
		{
			var w = viewPort.Width;
			var h = viewPort.Height;
			SetTopLeft(
				mpos.CellX + (Constants.TileSize - w) / 2,
				mpos.CellY + (Constants.TileSize - h) / 2
			);
		}

		public void Pan(int dx, int dy)
		{
			SetTopLeft(topLeft.X + dx, topLeft.Y + dy);
		}

		public XPos ScreenToMapCoord(int mx, int my)
		{
			if (!viewPort.Contains(mx, my))
				return new XPos(0, 0, -1, -1);
			var x = mx - viewPort.X + topLeft.X;
			var y = my - viewPort.Y + topLeft.Y;
			return new XPos(0, 0, x, y);
		}

		public Point MapToScreenCoord(int mapX, int mapY)
		{
			int x2 = mapX - topLeft.X + viewPort.X;
			int y2 = mapY - topLeft.Y + viewPort.Y;
			return new Point(x2, y2);
		}

		/// <summary>
		/// Calculates which cells are in view and from which screen coordinate rendering should begin.
		/// </summary>
		/// <param name="screenTopLeft">Start rendering from this screen coordinate.</param>
		/// <param name="cellBounds">Region of visible cells.</param>
		public void GetRenderArea(out Point screenTopLeft, out Rectangle cellBounds)
		{
			const int TileSize = Constants.TileSize;
			var screenOffsetX = topLeft.X % TileSize;
			var screenOffsetY = topLeft.Y % TileSize;
			var edgeX = (topLeft.X + viewPort.Width) % TileSize;
			var edgeY = (topLeft.Y + viewPort.Height) % TileSize;
			screenTopLeft = new Point(
				viewPort.X - screenOffsetX,
				viewPort.Y - screenOffsetY
			);
			cellBounds = new Rectangle(
				topLeft.X / TileSize,
				topLeft.Y / TileSize,
				(viewPort.Width  / TileSize) + ((screenOffsetX + edgeX) != 0 ? 1 : 0),
				(viewPort.Height / TileSize) + ((screenOffsetY + edgeY) != 0 ? 1 : 0)
			);
		}

		public void SetBounds(Map map)
		{
			var b = map.Bounds;
			var t = Constants.TileSize;

			mapBounds = new Rectangle(
				t * b.X,
				t * b.Y,
				t * b.Width,
				t * b.Height
			);

			SetTopLeft(
				t * b.Location.X,
				t * b.Location.Y
			);
		}
	}
}
