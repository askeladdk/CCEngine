using OpenTK.Audio.OpenAL;
using CCEngine.Collections;

namespace CCEngine.Audio
{
	public partial class AudioDevice
	{
		private class Voice : Resource
		{
			private byte[] samples;
			private int source;
			private int[] buffers;
			private BaseAudioStream stream;
			private int priority;

			public Voice()
			{
				this.samples = new byte[2048];
				this.source = AL.GenSource();
				this.buffers = AL.GenBuffers(3);
			}

			~Voice()
			{
				Dispose(false);
			}

			protected override bool Cleanup(bool dispose_unmanaged_objects)
			{
				if(dispose_unmanaged_objects)
				{
					Stop();
					AL.DeleteBuffers(buffers);
					AL.DeleteSource(source);
					return true;
				}

				return false;
			}

			public int Priority { get => priority; }

			public bool IsPlaying
			{
				get => AL.GetSourceState(source) == ALSourceState.Playing;
			}

			private int BuffersProcessed()
			{
				var nprocessed = 0;
				AL.GetSource(source, ALGetSourcei.BuffersProcessed, out nprocessed);
				return nprocessed;
			}

			private void Buffer(int buffer, BaseAudioStream stream)
			{
				var len = stream.Read(samples, 0, samples.Length);
				if(len > 0)
					AL.BufferData(buffer, stream.Format, samples, len, stream.SampleRate);
			}

			public void Stop()
			{
				AL.SourceStop(source);
				var nprocessed = BuffersProcessed();
				if(nprocessed > 0)
					AL.SourceUnqueueBuffers(source, nprocessed);
				this.stream = null;
			}

			public void Play(BaseAudioStream stream, int priority)
			{
				Stop();
				for(var i = 0; i < buffers.Length; i++)
					Buffer(buffers[i], stream);
				AL.SourceQueueBuffers(source, buffers.Length, buffers);

				if(!IsPlaying)
					AL.SourcePlay(source);
				this.stream = stream;
				this.priority = priority;
			}

			public bool Update()
			{
				if(stream == null)
					return false;

				if(stream.Length == 0)
				{
					Stop();
					return true;
				}

				var nprocessed = BuffersProcessed();

				while(nprocessed-- > 0)
				{
					var buffer = AL.SourceUnqueueBuffer(source);
					Buffer(buffer, stream);
					AL.SourceQueueBuffer(source, buffer);
				}

				if(!IsPlaying)
					AL.SourcePlay(source);
				return false;
			}
		}
	}
}