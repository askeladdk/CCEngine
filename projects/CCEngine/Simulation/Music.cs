using CCEngine.Audio;
using CCEngine.FileFormats;

namespace CCEngine.Simulation
{
	public class Music : IAudioSource
	{
		private string name;
		private int duration;
		private string scoreid;
		private bool normal;

		public string Name { get => name; }

		public int Duration { get => duration; }

		public bool Normal { get => normal; }

		public Music(string name, int duration, string scoreid, bool normal)
		{
			this.name = name;
			this.duration = duration;
			this.scoreid = scoreid;
			this.normal = normal;
		}

		public BaseAudioStream GetStream()
		{
			var source = Game.Instance.LoadAsset<IAudioSource>("{0}.AUD".F(scoreid),
				false, AudFileType.Streamed);
			return source?.GetStream();
		}
	}
}
