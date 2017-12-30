using System.Collections.Generic;
using CCEngine.FileFormats;
using CCEngine.GUI;

namespace CCEngine
{
	public partial class Game
	{
		private Dictionary<string, Font> fonts = new Dictionary<string, Font>();
		private GUI.GUI gui = new GUI.GUI();

		public GUI.GUI GUI { get => gui; }

		/// GUI interaction.
		public void Interact(IWidget widget)
		{
			gui.Interact(widget);
		}

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


		protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			gui.KeyDown(e);
		}

		protected override void OnMouseMove(OpenTK.Input.MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
			OpenTK.Point point;
			if(display.NormaliseScreenPosition(e.Position, out point))
				gui.MouseMove(point.X, point.Y);
		}

		protected override void OnMouseUp(OpenTK.Input.MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			gui.MousePress(e.Button, false);
		}

		protected override void OnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			gui.MousePress(e.Button, true);
		}
	}
}
