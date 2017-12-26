using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using CCEngine.Rendering;

namespace CCEngine.FileFormats
{
	public struct GlyphData
	{
		public int width;
		public int height;
		public int yoffset;
	}

	public class FntFile : Sprite
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct FontHeader
		{
			public ushort fsize; // file size in bytes including header
			public ushort sig0; // 0x0500
			public ushort sig1; // 0x000E
			public ushort cofs;
			public ushort wpos; // absolute offset to byte[1+nchars] of char widths
			public ushort cdata; // absolute offset to font glyph array[1+nchars]
			public ushort hpos; // absolute offset to CharHeight[1+nchars] of char heights
			public ushort _unk0e;
			public byte _always_zero;
			public byte nchars; // number of glyphs = 1+nchars
			public byte hmax; // maximum glyph height
			public byte wmax; // maximum glyph width
		}

		private GlyphData[] gdata;

		private FntFile(byte[][] glyphs, int wmax, int hmax, GlyphData[] gdata)
			: base(glyphs, wmax, hmax, null)
		{
			this.gdata = gdata;
		}

		public GlyphData GlyphData(int idx)
		{
			return gdata[idx];
		}

		private static void DecodeGlyph4bpp(Stream stream,
			int glyphw, int glyphh, byte[] pix, int stride)
		{
			var bpw = (glyphw + 1) / 2; // bytes per line
			int b;
			for(var y = 0; y < glyphh; y++)
			{
				var ofs = y * stride;
				for(var x = 0; x < bpw - 1; x++)
				{
					b = stream.ReadByte();
					if(b < 0)
						throw new Exception("Error reading font file");
					pix[ofs + 2 * x + 0] = (byte)(b & 0x0F);
					pix[ofs + 2 * x + 1] = (byte)((b & 0xF0) >> 4);
				}

				// special case for uneven widths
				b = stream.ReadByte();
				if(b < 0)
					throw new Exception("Error reading font file");
				pix[ofs + 2 * (bpw - 1) + 0] = (byte)(b & 0x0F);
				if((glyphw & 1) == 0)
					pix[ofs + 2 * (bpw - 1) + 1] = (byte)((b & 0xF0) >> 4);
			}
		}

		public static object Read(Stream stream)
		{
			// read the header
			FontHeader hdr = stream.ReadStruct<FontHeader>();
			if(hdr.sig0 != 0x0500 || hdr.sig1 != 0x000E)
				throw new Exception("Invalid font file");

			// allocate the buffers
			var nchars = 1 + hdr.nchars;
			var cofs = new ushort[nchars];
			var glyphs = new byte[256][];
			var gdata = new GlyphData[256];
	
			// glyph offsets
			stream.Seek(hdr.cofs, SeekOrigin.Begin);
			stream.ReadStructs<ushort>(cofs, nchars);

			// widths
			stream.Seek(hdr.wpos, SeekOrigin.Begin);
			for(var i = 0; i < nchars; i++)
				gdata[i].width = (byte)stream.ReadByte();

			// heights
			stream.Seek(hdr.hpos, SeekOrigin.Begin);
			for(var i = 0; i < nchars; i++)
			{
				gdata[i].yoffset = (byte)stream.ReadByte();
				gdata[i].height = (byte)stream.ReadByte();
			}

			// glyphs
			var glyphsz = hdr.wmax * hdr.hmax;
			var stride = hdr.wmax;
			for(var i = 0; i < nchars; i++)
			{
				stream.Seek(cofs[i], SeekOrigin.Begin);
				var w = gdata[i].width;
				var h = gdata[i].height;
				glyphs[i] = new byte[glyphsz];
				DecodeGlyph4bpp(stream, w, h, glyphs[i], stride);
			}

			// fill up to 256 glyphs
			for(var i = nchars; i < 256; i++)
			{
				glyphs[i] = glyphs[0];
				gdata[i] = gdata[0];
			}

			return new FntFile(glyphs, hdr.wmax, hdr.hmax, gdata);
		}
	}

	public class FontLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			using (var stream = handle.Open())
				return FntFile.Read(stream);
		}
	}
}