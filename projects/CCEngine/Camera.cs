using System;
using System.Diagnostics;
using System.Drawing;
using CCEngine.Simulation;

namespace CCEngine
{
	public class RenderBounds
	{
		private Point screenTopLeft;
		private Rectangle cellBounds;
		private Rectangle objectBounds;

		public Point ScreenTopLeft { get => screenTopLeft; }
		public Rectangle CellBounds { get => cellBounds; }
		public Rectangle ObjectBounds { get => objectBounds; }

		public RenderBounds(Camera cam)
		{
			const int TileSize = Constants.TileSize;
			var screenOffsetX = cam.TopLeft.X % TileSize;
			var screenOffsetY = cam.TopLeft.Y % TileSize;
			var edgeX = (cam.TopLeft.X + cam.ViewPort.Width) % TileSize;
			var edgeY = (cam.TopLeft.Y + cam.ViewPort.Height) % TileSize;
			this.screenTopLeft = new Point(
				cam.ViewPort.X - screenOffsetX,
				cam.ViewPort.Y - screenOffsetY
			);

			this.cellBounds = new Rectangle(
				cam.TopLeft.X / TileSize,
				cam.TopLeft.Y / TileSize,
				(cam.ViewPort.Width  / TileSize) + ((screenOffsetX + edgeX) != 0 ? 1 : 0),
				(cam.ViewPort.Height / TileSize) + ((screenOffsetY + edgeY) != 0 ? 1 : 0)
			);

			this.objectBounds = new Rectangle(
				Constants.TileSize * Math.Max(0, cellBounds.X),
				Constants.TileSize * Math.Max(0, cellBounds.Y),
				Constants.TileSize * Math.Min(cellBounds.Width, Constants.MapSize),
				Constants.TileSize * Math.Min(cellBounds.Height, Constants.MapSize)
			);
		}
	}

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

		public RenderBounds RenderBounds { get => new RenderBounds(this); }

		public void SetTopLeft(int mapX, int mapY)
		{
			topLeft = new Point(
				Helpers.Clamp(mapX, mapBounds.Left, mapBounds.Right - viewPort.Width),
				Helpers.Clamp(mapY, mapBounds.Top, mapBounds.Bottom - viewPort.Height)
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

		public CPos ScreenToMapCoord(int mx, int my)
		{
			Debug.Assert(viewPort.Contains(mx, my));
			return CPos.FromXY(
				Lepton.GetCell(Lepton.FromPixel(mx - viewPort.X + topLeft.X)),
				Lepton.GetCell(Lepton.FromPixel(my - viewPort.Y + topLeft.Y))
			);
		}

		public Point MapToScreenCoord(int mapX, int mapY)
		{
			int x2 = mapX - topLeft.X + viewPort.X;
			int y2 = mapY - topLeft.Y + viewPort.Y;
			return new Point(x2, y2);
		}

		public void SetBounds(Rectangle bounds)
		{
			var t = Constants.TileSize;

			mapBounds = new Rectangle(
				t * bounds.X,
				t * bounds.Y,
				t * bounds.Width,
				t * bounds.Height
			);

			SetTopLeft(
				t * bounds.Location.X,
				t * bounds.Location.Y
			);
		}
	}
}
