using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using CCEngine;
using CCEngine.Rendering;

namespace CCEngine.FileFormats
{
	public class PcxFile
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		unsafe private struct PcxHeader
		{
			public byte manufacturer;
			public byte version;
			public byte encoding;
			public byte bits_per_pixel;
			public ushort xmin;
			public ushort ymin;
			public ushort xmax;
			public ushort ymax;
			public ushort hres;
			public ushort vres;
			public fixed byte pal16[48];
			public byte reserved;
			public byte num_bits_planes;
			public ushort bytes_per_line;
			public ushort pal_type;
			public ushort h_scr_size;
			public ushort v_scr_size;
			public fixed byte padding[54];
		}

		public static Sprite Read(Stream stream)
		{
			PcxHeader header = stream.ReadStruct<PcxHeader>();

			if (header.bits_per_pixel != 8 || header.num_bits_planes != 1)
				throw new Exception("Pcx is not a 256 color image");

			int frame_w = header.xmax - header.xmin + 1;
			int frame_h = header.ymax - header.ymin + 1;
			byte[] frame;

			// uncompressed
			if(header.encoding == 0)
			{
				frame = stream.ReadBytes(frame_w * frame_h);
			}
			// rle compressed
			else
			{
				frame = new byte[frame_w * frame_h];
				int dstpos = 0;
				while(dstpos < frame.Length)
				{
					int color = stream.ReadByte();
					if (color < 0)
						throw new Exception("Pcx corrupt header");
					else if( (color & 0xc0) == 0xc0 )
					{
						int count = color & 0x3f;
						color = stream.ReadByte();
						if (color < 0)
							throw new Exception("Pcx corrupt header");
						while (count-- > 0) frame[dstpos++] = (byte)color;
					}
					else
					{
						frame[dstpos++] = (byte)color;
					}
				}
			}

			// 256 color palette
			Palette palette = null;
			if (stream.ReadByte() == 0x0c)
				palette = new Palette(stream, 0);

			return new Sprite(frame, frame_w, frame_h, palette);
		}
	}
}
