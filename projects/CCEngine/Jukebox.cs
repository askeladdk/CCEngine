using System;
using System.Linq;
using System.Collections.Generic;
using CCEngine.Simulation;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.ECS;
using CCEngine.Audio;

namespace CCEngine
{
	public class Jukebox
	{
		private List<Music> themes = new List<Music>();
		private AudioDevice device;

		public Jukebox(AudioDevice device, IConfiguration cfg)
		{
			this.device = device;
			foreach(var kv in cfg.Enumerate("Scores"))
			{
				var scoreid = kv.Value;
				var name = cfg.GetString(scoreid, "Name");
				var duration = cfg.GetInt(scoreid, "Duration");
				this.themes.Add(new Music(name, duration, scoreid));
			}
		}

		public void PlayRandom()
		{
			var rng = new Random();
			var mus = themes[rng.Next(themes.Count)];
			device.PlayMusic(mus);
		}
	}
}
