using System;
using System.Collections.Generic;
using CCEngine.Rendering;

namespace CCEngine.Simulation
{
	public class SpriteArt
	{
		private Sprite sprite;
		//private Sprite cameo;
		private Sequence sequence;

		public Sprite Sprite { get { return sprite; } }
		//public Sprite Cameo { get { return cameo; } }
		public IEnumerable<string> Animations { get { return sequence.Animations; } }

		public int GetNextFrame(int globalClock, BinaryAngle facing, string animId)
		{
			return sequence.GetNextFrame(globalClock, facing, animId);
		}

		public SpriteArt(Sprite sprite, Sequence sequence)
		{
			this.sprite = sprite;
			this.sequence = sequence;
		}
	}
}
