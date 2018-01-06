using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	public partial class AudioDevice : Resource
	{
		public event Action MusicFinished;

		private AudioContext context;
		private Voice[] voices;

		public AudioDevice(int nvoices)
		{
			context = new AudioContext();
			context.MakeCurrent();
			voices = new Voice[nvoices];
			for(var i = 0; i < voices.Length; i++)
				voices[i] = new Voice();
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
			Voice voice = null;
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
				MusicFinished?.Invoke();
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