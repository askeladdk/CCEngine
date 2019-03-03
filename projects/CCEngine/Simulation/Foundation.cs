using System;
using System.Linq;
using System.Collections.Generic;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Foundation specifies the width and height in cells of structures and terrain objects.
	/// </summary>
	public class Foundation
	{
		private int width;
		private int height;

		public int Width { get { return this.width; } }
		public int Height { get { return this.height; } }

		public Foundation(IConfiguration ini, string section)
		{
			this.width = ini.GetInt(section, "W");
			this.height = ini.GetInt(section, "H");
		}

		public override string ToString()
		{
			return "[{0}, {1}]".F(this.width, this.height);
		}
	}
}
