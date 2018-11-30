using System;
using System.Linq;
using System.IO;

namespace CCEngine.VFS
{
	public interface VFSFolder : IDisposable
	{
		VFSHandle Resolve(string filename);
	}

	public class FileSystemFolder : VFSFolder
	{
		private string path;
		private string[] files;

		public FileSystemFolder(string path)
		{
			this.path = path;
			this.files = (
				from f in Directory.GetFiles(path)
				orderby f.ToUpper()
				select Path.GetFileName(f).ToUpper()
			).ToArray();
		}

		public VFSHandle Resolve(string filename)
		{
			int i = this.files.BinarySearch(filename.ToUpper());
			if (i >= 0)
			{
				return new VFSHandle(Path.Combine(this.path, filename));
			}
			return null;
		}

		public void Dispose()
		{
			// Nothing to dispose.
		}

		public override string ToString()
		{
			return path;
		}
	}
}
