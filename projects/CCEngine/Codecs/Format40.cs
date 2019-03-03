using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCEngine.Codecs
{
	public static class Format40
	{
		public static void Decode(byte[] src, int srclen, byte[] dst, byte[] bse)
		{
			int srcpos = 0;
			int dstpos = 0;
			int bsepos = 0;
			int count  = 0;

			while(srcpos < srclen)
			{
				int cmd = FastBinaryStream.ReadByte(src, ref srcpos);

				if( (cmd & 0x80) == 0 )
				{
					// 00000000 c*8 v*8 = xor next c bytes with v
					if(cmd == 0)
					{
						count = FastBinaryStream.ReadByte(src, ref srcpos);
						byte color = FastBinaryStream.ReadByte(src, ref srcpos);
						while (count-- > 0) dst[dstpos++] = (byte)(bse[bsepos++] ^ color);
					}
					// 0ccccccc = xor the next c bytes from source with those in base
					else
					{
						count = cmd;
						while (count-- > 0) dst[dstpos++] = (byte)(src[srcpos++] ^ bse[bsepos++]);
					}
				}
				else
				{
					count = cmd & 0x7f;

					if(count == 0)
					{
						count = FastBinaryStream.ReadWord(src, ref srcpos);
						cmd     = count >> 8;

						// 10000000 0ccccccc c*8 = large copy base to dest for count
						if( (cmd & 0x80) == 0 )
						{
							if (count == 0) return;
							FastBinaryStream.Copy(bse, ref bsepos, dst, ref dstpos, count);
						}
						else
						{
							count &= 0x3fff;
							// 10000000 10cccccc c*8 = xor the next c bytes from source with those in base
							if( (cmd & 0x40) == 0 )
							{
								while (count-- > 0) dst[dstpos++] = (byte)(src[srcpos++] ^ bse[bsepos++]);
							}
							// 10000000 11cccccc c*8 v*8
							else
							{
								byte fill = src[srcpos++];
								while (count-- > 0) dst[dstpos++] = (byte)(bse[bsepos++] ^ fill);
							}
						}
					}
					// 1ccccccc = small copy base to dest for count
					else
					{
						FastBinaryStream.Copy(bse, ref bsepos, dst, ref dstpos, count);
					}
				}
			}
		}
	}
}
