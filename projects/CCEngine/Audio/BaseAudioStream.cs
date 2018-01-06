using System;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	public abstract class BaseAudioStream : Stream
	{
		public abstract int SampleRate { get; }

		public abstract ALFormat Format { get; }

		public override bool CanRead { get => true; }
		public override bool CanWrite { get => false; }
		public override bool CanSeek { get => false; }
		public override void Flush() { }

		public override void Write(byte[] data, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long where, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
	}
}