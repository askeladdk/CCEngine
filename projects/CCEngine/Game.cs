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

		// Simulation
		private int globalClock = 0;
		private Map map;

		// Misc
		private static Game instance;
		private Matrix4 projection;
		private SpriteBatch batch;
		private Camera camera;
		public MPos mousePos;

		public static Game Instance { get { return instance; } }

		public int GlobalClock { get { return this.globalClock; } }
		public Map Map { get { return this.map; } }

		public VFS.VFS VFS { get { return this.vfs; } }
		public Matrix4 Projection { get { return this.projection; } }
		public SpriteBatch SpriteBatch { get { return this.batch; } }
		public Camera Camera { get { return this.camera; } }

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
			//this.assets.RegisterLoader<World>(new WorldLoader());

			this.logger = new Logger(this, Logger.DEBUG);
			this.logic = new Logic.GameLogic(this);
			this.bus.Subscribe(this.logger);
			this.bus.Subscribe(this.logic);

			this.camera = new Camera(this.Width / (float)width);
		}

		public void Initialise()
		{
			this.batch = new SpriteBatch(4096);
			this.SetRules();
			this.LoadWorldData();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
		}

		protected override void OnResize(EventArgs e)
		{
			var dpiScale = this.camera.DpiScale;
			var unscaledw = (int)(ClientRectangle.Width / dpiScale);
			var unscaledh = (int)(ClientRectangle.Height / dpiScale);
			base.OnResize(e);
			GL.Viewport(ClientRectangle);
			this.projection = Matrix4.CreateOrthographicOffCenter(
				0, unscaledw, unscaledh, 0, -1, 1);
			this.camera.ViewPort = new System.Drawing.Rectangle(
				0,
				Constants.HUDTopBarHeight,
				unscaledw - Constants.HUDSideBarWidth,
				unscaledh
			);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			this.bus.ProcessMessages();
			if (!this.logic.Update((float)e.Time))
				this.Exit();
			this.globalClock++;

			this.Title = "CCEngine - FPS: {0}, Clock: {1}".F((int)this.UpdateFrequency, this.globalClock);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			this.logic.Render((float)e.Time);
			SwapBuffers();
		}

		protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			this.SendMessage(new MsgKeyDown(e));
		}

		protected override void OnMouseMove(OpenTK.Input.MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
			this.SendMessage(new MsgMouseMove(e));
		}

		protected override void OnMouseUp(OpenTK.Input.MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			this.SendMessage(new MsgMouseButton(e));
		}

		protected override void OnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			this.SendMessage(new MsgMouseButton(e));
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

		public void ScissorCamera()
		{
			var viewPort = this.camera.ViewPort;
			GL.Scissor(
				0,
				ClientRectangle.Height - viewPort.Height,
				viewPort.Width,
				viewPort.Height - viewPort.Y
			);
		}


		public T LoadAsset<T>(string filename, bool cache = true, object parameters = null)
			where T : class
		{
			return this.assets.Load<T>(filename, cache, parameters);
		}

		public void Log(int priority, string fmt, params object[] args)
		{
			this.SendMessage(new MsgLog(fmt.F(args), priority));
		}

		public void Log(string fmt, params object[] args)
		{
			this.SendMessage(new MsgLog(fmt.F(args), Logger.DEBUG));
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
				g.Log(2, GL.GetString(StringName.Vendor));
				g.Log(2, GL.GetString(StringName.Renderer));
				g.Log(2, GL.GetString(StringName.Version));
				g.Log(2, "Shader Language v{0}", GL.GetString(StringName.ShadingLanguageVersion));
				g.Log(2, "Maximum Texture Size {0}x{0}", GL.GetInteger(GetPName.MaxTextureSize));

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
