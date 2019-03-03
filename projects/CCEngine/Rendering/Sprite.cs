using System;
//using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;

namespace CCEngine.Rendering
{
	public class Sprite
	{
		private static byte[] Arrange(byte[][] frames, int frame_w, int frame_h,
			int tile_w, int tile_h, int image_nw, int image_nh, out Size image_size)
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

			image_size = new Size(image_w, image_h);
			return buffer;
		}

		private static byte[] Arrange(byte[][] frames, int frame_w, int frame_h,
			bool power_of_two, out Size image_size)
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

		private Size image_size;
		private Size frame_size;
		private Palette palette;
		private int nframes;
		private Texture texture;
		private int nframes_w;
		private float frame_texw;
		private float frame_texh;

		public int FrameCount { get => nframes; }
		public Size Size { get => frame_size; }
		public Palette Palette { get => palette; }
		public Texture Texture { get => texture; }

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

		private static Texture MakeTexture(byte[] buffer, Size sz)
		{
			return new Texture(buffer, sz.Width, sz.Height,
				PixelType.UnsignedByte,
				PixelFormat.Red,
				PixelInternalFormat.R8,
				TextureMinFilter.Nearest);
		}

		public Sprite(byte[] frame, int frame_w, int frame_h, Palette palette = null)
		{
			this.nframes    = 1;
			this.palette    = palette;
			this.frame_size = new Size(frame_w, frame_h);
			this.image_size = new Size(frame_w, frame_h);
			this.nframes_w  = 1;
			this.frame_texw = 1;
			this.frame_texh = 1;
			this.texture    = MakeTexture(frame, image_size);
		}

		public Sprite(byte[][] frames, int frame_w, int frame_h, Palette palette = null)
		{
			var buffer = Arrange(frames, frame_w, frame_h, false, out this.image_size);
			this.texture    = MakeTexture(buffer, image_size);
			this.nframes    = frames.Length;
			this.palette    = palette;
			this.frame_size = new Size(frame_w, frame_h);
			this.nframes_w  = image_size.Width / frame_size.Width;
			this.frame_texw = (float)frame_size.Width / image_size.Width;
			this.frame_texh = (float)frame_size.Height / image_size.Height;
		}

		public Sprite(byte[][] frames, int frame_w, int frame_h,
			int image_nw, int image_nh, Palette palette = null)
		{
			var buffer = Arrange(frames, frame_w, frame_h, frame_w, frame_h,
				image_nw, image_nh, out this.image_size);
			this.texture    = MakeTexture(buffer, image_size);
			this.nframes    = frames.Length;
			this.palette    = palette;
			this.frame_size = new Size(frame_w, frame_h);
			this.nframes_w  = image_size.Width / frame_size.Width;
			this.frame_texw = (float)frame_size.Width / image_size.Width;
			this.frame_texh = (float)frame_size.Height / image_size.Height;
		}

#if false
		public Bitmap ToBitmap(Palette palette = null)
		{
			if (!IsBuffered)
				return null;
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Bitmap bmp = new Bitmap(image_pixels.Width, image_pixels.Width, image_pixels.Width,
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

			int ntiles_w = this.frame_size.Width / Constants.TileSize;
			int ntiles_h = this.frame_size.Height / Constants.TileSize;
			int tilex = tilenum % ntiles_w;
			int tiley = tilenum / ntiles_w;
			float tiletexw = (float)Constants.TileSize / this.image_size.Width;
			float tiletexh = (float)Constants.TileSize / this.image_size.Height;
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
