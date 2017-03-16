using System;
using System.IO;
using CCEngine;

namespace CCEngine.FileFormats
{
    public class StrFile
    {
		public static string[] Read(Stream stream)
		{
			BinaryReader w = new BinaryReader(stream);
			int header     = w.ReadUInt16();
			int count      = header / 2;
			string[] table = new string[count - 1];
			int[] offsets  = new int[count];

			offsets[0] = header;
			for (int i = 1; i < count; i++)
			{
				offsets[i] = w.ReadUInt16();
			}

			for (int i = 0; i < count - 1; i++)
			{
				int start  = offsets[i + 0];
				int end    = offsets[i + 1];
				char[] buf = w.ReadChars(end - start);
				table[i]   = new string(buf, 0, buf.Length - 1);
			}

			return table;
		}

		public static void Write(Stream stream, string[] table)
		{
			BinaryWriter w = new BinaryWriter(stream);
			ushort offset  = (ushort)(2 + 2 * table.Length);

			w.Write(offset);
			foreach(string s in table)
			{
				offset += (ushort)(1 + s.Length);
				w.Write(offset);
			}

			foreach(string s in table)
			{
				byte[] buf = System.Text.Encoding.ASCII.GetBytes(s);
				w.Write(buf);
				w.Write((byte)0);
			}
			w.Write((byte)0);
		}
    }
}
