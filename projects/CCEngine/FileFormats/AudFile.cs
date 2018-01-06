using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Audio.OpenAL;
using CCEngine.Codecs;
using CCEngine.Audio;
using CCEngine.Collections;

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

		private class AudFileStream : BaseAudioStream, IAudioSource
		{
			private Stream rawStream;
			private ALFormat format;
			private int sampleRate;
			private int remaining;
			private int index;
			private short sample;
			private RingBuffer ring;

			public override ALFormat Format { get => format; }
			public override int SampleRate { get => sampleRate; }
			public override long Length { get => remaining; }

			public AudFileStream(Stream rawStream)
			{
				var hdr = rawStream.ReadStruct<AudHeader>();
				var stereo = (hdr.flags & FLAG_STEREO) != 0;
				var bits16 = (hdr.flags & FLAG_16BIT) != 0;

				if(!bits16)
					throw new Exception("Not 16-bit audio");
				if(hdr.type != TYPE_IMA_ADPCM)
					throw new Exception("Not IMA ADPCM audio");

				this.format = bits16
					? (stereo ? ALFormat.Stereo16 : ALFormat.Mono16)
					: (stereo ? ALFormat.Stereo8  : ALFormat.Mono8)
				;

				this.rawStream = rawStream;
				this.sampleRate = hdr.sampleRate;
				this.remaining = (int)hdr.sizeOutput;
				this.ring = new RingBuffer(4096);
			}

			private byte[] ReadNextChunk()
			{
				var chkhdr = this.rawStream.ReadStruct<ChunkHeader>();
				if(chkhdr.magic != CHUNK_MAGIC)
				{
					remaining = 0;
					return null;
				}

				byte[] samples = new byte[4 * chkhdr.sizeCompressed];
				var memstream = new MemoryStream(samples);

				ImaAdpcm.Decode(this.rawStream, memstream, chkhdr.sizeCompressed,
					ref index, ref sample);
				remaining = Math.Max(remaining - samples.Length, 0);
				return samples;
			}

			public override int Read(byte[] output, int offset, int count)
			{
				var total = 0;
				while(count > 0 && this.Length > 0)
				{
					if(ring.Length > 0)
					{
						var nread = ring.Read(output, offset, count);
						offset += nread;
						count -= nread;
						total += nread;
					}

					if(count > 0)
					{
						var chunk = ReadNextChunk();
						if(chunk != null)
							ring.Write(chunk, 0, chunk.Length);
					}
				}

				return total;
			}

			public BaseAudioStream GetStream()
			{
				return this;
			}
		}

		public static IAudioSource ReadToStream(Stream s)
		{
			return new AudFileStream(s);
		}

		public static IAudioSource ReadToBuffer(Stream s)
		{
			var samples = ReadToStream(s).GetStream();
			var buffer = new byte[samples.Length];
			samples.CopyTo(new MemoryStream(buffer));
			return new AudioBuffer(buffer, samples.SampleRate, samples.Format);
		}
	}

	public enum AudFileType
	{
		Buffered,
		Streamed,
	}

	public class AudLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			var type = AudFileType.Buffered;
			if(parameters != null)
				type = (AudFileType)parameters;
			switch(type)
			{
				case AudFileType.Buffered:
					return AudFile.ReadToBuffer(handle.Open());
				case AudFileType.Streamed:
					return AudFile.ReadToStream(handle.Open());
				default:
					throw new ArgumentException();
			}
		}
	}
}