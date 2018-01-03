using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	internal class AudioVoice : Resource
	{
		private byte[] samples;
		private int source;
		private int[] buffers;
		private IAudioStream stream;
		private int priority;

		public AudioVoice()
		{
			this.samples = new byte[2048];
			this.source = AL.GenSource();
			this.buffers = AL.GenBuffers(3);
		}

		~AudioVoice()
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

		private void Buffer(int buffer, IAudioStream stream)
		{
			var len = stream.ReadSamples(samples);
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

		public void Play(IAudioStream stream, int priority)
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

			if(stream.Empty)
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

	public class AudioDevice : Resource
	{
		public event Action MusicFinished;

		private AudioContext context;
		private AudioVoice[] voices;

		public AudioDevice(int nvoices)
		{
			context = new AudioContext();
			context.MakeCurrent();
			voices = new AudioVoice[nvoices];
			for(var i = 0; i < voices.Length; i++)
				voices[i] = new AudioVoice();
		}

		~AudioDevice()
		{
			Dispose(false);
		}

		protected override bool Cleanup(bool dispose_unmanaged_objects)
		{
			if(dispose_unmanaged_objects)
			{
				foreach(var voice in voices)
					voice.Dispose();
				return true;
			}
			return false;
		}

		public void PlayMusic(IAudioSource music)
		{
			voices[0].Play(music.GetStream(), 0);
		}

		public void StopMusic()
		{
			voices[0].Stop();
		}

		public void Play(IAudioSource sound, int priority)
		{
			AudioVoice voice = null;
			for(var i = 1; i < voices.Length; i++)
				if(!voices[i].IsPlaying)
					voice = voices[i];
			if(voice == null)
				for(var i = 1; i < voices.Length; i++)
					if(priority >= voices[i].Priority)
						voice = voices[i];
			if(voice != null)
				voice.Play(sound.GetStream(), priority);
		}

		public void Stop()
		{
			for(var i = 1; i < voices.Length; i++)
				voices[i].Stop();
		}

		public void Update()
		{
			if(voices[0].Update())
				MusicFinished.Invoke();
			for(var i = 1; i < voices.Length; i++)
				voices[i].Update();
		}

		public static string VendorString
		{
			get
			{
				var vendor = AL.Get(ALGetString.Vendor);
				var version = AL.Get(ALGetString.Version);
				var renderer = AL.Get(ALGetString.Renderer);
				return "{0} {1} {2}".F(vendor, renderer, version);
			}
		}
	}
}