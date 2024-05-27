using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;
using System;
namespace Portraiture.PlatoUI
{
	internal sealed class PlatoUIGame : IMinigame
	{
		private int BackgroundPos;

		public PlatoUIGame(string id, UIElement element, bool drawMouse = true, bool quitOnESC = true, bool clone = false, Texture2D background = null, Color? backgroundColor = null, bool movingBackground = false)
		{
			Id = id;
			DrawMouse = drawMouse;
			QuitOnESC = quitOnESC;
			if (backgroundColor.HasValue)
				BackgroundColor = backgroundColor.Value;
			BackgroundIsMoving = movingBackground;
			Background = background;
			Id = id;
			BaseMenu = UIElement.GetContainer("CurrentMinigame");
			if (element != null)
				BaseMenu.Add(clone ? element.Clone().WithBase(BaseMenu) : element.WithBase(BaseMenu));

			UIElement.Viewportbase.UpdateBounds();
			BaseMenu.UpdateBounds();
		}
		public bool Quit { get; set; }
		public UIElement BaseMenu { get; set; }
		public Texture2D Background { get; set; }
		public Color BackgroundColor { get; set; } = Color.White;
		private bool BackgroundIsMoving { get; }
		public string Id { get; set; }

		public bool DrawMouse { get; set; }

		public bool QuitOnESC { get; set; }

		public bool DoMainGameUpdate { get; set; } = false;

		public bool OverrideFreeMouseMovement { get; set; } = false;

		public Point LastMouse { get; set; } = Point.Zero;

		public void changeScreenSize()
		{
			BaseMenu.UpdateBounds();
		}

		public bool doMainGameUpdates()
		{
			return DoMainGameUpdate;
		}

		public void draw(SpriteBatch b)
		{
			drawBackground(b);
			UIHelper.DrawElement(b, BaseMenu);
			if (DrawMouse)
				drawMouse(b);
		}

		public void leftClickHeld(int x, int y)
		{
			BaseMenu.PerformClick(new Point(x, y), false, false, true);
		}

		public string minigameId()
		{
			return Id;
		}

		public bool overrideFreeMouseMovement()
		{
			return OverrideFreeMouseMovement;
		}

		public void receiveEventPoke(int data)
		{

		}

		public void receiveKeyPress(Keys k)
		{
			BaseMenu.PerformKey(k, false);
		}

		public void receiveKeyRelease(Keys k)
		{
			BaseMenu.PerformKey(k, true);

			if (k == Keys.Escape && QuitOnESC)
				quitGame();
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			BaseMenu.PerformClick(new Point(x, y), false, false, false);
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
			BaseMenu.PerformClick(new Point(x, y), true, false, false);

		}

		public void releaseLeftClick(int x, int y)
		{
			BaseMenu.PerformClick(new Point(x, y), false, true, false);
		}

		public void releaseRightClick(int x, int y)
		{
			BaseMenu.PerformClick(new Point(x, y), true, true, false);
		}

		public bool tick(GameTime time)
		{
			BaseMenu.PerformUpdate(time);

			if (UIElement.DragElement != null)
			{
				Point m = new Point(Game1.getMouseX(), Game1.getMouseY());

				if (m != LastMouse)
				{
					LastMouse = m;
					BaseMenu.PerformMouseMove(m);
				}
			}

			return Quit;
		}

		public void unload()
		{

		}

		public bool forceQuit()
		{
			Quit = true;
			return true;
		}

		public void drawBackground(SpriteBatch b)
		{
			if (Background is not null)
			{
				if (BackgroundIsMoving)
				{
					if (BackgroundPos < 0 - Background.Width)
						BackgroundPos = 0;
					for (int x = BackgroundPos; x < Game1.viewport.Width + Background.Width * 2; x += Background.Width)
						for (int y = BackgroundPos; y < Game1.viewport.Height + Background.Width * 2; y += Background.Width)
							b.Draw(Background, new Vector2(x, y), BackgroundColor);
				}
				else
				{
					float scale = Math.Max(Game1.viewport.Width / Background.Width, Game1.viewport.Height / Background.Height);
					int x = (Game1.viewport.Width - Background.Width) / 2;
					int y = (Game1.viewport.Height - Background.Height) / 2;
					b.Draw(Background, new Rectangle(x, y, Math.Max((int)(Background.Width * scale), Game1.viewport.Width), Math.Max((int)(Background.Height * scale), Game1.viewport.Height)), BackgroundColor);
				}
			}
		}

		public void drawMouse(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.mouseCursor, 16, 16), Color.White, 0.0f, Vector2.Zero, (float)(4.0 + Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 1f);
		}

		public void quitGame()
		{
			Quit = true;
		}
	}
}
