using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	public sealed class Shader : Resource
	{
		private int handle;

		public int Handle { get => handle; }

		public string InfoLog { get => GL.GetShaderInfoLog(handle); }

		private int CreateShader(string source, ShaderType type)
		{
			int handle = GL.CreateShader(type);
			GL.ShaderSource(handle, source);
			GL.CompileShader(handle);
			return handle;
		}

		public Shader(string source, ShaderType type)
		{
			handle = CreateShader(source, type);
		}

		public Shader(Stream s, ShaderType type)
		{
			handle = CreateShader(new StreamReader(s).ReadToEnd(), type);
		}

		~Shader()
		{
			Dispose(false);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if (dispose_unmanaged_objects)
			{
				GL.DeleteShader(handle);
				return true;
			}

			return false;
		}

		public bool CompileStatus
		{
			get
			{
				int result = 0;
				GL.GetShader(handle, ShaderParameter.CompileStatus, out result);
				return result != 0;
			}
		}

	}

	public class ShaderParameters
	{
		public ShaderType type;
	}

	public class ShaderLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			var p = (ShaderParameters)parameters;
			using(var stream = handle.Open())
				return new Shader(stream, p.type);
		}
	}
}
