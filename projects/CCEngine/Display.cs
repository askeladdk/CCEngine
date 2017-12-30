using System.Drawing;

namespace CCEngine
{
	public class Display
	{
		private Rectangle viewport;
		private Size resolution;
		private float dpiScale;
		private float resolutionScale;

		public Rectangle ViewPort { get { return viewport; } }
		public Size Resolution { get { return resolution; } }

		public Display(float dpiScale, int resw, int resh)
		{
			this.dpiScale = dpiScale;
			this.resolution = new Size(resw, resh);
		}

		public Rectangle UpdateViewPort(int screenw, int screenh)
		{
			var resw = (int)(resolution.Width * dpiScale);
			var resh = (int)(resolution.Height * dpiScale);
			var vieww = (screenw / resw) * resw;
			var viewh = (vieww * resh) / resw;
			var borderw = screenw - vieww;
			var borderh = screenh - viewh;
			this.viewport = new Rectangle(
				new Point(borderw / 2, borderh / 2),
				new Size(screenw - borderw, screenh - borderh)
			);
			this.resolutionScale = this.viewport.Width / (int)(resolution.Width * dpiScale);
			return viewport;
		}

		public bool NormaliseScreenPosition(Point mouse, out Point normalised)
		{
			if(viewport.Contains(mouse))
			{
				var scale = dpiScale * resolutionScale;
				normalised = new Point(
					(int)((mouse.X - viewport.X) / scale),
					(int)((mouse.Y - viewport.Y) / scale)
				);
				return true;
			}
			normalised = new Point();
			return false;
		}

		public Rectangle ScissorRectangle(int x, int y, int w, int h)
		{
			var scale = dpiScale * resolutionScale;
			return new Rectangle(
				(int)(scale * x) + viewport.X,
				(int)(scale * y) + viewport.Y,
				(int)(scale * w),
				(int)(scale * h)
			);
		}
	}
}