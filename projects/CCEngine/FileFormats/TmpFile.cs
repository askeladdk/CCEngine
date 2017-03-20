using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using CCEngine.Rendering;
using OpenTK;

namespace CCEngine.FileFormats
{
	public enum TileTypes : byte
	{
		Clear  = 0,
		Beach  = 1,
		Rock   = 2,
		Road   = 3,
		Water  = 4,
		River  = 5,
		Rough  = 6,
		Ore    = 7,
		Wall   = 8,
	}

	public class TmpFile : Sprite
	{
		private static TileTypes[] nibble_to_types =
		{
			TileTypes.Clear,
			TileTypes.Clear,
			TileTypes.Clear,
			TileTypes.Clear,
			TileTypes.Clear,
			TileTypes.Clear,
			TileTypes.Beach,
			TileTypes.Clear,
			TileTypes.Rock,
			TileTypes.Road,
			TileTypes.Water,
			TileTypes.River,
			TileTypes.Clear,
			TileTypes.Clear,
			TileTypes.Rough,
			TileTypes.Clear,
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct TmpHeader
		{
			public ushort cell_pixw;
			public ushort cell_pixh;
			public ushort ncells;
			public ushort allocated;
			public ushort xdim;
			public ushort ydim;
			public   uint size;
			public   uint images_offset;
			public   uint palettes;
			public   uint remaps;
			public   uint transflag_offset;
			public   uint typemap_offset;
			public   uint index_offset;
		}

		private readonly TileTypes[] typemap;
		private readonly Vector2 size;

		public Vector2 CellSize { get { return size; } }

		private TmpFile(byte[][] cells, TileTypes[] celltypes)
			: base(cells, Constants.TileSize, Constants.TileSize)
		{
			this.typemap = celltypes;
			this.size = new Vector2(1, 1);
		}

		private TmpFile(byte[][] cells, TileTypes[] celltypes, int xdim, int ydim)
			: base(cells, Constants.TileSize, Constants.TileSize, xdim, ydim)
		{
			this.typemap = celltypes;
			this.size = new Vector2(xdim, ydim);
		}

		public TileTypes GetTerrainType(int idx)
		{
			return typemap[idx % typemap.Length];
		}

		public static TmpFile Read(Stream stream)
		{
			TmpHeader hdr = stream.ReadStruct<TmpHeader>();

			int ncells = hdr.ncells;
			bool single = hdr.xdim == 1 && hdr.ydim == 1;

			// type maps
			stream.Seek(hdr.typemap_offset, SeekOrigin.Begin);
			TileTypes[] typemap = new TileTypes[single ? 1 : ncells];
			for(int i = 0; i < typemap.Length; i++)
				typemap[i] = nibble_to_types[stream.ReadByte() & 0xF];

			// cell image data
			byte[][] cells = new byte[ncells][];

			stream.Seek(hdr.index_offset, SeekOrigin.Begin);
			byte[] index = stream.ReadBytes(ncells);
			int cellsz = Constants.TileSize * Constants.TileSize;

			for (int i = 0; i < index.Length; i++)
			{
				if(index[i] != 255)
				{
					stream.Seek(hdr.images_offset + index[i] * cellsz, SeekOrigin.Begin);
					cells[i] = stream.ReadBytes(cellsz);
				}
				else
				{
					cells[i] = new byte[cellsz];
				}
			}

			if (single)
				return new TmpFile(cells, typemap);
			else
				return new TmpFile(cells, typemap, hdr.xdim, hdr.ydim);
		}
	}

	public class TmpLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			using (var stream = handle.Open())
				return TmpFile.Read(stream);
		}
	}
}
