using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	public sealed class Texture : Resource
	{
		readonly public int Handle;
		readonly public Vector2 Size;

		private void MakeTexture<T>(T[] buffer, int width, int height,
			PixelType type,
			PixelFormat format,
			PixelInternalFormat internalformat,
			TextureMinFilter filter) where T:struct
		{
			GCHandle h = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try
			{
				IntPtr p = h.AddrOfPinnedObject();
				GL.BindTexture(TextureTarget.Texture2D, this.Handle);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);
				GL.TexImage2D(TextureTarget.Texture2D, 0, internalformat, width, height, 0,
					format, type, p);
			}
			finally
			{
				if(h.IsAllocated)
					h.Free();
			}
		}

		public Texture(byte[] buffer, int width, int height,
			PixelType type,
			PixelFormat format,
			PixelInternalFormat internalformat,
			TextureMinFilter filter)
		{
			this.Handle = GL.GenTexture();
			this.Size = new Vector2(width, height);
			this.MakeTexture(buffer, width, height, type, format, internalformat, filter);
		}

		public Texture(uint[] buffer, int width, int height,
			PixelType type,
			PixelFormat format,
			PixelInternalFormat internalformat,
			TextureMinFilter filter)
		{
			this.Handle = GL.GenTexture();
			this.Size = new Vector2(width, height);
			this.MakeTexture(buffer, width, height, type, format, internalformat, filter);
		}

		public Texture(int width, int height,
			PixelType type,
			PixelFormat format,
			PixelInternalFormat internalformat,
			TextureMinFilter filter)
		{
			this.Handle = GL.GenTexture();
			this.Size = new Vector2(width, height);
			GL.BindTexture(TextureTarget.Texture2D, this.Handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);
			GL.TexImage2D(TextureTarget.Texture2D, 0, internalformat, width, height, 0,
				format, type, (IntPtr)0);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if(dispose_unmanaged_objects)
			{
				GL.DeleteTexture(this.Handle);
				return true;
			}
			return false;
		}

		public void Bind()
		{
			GL.BindTexture(TextureTarget.Texture2D, this.Handle);
		}

		public void Unbind()
		{
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}
	}
}
