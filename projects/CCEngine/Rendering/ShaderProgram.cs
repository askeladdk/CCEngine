using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	public sealed class ShaderProgram : Resource
	{
		private int handle;
		private Shader[] shaders;
		private Dictionary<string, int> parameters;

		public int Handle { get => handle; }

		public string InfoLog { get => GL.GetProgramInfoLog(handle); }

		public ShaderProgram(params Shader[] shaders)
		{
			this.handle = GL.CreateProgram();
			this.shaders = shaders;
			this.parameters = new Dictionary<string, int>();

			for (int i = 0; i < shaders.Length; ++i)
			{
				GL.AttachShader(handle, shaders[i].Handle);
			}
		}

		~ShaderProgram()
		{
			Dispose(false);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if (dispose_unmanaged_objects)
			{
				GL.DeleteProgram(handle);
				return true;
			}

			return false;
		}

		private Dictionary<string, int> MapAttributes(Dictionary<string, int> map)
		{
			int count;
			GL.GetProgram(handle, GetProgramParameterName.ActiveAttributes, out count);

			for (int i = 0; i < count; ++i)
			{
				int size;
				ActiveAttribType type;
				string name = GL.GetActiveAttrib(handle, i, out size, out type);
				int index = GL.GetAttribLocation(handle, name);
				map[name] = index;
			}

			return map;
		}

		private Dictionary<string, int> MapUniforms(Dictionary<string, int> map)
		{
			int count;
			GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out count);

			for (int i = 0; i < count; ++i)
			{
				string name = GL.GetActiveUniformName(handle, i);
				int index = GL.GetUniformLocation(handle, name);
				map[name] = index;
			}

			return map;
		}

		public bool Link()
		{
			GL.LinkProgram(handle);
			parameters.Clear();

			if (LinkStatus)
			{
				MapAttributes(MapUniforms(parameters));
				return true;
			}

			return false;
		}

		public void Bind()
		{
			GL.UseProgram(handle);
		}

		public void Unbind()
		{
			GL.UseProgram(0);
		}

		public bool LinkStatus
		{
			get
			{
				int result;
				GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out result);
				return result != 0;
			}
		}

		public int this[string name]
		{
			set
			{
				GL.BindAttribLocation(handle, value, name);
			}

			get
			{
				int idx;
				return parameters.TryGetValue(name, out idx) ? idx : -1;
			}
		}
	}
}
