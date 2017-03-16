using System;
using System.IO;
using System.Runtime.InteropServices;
using CCEngine.Codecs;
using CCEngine.Rendering;

namespace CCEngine.FileFormats
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ShpOffset
	{
		private uint offset;
		private uint refoff;

		public uint Offset { get { return this.offset & 0x00ffffff; } }
		public uint OffsetFormat { get { return this.offset >> 24; } }
		public uint Ref { get { return this.refoff & 0x00ffffff; } }
		public uint RefFormat { get { return this.refoff >> 24; } }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ShpHeader
	{
		public ushort nframes;
		public ushort unknown1;
		public ushort unknown2;
		public ushort width;
		public ushort height;
		public ushort maxframesize;
		public ushort flags;
	}

	public class ShpFile
	{
		private static void Decode(Stream stream, ShpOffset[] offsets, int width, int height,
			byte[][] frames, byte[] src, ref int recdepth)
		{
			if (recdepth > frames.Length)
				throw new NotImplementedException("Format 40/20 frames contain infinite loop!");

			for (uint i = 0; i < frames.Length; i++)
			{
				byte[] frame = new byte[width * height];
				byte[] refframe = null;
				ShpOffset curoff = offsets[i + 0];
				ShpOffset nxtoff = offsets[i + 1];
				int srclen = (int)(nxtoff.Offset - curoff.Offset);

				stream.Read(src, 0, srclen);

				switch(curoff.OffsetFormat)
				{
					// Format 20
					case 0x20:
						refframe = frames[i - 1];
						goto do_format40;
					// Format 40
					case 0x40:
						int j = Array.FindIndex(offsets, o => o.Offset == curoff.Ref);
						if (j < 0)
							throw new IndexOutOfRangeException("Invalid frame reference!");
						refframe = frames[j];
					do_format40:
						if (refframe == null)
						{
							recdepth++;
							Decode(stream, offsets, width, height, frames, src, ref recdepth);
							recdepth--;
						}
						Format40.Decode(src, srclen, frame, refframe);
						break;
					// Format 80
					case 0x80:
						Format80.Decode(src, 0, frame, 0, srclen);
						break;
					// Invalid format
					default:
						throw new NotImplementedException("Invalid format!");
				}

				frames[i] = frame;
			}
		}

		private static byte[][] Decode(Stream stream, ShpOffset[] offsets, int width, int height, int nframes)
		{
			int recdepth = 0;
			byte[] src = new byte[width * height];
			byte[][] frames = new byte[nframes][];
			Decode(stream, offsets, width, height, frames, src, ref recdepth);

			return frames;
		}

		public static Sprite Read(Stream stream)
		{
			ShpHeader header = stream.ReadStruct<ShpHeader>();
			ShpOffset[] offsets = new ShpOffset[2 + header.nframes];
			for (uint i = 0; i < offsets.Length; i++)
				offsets[i] = stream.ReadStruct<ShpOffset>();

			int frame_w = header.width;
			int frame_h = header.height;
			byte[][] frames = Decode(stream, offsets, frame_w, frame_h, header.nframes);

			return new Sprite(frames, frame_w, frame_h);
		}
	}
}
