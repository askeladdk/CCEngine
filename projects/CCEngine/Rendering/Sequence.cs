using System;
using System.Collections.Generic;

namespace CCEngine.Rendering
{
	public class Sequence
	{
		private class SequenceEntry
		{
			public readonly int offset;
			public readonly int stride;
			public readonly int framesPerFacing;
			public readonly int facings;
			public readonly int rate;

			public SequenceEntry(int offset, int stride, int framesPerFacing, int facings, int rate)
			{
				this.offset = offset;
				this.stride = stride;
				this.framesPerFacing = framesPerFacing;
				this.facings = facings;
				this.rate = rate;
			}
		}

		private readonly Dictionary<string, SequenceEntry> sequences =
			new Dictionary<string, SequenceEntry>();

		public void Add(string animId, int offset, int stride, int framesPerFacing, int facings, int rate)
		{
			sequences[animId] = new SequenceEntry(offset, stride, framesPerFacing, facings, rate);
		}

		public IEnumerable<string> Animations
		{
			get { return sequences.Keys; }
		}

		public bool Contains(string animId)
		{
			return sequences.ContainsKey(animId);
		}

		public int GetNextFrame(int globalClock, int facing, string animId)
		{
			var seq = sequences[animId];
			return
				seq.offset
				+ seq.stride * Facing.Scale(facing, seq.facings)
				+ (globalClock / seq.rate) % seq.framesPerFacing;
		}
	}
}
