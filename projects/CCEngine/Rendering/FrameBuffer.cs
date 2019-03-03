using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	public sealed class FrameBuffer : Resource
	{
		private int handle;
		private Texture colorAttachment;
		private Texture depthAttachment;
		private OpenTK.Matrix4 projection;
		private Rectangle viewport;

		public int Handle { get => handle; }

		public Rectangle Viewport { get => viewport; }

		public Texture ColorAttachment { get => colorAttachment; }

		public OpenTK.Matrix4 Projection { get => projection; }

		public FramebufferErrorCode Status
		{
			get => GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		}

		/// Create a reference to the actual screen.
		public FrameBuffer(Rectangle viewport)
		{
			this.handle = 0;
			this.viewport = viewport;
			this.projection = OpenTK.Matrix4.CreateOrthographicOffCenter(
				0, viewport.Width, viewport.Height, 0, -1, 1);
			this.colorAttachment = null;
		}

		/// Create an off-screen buffer.
		public FrameBuffer(Size size, PixelFormat format)
		{
			this.handle = GL.GenFramebuffer();
			this.viewport = new Rectangle(new Point(0, 0), size);
			this.projection = OpenTK.Matrix4.CreateOrthographicOffCenter(
				0, size.Width, size.Height, 0, -1, 1);

			Bind();

			this.colorAttachment = new Texture(size.Width, size.Height,
				PixelType.UnsignedByte, format,
				(PixelInternalFormat)format, TextureMinFilter.Nearest);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D,
				colorAttachment.Handle, 0);

			Unbind();
		}

		~FrameBuffer()
		{
			Dispose(false);
		}

		public void Bind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
		}

		public void Unbind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public void SetViewport()
		{
			GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if (!dispose_unmanaged_objects)
			{
				colorAttachment = null;
				depthAttachment = null;
				return false;
			}
			else if(handle != 0)
			{
				colorAttachment.Dispose();
				if(depthAttachment != null)
					depthAttachment.Dispose();
				GL.DeleteFramebuffer(handle);
				handle = 0;
			}
			return true;
		}
	}
}
