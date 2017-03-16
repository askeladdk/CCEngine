using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using CCEngine.Codecs;
using CCEngine.Rendering;

namespace CCEngine.FileFormats
{
	public class CpsFile
	{
		private static readonly uint FLAG_PALETTE = 0x03000000;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		unsafe private struct CpsHeader
		{
			public ushort file_size;
			public ushort unknown;
			public ushort image_size;
			public uint   flags;
		}

		public static Sprite Read(Stream stream)
		{
			CpsHeader header = stream.ReadStruct<CpsHeader>();
			int src_size     = header.file_size - Helpers.SizeOf<CpsHeader>() + 2;
			Palette palette  = null;

			if( (header.flags & FLAG_PALETTE) == FLAG_PALETTE )
			{
				palette = new Palette(stream, 2, false);
				src_size -= 768;
			}

			byte[] src = stream.ReadBytes(src_size);
			byte[] dst = new byte[header.image_size];
			Format80.Decode(src, 0, dst, 0, src.Length);

			return new Sprite(dst, 320, 200, palette);
		}
	}
}
