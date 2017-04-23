using System;
using System.Linq;
using System.Collections.Generic;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	public class Foundation
	{
		private bool[,] occupied;
		private int shiftx;
		private int shifty;

		public int Width { get { return this.occupied.GetLength(1); } }
		public int Height { get { return this.occupied.GetLength(0); } }
		public int ShiftX { get { return this.shiftx; } }
		public int ShiftY { get { return this.shifty; } }

		public bool IsOccupied(int x, int y)
		{
			return this.occupied[y, x];
		}

		public Foundation(IConfiguration ini, string section)
		{
			int h = ini.Enumerate(section).Count();
			int w = ini.GetString(section, "0", string.Empty).Length;

			if (h == 0 || w == 0)
				throw new Exception("[{0}] empty grid".F(section));

			this.shiftx = ini.GetInt(section, "ShiftX", 0);
			this.shifty = ini.GetInt(section, "ShiftY", 0);
			this.occupied = new bool[h, w];

			for (int y = 0; y < h; y++)
			{
				var s = ini.GetString(section, y.ToString());
				if (s == null)
					break;
				if (s.Length != w)
					throw new Exception("[{0}] unequal grid".F(section));
				for (int x = 0; x < w; x++)
					this.occupied[y, x] = (s[x] == 'x');
			}
		}
	}
}
