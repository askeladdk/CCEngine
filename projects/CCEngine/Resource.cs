using System;

namespace CCEngine
{
	public abstract class Resource : IDisposable
	{
		private bool disposed = false;

		public bool IsDisposed
		{
			get { return disposed; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool dispose_unmanaged_objects)
		{
			if (!disposed)
			{
				disposed = Cleanup(dispose_unmanaged_objects);
				GC.SuppressFinalize(this);
			}
		}

		protected virtual bool Cleanup(bool dispose_unmanaged_objects)
		{
			return true;
		}
	}
}
