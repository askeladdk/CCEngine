using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Graphics;
using CCEngine.Algorithms;
using CCEngine.FileFormats;
using CCEngine.Rendering;
using CCEngine.ECS;

namespace CCEngine.Simulation
{
	public class Map : IGrid<MovementZone>
	{
		private ObjectStore objectStore;
		private Rectangle bounds;
		private Theater theater;
		private CPos highlight;

		// map layers
		// 0 land
		// 1 overlays
		// 2 smudges
		// 3 structuregrids
		// 4 mobile units+infantry
		// 5 influence map

		private LayerLand land = new LayerLand();
		private LayerOverlay overlay = new LayerOverlay();
		private LayerGrid grid = new LayerGrid();

		public Rectangle Bounds { get => bounds; }
		public Theater Theater { get => theater; }

		public CPos Highlight
		{
			get => highlight;
			set => highlight = value;
		}

		public Map(ObjectStore objectStore)
		{
			this.objectStore = objectStore;
		}

		public void Load(Scenario scenario)
		{
			bounds = scenario.Bounds;
			theater = scenario.Theater;
			land.Load(scenario);
			overlay.Load(scenario);
			grid.Clear();
		}

		public void Clear()
		{
			land.Clear();
			overlay.Clear();
			grid.Clear();
		}

		public LandType GetLandType(CPos cell)
		{
			switch(overlay[cell].Type)
			{
				case OverlayType.Obstacle:
					return LandType.Rock;
				case OverlayType.Wall:
					return LandType.Wall;
				case OverlayType.ResourceCommon:
				case OverlayType.ResourcePrecious:
					return LandType.Resource;
				default:
					return land[cell].LandType;
			}
		}

		public (TmpFile template, int index) GetTemplate(CPos cell)
		{
			var x = land[cell];
			return (theater.GetTemplate(x.TemplateID), (int)x.TemplateIndex);
		}

		public bool CanPlace(StructureGrid grid, CPos cell)
		{
			var cellid = cell.CellId;
			for (int i = 0; i < grid.Length; i++)
			{
				var cpos = new CPos((ushort)(cellid + grid[i]));
				var landtype = GetLandType(cpos);
				var land = objectStore.GetLand(landtype);
				if(!land.IsBuildable || !this.grid[cpos].IsPassable)
					return false;
			}
			return true;
		}

		public void Place(CPos cell, Entity entity, StructureGrid grid)
		{
			this.grid.Place(cell, entity, grid);
		}

		public void Place(CPos cell, Entity entity)
		{
			this.grid.Place(cell, entity);
		}

		public void Unplace(CPos cell)
		{
			this.grid.Clear(cell);
		}

		public bool IsPassable(CPos cell, MovementZone mz)
		{
			var land = objectStore.GetLand(GetLandType(cell));
			return land.IsPassable(mz) && grid.IsPassable(cell);
		}

		private static (int dx, int dy, int cost)[] cellOffsets =
		{
			( 0, -1, 10),
			( 1, -1, 14),
			( 1,  0, 10),
			( 1,  1, 14),
			( 0,  1, 10),
			(-1,  1, 14),
			(-1,  0, 10),
			(-1, -1, 14),
		};

		IEnumerable<(CPos, int)> IGrid<MovementZone>.GetPassableNeighbors(MovementZone mz, CPos cpos)
		{
			foreach(var offset in cellOffsets)
			{
				var neighbor = cpos.Translate(offset.dx, offset.dy);
				if (bounds.Contains(neighbor) && IsPassable(neighbor, mz))
					yield return (neighbor, offset.cost);
			}
		}

		Land IGrid<MovementZone>.GetLandAt(CPos cell)
		{
			return objectStore.GetLand(GetLandType(cell));
		}

		public void Render(RenderArgs args)
		{
			land.Render(this, args);
			args.renderer.Flush();
			overlay.Render(this, args);
			args.renderer.Flush();
		}

		public void Update()
		{
			//overlay.Update(bounds);
		}
	}
}
