using System;
using System.IO;

namespace CCEngine.Collections
{
	/// Ring buffer conceptually provides a finite window into an infinite byte stream.
	/// Writing to the buffer expands the window up to the capacity. When the capacity
	/// is reached further writes will move the oldest elements out of view.
	/// Reading from the buffer also moves the oldest elements out of view by shrinking the window.
	public class RingBuffer : Stream
	{
		private byte[] buffer;
		private uint head;
		private uint tail;

		public int Capacity { get => buffer.Length; }

		public override long Length { get => head - tail; }

		/// Creates a new ring buffer.
		/// The capacity must be a power of two.
		public RingBuffer(int capacity)
		{
			if( (capacity & (capacity - 1)) != 0 )
				throw new ArgumentException("Capacity must be a power of two.");
			this.buffer = new byte[capacity];
			this.head = 0;
			this.tail = 0;
		}

		public void Clear()
		{
			this.head = 0;
			this.tail = 0;
		}

		public override void Write(byte[] data, int offset, int count)
		{
			var overflow = count > (Capacity - Length);
			var ncopied = 0;

			while(ncopied < count)
			{
				var mhead = (int)(head & (Capacity - 1));
				var num = Math.Min(Capacity - mhead, count - ncopied);
				Buffer.BlockCopy(data, offset + ncopied, buffer, mhead, num);
				ncopied += num;
				head += (uint)num;
			}

			if(overflow)
				tail = head - (uint)Capacity;
		}

		public override int Read(byte[] data, int offset, int count)
		{
			var ncopied = 0;
			count = Math.Min(count, (int)Length);
			while(ncopied != count)
			{
				var mtail = (int)(tail & (Capacity - 1));
				var num = Math.Min(Capacity - mtail, count - ncopied);
				Buffer.BlockCopy(buffer, mtail, data, offset + ncopied, num);
				ncopied += num;
				tail += (uint)num;
			}
			return count;
		}

		public override bool CanRead { get => true; }
		public override bool CanWrite { get => true; }
		public override bool CanSeek { get => false; }
		public override void Flush() { }

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