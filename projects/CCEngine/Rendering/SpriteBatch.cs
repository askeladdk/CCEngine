using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;

namespace CCEngine.Rendering
{
	public class SpriteBatch
	{
		// Instance data streamed to GPU.
		[StructLayout(LayoutKind.Sequential)]
		private struct SpriteInstance
		{
			public Vector2 tile_xy;
			public Vector2 tile_wh;
			public Vector4 frame_xywh;
			public float remap;
			public float cloak;
			public int color;
			//public int frame_x;
			//public int frame_y;
		}

		// Quad XY coordinates.
		private static float[] quad_xy =
		{
			0, 0,
			0, 1,
			1, 0,
			1, 1,
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

		// Shared buffers.
		private static BufferObject<float> vbo_xy;
		private static BufferObject<float> vbo_uv;
		private static BufferObject<byte>  ibo_ix;
		private static ShaderProgram prog_default;

		private static void MakeVAO(int size, out VertexArrayObject vao,
			out BufferObject<SpriteInstance> vbo_spr)
		{
			if (vbo_xy == null)
			{
				vbo_xy = new BufferObject<float>(quad_xy, BufferTarget.ArrayBuffer);
				vbo_uv = new BufferObject<float>(quad_uv, BufferTarget.ArrayBuffer);
				ibo_ix = new BufferObject<byte>(quad_el, BufferTarget.ElementArrayBuffer);
			}

			vbo_spr = new BufferObject<SpriteInstance>(
				size, BufferTarget.ArrayBuffer, BufferUsageHint.DynamicDraw);
			vao = new VertexArrayObject();
			vao.Bind();
			vbo_xy.Bind();
			vao.SetAttrib(0, 2, VertexAttribPointerType.Float);
			vbo_uv.Bind();
			vao.SetAttrib(1, 2, VertexAttribPointerType.Float);

			int stride = Helpers.SizeOf<SpriteInstance>();
			vbo_spr.Bind();
			vao.SetAttrib(2, 4, VertexAttribPointerType.Float, false, stride, 0, 1);
			vao.SetAttrib(3, 4, VertexAttribPointerType.Float, false, stride, 16, 1);
			vao.SetAttrib(4, 2, VertexAttribPointerType.Float, false, stride, 32, 1);
			vao.SetAttrib(5, 4, VertexAttribPointerType.UnsignedByte, true, stride, 40, 1);
			vbo_spr.Unbind();
			vao.Enable(0);
			vao.Enable(1);
			vao.Enable(2);
			vao.Enable(3);
			vao.Enable(4);
			vao.Enable(5);
			vao.Unbind();
		}

		private static ShaderProgram GetDefaultProgram()
		{
			if (prog_default == null)
			{
				Game g = Game.Instance;
				Shader vert = g.LoadAsset<Shader>("vert.glsl", true, new ShaderParameters
				{
					type = ShaderType.VertexShader
				});

				Shader frag = g.LoadAsset<Shader>("frag.glsl", true, new ShaderParameters
				{
					type = ShaderType.FragmentShader
				});

				prog_default = new ShaderProgram(vert, frag);
				if (!prog_default.Link())
				{
					g.Log(0, vert.InfoLog);
					g.Log(0, frag.InfoLog);
					g.Log(0, prog_default.InfoLog);
					throw new Exception("Shader program link error");
				}
			}

			return prog_default;
		}
		
		private SpriteInstance[] batch;
		private VertexArrayObject vao;
		private BufferObject<SpriteInstance> vbo_spr;
		private ShaderProgram program;
		private Palette palette;
		private Sprite sprite;
		private int color = unchecked((int)0xffffffff);
		private int count = 0;
		private int render_calls = 0;
		private int render_calls_total = 0;
		private int max_rendered = 0;
		private bool drawing = false;
		private bool blending = false;

		public int RenderCalls { get { return render_calls; } }
		public int TotalRenderCalls { get { return render_calls_total; } }
		public int MaxRendered { get { return max_rendered; } }

		public SpriteBatch(int size)
		{
			this.batch = new SpriteInstance[size];
			this.program = GetDefaultProgram();
			MakeVAO(size, out this.vao, out this.vbo_spr);
		}

		public SpriteBatch Render(int frame, int remap, float x, float y, float cloak)
		{
			if (!drawing)
				throw new Exception("SpriteBatch.Begin() must be called before Draw()");
			if (sprite == null)
				throw new Exception("SpriteBatch.Draw() has no sprite to draw");
			if (count == batch.Length)
				Flush();
			batch[count++] = new SpriteInstance
			{
				color = this.color,
				tile_xy = new Vector2(x, y),
				tile_wh = sprite.FramePixels,
				frame_xywh = sprite.GetFrameRegion(frame),
				remap = (float)remap,
				cloak = cloak,
			};
			return this;
		}

		public SpriteBatch Render(float x, float y)
		{
			if (!drawing)
				throw new Exception("SpriteBatch.Begin() must be called before Draw()");
			if (sprite == null)
				throw new Exception("SpriteBatch.Draw() has no sprite to draw");
			if (count == batch.Length)
				Flush();
			batch[count++] = new SpriteInstance
			{
				color = this.color, //unchecked((int)0xffffffff),
				tile_xy = new Vector2(x, y),
				tile_wh = sprite.ImagePixels,
				frame_xywh = Sprite.DefaultRegion,
				remap = 0.0f,
				cloak = 0.0f,
			};
			return this;
		}

		public SpriteBatch Begin()
		{
			if (drawing)
				throw new Exception("SpriteBatch is already drawing");
			render_calls = 0;
			drawing = true;
			return this;
		}

		public SpriteBatch Flush()
		{
			if (count == 0)
				return this;

			Texture sprite_tex = sprite.ToTexture();
			Texture palette_tex = palette.ToTexture();

			// bind the textures
			GL.ActiveTexture(TextureUnit.Texture0);
			sprite_tex.Bind();
			GL.ActiveTexture(TextureUnit.Texture1);
			palette_tex.Bind();

			// prepare the program
			Matrix4 projection = Game.Instance.Projection;
			program.Bind();
			GL.UniformMatrix4(program["projection"], false, ref projection);
			// tell the program on which texture units the textures are
			GL.Uniform1(program["sprite"], 0);
			GL.Uniform1(program["palette"], 1);

			// copy the instance data
			vbo_spr.Bind();
			vbo_spr.Update(batch, 0, count);
			vbo_spr.Unbind();

			// turn on blending
			if (blending)
			{
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
			}

			// do the render call
			vao.Bind();
			ibo_ix.Bind();
			GL.DrawElementsInstanced(PrimitiveType.Triangles,
				SpriteBatch.ibo_ix.Length, DrawElementsType.UnsignedByte, (IntPtr)0, count);

			if (blending)
				GL.Disable(EnableCap.Blend);

			// unbind everything
			vao.Unbind();
			ibo_ix.Unbind();
			program.Unbind();
			palette_tex.Unbind();
			sprite_tex.Unbind();

			// gather statistics
			render_calls++;
			render_calls_total++;
			max_rendered = Math.Max(max_rendered, count);
			count = 0;

			return this;
		}

		public SpriteBatch End()
		{
			Flush();
			drawing = false;
			return this;
		}

		public SpriteBatch SetSprite(Sprite sprite)
		{
			if(this.sprite != sprite)
			{
				Flush();
				this.sprite = sprite;
			}
			return this;
		}

		public SpriteBatch SetPalette(Palette palette)
		{
			if (this.palette != palette)
			{
				Flush();
				this.palette = palette;
			}
			return this;
		}

		public SpriteBatch SetBlending(bool blending)
		{
			if (this.blending != blending)
				Flush();
			this.blending = blending;
			return this;
		}

		public SpriteBatch SetColor(Color4 color)
		{
			int a = color.ToArgb();
			if (this.color != a)
				Flush();
			this.color = a;
			return this;
		}
	}
}
