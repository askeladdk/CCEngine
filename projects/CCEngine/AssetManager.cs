using System;
using System.IO;
using System.Collections.Generic;
using CCEngine.VFS;

namespace CCEngine
{
	public interface IAssetLoader
	{
		object Load(AssetManager assets, VFSHandle handle, object parameters);
	}

	public class AssetManager : IDisposable
	{
		private readonly Dictionary<string, object> assets = new Dictionary<string, object>();
		private readonly Dictionary<Type, IAssetLoader> loaders;
		private readonly AssetManager parent;
		private readonly VFS.VFS vfs;

		private bool TryGet(string filename, out object asset)
		{
			if (this.assets.TryGetValue(filename, out asset))
				return true;
			if (this.parent != null)
				return this.parent.TryGet(filename, out asset);
			return false;
		}

		public AssetManager(VFS.VFS vfs)
		{
			this.loaders = new Dictionary<Type, IAssetLoader>();
			this.parent  = null;
			this.vfs     = vfs;
		}

		public AssetManager(AssetManager parent)
		{
			this.loaders = parent.loaders;
			this.parent  = parent;
			this.vfs     = parent.vfs;
		}

		public void RegisterLoader<T>(IAssetLoader loader)
		{
			this.loaders[typeof(T)] = loader;
		}

		private T Load<T>(AssetManager assets, string filename, bool cache, object parameters)
			where T : class
		{
			object asset;
			filename = filename.ToUpper();
			if (!cache || !this.TryGet(filename, out asset))
			{
				IAssetLoader loader;
				if (!this.loaders.TryGetValue(typeof(T), out loader))
					throw new Exception("Asset {0} has no loader".F(filename));
				VFSHandle handle = this.vfs.Resolve(filename);
				if (handle == null)
					return null; //throw new Exception("Asset {0} not found".F(filename));
				asset = loader.Load(assets, handle, parameters);
				if(asset == null)
					return null; //throw new Exception("Asset {0} failed to load".F(filename));
				if (cache)
					this.assets[filename] = asset;
			}
			return (T)asset;
		}

		public T Load<T>(string filename, bool cache = true, object parameters = null)
			where T : class
		{
			return Load<T>(this, filename, cache, parameters);
		}

		public void Dispose()
		{
			this.assets.Clear();
		}
	}
}
