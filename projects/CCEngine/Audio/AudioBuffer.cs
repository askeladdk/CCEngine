using System;
using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	public class AudioBuffer : IAudioSource
	{
		private class AudioBufferStream : IAudioStream
		{
			private AudioBuffer buffer;
			private int offset;

			public int SampleRate { get => buffer.sampleRate; }
			public ALFormat Format { get => buffer.format; }
			public bool Empty { get => (buffer.samples.Length - offset) == 0; }

			public AudioBufferStream(AudioBuffer buffer)
			{
				this.buffer = buffer;
				this.offset = 0;
			}

			public int ReadSamples(byte[] samples)
			{
				var nsamples = samples.Length;
				if(offset + nsamples > buffer.samples.Length)
					nsamples = buffer.samples.Length - offset;
				buffer.ReadSamples(samples, 0, offset, nsamples);
				offset += nsamples;
				return nsamples;
			}
		}

		private byte[] samples;
		private int sampleRate;
		private ALFormat format;

		public AudioBuffer(byte[] samples, int sampleRate, ALFormat format)
		{
			this.samples = samples;
			this.sampleRate = sampleRate;
			this.format = format;
		}

		public void ReadSamples(byte[] dst, int dstpos, int srcpos, int count)
		{
			Array.Copy(samples, srcpos, dst, dstpos, count);
		}

		public IAudioStream GetStream()
		{
			return new AudioBufferStream(this);
		}
	}
}