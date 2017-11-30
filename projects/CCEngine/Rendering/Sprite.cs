using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;

namespace CCEngine.Rendering
{
	public class Sprite
	{
		private static byte[] Arrange(byte[][] frames, int frame_w, int frame_h,
			int tile_w, int tile_h, int image_nw, int image_nh, out Vector2 image_size)
		{
			byte[] buffer = new byte[tile_w * tile_h * image_nw * image_nh];
			int image_w = image_nw * tile_w;
			int image_h = image_nh * tile_h;

			for (int j = 0; j < frames.Length; j++)
			{
				int x = j % image_nw;
				int y = j / image_nw;
				int offset = (y * image_nw * tile_h + x) * tile_w;
				for (int h = 0; h < frame_h; h++)
				{
					Array.Copy(frames[j], h * frame_w, buffer, offset + h * image_w, frame_w);
				}
			}

			image_size = new Vector2(image_w, image_h);
			return buffer;
		}

		private static byte[] Arrange(byte[][] frames, int frame_w, int frame_h,
			bool power_of_two, out Vector2 image_size)
		{
			// nr frames in width and height
			int nframes = frames.Length;
			int image_nw, image_nh;

			// width and height of the tile where the frame will be copied to
			int tile_w = frame_w;
			int tile_h = frame_h;

			if(power_of_two)
			{
				Helpers.FactorPowerOfTwo(Helpers.NextPowerOfTwo(nframes), out image_nw, out image_nh);
				tile_w = Helpers.NextPowerOfTwo(tile_w);
				tile_h = Helpers.NextPowerOfTwo(tile_h);
			}
			else
			{
				Helpers.FactorPowerOfTwo(nframes, out image_nw, out image_nh);
			}

			return Arrange(frames, frame_w, frame_h, tile_w, tile_h, image_nw, image_nh, out image_size);
		}

		public static Vector4 DefaultRegion = new Vector4(0, 0, 1, 1);

		private readonly Vector2 image_pixels;
		private readonly Vector2 frame_pixels;
		//private readonly Vector4[] frame_regions;
		private readonly Palette palette;
		private readonly int nframes;
		private byte[] buffer;
		private Texture texture;
		private int nframes_w;
		private float frame_texw;
		private float frame_texh;

		public int FrameCount { get { return nframes; } }
		public Vector2 ImagePixels { get { return image_pixels; } }
		public Vector2 FramePixels { get { return frame_pixels; } }
		public Palette Palette { get { return palette; } }
		public byte[] Buffer { get { return buffer; } }
		public bool IsBuffered { get { return buffer != null; } }

		/*
		private Vector4[] MakeRegions(int nframes, int nframes_w, float xratio, float yratio)
		{
			Vector4[] regions = new Vector4[nframes];
			for (int i = 0; i < nframes; i++)
			{
				int x = i % nframes_w;
				int y = i / nframes_w;
				regions[i] = new Vector4(x * xratio, y * yratio, xratio, yratio);
			}
			return regions;
		}
		*/

		public Sprite(byte[] frame, int frame_w, int frame_h, Palette palette = null)
		{
			this.nframes     = 1;
			this.palette     = palette;
			this.frame_pixels = new Vector2(frame_w, frame_h);
			this.image_pixels = new Vector2(frame_w, frame_h);
			this.buffer      = frame;
			this.nframes_w   = 1;
			this.frame_texw  = 1;
			this.frame_texh  = 1;
		}

		public Sprite(byte[][] frames, int frame_w, int frame_h, Palette palette = null)
		{
			this.nframes     = frames.Length;
			this.palette     = palette;
			this.frame_pixels = new Vector2(frame_w, frame_h);
			this.buffer      = Arrange(frames, frame_w, frame_h, false, out this.image_pixels);
			this.nframes_w   = (int)(image_pixels.X / frame_pixels.X);
			this.frame_texw  = frame_pixels.X / image_pixels.X;
			this.frame_texh  = frame_pixels.Y / image_pixels.Y;
		}

		public Sprite(byte[][] frames, int frame_w, int frame_h,
			int image_nw, int image_nh, Palette palette = null)
		{
			this.nframes     = frames.Length;
			this.palette     = palette;
			this.frame_pixels = new Vector2(frame_w, frame_h);
			this.buffer      = Arrange(frames, frame_w, frame_h, frame_w, frame_h,
				image_nw, image_nh, out this.image_pixels);
			this.nframes_w   = (int)(image_pixels.X / frame_pixels.X);
			this.frame_texw  = frame_pixels.X / image_pixels.X;
			this.frame_texh  = frame_pixels.Y / image_pixels.Y;
		}

		public Texture ToTexture(bool discard_buffer = false)
		{
			if (IsBuffered && texture == null)
			{
				texture = new Texture(buffer, (int)image_pixels.X, (int)image_pixels.Y,
					PixelType.UnsignedByte,
					PixelFormat.Red,
					PixelInternalFormat.R8,
					TextureMinFilter.Nearest);
			}
			if (discard_buffer)
				buffer = null;
			return texture;
		}

#if false
		public Bitmap ToBitmap(Palette palette = null)
		{
			if (!IsBuffered)
				return null;
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Bitmap bmp = new Bitmap((int)image_pixels.X, (int)image_pixels.Y, (int)image_pixels.X,
				System.Drawing.Imaging.PixelFormat.Format8bppIndexed, handle.AddrOfPinnedObject());
			handle.Free();

			if (palette == null)
				palette = this.palette;
			if (palette != null && palette.IsBuffered)
			{
				var bmppal = bmp.Palette;
				for (int i = 0; i < 256; i++)
				{
					bmppal.Entries[i] = Color.FromArgb((int)palette.Buffer[i]);
				}
				bmp.Palette = bmppal;
			}
			return bmp;
		}
#endif

		public Vector4 GetFrameRegion(int framenum)
		{
			framenum %= nframes;
			int x = framenum % this.nframes_w;
			int y = framenum / this.nframes_w;
			float texw = this.frame_texw;
			float texh = this.frame_texh;
			return new Vector4(x * texw, y * texh, texw, texh);
		}

		public Vector4 GetFrameRegion(int framenum, int tilenum)
		{
			framenum %= nframes;
			int x = framenum % this.nframes_w;
			int y = framenum / this.nframes_w;
			float texw = this.frame_texw;
			float texh = this.frame_texh;

			int ntiles_w = (int)(this.frame_pixels.X / (float)Constants.TileSize);
			int ntiles_h = (int)(this.frame_pixels.Y / (float)Constants.TileSize);
			int tilex = tilenum % ntiles_w;
			int tiley = tilenum / ntiles_w;
			float tiletexw = (float)Constants.TileSize / this.image_pixels.X;
			float tiletexh = (float)Constants.TileSize / this.image_pixels.Y;
			return new Vector4(
				x * texw + tilex * tiletexw,
				y * texh + tiley * tiletexh,
				tiletexw,
				tiletexh
			);
		}
	}

	public class SpriteLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			string filename = handle.FileName;
			using (var stream = handle.Open())
				if (filename.EndsWith(".CPS"))
					return CpsFile.Read(stream);
				else if (filename.EndsWith(".PCX"))
					return PcxFile.Read(stream);
				else
					return ShpFile.Read(stream);
		}
	}
}
