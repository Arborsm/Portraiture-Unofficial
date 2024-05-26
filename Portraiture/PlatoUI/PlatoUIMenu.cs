using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
namespace Portraiture.PlatoUI
{
    public sealed class PlatoUIMenu : IClickableMenu
    {
        private int BackgroundPos;

        private readonly float lastUIZoom;

        public PlatoUIMenu(string id, UIElement element, bool clone = false, Texture2D background = null, Color? backgroundColor = null, bool movingBackground = false)
            : base(0, 0, Game1.viewport.Width, Game1.viewport.Height)
        {
#if ANDROID
#else
            lastUIZoom = Game1.options.desiredUIScale;
            Game1.options.desiredUIScale = Game1.options.desiredBaseZoomLevel;
            PortraitureMod.helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
#endif

            if (backgroundColor.HasValue)
                BackgroundColor = backgroundColor.Value;
            BackgroundIsMoving = movingBackground;
            Background = background;
            Id = id;
            BaseMenu = UIElement.GetContainer("CurrentMenu");
            if (element != null)
                BaseMenu.Add(clone ? element.Clone().WithBase(BaseMenu) : element.WithBase(BaseMenu));

            BaseMenu.UpdateBounds();
        }
        public UIElement BaseMenu { get; set; }
        public Texture2D Background { get; set; }
        public Color BackgroundColor { get; set; } = Color.White;
        private bool BackgroundIsMoving { get; set; }

        public Point LastMouse { get; set; } = Point.Zero;

        private Action<SpriteBatch> BeforeDrawAction { get; set; } = null;
        private Action<SpriteBatch> AfterDrawAction { get; set; } = null;


        public string Id { get; set; }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
#if ANDROID
#else
            if (!(Game1.activeClickableMenu is PlatoUIMenu))
            {
                Game1.options.desiredUIScale = lastUIZoom;
                PortraitureMod.helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            }
#endif
        }

        public void ClearMenu(UIElement element = null)
        {
            BaseMenu.Clear(element);
        }

        public override void draw(SpriteBatch b)
        {
            BeforeDrawAction?.Invoke(b);
            drawBackground(b);
            UIHelper.DrawElement(b, BaseMenu);
            drawMouse(b);
            AfterDrawAction?.Invoke(b);
        }

        public override void drawBackground(SpriteBatch b)
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

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            xPositionOnScreen = 0;
            yPositionOnScreen = 0;
            width = newBounds.Width;
            height = newBounds.Height;

            UIElement.Viewportbase.UpdateBounds();
            BaseMenu.UpdateBounds();
        }

        public override void performHoverAction(int x, int y)
        {
            BaseMenu.PerformHover(new Point(x, y));
        }

        public override void update(GameTime time)
        {
            if (time.TotalGameTime.Ticks % 3 == 0)
                BackgroundPos--;

            Point m = new Point(Game1.getMouseX(), Game1.getMouseY());

            if (m != LastMouse)
            {
                LastMouse = m;
                BaseMenu.PerformMouseMove(m);
            }

            BaseMenu.PerformUpdate(time);
        }

        public override void receiveKeyPress(Keys key)
        {
            BaseMenu.PerformKey(key, false);
            base.receiveKeyPress(key);
        }

        public override void releaseLeftClick(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y), false, true, false);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            BaseMenu.PerformScroll(direction);
            base.receiveScrollWheelAction(direction);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            BaseMenu.PerformClick(new Point(x, y), false, false, false);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            BaseMenu.PerformClick(new Point(x, y), true, false, false);
        }

        public override void leftClickHeld(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y), false, false, true);
        }
    }
}
