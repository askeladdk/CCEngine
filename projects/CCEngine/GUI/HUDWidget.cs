using System;
using System.Drawing;
using System.Collections.Generic;
using CCEngine.Rendering;

namespace CCEngine.GUI
{
	/// This GUI widget shows and controls the main game HUD.
	public class HUDWidget : IWidget
	{
		public const int TabOptions = 0;
		public const int TabCredits = 1;
		public const int TabTimer = 2;
		public const int TabSidebar = 3;
		public const int TabCount = 4;

		public const int ButtonRepair = 0;
		public const int ButtonSell = 1;
		public const int ButtonMap = 2;
		public const int ButtonCount = 3;

		private static Rectangle[] tabRects = new Rectangle[] {
			new Rectangle(0, 7, 160, 14),
			new Rectangle(320, 7, 160, 14),
			new Rectangle(160, 7, 160, 14),
			new Rectangle(480, 7, 160, 14),
		};

		private static Rectangle[] buttonRects = new Rectangle[] {
			new Rectangle(498, 150, 34, 24),
			new Rectangle(543, 150, 34, 24),
			new Rectangle(588, 150, 34, 24),
		};

		private static int[] tabOrder = new int[] {
			TabOptions,
			TabTimer,
			TabCredits,
			TabSidebar,
		};

		private RegionWidget fieldWidget;
		private RegionWidget[] tabWidgets = new RegionWidget[TabCount];
		private RegionWidget[] btnWidgets = new RegionWidget[ButtonCount];
		private Sprite sradrfrm;
		private Sprite sideradr;
		private Sprite tabs;
		private Sprite side2;
		private Sprite side3;
		private Sprite[] sidebtn = new Sprite[ButtonCount];
		private Sprite powerbar;
		private bool[] tabPressed = new bool[TabCount];
		private bool[] tabHidden = new bool[TabCount];
		private bool[] btnPressed = new bool[ButtonCount];
		private bool[] btnDisabled = new bool[ButtonCount];

		public bool CanInteract { get => false; }
		public Rectangle Region { get => Rectangle.Empty; }

		public IEnumerable<IWidget> Children
		{
			get
			{
				yield return fieldWidget;
				for(var i = 0; i < TabCount; i++)
					if(!tabHidden[i])
						yield return tabWidgets[i];
				for(var i = 0; i < ButtonCount; i++)
					if(!btnDisabled[i])
						yield return btnWidgets[i];
			}
		}

		public bool IsTabPressed(int tab)
		{
			return tabPressed[tab];
		}

		public void SetTabVisibility(int tab, bool visible)
		{
			tabHidden[tab] = !visible;
		}

		public bool IsButtonPressed(int btn)
		{
			return btnPressed[btn];
		}

		public void SetButtonEnabled(int btn, bool enabled)
		{
			btnDisabled[btn] = !enabled;
		}

		private void OnInteraction_Tab(object sender, InteractionEventArgs e)
		{
			var g = Game.Instance;
			var iaction = e.Interaction;

			for(int i = 0; i < TabCount; i++)
			{
				if(tabWidgets[i] == sender)
				{
					tabPressed[i] =
						(iaction == Interaction.Enter || iaction == Interaction.Hold);
					return;
				}
			}
		}

		private void OnInteraction_Button(object sender, InteractionEventArgs e)
		{
			for(int i = 0; i < ButtonCount; i++)
			{
				if(!btnDisabled[i] && btnWidgets[i] == sender)
				{
					btnPressed[i] = (e.Interaction == Interaction.Enter);
					return;
				}
			}
		}

		public HUDWidget()
		{
			var g = Game.Instance;
			sradrfrm = g.LoadAsset<Sprite>("NRADRFRM.SHP");
			sideradr = g.LoadAsset<Sprite>("NATORADR.SHP");
			side2 = g.LoadAsset<Sprite>("SIDE2NA.SHP");
			side3 = g.LoadAsset<Sprite>("SIDE3NA.SHP");
			tabs = g.LoadAsset<Sprite>("TABS.SHP");
			powerbar = g.LoadAsset<Sprite>("POWERBAR.SHP");

			sidebtn[0] = g.LoadAsset<Sprite>("REPAIR.SHP");
			sidebtn[1] = g.LoadAsset<Sprite>("SELL.SHP");
			sidebtn[2] = g.LoadAsset<Sprite>("MAP.SHP");

			fieldWidget = new RegionWidget(0, 16, 480, 384);

			for(int i = 0; i < TabCount; i++)
			{
				tabWidgets[i] = new RegionWidget(tabRects[i]);
				tabWidgets[i].Interaction += OnInteraction_Tab;
			}

			for(int i = 0; i < ButtonCount; i++)
			{
				btnWidgets[i] = new RegionWidget(buttonRects[i]);
				btnWidgets[i].Interaction += OnInteraction_Button;
			}
		}

		public void OnInteraction(GUI gui, Interaction interaction)
		{
			throw new NotImplementedException();
		}

		public void Render(SpriteBatch batch)
		{
			// draw the top tabs
			batch.SetSprite(tabs);
			for(int i = 0; i < TabCount; i++)
			{
				if(tabHidden[i])
					continue;
				var ofs = tabPressed[i] ? 1 : 0;
				var tab = tabWidgets[i];
				batch.Render(2 * i + ofs, 0, tab.Region.Left + 80, 7);
			}

			// draw the sidebar
			batch.SetSprite(sradrfrm);
			batch.Render(1, 0, 480 + 80, 16 + 80);
			batch.SetSprite(sideradr);
			batch.Render(0, 0, 480 + 80, 16 + 70);
			batch.SetSprite(side2);
			batch.Render(0, 0, 480 + 80, 176 + 50);
			batch.SetSprite(side3);
			batch.Render(0, 0, 480 + 80, 276 + 62);
			batch.SetSprite(powerbar);
			batch.Render(0, 0, 480 + 10, 176 + 56);
			batch.Render(1, 0, 480 + 10, 288 + 56);

			// draw the sidebar buttons
			for(var i = 0; i < ButtonCount; i++)
			{
				var btn = btnWidgets[i];
				var ofs = 0;
				if(btnDisabled[i])
					ofs = 2;
				else if(btnPressed[i])
					ofs = 1;
				batch.SetSprite(sidebtn[i]);
				batch.Render(ofs, 0, btn.Region.X + 16, btn.Region.Y + 12);
			}
		}
	}
}