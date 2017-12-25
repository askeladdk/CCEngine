using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using CCEngine.FileFormats;

namespace CCEngine.Rendering
{
	/// <summary>
	/// 
	/// </summary>
	public class Renderer
	{
		private static Matrix4 Transform(float tx, float ty, float sx, float sy, float r)
		{
			Matrix4 scale, rotate, translate;
			Matrix4.CreateScale(sx, sy, 1, out scale);
			Matrix4.CreateRotationZ(r, out rotate);
			Matrix4.CreateTranslation(tx, ty, 0, out translate);
			return Matrix4.Mult(Matrix4.Mult(scale, rotate), translate);
		}

		private static ShaderProgram LoadShaderProgram(string vertfile, string fragfile)
		{
			var g = Game.Instance;
			var vert = g.LoadAsset<Shader>(vertfile, true, new ShaderParameters
			{
				type = ShaderType.VertexShader
			});

			var frag = g.LoadAsset<Shader>(fragfile, true, new ShaderParameters
			{
				type = ShaderType.FragmentShader
			});

			var program = new ShaderProgram(vert, frag);
			if (!program.Link())
			{
				g.Log(LogLevel.Error, vert.InfoLog);
				g.Log(LogLevel.Error, frag.InfoLog);
				g.Log(LogLevel.Error, program.InfoLog);
				throw new Exception("Shader program link error");
			}

			return program;
		}

		private SpriteBatch spriteBatch;
		private RectangleBatch rectangleBatch;

		public Renderer()
		{
			this.spriteBatch = new SpriteBatch(
				LoadShaderProgram("vert.glsl", "frag.glsl"),
				2048
			);
			this.rectangleBatch = new RectangleBatch(
				LoadShaderProgram("rect.v.glsl", "rect.f.glsl"),
				256
			);
		}

		public void SetPalette(Palette palette)
		{
			spriteBatch.SetPalette(palette);
		}

		public void Flush()
		{
			spriteBatch.Flush();
			rectangleBatch.Flush();
		}

		public void Rectangle(int x, int y, int w, int h, int color)
		{
			rectangleBatch.Queue(new RectangleInstance{
				// translation = new Vector2(x + w/2, y + h/2),
				// scaling = new Vector2(w, h),
				// rotation = 0,
				transform = Transform(x + w/2, y + h/2, w, h, 0),
				color = color,
			});
		}

		public void Line(int x0, int y0, int x1, int y1, int color, int thickness = 1)
		{
			var dx = x1 - x0;
			var dy = y1 - y0;
			var rotation = MathF.Atan2(dy, dx);
			var width = MathF.Sqrt(dx * dx + dy * dy);
			rectangleBatch.Queue(new RectangleInstance{
				// translation = new Vector2(x0 + dx/2, y0 + dy/2),
				// scaling = new Vector2(width, thickness),
				// rotation = rotation,
				transform = Transform(x0 + dx/2, y0 + dy/2, width, thickness, rotation),
				color = color,
			});
		}

		public void Blit(Sprite sprite, int frame, int x, int y,
			int color = -1, byte remap = 0, byte cloak = 0)
		{
			var w = sprite.FrameSize.Width;
			var h = sprite.FrameSize.Height;
			var t = sprite.ToTexture().Handle;
			var instance = new TexturedQuadInstance
			{
				translation = new Vector2(x, y),
				scaling = new Vector2(w, h),
				region = sprite.GetFrameRegion(frame),
				modifiers = ((remap & 7) * 32) | (cloak << 8),
				channel = color,
			};
			spriteBatch.Queue(t, instance);
		}
	}
}
