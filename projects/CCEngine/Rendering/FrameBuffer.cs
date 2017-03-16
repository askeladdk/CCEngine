using System;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	public class FrameBuffer : Resource
	{
		public readonly int Handle;
		public readonly Texture Texture;

		public FrameBuffer(int width, int height)
		{
			this.Handle = GL.GenFramebuffer();
			this.Texture = new Texture(width, height,
				PixelType.UnsignedByte, PixelFormat.Rgba, PixelInternalFormat.Rgba, TextureMinFilter.Nearest);
			Bind();
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.Color, TextureTarget.Texture2D, this.Texture.Handle, 0);
			Unbind();
		}

		public void Bind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.Handle);
		}

		public void Unbind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if (!dispose_unmanaged_objects)
				return false;
			GL.DeleteFramebuffer(this.Handle);
			this.Texture.Dispose();
			return true;
		}
	}
}
