using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using CCEngine.Codecs;
using CCEngine.FileFormats;
using CCEngine.Collections;
using CCEngine.Rendering;
using CCEngine.ECS;
using CCEngine.Algorithms;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Simulation
{
	public class Map : IGrid
	{
		private readonly MapCell[] cells;
		private readonly Rectangle bounds;
		private readonly Theater theater;
		private Registry registry;

		private Rectangle objectBounds;
		private CPos cellHighlight = new CPos(0, 0);
		private CPos[] pathHighlight;

		public Registry Registry { get { return registry; } }
		public Rectangle Bounds { get { return bounds; } }
		public Theater Theater { get { return theater; } }
		public CPos CellHighLight { set { this.cellHighlight = value; } }
		public CPos[] PathHighLight { set { this.pathHighlight = value; } }


		private Map(IConfiguration cfg)
		{
			this.registry = CreateRegistry();
			var theaterid = cfg.GetString("Map", "Theater");
			this.theater = Game.Instance.GetTheater(theaterid);
			this.bounds = ReadBounds(cfg);
			this.cells = ReadTiles(cfg);
			SpawnTerrains(cfg);
			SpawnUnits(cfg);
		}

		private Rectangle ReadBounds(IConfiguration cfg)
		{
			int mapx = cfg.GetInt("Map", "X");
			int mapy = cfg.GetInt("Map", "Y");
			int mapw = cfg.GetInt("Map", "Width");
			int maph = cfg.GetInt("Map", "Height");
			return new Rectangle(mapx, mapy, mapw, maph);
		}

		/// <summary>
		/// Create registry and attach processors.
		/// </summary>
		/// <returns></returns>
		private Registry CreateRegistry()
		{
			var registry = new Registry();
			PAnimation.Attach(registry);
			PRender.Attach(registry);
			return registry;
		}

		public MapCell GetCell(int cellId)
		{
			return cells[cellId];
		}

		public MapCell GetCell(int x, int y)
		{
			return this.GetCell(y * Constants.MapSize + x);
		}

		public bool IsCellPassable(MovementZone mz, CPos cpos)
		{
			return this.GetCell(cpos.X, cpos.Y).IsPassable(mz);
		}

		public void Update(float dt)
		{
			this.registry.Update(dt);
		}

		private bool TryPlace(int entityId)
		{
			CPlacement placement;
			if(!this.registry.TryGetComponent<CPlacement>(entityId, out placement))
				return false;

			var pose = this.registry.GetComponent<CPose>(entityId);
			var center = pose.CellLocation.CellId;

			var occupy = placement.OccupyGrid;
			for (int i = 0; i < occupy.Length; i++)
			{
				var tile = this.GetCell(center + occupy[i]);
				tile.OccupyEntityId = entityId;
			}

			return true;
		}

		#region IGrid

		private static Tuple<int, int, int>[] cellOffsets =
		{
			// dx, dy, cost
			Tuple.Create(-1, -1, 14),
			Tuple.Create( 0, -1, 10),
			Tuple.Create( 1, -1, 14),
			Tuple.Create(-1,  0, 10),
			Tuple.Create( 1,  1, 14),
			Tuple.Create( 0,  1, 10),
			Tuple.Create(-1,  1, 14),
			Tuple.Create( 1,  0, 10),
		};

		IEnumerable<Tuple<CPos, int>> IGrid.GetPassableNeighbors(MovementZone mz, CPos cpos)
		{
			foreach(var co in cellOffsets)
			{
				var neighbor = cpos.Translate(co.Item1, co.Item2);
				if (bounds.Contains(neighbor) && IsCellPassable(mz, neighbor))
					yield return Tuple.Create(neighbor, co.Item3);
			}
		}
		#endregion

		#region Rendering

		public Rectangle ObjectBounds
		{
			get { return this.objectBounds; }
		}

		public void Render(float dt)
		{
			var batch = Game.Instance.SpriteBatch;
			batch.SetPalette(this.theater.Palette);

			// Calculate viewable map area.
			Point screenTopLeft;
			Rectangle cellBounds;
			Game.Instance.Camera.GetRenderArea(out screenTopLeft, out cellBounds);

			// Only render objects that are within this map pixel rectangle.
			this.objectBounds = new Rectangle(
				Constants.TileSize * Math.Max(0, cellBounds.X - 0),
				Constants.TileSize * Math.Max(0, cellBounds.Y - 0),
				Constants.TileSize * Math.Min(cellBounds.Width + 0, Constants.MapSize),
				Constants.TileSize * Math.Min(cellBounds.Height + 0, Constants.MapSize)
			);

			// Crop rendering to HUD camera area.
			Game.Instance.ScissorCamera();
			GL.Enable(EnableCap.ScissorTest);

			// Render ground layer.
			batch.SetBlending(false);
			this.RenderGround(batch, cellBounds, screenTopLeft);

			// Render entities.
			batch.SetBlending(true);
			//this.RenderOccupyLayer(batch, cellBounds, screenTopLeft);
			this.registry.Render(dt);

			// Finish up.
			batch.Flush();
			batch.SetBlending(false);
			GL.Disable(EnableCap.ScissorTest);
		}

		public void RenderGround(SpriteBatch batch, Rectangle cellBounds, Point screenTopLeft)
		{
			int screenY = screenTopLeft.Y;
			for (int y = cellBounds.Top; y < cellBounds.Bottom; y++)
			{
				int screenX = screenTopLeft.X;
				for (int x = cellBounds.Left; x < cellBounds.Right; x++)
				{
					var cpos = new CPos(x, y);
					var cell = this.GetCell(x, y);
					var landtype = cell.Land.Type;
					bool highlight = false;

					if(cpos.Equals(this.cellHighlight))
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.LightBlue);
					}
					else if (this.pathHighlight != null && this.pathHighlight.Contains(cpos))
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.Blue);
					}
					else if (cell.OccupyEntityId != 0)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.Purple);
					}

					else if(landtype == LandType.Rock)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.Red);
					}
					else if (landtype == LandType.Rough)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.Yellow);
					}
					else if (landtype == LandType.Road)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.YellowGreen);
					}
					else if (landtype == LandType.Beach)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.LightSeaGreen);
					}
					else if (landtype == LandType.Water)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.LightGreen);
					}
					else if (landtype == LandType.River)
					{
						highlight = true;
						batch.SetColor(OpenTK.Graphics.Color4.Green);
					}
					batch
						.SetSprite(this.theater.GetTemplate(cell.TmpId))
						.Render(cell.TmpIndex, 0, screenX, screenY);
					if (highlight)
						batch.SetColor();
					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}
		}

