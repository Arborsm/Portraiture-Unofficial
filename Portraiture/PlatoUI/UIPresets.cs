using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Portraiture.PlatoUI.Presets;
using StardewValley;
using System;
using System.Collections.Generic;
namespace Portraiture.PlatoUI
{
    public class UIPresets
    {

        public static UIElement GetColorPicker(Color oldColor, Action<Color> updateHandler = null, bool small = false)
        {
            int size = small ? 60 : 100;
            return PlatoUIColorPicker.getColorPickerMenu(oldColor, updateHandler, size);
        }

        public static UIElement GetColorPicker(List<Color> oldColors, Action<int, Color> updateHandler = null, bool small = false)
        {
            int index = 0;

            int size = small ? 60 : 100;

            UIElement container = UIElement.GetContainer("MultiPicker", 0, UIHelper.GetCentered(0, 0, size * 8, size * 4));

            UIElement currentColorPicker = GetColorPicker(oldColors[index], c =>
            {
                updateHandler?.Invoke(index, c);
                container.GetElementById("ColorCircle>" + index).Color = c;
            }, small);

            Func<UIElement, UIElement, Rectangle> cpPositioner = UIHelper.GetTopRight(0, 0, currentColorPicker.Bounds.Width, currentColorPicker.Bounds.Height);
            currentColorPicker.Positioner = cpPositioner;
            currentColorPicker.Z = 1;

            Texture2D circle = PyDraw.getCircle(size * 2, Color.White, Color.Transparent);

            for (int i = 0; i < oldColors.Count; i++)
            {
                int y = -size / 5 + i * (int)(size * 1.1f);
                UIElement colorContainer = UIElement.GetImage(UIHelper.DarkTheme, Color.White * 0.75f, "Color>" + i, 1f, i == index ? 3 : 0, UIHelper.GetTopLeft((int)((100 - size) * 0.5f), y, size, size)).WithTypes("ColorPick").AsTiledBox(size / 5, true).WithInteractivity(click: (_, right, release, _, element) =>
                {
                    int idx = int.Parse(element.Id.Split('>')[1]);

                    if (index == idx)
                        return;

                    if (!right && release)
                    {
                        foreach (UIElement e in container.GetElementsByType(true, "ColorPick"))
                            e.Z = 0;

                        index = idx;
                        element.Z = 3;

                        currentColorPicker.Color = container.GetElementById("ColorCircle>" + idx).Color;
                        currentColorPicker.GetElementById("CPB_Old").Color = oldColors[idx];

                        container.UpdateBounds();
                    }
                });
                UIElement colorCircle = UIElement.GetImage(circle, oldColors[i], "ColorCircle>" + i, positioner: UIHelper.GetCentered(0, 0, 0.4f));
                colorContainer.Add(colorCircle);
                container.Add(colorContainer);
            }
            container.Add(currentColorPicker);

            return container;
        }

        public static UIElement GetCloseButton(Action closingAction)
        {
            ((AnimatedTexture2D)UIHelper.BounceClose).Paused = true;
            ((AnimatedTexture2D)UIHelper.BounceClose).CurrentFrame = 0;
            ((AnimatedTexture2D)UIHelper.BounceClose)?.SetSpeed(12);

            return UIElement.GetImage(UIHelper.BounceClose, Color.White, "CloseBtn", 1, 9, UIHelper.GetTopRight(20, -40, 40)).WithInteractivity(click: (_, _, released, _, _) =>
            {
                if (released)
                    closingAction?.Invoke();
            }, hover: (_, hoverin, element) =>
            {
                if (hoverin != element.WasHover)
                    Game1.playSound("smallSelect");

                AnimatedTexture2D a = element.Theme as AnimatedTexture2D;

                if (hoverin)
                {
                    if (a != null)
                        a.Paused = false;
                }
                else
                {
                    if (a == null)
                        return;
                    a.Paused = true;
                    a.CurrentFrame = 0;
                }

            });
        }
    }
}
