using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;


namespace CCEngine.Rendering
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RectangleInstance
	{
		public Matrix4 transform;
		public int color;
	}

	public class RectangleBatch : UnsortedBatch<RectangleInstance>
	{
		// Quad XY coordinates.
		private static float[] quad_xy =
		{
			-0.5f, -0.5f,
			-0.5f,  0.5f,
			 0.5f, -0.5f,
			 0.5f,  0.5f,
		};

		// Quad index buffer.
		private static byte[] quad_el = 
		{
			0, 1, 2, 1, 2, 3,
		};

		private BufferObject<float> vbo_xy;
		private BufferObject<byte>  ebo_ix;
		private VertexArrayObject vao;
		private ShaderProgram program;

		private void MakeVAO()
		{
			vbo_xy = new BufferObject<float>(quad_xy, BufferTarget.ArrayBuffer);
			ebo_ix = new BufferObject<byte>(quad_el, BufferTarget.ElementArrayBuffer);
			vao = new VertexArrayObject();

			vao.Bind();
			vbo_xy.Bind();
			vao.SetAttrib(0, 2, VertexAttribPointerType.Float);

			int stride = this.InstanceStride;
			var vbo = this.VBO;
			vbo.Bind();
			vao.SetAttrib(1, 4, VertexAttribPointerType.Float, false, stride, 0, 1);
			vao.SetAttrib(2, 4, VertexAttribPointerType.Float, false, stride, 16, 1);
			vao.SetAttrib(3, 4, VertexAttribPointerType.Float, false, stride, 32, 1);
			vao.SetAttrib(4, 4, VertexAttribPointerType.Float, false, stride, 48, 1);
			vao.SetAttrib(5, 4, VertexAttribPointerType.UnsignedByte, true, stride, 64, 1);
			vbo.Unbind();
			vao.Enable(0);
			vao.Enable(1);
			vao.Enable(2);
			vao.Enable(3);
			vao.Enable(4);
			vao.Enable(5);
			vao.Unbind();
		}

		public RectangleBatch(ShaderProgram program, int maxInstances) : base(maxInstances)
		{
			this.program = program;
			MakeVAO();
		}

		protected override void BeginSubmit()
		{
			Matrix4 projection = Game.Instance.Projection;
			program.Bind();
			GL.UniformMatrix4(program["projection"], false, ref projection);

			vao.Bind();
			ebo_ix.Bind();
		}

		protected override void Submit(int count)
		{
			GL.DrawElementsInstanced(PrimitiveType.Triangles,
				ebo_ix.Length, DrawElementsType.UnsignedByte, (IntPtr)0, count);
		}

		protected override void EndSubmit()
		{
			ebo_ix.Unbind();
			vao.Unbind();

			program.Unbind();
		}
	}
}