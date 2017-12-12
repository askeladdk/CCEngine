using System;
using System.Drawing;

namespace CCEngine
{
	public static class RectangleExt
	{
		public static bool Contains(this Rectangle rect, CPos cpos)
		{
			return rect.Contains(cpos.X, cpos.Y);
		}

		public static Rectangle Translate(this Rectangle rect, int dx, int dy)
		{
			return new Rectangle(dx + rect.Left, dy + rect.Top, dx + rect.Width, dy + rect.Height);
		}
	}
}