#if false
		public void RenderOccupyLayer(SpriteBatch batch, Rectangle cellBounds, Point screenTopLeft)
		{
			batch.SetColor(OpenTK.Graphics.Color4.Yellow);
			int screenY = screenTopLeft.Y;
			for (int y = cellBounds.Top; y < cellBounds.Bottom; y++)
			{
				int screenX = screenTopLeft.X;
				for (int x = cellBounds.Left; x < cellBounds.Right; x++)
				{
					var tile = this.GetTile(x, y);
					var entityId = tile.OccupyEntityId;

					if (entityId != 0)
					{
						var animation = this.registry.GetComponent<CAnimation>(entityId);

						batch
							.SetSprite(animation.Sprite)
							.RenderTile(animation.Frame, tile.OccupyTileId, screenX, screenY);
					}
					screenX += Constants.TileSize;
				}
				screenY += Constants.TileSize;
			}
			batch.SetColor();
		}
#endif
		
		#endregion

		#region Map Loading

		private void UnpackSection(IConfiguration cfg, string section, byte[] buffer, int ofs, int len)
		{
			if (!cfg.Contains(section))
				throw new Exception("[{0}] missing".F(section));
			var sb = string.Join(string.Empty, cfg.Enumerate(section).Select(x => x.Value));
			var bin = Convert.FromBase64String(sb);

			using (var br = new BinaryReader(new MemoryStream(bin)))
			{
				int max = ofs + len;
				while (ofs < max)
				{
					int cmpsz = br.ReadUInt16(); // compressed size
					int ucmpsz = br.ReadUInt16(); // uncompressed size
					var chunk = br.ReadBytes(cmpsz);
					Format80.Decode(chunk, 0, buffer, ofs, cmpsz);
					ofs += ucmpsz;
				}
			}
		}

		private MapCell[] ReadTiles(IConfiguration cfg)
		{
			var theater = this.theater;
			byte[] mapdata = new byte[4 * Constants.MapCellCount];

			UnpackSection(cfg, "MapPack", mapdata, 0, 3 * Constants.MapCellCount);
			UnpackSection(cfg, "OverlayPack", mapdata, 3 * Constants.MapCellCount, Constants.MapCellCount);

			var rng = new Random(0);
			var cells = new MapCell[Constants.MapCellCount];

			for (int i = 0; i < cells.Length; i++)
			{
				int x = i % Constants.MapSize;
				int y = i / Constants.MapSize;
				byte tmpid0  = mapdata[2 * i + 0];
				byte tmpid1  = mapdata[2 * i + 1];
				ushort tmpid = (ushort)(tmpid1 << 8 | tmpid0);
				byte tmpidx  = mapdata[2 * Constants.MapCellCount + i];
				//byte ovridx  = mapdata[3 * Constants.MapCells + i];
				var tmp = theater.GetTemplate(tmpid);
				// Replace invalid tiles with the clear tile.
				if (tmpid == 0 || tmpid == 65535 || tmp == null || tmpidx >= tmp.FrameCount)
				{
					tmpid = 255;
					tmp = theater.GetTemplate(tmpid);
				}
				if (tmpid == 255)
					tmpidx = (byte)rng.Next(tmp.FrameCount);
				var type = tmp.GetLandType(tmpidx);
				var land = Game.Instance.GetLand(type);
				cells[i] = new MapCell(tmpid, tmpidx, land);
			}

			return cells;
		}

		private void SpawnTerrains(IConfiguration cfg)
		{
			// [TERRAINS]
			// cell=terrainId
			foreach(var kv in cfg.Enumerate("TERRAIN"))
			{
				var cell = ushort.Parse(kv.Key);
				var trnId = kv.Value;

				var bp = Game.Instance.GetTerrainType(trnId, this.theater);

				if(bp != null)
				{
					var attrs = new AttributeTable
					{
						{"Pose.Location", new MPos(cell)},
					};
					//var entityId = this.registry.Spawn(bp, attrs);
					//this.TryPlace(entityId);
					Game.Instance.SendMessage(new MsgSpawnEntity(trnId, TechnoType.Terrain, attrs));
				}
			}
		}

		private void SpawnUnits(IConfiguration cfg)
		{
			// [UNITS]
			// num=country,type,health,cell,facing,action,trig
			if (!cfg.Contains("UNITS"))
				return;
			foreach (var kv in cfg.Enumerate("UNITS"))
			{
				var data = kv.Value.Split(',');
				if (data.Length < 7)
					continue;
				var technoId = data[1];
				var cell = ushort.Parse(data[3]);
				var facing = int.Parse(data[4]);
				var bp = Game.Instance.GetUnitType(technoId);
				if(bp != null)
				{
					var attrs = new AttributeTable
					{
						{"Pose.Location", new MPos(cell)},
						{"Pose.Facing", facing},
					};
					//Game.Instance.Log(Logger.DEBUG, "Spawn {0}\n{1}", technoId, attrs);
					//this.registry.Spawn(bp, attrs);
					Game.Instance.SendMessage(new MsgSpawnEntity(technoId, TechnoType.Vehicle, attrs));
				}
			}
		}

		public void HandleMessage(IMessage msg)
		{
			MsgSpawnEntity spawn;

			if(msg.Is<MsgSpawnEntity>(out spawn))
			{
				this.SpawnEntity(spawn.ID, spawn.TechnoType, spawn.Table);
			}
		}

		private void SpawnEntity(string id, TechnoType type, IAttributeTable table)
		{
			var g = Game.Instance;
			Blueprint bp;

			switch(type)
			{
				case TechnoType.Vehicle:
					bp = g.GetUnitType(id);
					Registry.Spawn(bp, table);
					break;
				case TechnoType.Terrain:
					bp = g.GetTerrainType(id, this.theater);
					var eid = Registry.Spawn(bp, table);
					this.TryPlace(eid);
					break;
				default:
					throw new Exception("Not implemented");
			}

			g.Log("Spawned {0}\n{1}", id, table);
		}

		public static Map Read(Stream stream, AssetManager assets)
		{
			var cfg = IniFile.Read(stream);
			return new Map(cfg);
#if false
			// Read Map section.
			int mapx = cfg.GetInt("Map", "X");
			int mapy = cfg.GetInt("Map", "Y");
			int mapw = cfg.GetInt("Map", "Width");
			int maph = cfg.GetInt("Map", "Height");
			Rectangle bounds = new Rectangle(mapx, mapy, mapw, maph);

			var theater_name = cfg.GetString("Map", "Theater");
			var theater = w.GetTheater(theater_name);
			if (theater == null)
				throw new Exception("Invalid theater {0}".F(theater_name));

			var tiles = ReadTiles(cfg, theater);
			var terrains = ReadTerrains(w, cfg, theater);

			return new Map(tiles, bounds, theater, terrains);
#endif
			}

		#endregion
	}

	public class MapLoader : IAssetLoader
	{
		public object Load(AssetManager assets, VFS.VFSHandle handle, object parameters)
		{
			using(var stream = handle.Open())
				return Map.Read(stream, assets);
		}
	}
}
