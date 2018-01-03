using CCEngine.Audio;

namespace CCEngine.Simulation
{
	public class Music : IAudioSource
	{
		private string name;
		private int duration;
		private IAudioSource source;

		public string Name { get => name; }

		public int Duration { get => duration; }

		public Music(string name, int duration, IAudioSource source)
		{
			this.name = name;
			this.duration = duration;
			this.source = source;
		}

		public IAudioStream GetStream()
		{
			return source.GetStream();
		}
	}
}