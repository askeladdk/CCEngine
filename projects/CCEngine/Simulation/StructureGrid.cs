using System;
using System.Collections.Generic;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Structure grid specifies the occupied cell offsets from a placed structure on the map.
	/// </summary>
	public class StructureGrid
	{
		private int[] offsets;

		public int Length
		{
			get { return this.offsets.Length; }
		}

		public int this[int i]
		{
			get { return this.offsets[i]; }
		}

		public StructureGrid(IConfiguration ini, string section)
		{
			var offsets = new List<int>();
			var shiftx = ini.GetInt(section, "ShiftX", 0);
			var shifty = ini.GetInt(section, "ShiftY", 0);
			var lineno = 0;

			while(true)
			{
				var s = ini.GetString(section, lineno.ToString());
				if(s == null)
					break;
				for(var charno = 0; charno < s.Length; charno++)
				{
					if(s[charno] != '-')
					{
						int ofs = charno + shiftx + Constants.MapSize * (lineno + shifty);
						offsets.Add(ofs);
					}
				}

				lineno++;
			}

			this.offsets = offsets.ToArray();
		}
	}
}
