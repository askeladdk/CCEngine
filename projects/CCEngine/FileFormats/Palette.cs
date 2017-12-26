using System;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using CCEngine.Rendering;

namespace CCEngine.FileFormats
{
	public class Palette
	{
		public const  int RemapStart  = 80;
		public const  int RemapCount  = 16;
		public const  int Transparent = 0;
		public const  int Shadow      = 4;
		public const uint ShadowAlpha = 128;

		private uint[] buffer;
		private Texture texture;
		private bool remappable = false;

		private static uint[] ReadBuffer(Stream stream, int shift, bool fix_special)
		{
			uint[] buffer = new uint[256];
			byte[] tmp = new byte[3];
			for (int i = 0; i < buffer.Length; i++)
			{
				stream.Read(tmp, 0, 3);
				uint r = (uint)(tmp[0] << shift);
				uint g = (uint)(tmp[1] << shift);
				uint b = (uint)(tmp[2] << shift);
				uint a = 255;
				buffer[i] = (a << 24) | (r << 16) | (g << 8) | b;
			}

			buffer[Transparent] = 0;
			if (fix_special)
			{
				buffer[Shadow] = ShadowAlpha << 24;
			}

			return buffer;
		}

		public bool IsBuffered { get { return buffer != null; } }

		public bool IsRemappable { get { return remappable; } }
		public uint[] Buffer { get { return buffer; } }

		public Palette(Stream stream, int shift, bool fix_special)
		{
			this.buffer = ReadBuffer(stream, shift, fix_special);
		}

		public Texture ToTexture(bool discard_buffer = false)
		{
			if (this.IsBuffered && texture == null)
			{
				texture = new Texture(buffer, 256, remappable ? Constants.HouseColors : 1,
					PixelType.UnsignedByte, PixelFormat.Bgra,
					PixelInternalFormat.Rgba, TextureMinFilter.Nearest);
				if (discard_buffer)
					buffer = null;
			}
			return texture;
		}

		public Palette MakeRemappable(Sprite palette_cps, bool discard_buffer = false)
		{
			if(this.IsRemappable)
			{
				if (discard_buffer)
					buffer = null;
				return this;
			}
			else if (this.IsBuffered && palette_cps.IsBuffered)
			{
				uint[] remap = new uint[256 * Constants.HouseColors];

				// Copy the theatre palette several times and fill in the remappable colors from the cps.
				for (int i = 0; i < Constants.HouseColors; i++)
				{
					Array.Copy(buffer, 0, remap, i * 256, 256);
					for (int j = 0; j < RemapCount; j++)
					{
						byte pal_idx = palette_cps.Buffer[320 * i + j];
						remap[i * 256 + j + RemapStart] = buffer[pal_idx];
					}
				}

				this.remappable = true;
				this.buffer = discard_buffer ? null : remap;
				return this;
			}
			else
			{
				if (discard_buffer)
					buffer = null;
				return null;
			}
		}
	}

	class PaletteParameters
	{
		public int shift;
		public bool fix_special;
	}

	class PaletteLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			var shift = 2; // ra1 palette colours are stored as 6-bit values.
			var fix_special = false;
			if(parameters != null)
			{
				var p = (PaletteParameters)parameters;
				shift = p.shift;
				fix_special = p.fix_special;
			}
			using (var stream = handle.Open())
				return new Palette(stream, shift, fix_special);
		}
	}
}
