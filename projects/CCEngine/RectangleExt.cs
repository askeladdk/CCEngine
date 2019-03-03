using System;
using System.Drawing;

namespace CCEngine
{
	public static class RectangleExt
	{
		public static bool Contains(this Rectangle rect, CPos cpos)
		{
			return rect.Contains(cpos.CellX, cpos.CellY);
		}

		public static Rectangle Translate(this Rectangle rect, int dx, int dy)
		{
			return new Rectangle(dx + rect.Left, dy + rect.Top, dx + rect.Width, dy + rect.Height);
		}

		public static Rectangle ToSystemDrawing(this OpenTK.Rectangle r)
		{
			return new Rectangle(r.X, r.Y, r.Width, r.Height);
		}
	}
}
