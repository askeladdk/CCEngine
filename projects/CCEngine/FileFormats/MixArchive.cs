using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using CCEngine.Crypto;
using CCEngine.VFS;

namespace CCEngine.FileFormats
{
	/// <summary>
	/// Mix file version.
	/// </summary>
	public enum MixFileVersion : int
	{
		CNC = 0, // No flags, simple hash
		RA1 = 1, // Flags, simple hash
		TS  = 2, // Flags, improved hash
	}

	/// <summary>
	/// Mix file loader.
	/// </summary>
	public class MixArchive : VFSFolder
	{
		private const uint FlagChecksum  = 0x00010000;
		private const uint FlagEncrypted = 0x00020000;

		[StructLayout(LayoutKind.Sequential, Pack=1)]
		unsafe private struct HeaderStruct
		{
			public ushort nfiles;
			public int size;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		unsafe private struct IndexEntry
		{
			public uint id;
			public int offset;
			public int size;
		};

		private readonly int body_offset;
		private readonly IndexEntry[] index;
		private readonly VFSHandle handle;
		private readonly MixFileVersion version;

		private unsafe uint GetId(string name)
		{
			// TS and RA2
			if (this.version == MixFileVersion.TS)
			{
				int len = name.Length;
				int buflen = len;
				bool obfuscate = (len & 3) != 0;

				if (obfuscate) buflen += 1 + 3 - (len & 3);

				// small temp buffer
				byte* buf = stackalloc byte[buflen];

				for (int i = 0; i < len; ++i) buf[i] = (byte)char.ToUpper(name[i]);

				if (obfuscate)
				{
					int a = len >> 2;
					byte c = buf[a << 2];
					buf[len] = (byte)(len - (a << 2));
					for (int i = len + 1; i < buflen; ++i) buf[i] = c;
				}

				return CRC.Compute(buf, buflen);
			}
			// C&C and RA1
			else
			{
				name = name.ToUpper();
				int i = 0;
				uint id = 0;
				int len = name.Length;
				while (i < len)
				{
					uint a = 0;
					for (int j = 0; j < 4; j++)
					{
						a >>= 8;
						if (i < len) a += (uint)name[i] << 24;
						i++;
					}
					id = (id << 1 | id >> 31) + a;
				}
				return id;
			}
		}

		private unsafe void ParseHeader(byte[] buffer, out int nfiles)
		{
			fixed (byte* bp = buffer)
			{
				HeaderStruct* h = (HeaderStruct*)(bp);
				nfiles = h->nfiles;
			}
		}

		private unsafe IndexEntry[] ParseIndex(byte[] buffer, int nfiles, out IndexEntry[] index)
		{
			index = new IndexEntry[nfiles];
			fixed (byte* bp = &buffer[0])
			{
				IndexEntry* e = (IndexEntry*)(bp);
				for (int i = 0; i < nfiles; ++i)
				{
					index[i] = e[i];
				}
			}
			Array.Sort(index, (a, b) => a.id < b.id ? -1 : 1);
			return index;
		}

		private IndexEntry[] ReadHeader(Stream stream, out int body_offset)
		{
			BinaryReader reader = new BinaryReader(stream);
			uint flags = (this.version == MixFileVersion.CNC) ? 0 : reader.ReadUInt32();
			int nfiles = 0;
			IndexEntry[] index;

			// The mix is encrypted.
			if ((flags & FlagEncrypted) != 0)
			{
				// Read the Mix key and convert it to a Blowfish key.
				byte[] key_source = reader.ReadBytes(80);
				byte[] key = new BlowfishKeyProvider().DecryptKey(key_source);
				Blowfish bf = new Blowfish(key);

				// Parse the header.
				byte[] header_buf = stream.ReadBytes(8);
				bf.Decipher(header_buf, 8);
				this.ParseHeader(header_buf, out nfiles);

				// Parse the index.
				int index_size   = (12 * nfiles + 5) & ~7;
				byte[] index_buf = stream.ReadBytes(index_size);
				bf.Decipher(index_buf, index_size);
				Array.Copy(index_buf, 0, index_buf, 2, index_size - 2);
				Array.Copy(header_buf, 6, index_buf, 0, 2);
				this.ParseIndex(index_buf, nfiles, out index);
			}
			else
			{
				byte[] header_buf = new byte[6];
				stream.Read(header_buf, 0, 6);
				this.ParseHeader(header_buf, out nfiles);
				
				byte[] index_buf = new byte[12 * nfiles];
				stream.Read(index_buf, 0, 12 * nfiles);
				this.ParseIndex(index_buf, nfiles, out index);
			}

			body_offset = (int)stream.Position;
			return index;
		}

		public MixArchive(VFSHandle handle, MixFileVersion version)
		{
			this.handle  = handle;
			this.version = version;
			this.index   = this.ReadHeader(handle.Open(), out this.body_offset);
		}

		public VFSHandle Resolve(string filename)
		{
			uint id = this.GetId(filename);
			int i = this.index.BinarySearch(id, x => x.id);
			if (i < 0) return null;
			return this.handle.OpenView(filename, this.body_offset + this.index[i].offset, this.index[i].size);
		}

		public void Dispose()
		{
			this.handle.Dispose();
		}

		public override string ToString()
		{
			return handle.FileName;
		}
	}
}
