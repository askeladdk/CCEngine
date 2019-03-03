using System;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using CCEngine.Rendering;

namespace CCEngine.FileFormats
{
	/// Palette.
	public class Palette
	{
		private const  int RemapStart  = 80;
		private const  int RemapCount  = 16;
		private const  int Transparent = 0;
		private const  int ShadowIndex = 4;
		private const uint ShadowColor = 0x80000000;

		/// Read palette into memory.
		private static void ReadBuffer(Stream stream, uint[] buffer, int shift, bool hasShadow)
		{
			byte[] tmp = new byte[3];
			for (int i = 0; i < 256; i++)
			{
				stream.Read(tmp, 0, 3);
				uint r = (uint)(tmp[0] << shift);
				uint g = (uint)(tmp[1] << shift);
				uint b = (uint)(tmp[2] << shift);
				uint a = 255;
				buffer[i] = (a << 24) | (r << 16) | (g << 8) | b;
			}

			if (hasShadow)
				buffer[ShadowIndex] = ShadowColor;
			buffer[Transparent] = 0;
		}

		/// Make palette usable for remapping.
		private static void MakeRemappable(uint[] palette, byte[] cpsRemap, int nremaps)
		{
			// Copy the palette several times and fill in the remappable colors from the cps.
			for (var h = 1; h < nremaps; h++)
			{
				Array.Copy(palette, 0, palette, h * 256, 256);
				for (var i = 0; i < RemapCount; i++)
				{
					byte pal_idx = cpsRemap[320 * h + i];
					palette[h * 256 + i + RemapStart] = palette[pal_idx];
				}
			}
		}

		private uint[] palette;
		private Texture texture;
		private bool remappable;
		private bool cycles;

		/// Get the texture.
		public Texture Texture { get => texture; }

		/// Whether this texture is remappable.
		public bool IsRemappable { get => remappable; }

		/// Whether this texture can cycle its colours.
		public bool Cycles { get => cycles; }

		public Palette(Stream stream, int shift = 2,
			bool hasShadow = false, bool cycles = false, byte[] cpsRemap = null)
		{
			remappable = (cpsRemap != null);
			var height = remappable ? Constants.HouseColors : 1;
			var palette = new uint[256 * height];
			ReadBuffer(stream, palette, shift, hasShadow);
			if(remappable)
				MakeRemappable(palette, cpsRemap, Constants.HouseColors);
			texture = new Texture(palette, 256, height, PixelType.UnsignedByte,
				PixelFormat.Bgra, PixelInternalFormat.Rgba, TextureMinFilter.Nearest);
			if(cycles)
				this.palette = palette;
			this.cycles = cycles;
		}
	}

	public class PaletteParameters
	{
		public int shift;
		public bool hasShadow;
		public bool cycles;
		public byte[] cpsRemap;
	}

	class PaletteLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			var shift = 2; // ra1 palette colours are stored as 6-bit values.
			var hasShadow = false;
			var cycles = false;
			byte[] cpsRemap = null;
			if(parameters != null)
			{
				var p = (PaletteParameters)parameters;
				shift = p.shift;
				hasShadow = p.hasShadow;
				cycles = p.cycles;
				cpsRemap = p.cpsRemap;
			}
			using (var stream = handle.Open())
				return new Palette(stream, shift, hasShadow, cycles, cpsRemap);
		}
	}
}
