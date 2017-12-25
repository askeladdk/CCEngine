using System;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	sealed class VertexArrayObject : Resource
	{
		private int handle;

		public int Handle { get => handle; }

		public VertexArrayObject()
		{
			GL.GenVertexArrays(1, out handle);
		}

		~VertexArrayObject()
		{
			Dispose(false);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if (dispose_unmanaged_objects)
			{
				GL.DeleteVertexArrays(1, ref handle);
				return true;
			}

			return false;
		}

		public void SetAttrib(int index, int nelem, VertexAttribPointerType type,
			bool normalized = false, int stride = 0, int offset = 0, int divisor = 0)
		{
			GL.VertexAttribPointer(index, nelem, type, normalized, stride, offset);
			GL.VertexAttribDivisor(index, divisor);
		}

		public void Enable(int index)
		{
			GL.EnableVertexAttribArray(index);
		}

		public void Disable(int index)
		{
			GL.DisableVertexAttribArray(index);
		}

		public void Bind()
		{
			GL.BindVertexArray(handle);
		}

		public void Unbind()
		{
			GL.BindVertexArray(0);
		}
	}
}
