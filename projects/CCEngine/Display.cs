using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace CCEngine
{
	public class Display
	{
		private static Rectangle CalcViewport(Size screen, Size res, float dpiScale)
		{
			var resw = (int)(res.Width * dpiScale);
			var resh = (int)(res.Height * dpiScale);
			var vieww = (screen.Width / resw) * resw;
			var viewh = (vieww * resh) / resw;
			var borderw = screen.Width - vieww;
			var borderh = screen.Height - viewh;
			return new Rectangle(
				new Point(borderw / 2, borderh / 2),
				new Size(screen.Width - borderw, screen.Height - borderh)
			);
		}

		private Rectangle viewport;
		private Size resolution;
		private float dpiScale;
		private float resolutionScale;

		public Rectangle Viewport { get => viewport; }
		public Size Resolution { get => resolution; }

		public Display(float dpiScale, int resw, int resh, Rectangle clientRectangle)
		{
			this.dpiScale = dpiScale;
			this.resolution = new Size(resw, resh);
			this.viewport = CalcViewport(clientRectangle.Size, this.resolution, dpiScale);
			this.resolutionScale = viewport.Width / (resw * dpiScale);
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
	}
}