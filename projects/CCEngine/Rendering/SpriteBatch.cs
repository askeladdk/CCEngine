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
	public struct TexturedQuadInstance
	{
		public Vector2 translation;
		public Vector2 scaling;
		public Vector4 region;
		public int modifiers;
		public int channel;
	}

	public class SpriteBatch : SortedBatch<int, TexturedQuadInstance>
	{
		// Quad XY coordinates.
		private static float[] quad_xy =
		{
			-0.5f, -0.5f,
			-0.5f, 0.5f,
			0.5f, -0.5f,
			0.5f, 0.5f,
		};

		// Quad UV coordinates.
		private static float[] quad_uv = 
		{
			0, 0,
			0, 1,
			1, 0,
			1, 1,
		};

		// Quad index buffer.
		private static byte[] quad_el = 
        {
            0, 1, 2, 1, 2, 3,
        };

		private BufferObject<float> vbo_xy;
		private BufferObject<float> vbo_uv;
		private BufferObject<byte>  ebo_ix;
		private VertexArrayObject vao;
		private ShaderProgram program;
		private Texture paletteTexture;

		private void MakeVAO()
		{
			vbo_xy = new BufferObject<float>(quad_xy, BufferTarget.ArrayBuffer);
			vbo_uv = new BufferObject<float>(quad_uv, BufferTarget.ArrayBuffer);
			ebo_ix = new BufferObject<byte>(quad_el, BufferTarget.ElementArrayBuffer);
			vao = new VertexArrayObject();

			vao.Bind();
			vbo_xy.Bind();
			vao.SetAttrib(0, 2, VertexAttribPointerType.Float);
			vbo_uv.Bind();
			vao.SetAttrib(1, 2, VertexAttribPointerType.Float);

			int stride = this.InstanceStride;
			var vbo_spr = this.VBO;
			vbo_spr.Bind();
			vao.SetAttrib(2, 2, VertexAttribPointerType.Float, false, stride, 0, 1);
			vao.SetAttrib(3, 2, VertexAttribPointerType.Float, false, stride, 8, 1);
			vao.SetAttrib(4, 4, VertexAttribPointerType.Float, false, stride, 16, 1);
			vao.SetAttrib(5, 4, VertexAttribPointerType.UnsignedByte, true, stride, 32, 1);
			vao.SetAttrib(6, 4, VertexAttribPointerType.UnsignedByte, true, stride, 36, 1);
			vbo_spr.Unbind();

			vao.Enable(0);
			vao.Enable(1);
			vao.Enable(2);
			vao.Enable(3);
			vao.Enable(4);
			vao.Enable(5);
			vao.Enable(6);
			vao.Unbind();
		}

		public SpriteBatch(ShaderProgram program, int maxInstances) : base(maxInstances)
		{
			this.program = program;
			MakeVAO();
		}

		public void SetPalette(Palette palette)
		{
			this.paletteTexture = palette.ToTexture();
		}

		protected override void BeginSubmit()
		{
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, paletteTexture.Handle);

			// prepare the program
			Matrix4 projection = Game.Instance.Projection;
			program.Bind();
			GL.UniformMatrix4(program["projection"], false, ref projection);
			// tell the program on which texture units the textures are
			GL.Uniform1(program["sprite"], 0);
			GL.Uniform1(program["palette"], 1);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
			vao.Bind();
			ebo_ix.Bind();
		}

		protected override void Submit(int texture, int count)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, texture);
			GL.DrawElementsInstanced(PrimitiveType.Triangles,
				ebo_ix.Length, DrawElementsType.UnsignedByte, (IntPtr)0, count);
		}

		protected override void EndSubmit()
		{
			ebo_ix.Unbind();
			vao.Unbind();

			program.Unbind();
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Disable(EnableCap.Blend);
		}
	}
}