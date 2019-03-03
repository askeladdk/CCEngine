/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 * 
 * Modified from OpenRA source code.
 */
using System;
using System.IO;

namespace CCEngine.Codecs
{
	public static class FastBinaryStream
	{
		public static byte ReadByte(byte[] src, ref int srcpos)
		{
			return src[srcpos++];
		}

		public static int ReadWord(byte[] src, ref int srcpos)
		{
			int x = BitConverter.ToUInt16(src, srcpos);
			srcpos += 2;
			return x;
		}

		public static void Copy(byte[] src, ref int srcpos, byte[] dst, ref int dstpos, int count)
		{
			Array.Copy(src, srcpos, dst, dstpos, count);
			srcpos += count;
			dstpos += count;
		}
	}

	public static class Format80
	{
		private static void ReplicatePrevious(byte[] dst, ref int dstpos, int cpypos, int count)
		{
			if (dstpos - cpypos == 1)
			{
				for (int end = dstpos + count; dstpos < end; dstpos++)
					dst[dstpos] = dst[dstpos - 1];
			}
			else
			{
				for (int end = dstpos + count; dstpos < end; dstpos++)
					dst[dstpos] = dst[cpypos++];
			}
		}

		public static int Decode(byte[] src, int srcpos, byte[] dst, int dstpos, int srclen, bool reverse = false)
		{
			int dstofs = dstpos;

			while (srcpos < srclen)
			{
				int cmd = FastBinaryStream.ReadByte(src, ref srcpos);

				// case 2
				if ((cmd & 0x80) == 0)
				{
					int count  = ((cmd & 0x70) >> 4) + 3;
					int relpos = ((cmd & 0x0f) << 8) + FastBinaryStream.ReadByte(src, ref srcpos);
					if (dstpos + count > dst.Length)
						return dstpos;
					ReplicatePrevious(dst, ref dstpos, dstpos - relpos, count);
				}
				// case 1
				else if ((cmd & 0x40) == 0)
				{
					int count = cmd & 0x3f;
					if (count == 0)
						break;
					FastBinaryStream.Copy(src, ref srcpos, dst, ref dstpos, count);
				}
				else
				{
					cmd &= 0x3f;
					// case 4
					if (cmd == 0x3e)
					{
						var count = FastBinaryStream.ReadWord(src, ref srcpos);
						var color = FastBinaryStream.ReadByte(src, ref srcpos);

						for (int end = dstpos + count; dstpos < end; dstpos++)
							dst[dstpos] = color;
					}
					// case 5
					else if (cmd == 0x3f)
					{
						int count  = FastBinaryStream.ReadWord(src, ref srcpos);
						int cpypos = FastBinaryStream.ReadWord(src, ref srcpos) + dstofs;
						if (reverse)
							cpypos = dstpos - cpypos;
						if (cpypos >= dstpos)
							throw new IndexOutOfRangeException("cpypos >= dstpos");

						for (int end = dstpos + count; dstpos < end; dstpos++)
							dst[dstpos] = dst[cpypos++];
					}
					// case 3
					else
					{
						int count  = cmd + 3;
						int cpypos = FastBinaryStream.ReadWord(src, ref srcpos) + dstofs;
						if (reverse)
							cpypos = dstpos - cpypos;
						if (cpypos >= dstpos)
							throw new IndexOutOfRangeException("cpypos >= dstpos");

						for (int end = dstpos + count; dstpos < end; dstpos++)
							dst[dstpos] = dst[cpypos++];
					}
				}
			}

			return dstpos;
		}
	}
}
