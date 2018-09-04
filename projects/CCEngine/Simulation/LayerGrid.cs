using System;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public struct GridCell
	{
		private Entity entity;

		public Entity Entity { get => entity; }

		public bool IsPassable { get => entity.Equals(Entity.Invalid); }

		public GridCell(Entity entity)
		{
			this.entity = entity;
		}
	}

	public class LayerGrid
	{
		private GridCell[] layer = new GridCell[Constants.MapCellCount];

		public void Clear()
		{
			for(var i = 0; i < layer.Length; i++)
				layer[i] = new GridCell(Entity.Invalid);
		}

		public bool CanPlace(CPos cell, StructureGrid grid)
		{
			var cellid = cell.CellId;
			for (int i = 0; i < grid.Length; i++)
				if(!layer[cellid + grid[i]].IsPassable)
					return false;
			return true;
		}

		public void Place(CPos cell, Entity entity, StructureGrid grid)
		{
			var cellid = cell.CellId;
			for (int i = 0; i < grid.Length; i++)
				layer[cellid + grid[i]] = new GridCell(entity);
		}

		public void Place(CPos cell, Entity entity)
		{
			layer[cell.CellId] = new GridCell(entity);
		}

		public void Clear(CPos cell)
		{
			layer[cell.CellId] = new GridCell();
		}

		public bool IsPassable(CPos cell)
		{
			return layer[cell.CellId].IsPassable;
		}

		public GridCell this[CPos cell]
		{
			get => layer[cell.CellId];
		}
	}
}