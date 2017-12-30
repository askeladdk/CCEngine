using System;
using System.Drawing;
using System.Collections.Generic;
using CCEngine.Rendering;

namespace CCEngine.GUI
{
	public class TopBarWidget : IWidget
	{
		public const int TabOptions = 0;
		public const int TabCredits = 1;
		public const int TabTimer = 2;
		public const int TabSidebar = 3;
		public const int TabCount = 4;

		private static Rectangle[] tabRegions = new Rectangle[] {
			new Rectangle(0, 7, 160, 14),
			new Rectangle(320, 7, 160, 14),
			new Rectangle(160, 7, 160, 14),
			new Rectangle(480, 7, 160, 14),
		};

		private static string[] tabText = new string[] {
			"Options",
			"0",
			"00:00",
			"Sidebar"
		};

		private RegionWidget[] tabWidgets = new RegionWidget[TabCount];
		private bool[] tabPressed = new bool[TabCount];
		private bool[] tabHidden = new bool[TabCount];
		private Sprite tabs;

		public bool CanInteract { get => false; }
		public Rectangle Region { get => Rectangle.Empty; }

		public IEnumerable<IWidget> Children
		{
			get
			{
				for(var i = 0; i < TabCount; i++)
					if(!tabHidden[i])
						yield return tabWidgets[i];
			}
		}

		public TopBarWidget()
		{
			var g = Game.Instance;
			tabs = g.LoadAsset<Sprite>("TABS.SHP");

			for(int i = 0; i < TabCount; i++)
			{
				tabWidgets[i] = new RegionWidget(tabRegions[i]);
				tabWidgets[i].Interaction += OnInteraction_Tab;
			}
		}

		public void SetTabVisibility(int tab, bool visible)
		{
			tabHidden[tab] = !visible;
		}

		public void OnInteraction(object sender, InteractionEventArgs e)
		{
			throw new NotImplementedException();
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

		public void Render(Renderer renderer)
		{
			var g = Game.Instance;
			var font = g.GetFont("12METFNT");
			for(int i = 0; i < TabCount; i++)
			{
				if(tabHidden[i])
					continue;
				var ofs = tabPressed[i] ? 1 : 0;
				var tab = tabWidgets[i];
				renderer.Blit(tabs, 2 * i + ofs, tab.Region.Left + 80, 7);
				renderer.Text(font, tabText[i], tab.Region.Left + 80, 0, true);
			}
		}
	}
}