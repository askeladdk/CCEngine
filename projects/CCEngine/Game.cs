﻿using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CCEngine.FileFormats;
using CCEngine.VFS;
using CCEngine.Rendering;
using CCEngine.Simulation;
using CCEngine.Collections;

namespace CCEngine
{
	public partial class Game : GameWindow
	{
		// Asset management
		private VFS.VFS vfs;
		private AssetManager assets;

		// Messaging
		private MessageBus bus = new MessageBus();

		// Game systems
		private Logger logger;
		private Logic.GameLogic logic;
		private Renderer renderer;

		// Simulation
		private int globalClock = 0;
		private Map map;

		// Misc
		private static Game instance;
		private Camera camera;
		private Display display;
		private float interpolatedTime = 0;

		public static Game Instance { get => instance; }

		public int GlobalClock { get => this.globalClock; }
		public Map Map { get => this.map; }

		public VFS.VFS VFS { get => this.vfs; }
		public Renderer Renderer { get => this.renderer; }
		public Camera Camera { get => this.camera; }
		public Display Display { get => this.display; }
		public ECS.Registry Registry { get => this.map.Registry; }

		private Game(int width, int height,
			GameWindowFlags flags = GameWindowFlags.Default,
			VSyncMode vsync = VSyncMode.Off, int major = 3, int minor = 3)
			: base(width, height, GraphicsMode.Default, "CCEngine", flags,
				DisplayDevice.Default, major, minor, GraphicsContextFlags.ForwardCompatible)
		{
			Game.instance = this;
			this.vfs = new VFS.VFS();
			// Set up the asset loader.
			this.assets = new AssetManager(this.vfs);
			this.assets.RegisterLoader<IniFile>(new IniLoader());
			this.assets.RegisterLoader<Shader>(new ShaderLoader());
			this.assets.RegisterLoader<Palette>(new PaletteLoader());
			this.assets.RegisterLoader<Sprite>(new SpriteLoader());
			this.assets.RegisterLoader<TmpFile>(new TmpLoader());
			this.assets.RegisterLoader<Map>(new MapLoader());
			this.assets.RegisterLoader<FntFile>(new FontLoader());
			this.assets.RegisterLoader<BinFile>(new BinFLoader());
			//this.assets.RegisterLoader<World>(new WorldLoader());

			this.logger = new Logger(this, LogLevel.Debug);
			this.logic = new Logic.GameLogic(this);
			this.bus.Subscribe(this.logger);
			this.bus.Subscribe(this.logic);

			// Initialise camera and logical resolution.
			this.camera = new Camera();

			var cr = this.ClientRectangle;
			this.display = new Display(this.Width / (float)width,
				640, 400, ClientRectangle.ToSystemDrawing());

			var res = this.display.Resolution;
			this.camera.ViewPort = new System.Drawing.Rectangle(
				0,
				Constants.HUDTopBarHeight,
				res.Width - Constants.HUDSideBarWidth,
				res.Height - Constants.HUDTopBarHeight
			);
		}

		public void Initialise()
		{
			this.renderer = new Renderer(this.display);
			this.InitialiseGUI();
			this.SetRules();
			this.LoadWorldData();
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			this.bus.ProcessMessages();
			if (!this.logic.Update((float)e.Time))
				this.Exit();
			this.gui.Flip();
			this.globalClock++;

			this.Title = "CCEngine - FPS: {0}, Clock: {1}".F((int)this.UpdateFrequency, this.globalClock);
			this.interpolatedTime = 0;
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			var alpha = Helpers.Clamp(interpolatedTime / (float)UpdatePeriod, 0.0f, 1.0f);
			interpolatedTime += (float)e.Time;
			this.logic.Render(alpha);
			SwapBuffers();
		}

		public void SendMessage(IMessage message)
		{
			this.bus.SendMessage(message);
		}

#if false
		public Bitmap GrabScreenshot(bool flip = false)
		{
			if (GraphicsContext.CurrentContext == null)
				throw new GraphicsContextMissingException();

			Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
			System.Drawing.Imaging.BitmapData data = bmp.LockBits(this.ClientRectangle,
				System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, this.ClientSize.Width, this.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			bmp.UnlockBits(data);
			if (flip)
				bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
			return bmp;
		}
#endif

		public T LoadAsset<T>(string filename, bool cache = true, object parameters = null)
			where T : class
		{
			return this.assets.Load<T>(filename, cache, parameters);
		}

		public void Log(LogLevel priority, string fmt, params object[] args)
		{
			this.logger.Log(priority, fmt, args);
		}

		public void Log(string fmt, params object[] args)
		{
			this.logger.Log(LogLevel.Debug, fmt, args);
		}

		public void SetState(int state)
		{
			this.SendMessage(new MsgGotoState(state));
		}

		public void LoadMap(string mapfile)
		{
			var map = assets.Load<Map>(mapfile, false);
			if (map == null)
				throw new Exception("Map {0} not found!".F(mapfile));
			this.map = map;
			this.camera.SetBounds(this.map);
		}

		static void Main(string[] args)
		{
			IConfiguration config = null;

			using(var stream = new StreamReader("ccengine.ini"))
				config = IniFile.Read(stream.BaseStream);

			int screenw = config.GetInt("Video", "Width", 640);
			int screenh = config.GetInt("Video", "Height", 480);
			var wflag = config.GetBool("Video", "Fullscreen", false)
				? GameWindowFlags.Fullscreen
				: GameWindowFlags.FixedWindow;

			using (Game g = new Game(screenw, screenh, wflag, VSyncMode.Adaptive))
			{
				// Log some OpenGL stats
				g.Log(LogLevel.Info, GL.GetString(StringName.Vendor));
				g.Log(LogLevel.Info, GL.GetString(StringName.Renderer));
				g.Log(LogLevel.Info, GL.GetString(StringName.Version));
				g.Log(LogLevel.Info, "Shader Language v{0}", GL.GetString(StringName.ShadingLanguageVersion));
				g.Log(LogLevel.Info, "Maximum Texture Size {0}x{0}", GL.GetInteger(GetPName.MaxTextureSize));

				// Load the virtual file system.
				var roots = config.GetString("CCEngine", "Roots").Split(',');
				var mixver = (MixFileVersion)config.GetInt("CCEngine", "MixVersion");
				foreach(var root in roots)
					g.VFS.AddRoot(Path.GetFullPath(root));
				foreach (var mixfile in config.Enumerate("LoadOrder"))
					g.VFS.AddMix(mixfile.Value, mixver);

				// Load initial assets.
				g.Initialise();

				// Start the game!
				g.SetState(1);
				g.Run(Constants.FrameRate);
			}
		}
	}
}
