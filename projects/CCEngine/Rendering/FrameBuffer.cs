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

		public int Handle { get => handle; }

		public Size Size { get => colorAttachment.Size; }

		public Texture ColorAttachment { get => colorAttachment; }

		public OpenTK.Matrix4 Projection { get => projection; }

		public FramebufferErrorCode Status
		{
			get => GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		}

		public FrameBuffer(int width, int height)
		{
			this.handle = 0;
			this.projection = OpenTK.Matrix4.CreateOrthographicOffCenter(
				0, width, height, 0, -1, 1);
			this.colorAttachment = null;
		}

		public FrameBuffer(int width, int height, PixelFormat format, bool withDepthBuffer)
		{
			this.handle = GL.GenFramebuffer();
			this.projection = OpenTK.Matrix4.CreateOrthographicOffCenter(
				0, width, height, 0, -1, 1);

			Bind();

			this.colorAttachment = new Texture(width, height,
				PixelType.UnsignedByte, format,
				(PixelInternalFormat)format, TextureMinFilter.Nearest);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
				FramebufferAttachment.Color, TextureTarget.Texture2D,
				colorAttachment.Handle, 0);

			if(withDepthBuffer)
			{
				this.depthAttachment = new Texture(width, height,
					PixelType.UnsignedByte, PixelFormat.DepthComponent,
					PixelInternalFormat.DepthComponent, TextureMinFilter.Nearest);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
					FramebufferAttachment.Depth, TextureTarget.Texture2D,
					depthAttachment.Handle, 0);
			}

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
