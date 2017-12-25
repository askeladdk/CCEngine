using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using CCEngine;

namespace CCEngine.Rendering
{
	public class BufferObject : Resource
	{
		readonly private BufferTarget target;
		readonly private int nelems;
		private int handle;

		protected BufferObject(BufferTarget target, int nelems)
		{
			GL.GenBuffers(1, out this.handle);
			this.target = target;
			this.nelems = nelems;
		}

		public IntPtr Map(BufferAccess access = BufferAccess.WriteOnly)
		{
			return GL.MapBuffer(target, access);
		}

		public void Unmap()
		{
			GL.UnmapBuffer(target);
		}

		public void Bind()
		{
			GL.BindBuffer(target, Handle);
		}

		public void Unbind()
		{
			GL.BindBuffer(target, 0);
		}

		public BufferTarget Target
		{
			get { return target; }
		}

		public int Handle
		{
			get { return handle; }
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if (dispose_unmanaged_objects)
			{
				GL.DeleteBuffers(1, ref handle);
				return true;
			}
			return false;
		}
		public int Length
		{
			get { return nelems; }
		}
	}

	public sealed class BufferObject<T> : BufferObject where T : struct
	{
		public BufferObject(T[] data, BufferTarget target,
			BufferUsageHint hint = BufferUsageHint.StaticDraw)
			: base(target, data.Length)
		{
			Bind();
			GL.BufferData(target, (IntPtr)(Helpers.SizeOf<T>() * data.Length), data, hint);
			Unbind();
		}

		public BufferObject(int nelems, BufferTarget target,
			BufferUsageHint hint = BufferUsageHint.StaticDraw)
			: base(target, nelems)
		{
			Bind();
			GL.BufferData(target, (IntPtr)(Helpers.SizeOf<T>() * nelems), IntPtr.Zero, hint);
			Unbind();
		}

		~BufferObject()
		{
			Dispose(false);
		}

		public void Update(T[] src, int srcOffset, int dstOffset, int length)
		{
			var sz = Helpers.SizeOf<T>();
			var h = GCHandle.Alloc(src, GCHandleType.Pinned);
			var p = h.AddrOfPinnedObject();
			var data = IntPtr.Add(p, sz * srcOffset);
			GL.BufferSubData(Target, (IntPtr)(sz * dstOffset), (IntPtr)(sz * length), data);
			h.Free();
		}

		public void Update(T[] data, int offset, int length)
		{
			int sz = Helpers.SizeOf<T>();
			GL.BufferSubData(Target, (IntPtr)(sz * offset), (IntPtr)(sz * length), data);
		}

		public void VertexAttribPointer(int index,
			int stride = 0, int offset = 0, bool normalized = false,
			VertexAttribPointerType type = VertexAttribPointerType.Float)
		{
			int sz = Helpers.SizeOf<T>();
			GL.VertexAttribPointer(index, sz, type, normalized, sz * stride, sz * offset);
		}
	}
}
