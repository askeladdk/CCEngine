using System.Collections.Generic;
using CCEngine.FileFormats;
using CCEngine.GUI;

namespace CCEngine
{
	public partial class Game
	{
		private Dictionary<string, Font> fonts = new Dictionary<string, Font>();

		public Font GetFont(string name)
		{
			return fonts[name];
		}

		/// The GUI data is loaded once when the game starts.
		private void InitialiseGUI()
		{
			var ini = LoadAsset<IniFile>("gui.ini", false);
			foreach(var kv in ini.Enumerate("Fonts"))
			{
				var name = kv.Value;
				var fntid = ini.GetString(name, "Font");
				var palid = ini.GetString(name, "Palette");
				if(fntid == null || palid == null)
					continue;
				var fnt = LoadAsset<FntFile>("{0}.FNT".F(fntid));
				var pal = LoadAsset<Palette>("{0}.PAL".F(palid));
				fonts[name] = new Font(fnt, pal);
			}
		}
	}
}
