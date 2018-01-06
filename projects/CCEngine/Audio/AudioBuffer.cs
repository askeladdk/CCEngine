using System;
using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	public class AudioBuffer : IAudioSource
	{
		private class AudioBufferStream : BaseAudioStream
		{
			private AudioBuffer buffer;
			private int samplesOffset;

			public override int SampleRate { get => buffer.sampleRate; }
			public override ALFormat Format { get => buffer.format; }

			public override long Length { get => buffer.samples.Length - samplesOffset; }

			public AudioBufferStream(AudioBuffer buffer)
			{
				this.buffer = buffer;
				this.samplesOffset = 0;
			}

			public override int Read(byte[] output, int offset, int count)
			{
				if(samplesOffset + count > buffer.samples.Length)
					count = buffer.samples.Length - samplesOffset;
				Buffer.BlockCopy(buffer.samples, samplesOffset, output, offset, count);
				samplesOffset += count;
				return count;
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

		public BaseAudioStream GetStream()
		{
			return new AudioBufferStream(this);
		}
	}
}