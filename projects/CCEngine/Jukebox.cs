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
				var normal = cfg.GetBool(scoreid, "Normal");
				this.themes.Add(new Music(name, duration, scoreid, normal));
			}
		}

		public void PlayRandom()
		{
			var rng = new Random();
			while(true)
			{
				var mus = themes[rng.Next(themes.Count)];
				if(mus.Normal)
				{
					device.PlayMusic(mus);
					break;
				}
			}
		}
	}
}
