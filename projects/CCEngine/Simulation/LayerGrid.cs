namespace CCEngine.Simulation
{
	public struct GridCell
	{
		private int entityId;

		public int EntityID { get => entityId; }

		public bool IsPassable { get => entityId == 0; }

		public GridCell(int entityId = 0)
		{
			this.entityId = entityId;
		}
	}

	public class LayerGrid
	{
		private GridCell[] layer = new GridCell[Constants.MapCellCount];

		public void Clear()
		{
			for(var i = 0; i < layer.Length; i++)
				layer[i] = new GridCell();
		}

		public bool CanPlace(StructureGrid grid, CPos cell)
		{
			var cellid = cell.CellId;
			for (int i = 0; i < grid.Length; i++)
				if(!layer[cellid + grid[i]].IsPassable)
					return false;
			return true;
		}

		public void Place(StructureGrid grid, CPos cell, int entityId)
		{
			var cellid = cell.CellId;
			for (int i = 0; i < grid.Length; i++)
				layer[cellid + grid[i]] = new GridCell(entityId);
		}

		public GridCell this[CPos cell]
		{
			get => layer[cell.CellId];
		}
	}
}