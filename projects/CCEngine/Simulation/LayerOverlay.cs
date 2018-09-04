using System;
using System.Drawing;

namespace CCEngine.Simulation
{
	public struct OverlayCell
	{
		public const byte Empty = 255;
		public const int OwnerMask = 31;
		public const int TypeMask = 224;

		private short state;
		private byte overlayId;
		private byte flags;

		public byte OverlayID { get => overlayId; }
		public short State { get => state; }
		public int Owner { get => (byte)(flags & OwnerMask); }
		public OverlayType Type { get => (OverlayType)(flags & TypeMask); }

		public OverlayCell(byte overlayId = Empty, OverlayType type = OverlayType.Empty,
			short state = 0, int owner = 0)
		{
			this.overlayId = overlayId;
			this.flags = (byte)((byte)type | (owner & OwnerMask));
			this.state = state;
		}

		private OverlayCell(byte overlayId, byte flags, short state)
		{
			this.overlayId = overlayId;
			this.flags = flags;
			this.state = state;
		}

		public OverlayCell SetState(short state)
		{
			return new OverlayCell(overlayId, flags, state);
		}
	}

	public class LayerOverlay
	{
		private OverlayCell[] layer = new OverlayCell[Constants.MapCellCount];

		public void Clear()
		{
			for(var i = 0; i < layer.Length; i++)
				layer[i] = new OverlayCell();
		}

		private void Initialise(Rectangle bounds)
		{
			for(var y = bounds.Top; y < bounds.Bottom; y++)
			{
				for(var x = bounds.Left; x < bounds.Right; x++)
				{
					var cell = new CPos(x, y);
					switch(layer[cell.CellId].Type)
					{
						case OverlayType.Wall:
							UpdateWall(cell);
							break;
						case OverlayType.ResourceCommon:
							InitialiseResourceCommon(cell);
							break;
						case OverlayType.ResourcePrecious:
							InitialiseResourcePrecious(cell);
							break;
					}
				}
			}
		}

		public void Load(Scenario scenario)
		{
			var cells = scenario.Cells;
			for(var i = 0; i < cells.Length; i++)
			{
				var type = OverlayType.Empty;
				var overlayId = cells[i].overlayId;
				var v = scenario.Theater.GetOverlay(overlayId);
				if(v != null)
					type = v.Type;
				layer[i] = new OverlayCell(overlayId, type);
			}

			Initialise(scenario.Bounds);
		}

		public OverlayCell this[CPos cell]
		{
			get => layer[cell.CellId];
		}

		/// True if two cells have the same type and overlay id.
		private bool IsSame(int here, int there)
		{
			var cell1 = layer[here];
			var cell2 = layer[there];
			return cell1.Type == cell2.Type && cell1.OverlayID == cell2.OverlayID;
		}

		/// Update the state of the wall.
		/// The state specifies the frame to display.
		private void UpdateWall(CPos cell)
		{
			ushort here = cell.CellId;
			short state = 0;
			// North
			if(IsSame(here, here - Constants.MapSize))
				state |= 1;
			// East
			if(IsSame(here, here + 1))
				state |= 2;
			// South
			if(IsSame(here, here + Constants.MapSize))
				state |= 4;
			// West
			if(IsSame(here, here - 1))
				state |= 8;
			layer[here] = layer[here].SetState(state);
		}

		/// Count the number of neighbours (0-4) of the given type.
		private int CountNeighbours(CPos cell, OverlayType type)
		{
			var here = cell.CellId;
			var neighbours = 0;
			// North
			if(layer[here - Constants.MapSize].Type == type)
				neighbours += 1;
			// East
			if(layer[here + 1].Type == type)
				neighbours += 1;
			// South
			if(layer[here + Constants.MapSize].Type == type)
				neighbours += 1;
			// West
			if(layer[here - 1].Type == type)
				neighbours += 1;
			return neighbours;
		}

		/// Set initial state of resources.
		private void InitialiseResourceCommon(CPos cell)
		{
			var neighbours = CountNeighbours(cell, OverlayType.ResourceCommon);
			var state = (short)Math.Max(3 * (1 + Math.Min(neighbours, 3)) - 1, 0);
			var here = cell.CellId;
			layer[here] = layer[here].SetState(state);
		}

		/// Set initial state of precious resources.
		private void InitialiseResourcePrecious(CPos cell)
		{
			var neighbours = CountNeighbours(cell, OverlayType.ResourcePrecious);
			var state = (short)Math.Min(neighbours, 3);
			var here = cell.CellId;
			layer[here] = layer[here].SetState(state);
		}

		/// Update the state of one particular cell.
		public void Update(CPos cell)
		{
			switch(layer[cell.CellId].Type)
			{
				case OverlayType.Wall:
					UpdateWall(cell);
					break;
			}
		}

		/// Update the state of all cells within bounds.
		public void Update(Rectangle bounds)
		{
			for(var y = bounds.Top; y < bounds.Bottom; y++)
			{
				for(var x = bounds.Left; x < bounds.Right; x++)
				{
					Update(new CPos(x, y));
				}
			}
		}

		public void Render(Map map, RenderArgs args)
		{
			var bounds = args.bounds;
			int screenY = bounds.ScreenTopLeft.Y + Constants.TileSizeHalf;
			for (int y = bounds.CellBounds.Top; y < bounds.CellBounds.Bottom; y++)
			{
				int screenX = bounds.ScreenTopLeft.X + Constants.TileSizeHalf;
				for (int x = bounds.CellBounds.Left; x < bounds.CellBounds.Right; x++)
				{
					var cpos = new CPos(x, y);
					var cell = layer[cpos.CellId];
					var ovrid = cell.OverlayID;
					if(ovrid != 255)
					{
						var ovrspr = map.Theater.GetOverlay(ovrid);
						var frame = 0;
						switch(cell.Type)
						{
							case OverlayType.Wall:
							case OverlayType.ResourceCommon:
							case OverlayType.ResourcePrecious:
								frame = cell.State;
								break;
						}
						args.renderer.Blit(ovrspr.Art, frame, screenX, screenY);
					}

					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}
		}
	}
}