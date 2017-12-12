using System;

namespace CCEngine.Simulation
{
	/// <summary>
	/// Map cell stores all information relating to a cell on the map.
	/// </summary>
	public class MapCell
	{
		// Ground layer
		private ushort tmpId;
		private ushort tmpIndex;
		private Land land;

		// Occupy layer
		private int occupyEntityId;

		public ushort TmpId { get { return tmpId; } }
		public ushort TmpIndex { get { return tmpIndex; } }
		public Land Land { get { return land; } }

		public int OccupyEntityId
		{
			get { return occupyEntityId; }
			set { occupyEntityId = value; }
		}

		public bool IsPassable(MovementZone mz)
		{
			return land.IsPassable(mz) && (occupyEntityId == 0);
		}

		public MapCell(ushort tmpid, ushort tmpidx, Land land)
		{
			this.tmpId = tmpid;
			this.tmpIndex = tmpidx;
			this.land = land;
		}

		public override string ToString()
		{
			return "{0}.{1}.{2} [{3}]".F(tmpId, tmpIndex, land, occupyEntityId);
		}
	}
}
