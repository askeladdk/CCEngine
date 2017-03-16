using System;
using System.Collections.Generic;
using CCEngine.FileFormats;

namespace CCEngine.VFS
{
	public class VFS : IDisposable
	{
		private List<VFSFolder> folders = new List<VFSFolder>();

		public void AddRoot(string path)
		{
			this.folders.Add(new FileSystemFolder(path));
		}

		public bool AddMix(string filename, MixFileVersion mixver)
		{
			VFSHandle handle = this.Resolve(filename);

			if (handle != null)
			{
				this.folders.Add(new MixArchive(handle, mixver));
				return true;
			}

			return false;
		}

		public VFSHandle Resolve(string filename)
		{
			foreach(VFSFolder folder in this.folders)
			{
				VFSHandle handle = folder.Resolve(filename);
				if (handle != null) return handle;
			}

			return null;
		}

		public void Dispose()
		{
			foreach(VFSFolder folder in this.folders)
			{
				folder.Dispose();
			}

			this.folders.Clear();
		}
	}
}
