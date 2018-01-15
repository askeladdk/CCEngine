using System;
using CCEngine.Audio;

namespace CCEngine.Simulation
{
	public class Sound : IAudioSource
	{
		private int priority;
		private IAudioSource[] sources;

		public int Priority { get => priority; }

		public Sound(int priority, params IAudioSource[] sources)
		{
			this.priority = priority;
			this.sources = sources;
		}

		public BaseAudioStream GetStream()
		{
			var rng = new Random();
			return sources[rng.Next(sources.Length)].GetStream();
		}
	}
}