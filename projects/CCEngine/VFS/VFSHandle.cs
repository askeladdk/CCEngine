using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace CCEngine.VFS
{
	public class VFSHandle : IDisposable
	{
		private readonly MemoryMappedFile mmf;
		private readonly VFSHandle parent;
		private readonly long offset;
		private readonly long size;
		private readonly string filename;

		public long Size { get { return size; } }
		public string FileName { get { return filename; } }

		private MemoryMappedFile GetFile()
		{
			if (this.parent != null) return this.parent.GetFile();
			return this.mmf;
		}

		public VFSHandle(string filename)
		{
			FileInfo fi   = new FileInfo(filename);
			this.parent   = null;
			this.mmf = MemoryMappedFile.CreateFromFile(
				File.Open(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read),
				null,
				0,
				MemoryMappedFileAccess.Read,
				null,
				HandleInheritability.None,
				false
			);
			this.offset   = 0;
			this.size     = fi.Length;
			this.filename = filename;
		}

		private VFSHandle(string filename, VFSHandle parent, long offset, long size)
		{
			this.parent   = parent;
			this.mmf      = null;
			this.offset   = offset;
			this.size     = size;
			this.filename = filename;
		}

		public Stream Open()
		{
			return this.GetFile().CreateViewStream(this.offset, this.size, MemoryMappedFileAccess.Read);
		}

		public VFSHandle OpenView(string filename, long offset, long size)
		{
			return new VFSHandle(filename, this, this.offset + offset, size);
		}

		public void Dispose()
		{
			if (this.mmf != null)
			{
				this.mmf.Dispose();
			}
		}
	}
}
