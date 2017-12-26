using CCEngine.FileFormats;
using CCEngine.Rendering;
using OpenTK;

namespace CCEngine.GUI
{
	/// Encapsulates a file font with a corresponding palette.
	public class Font
	{
		private FntFile fnt;
		private Palette pal;

		/// The font sprite.
		public Sprite FontFace { get => fnt; }

		/// The palette.
		public Palette Palette { get => pal; }

		/// Maximum width and height of a glyph in pixels.
		public Size CharacterSize { get => fnt.FrameSize; }

		// Constructor.
		public Font(FntFile fnt, Palette pal)
		{
			this.fnt = fnt;
			this.pal = pal;
		}

		/// Width in pixels of a string.
		/// The width can exceed the width of the screen if the string is too long.
		public int StringWidth(string s)
		{
			var w = 0;
			foreach(var c in s)
			{
				var d = fnt.GlyphData(c);
				w += d.width;
			}
			return w;
		}

		/// Get an individual glyph's position data.
		public GlyphData GlyphData(int idx)
		{
			return fnt.GlyphData(idx);
		}
	}
}