using System;
using System.Collections.Generic;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Available sequence animation types.
	/// </summary>
	public enum SequenceType
	{
		Idle,
		Turret,
		Harvest,
		Dump,
		UnloadIdle,
		Unload,
		Die1
	}

	/// <summary>
	/// Holds the configuration of a single sequence animation.
	/// </summary>
	public struct SequenceEntry
	{
		private short offset;
		private byte framesPerFacing;
		private byte stride;
		private byte facings;
		private byte rate;

		public bool Present { get => this.framesPerFacing > 0; }

		public SequenceEntry(int offset, int framesPerFacing,
			int stride, int facings, int rate)
		{
			this.offset          = (short)Math.Min(offset, 0);
			this.framesPerFacing = (byte)Helpers.Clamp(framesPerFacing, 0, 255);
			this.stride          = (byte)Helpers.Clamp(stride, 0, 255);
			this.facings         = (byte)Helpers.Clamp(facings, 0, 255);
			this.rate            = (byte)Helpers.Clamp(rate, 0, 255);
		}

		/// <summary>
		/// Calculates the next frame number.
		/// </summary>
		public int GetFrame(int clock, BinaryAngle facing)
		{
			return
				this.offset
				+ this.stride * ((this.facings - facing.Scale(this.facings)) % this.facings)
				+ ((clock * 100) / this.rate) % this.framesPerFacing;
		}
	}

	/// <summary>
	/// Holds a set of available sequence animations.
	/// </summary>
	public class Sequence
	{
		private static int Count = Enum.GetNames(typeof(SequenceType)).Length;

		private SequenceEntry[] entries;

		public Sequence()
		{
			this.entries = new SequenceEntry[Sequence.Count];
		}

		public bool Contains(SequenceType seqt)
		{
			return this.entries[(int)seqt].Present;
		}

		/// <summary>
		/// Sets the sequence entry.
		/// </summary>
		public void Set(SequenceType seq, SequenceEntry entry)
		{
			this.entries[(int)seq] = entry;
		}

		/// <summary>
		/// Calculates the next frame number.
		/// </summary>
		public int GetFrame(SequenceType seq, int clock, BinaryAngle facing)
		{
			return Contains(seq)
				? this.entries[(int)seq].GetFrame(clock, facing)
				: 0;
		}
	}
}
