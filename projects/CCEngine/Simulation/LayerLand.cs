using System.Drawing;
using OpenTK.Graphics;
using CCEngine.Rendering;

namespace CCEngine.Simulation
{
	public struct LandCell
	{
		private ushort templateId;
		private byte index;
		private LandType landType;

		public ushort TemplateID { get => templateId; }
		public byte TemplateIndex { get => index; }
		public LandType LandType { get => landType; }

		public LandCell(ushort templateId = 0, byte index = 0, LandType landType = LandType.Clear)
		{
			this.templateId = templateId;
			this.index = index;
			this.landType = landType;
		}

		public override string ToString()
		{
			return "({0}, {1}, {2})".F(templateId, index, landType);
		}
	}

	public class LayerLand
	{
		private LandCell[] layer = new LandCell[Constants.MapCellCount];

		public void Clear()
		{
			for(var i = 0; i < layer.Length; i++)
				layer[i] = new LandCell();
		}

		public void Load(Scenario scenario)
		{
			var theater = scenario.Theater;
			var cells = scenario.Cells;
			for(var i = 0; i < cells.Length; i++)
			{
				var templateId = cells[i].templateId;
				var index = cells[i].templateIndex;
				var landtype = theater.GetTemplate(templateId).GetLandType(index);
				layer[i] = new LandCell(templateId, index, landtype);
			}
		}

		public LandCell this[CPos cell]
		{
			get => layer[cell.CellId];
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
					var cpos = CPos.FromXY(x, y);
					var color = (cpos == map.Highlight) ? Color4.LightBlue : Color4.White;
					var (tmp, tmpidx) = map.GetTemplate(cpos);
					args.renderer.Blit(tmp, tmpidx, screenX, screenY, color.ToArgb());
					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}
		}

	}
}