using CCEngine.Audio;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	public class Music : IAudioSource
	{
		private string name;
		private int duration;
		private string scoreid;

		public string Name { get => name; }

		public int Duration { get => duration; }

		public Music(string name, int duration, string scoreid)
		{
			this.name = name;
			this.duration = duration;
			this.scoreid = scoreid;
		}

		public BaseAudioStream GetStream()
		{
			var source = Game.Instance.LoadAsset<IAudioSource>("{0}.AUD".F(scoreid),
				false, AudFileType.Streamed);
			return source?.GetStream();
		}
	}
}
