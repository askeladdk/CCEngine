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

		public static bool Contains(this Rectangle rect, MPos mpos)
		{
			return rect.Contains(mpos.XProj2D, mpos.YProj2D);
		}

		public static Rectangle Translate(this Rectangle rect, CPos cpos)
		{
			var x = cpos.X;
			var y = cpos.Y;
			return new Rectangle(x + rect.Left, y + rect.Top, x + rect.Width, y + rect.Height);
		}

		public static Rectangle Translate(this Rectangle rect, MPos mpos)
		{
			var x = mpos.XProj2D;
			var y = mpos.YProj2D;
			return new Rectangle(x + rect.Left, y + rect.Top, rect.Width, rect.Height);
		}
	}
}
