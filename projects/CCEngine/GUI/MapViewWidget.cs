using System;
using System.Drawing;
using System.Collections.Generic;
using CCEngine.Simulation;
using CCEngine.Rendering;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.GUI
{
	public class MapViewWidget : IWidget
	{
		public event EventHandler<InteractionEventArgs> Interaction;

		public bool CanInteract { get => true; }
		public IEnumerable<IWidget> Children { get => null; }
		public Rectangle Region { get => new Rectangle(0, 16, 480, 384); }

		public void OnInteraction(object sender, InteractionEventArgs e)
		{
			Interaction?.Invoke(sender, e);
		}

		public MapViewWidget()
		{

		}

		private void RenderGround(Renderer renderer, Map map,
			Rectangle cellBounds, Point screenTopLeft)
		{
			int screenY = screenTopLeft.Y + Constants.TileSizeHalf;
			for (int y = cellBounds.Top; y < cellBounds.Bottom; y++)
			{
				int screenX = screenTopLeft.X + Constants.TileSizeHalf;
				for (int x = cellBounds.Left; x < cellBounds.Right; x++)
				{
					var cpos = new CPos(x, y);
					var cell = map.GetCell(cpos);
					var landtype = cell.Land.Type;

					var color = Color4.White;

					if(cpos == map.cellHighlight)
						color = Color4.LightBlue;
					else if (cell.OccupyEntityId != 0)
						color = Color4.Purple;
					else if(landtype == LandType.Rock)
						color = Color4.Red;
					else if (landtype == LandType.Rough)
						color = Color4.Yellow;
					else if (landtype == LandType.Road)
						color = Color4.YellowGreen;
					else if (landtype == LandType.Beach)
						color = Color4.LightSeaGreen;
					else if (landtype == LandType.Water)
						color = Color4.LightGreen;
					else if (landtype == LandType.River)
						color = Color4.Green;

					renderer.Blit(map.Theater.GetTemplate(cell.TmpId),
						cell.TmpIndex, screenX, screenY, color.ToArgb());
					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}
		}

		public void Render(Renderer renderer, float dt)
		{
			var g = Game.Instance;
			var map = g.Map;
			// Calculate viewable map area.
			Point screenTopLeft;
			Rectangle cellBounds;
			g.Camera.GetRenderArea(out screenTopLeft, out cellBounds);

			// Only render objects that are within this map pixel rectangle.
			var objectBounds = new Rectangle(
				Constants.TileSize * Math.Max(0, cellBounds.X),
				Constants.TileSize * Math.Max(0, cellBounds.Y),
				Constants.TileSize * Math.Min(cellBounds.Width, Constants.MapSize),
				Constants.TileSize * Math.Min(cellBounds.Height, Constants.MapSize)
			);

			// Crop rendering to HUD camera area.
			GL.Scissor(0, 0, 480, 384); // (0,0) is bottom left
			GL.Enable(EnableCap.ScissorTest);

			// Render ground layer.
			RenderGround(renderer, map, cellBounds, screenTopLeft);
			renderer.Flush();

			var renderArgs = new RenderArgs {
				alpha = dt,
				objectBounds = objectBounds,
			};

			g.Registry.Render(renderArgs);
			renderer.Flush();

			GL.Disable(EnableCap.ScissorTest);
		}
	}
}