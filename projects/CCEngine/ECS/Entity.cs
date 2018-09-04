using System;

namespace CCEngine.ECS
{
	public struct Entity : IEquatable<Entity>, IComparable<Entity>
	{
		private const uint EID_MASK = 0x00FFFFFF;
		private const uint GEN_MASK = 0xFF000000;
		private const int GEN_SHIFT = 24;

		private uint handle;

		public static readonly Entity Invalid = new Entity(0xffffffff);

		private Entity(uint handle)
		{
			this.handle = handle;
		}

		private Entity(int eid, byte gen)
		{
			this.handle = (uint)eid | (uint)(gen << GEN_SHIFT);
		}

		public int ID
		{
			get => (int)(handle & EID_MASK);
		}

		public byte Generation
		{
			get => (byte)((handle & GEN_MASK) >> GEN_SHIFT);
		}

		public bool Equals(Entity other)
		{
			return handle == other.handle;
		}

		public override bool Equals(object obj)
		{
			return Equals((Entity)obj);
		}

		public override int GetHashCode()
		{
			return handle.GetHashCode();
		}

		public override string ToString()
		{
			return "{0}:{1}".F(ID, Generation);
		}

		public int CompareTo(Entity that)
		{
			return (int)(this.handle - that.handle);
		}

		public static Entity Create(int eid, byte gen)
		{
			return new Entity(eid, gen);
		}
	}
}