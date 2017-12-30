using System;
using System.Drawing;
using System.Collections.Generic;
using CCEngine.Rendering;

namespace CCEngine.GUI
{
	public class SideBarWidget : IWidget
	{
		public const int ButtonRepair = 0;
		public const int ButtonSell = 1;
		public const int ButtonMap = 2;
		public const int ButtonCount = 3;

		private static Rectangle[] buttonRegions = new Rectangle[] {
			new Rectangle(498, 150, 34, 24),
			new Rectangle(543, 150, 34, 24),
			new Rectangle(588, 150, 34, 24),
		};

		private RegionWidget[] btnWidgets = new RegionWidget[ButtonCount];
		private Sprite sradrfrm;
		private Sprite sideradr;
		private Sprite side2;
		private Sprite side3;
		private Sprite[] sidebtn = new Sprite[ButtonCount];
		private Sprite powerbar;
		private bool[] btnPressed = new bool[ButtonCount];
		private bool[] btnDisabled = new bool[ButtonCount];

		public bool CanInteract { get => false; }

		public Rectangle Region { get => Rectangle.Empty; }

		public IEnumerable<IWidget> Children
		{
			get
			{
				for(var i = 0; i < ButtonCount; i++)
					if(!btnDisabled[i])
						yield return btnWidgets[i];
			}
		}

		public SideBarWidget()
		{
			var g = Game.Instance;
			sradrfrm = g.LoadAsset<Sprite>("NRADRFRM.SHP");
			sideradr = g.LoadAsset<Sprite>("NATORADR.SHP");
			side2 = g.LoadAsset<Sprite>("SIDE2NA.SHP");
			side3 = g.LoadAsset<Sprite>("SIDE3NA.SHP");
			powerbar = g.LoadAsset<Sprite>("POWERBAR.SHP");

			sidebtn[0] = g.LoadAsset<Sprite>("REPAIR.SHP");
			sidebtn[1] = g.LoadAsset<Sprite>("SELL.SHP");
			sidebtn[2] = g.LoadAsset<Sprite>("MAP.SHP");

			for(int i = 0; i < ButtonCount; i++)
			{
				btnWidgets[i] = new RegionWidget(buttonRegions[i]);
				btnWidgets[i].Interaction += OnInteraction_Button;
			}
		}

		public void OnInteraction(object sender, InteractionEventArgs e)
		{
			throw new NotImplementedException();
		}

		public bool IsButtonPressed(int btn)
		{
			return btnPressed[btn];
		}

		public void SetButtonEnabled(int btn, bool enabled)
		{
			btnDisabled[btn] = !enabled;
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

		public void Render(Renderer renderer)
		{
			renderer.Blit(sradrfrm, 1, 480 + 80, 16 + 80);
			renderer.Blit(sideradr, 0, 480 + 80, 16 + 70);
			renderer.Blit(side2, 0, 480 + 80, 176 + 50);
			renderer.Blit(side3, 0, 480 + 80, 276 + 62);
			renderer.Blit(powerbar, 0, 480 + 10, 176 + 56);
			renderer.Blit(powerbar, 1, 480 + 10, 288 + 56);

			// draw the sidebar buttons
			for(var i = 0; i < ButtonCount; i++)
			{
				var btn = btnWidgets[i];
				var ofs = 0;
				if(btnDisabled[i])
					ofs = 2;
				else if(btnPressed[i])
					ofs = 1;
				renderer.Blit(sidebtn[i], ofs, btn.Region.X + 16, btn.Region.Y + 12);
			}
		}
	}
}