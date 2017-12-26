using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using CCEngine.FileFormats;

namespace CCEngine.Rendering
{
	/// Rendering Subsystem.
	public class Renderer
	{
		/// Build a transformation matrix that is translated, scaled and rotated.
		private static Matrix4 Transform(float tx, float ty, float sx, float sy, float r)
		{
			Matrix4 scale, rotate, translate;
			Matrix4.CreateScale(sx, sy, 1, out scale);
			Matrix4.CreateRotationZ(r, out rotate);
			Matrix4.CreateTranslation(tx, ty, 0, out translate);
			return Matrix4.Mult(Matrix4.Mult(scale, rotate), translate);
		}

		/// Load a shader program.
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

		/// Enumerates all possible batches. Only one at a time can be active.
		private enum BatchType
		{
			None,
			Sprite,
			Rectangle
		}

		private SpriteBatch spriteBatch;
		private RectangleBatch rectangleBatch;
		private BatchType activeBatch = BatchType.None;
		private Stack<Texture> paletteStack = new Stack<Texture>();
		private Texture activePalette = null;

		/// Constructor.
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

		/// Set the active palette.
		/// The render queue will be flushed if the new palette is
		/// different than the currently active palette.
		public void PushPalette(Palette palette)
		{
			var paltex = palette.ToTexture();
			if(paltex != activePalette)
				Flush();
			paletteStack.Push(activePalette);
			spriteBatch.SetPalette(paltex);
			activePalette = paltex;
		}

		/// Reactivate the previous palette.
		public void PopPalette()
		{
			Texture palette;
			if(!paletteStack.TryPop(out palette))
				return;
			if(palette != activePalette)
				Flush();
			spriteBatch.SetPalette(palette);
			activePalette = palette;
		}

		/// Send the render queue to the GPU.
		public void Flush()
		{
			switch(activeBatch)
			{
				case BatchType.Sprite:
					spriteBatch.Flush();
					break;
				case BatchType.Rectangle:
					rectangleBatch.Flush();
					break;
			}
		}

		/// Choose active batch and flush if needed.
		private void Flush(BatchType switchto)
		{
			if(activeBatch != switchto)
			{
				Flush();
				activeBatch = switchto;
			}
		}

		/// Draw a rectangle.
		public void Rectangle(int x, int y, int w, int h, int color)
		{
			Flush(BatchType.Rectangle);
			rectangleBatch.Queue(new RectangleInstance{
				transform = Transform(x + w/2, y + h/2, w, h, 0),
				color = color,
			});
		}

		/// Draw a line.
		public void Line(int x0, int y0, int x1, int y1, int color, int thickness = 1)
		{
			Flush(BatchType.Rectangle);
			var dx = x1 - x0;
			var dy = y1 - y0;
			var rotation = MathF.Atan2(dy, dx);
			var width = MathF.Sqrt(dx * dx + dy * dy);
			rectangleBatch.Queue(new RectangleInstance{
				transform = Transform(x0 + dx/2, y0 + dy/2, width, thickness, rotation),
				color = color,
			});
		}

		/// Blit a sprite.
		public void Blit(Sprite sprite, int frame, int x, int y,
			int color = -1, byte remap = 0, byte cloak = 0)
		{
			Flush(BatchType.Sprite);
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

		/// Blit text of a given font.
		public void Text(GUI.Font font, string text, int x, int y, bool centered = false, int color = -1)
		{
			if(text.Length == 0)
				return;
			PushPalette(font.Palette);

			x += font.CharacterSize.Width / 2;
			y += font.CharacterSize.Height / 2;

			if(centered)
			{
				var wtotal = font.StringWidth(text);
				x -= wtotal / 2;
			}

			foreach(var c in text)
			{
				var d = font.GlyphData(c);
				Blit(font.FontFace, c, x, y + d.yoffset, color);
				x += d.width;
			}

			PopPalette();
		}
	}
}
