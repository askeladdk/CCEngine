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
		private float dpiScale;

		public Point TopLeft { get { return topLeft; } }

		public float DpiScale { get {return dpiScale; } }

		public Rectangle ViewPort
		{
			get
			{
				return new Rectangle(
					(int)(dpiScale * viewPort.X),
					(int)(dpiScale * viewPort.Y),
					(int)(dpiScale * viewPort.Width),
					(int)(dpiScale * viewPort.Height)
				);
			}

			set
			{
				viewPort = value;
			}
		}

		public Camera(float dpiScale)
		{
			this.dpiScale = dpiScale;
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
		public void CenterAt(MPos mpos)
		{
			var w = viewPort.Width;
			var h = viewPort.Height;
			SetTopLeft(
				mpos.X + (Constants.TileSize - w) / 2,
				mpos.Y + (Constants.TileSize - h) / 2
			);
		}

		public void Pan(int dx, int dy)
		{
			SetTopLeft(topLeft.X + dx, topLeft.Y + dy);
		}

		public MPos ScreenToMapCoord(Point mouse)
		{
			var mx = (int)(mouse.X / dpiScale);
			var my = (int)(mouse.Y / dpiScale);
			if (!viewPort.Contains(mx, my))
				return new MPos(-1, -1, 0);
			var x = mx - viewPort.X + topLeft.X;
			var y = my - viewPort.Y + topLeft.Y;
			return new MPos(x, y, 0);
		}

		public MPos ScreenToMapCoord(OpenTK.Point mouse)
		{
			return ScreenToMapCoord(new Point(mouse.X, mouse.Y));
		}

		public Point MapToScreenCoord(int mapX, int mapY)
		{
			int x2 = mapX - topLeft.X + viewPort.X;
			int y2 = mapY - topLeft.Y + viewPort.Y;
			return new Point(x2, y2);
		}

		public MPos ScreenToMapCoord(int screenX, int screenY)
		{
			int x = screenX + topLeft.X + viewPort.X;
			int y = screenY + topLeft.Y + viewPort.Y;
			return new MPos(x, y, 0);
		}

		public Point MapToScreenCoord(Point coord)
		{
			return MapToScreenCoord(coord.X, coord.Y);
		}

		public Point MapToScreenCoord(MPos mpos)
		{
			return MapToScreenCoord(mpos.XProj2D, mpos.YProj2D);
		}

		/// <summary>
		/// Calculates which cells are in view and from which screen coordinate rendering should begin.
		/// </summary>
		/// <param name="screenTopLeft">Start rendering from this screen coordinate.</param>
		/// <param name="cellBounds">Region of visible cells.</param>
		public void GetRenderArea(out Point screenTopLeft, out Rectangle cellBounds)
		{
			var screenOffsetX = topLeft.X % Constants.TileSize;
			var screenOffsetY = topLeft.Y % Constants.TileSize;
			screenTopLeft = new Point(
				viewPort.X - screenOffsetX,
				viewPort.Y - screenOffsetY
			);
			cellBounds = new Rectangle(
				topLeft.X / Constants.TileSize,
				topLeft.Y / Constants.TileSize,
				(viewPort.Width  / Constants.TileSize) + (screenOffsetX != 0 ? 1 : 0),
				(viewPort.Height / Constants.TileSize) + (screenOffsetY != 0 ? 1 : 0)
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
