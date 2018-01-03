using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Audio.OpenAL;
using CCEngine.Codecs;
using CCEngine.Audio;

namespace CCEngine.FileFormats
{
	public class AudFile
	{
		private const int FLAG_STEREO = 1;
		private const int FLAG_16BIT = 2;
		private const int TYPE_WS_ADPCM = 1;
		private const int TYPE_IMA_ADPCM = 99;
		private const uint CHUNK_MAGIC = 0x0000DEAF;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct AudHeader
		{
			public ushort sampleRate;
			public uint sizeFile;
			public uint sizeOutput;
			public byte flags;
			public byte type;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct ChunkHeader
		{
			public ushort sizeCompressed;
			public ushort sizeUncompressed;
			public uint magic;
		}

		public static IAudioSource ReadToBuffer(Stream s)
		{
			var hdr = s.ReadStruct<AudHeader>();
			var stereo = (hdr.flags & FLAG_STEREO) != 0;
			var bits16 = (hdr.flags & FLAG_16BIT) != 0;

			if(!bits16)
				throw new Exception("Not 16-bit audio");
			if(hdr.type != TYPE_IMA_ADPCM)
				throw new Exception("Not IMA ADPCM audio");

			var format = bits16
				? (stereo ? ALFormat.Stereo16 : ALFormat.Mono16)
				: (stereo ? ALFormat.Stereo8  : ALFormat.Mono8)
			;

			// decoding state
			int index = 0;
			short sample = 0;
			// output
			var samples = new byte[hdr.sizeOutput];
			var stream = new MemoryStream(samples);
			var writer = new BinaryWriter(stream);

			var remaining = samples.Length;

			while(remaining > 0)
			{
				var chkhdr = s.ReadStruct<ChunkHeader>();
				if(chkhdr.magic != CHUNK_MAGIC)
					throw new Exception("Invalid chunk magic");
				var src = s.ReadBytes(chkhdr.sizeCompressed);
				ImaAdpcm.Decode(writer, src, 0, src.Length, ref index, ref sample);
				remaining -= chkhdr.sizeUncompressed;
			}

			return new AudioBuffer(samples, hdr.sampleRate, format);
		}
	}

	public class AudLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			return AudFile.ReadToBuffer(handle.Open());
		}
	}
}