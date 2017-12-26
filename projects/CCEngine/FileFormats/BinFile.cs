using System;
using System.IO;

namespace CCEngine.FileFormats
{
	/// Generic binary blob.
	public class BinFile
	{
		private byte[] bytes;

		// Get the data.
		public byte[] Bytes { get => bytes; }

		public BinFile(byte[] bytes)
		{
			this.bytes = bytes;
		}

		public BinFile(Stream stream)
		{
			this.bytes = stream.ReadBytes((int)stream.Length);
		}
	}

	public class BinFLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			var filename = handle.FileName;
			using(var stream = handle.Open())
			{
				Palette palette;
				// Read a CPS as binary (for palette.cps).
				if(filename.EndsWith(".CPS"))
					return new BinFile(CpsFile.ReadAsBytes(stream, out palette));
				else
					return new BinFile(stream);
			}
		}
	}
}
