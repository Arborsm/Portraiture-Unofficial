using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
namespace Portraiture.PlatoUI.Presets
{
	internal class PlatoUIColorPicker
	{

		internal static UIElement getColorPickerMenu(Color color, Action<Color> handler = null, int size = 100)
		{
			ColorToHSV(color, out double h, out double s, out double v);
			return getColorPickerMenu((int)h, s, v, color.A / 255f, handler, size);
		}

		private static UIElement getColorPickerMenu(int oldH = 0, double oldS = 0, double oldV = 0, float oldAlpha = 1f, Action<Color> handler = null, int size = 100)
		{
			int margin = size / 10;
			int pickerWidth = margin * 4;
			int pickerHeight = size * 3;
			int mainPickerWidth = size * 5;
			int baseValue = oldH;
			float currentAlpha = oldAlpha;
			int pickPosition = (int)(baseValue / 360f * pickerHeight);
			int pickAlphaPosition = (int)((1f - oldAlpha) * (mainPickerWidth - pickerWidth));
			int lastBaseValue = baseValue;

			Vector2 colorChoiceMade = new Vector2((float)(oldS * mainPickerWidth), (float)((1d - oldV) * (pickerHeight - margin - pickerWidth) - 1f));
			Color[] baseColors = new Color[mainPickerWidth * (pickerHeight - margin - pickerWidth)];

			Color old = ColorFromHSV(oldH, oldS, oldV) * oldAlpha;
			Color currentColor;
			Color lastColor = old;
			Texture2D cp = PyDraw.getRectangle(1, 361, (i, _, _) => ColorFromHSV(i, 1d, 1d));

			UIElement colorPreview = UIElement.GetImage(UIHelper.PlainTheme, old, "CPB_Preview", positioner: UIHelper.GetBottomLeft(0, 0, size, pickerHeight / 2 - margin / 2));
			UIElement colorOld = UIElement.GetImage(UIHelper.PlainTheme, old, "CPB_Old", positioner: UIHelper.GetTopLeft(0, 0, size, pickerHeight / 2 - margin / 2)).WithInteractivity(click: (_, r, _, _, _) =>
			{
				if (!r) return;
				baseValue = oldH;
				pickPosition = (int)(baseValue / 360f * pickerHeight);
				pickAlphaPosition = (int)((1f - oldAlpha) * (mainPickerWidth - pickerWidth));
				currentAlpha = oldAlpha;
				colorChoiceMade = new Vector2((float)(oldS * mainPickerWidth), (float)((1d - oldV) * (pickerHeight - margin - pickerWidth) - 1f));
			});

			Texture2D ap = PyDraw.getRectangle(pickerHeight, 1, (x, _, w, _) => Color.White * (1f - (float)x / w));


			UIElement container = UIElement.GetContainer("ColorPickerContainer", positioner: UIHelper.GetCentered(0, 0, pickerWidth + margin + mainPickerWidth + margin + size + margin * 6, pickerHeight + margin * 6));
			container.Color = Color.Transparent;
			UIElement box = UIElement.GetImage(UIHelper.DarkTheme, Color.White * 0.75f, "CPB_Box").AsTiledBox(size / 5, true);

			UIElement canvas = UIElement.GetImage(UIHelper.PlainTheme, Color.Transparent, "ColorPicker", positioner: UIHelper.GetCentered(0, 0, pickerWidth + margin + mainPickerWidth + margin + size, pickerHeight));

			UIElement colorPickerBasePick = UIElement.GetImage(UIHelper.ArrowLeft, Color.Wheat, "CPB_Pick", positioner: (t, p) =>
			{
				int w = p.Bounds.Width;
				float s = (float)w / t.Theme.Width;
				int h = (int)(s * t.Theme.Width);
				return new Rectangle(p.Bounds.Right - w / 3, p.Bounds.Y + pickPosition - h / 2, w, h);
			});

			UIElement colorPickPointer = UIElement.GetImage(PyDraw.getBorderedRectangle(8, 8, Color.Transparent, 1, Color.White), Color.White, "CPB_Pointer", positioner: (t, p) => new Rectangle((int)colorChoiceMade.X + p.Bounds.X - t.Theme.Width / 2, (int)colorChoiceMade.Y + p.Bounds.Y - t.Theme.Height / 2, t.Theme.Width, t.Theme.Height));


			Texture2D first = getMainColorPickerFromBase(baseValue, mainPickerWidth, pickerHeight - margin - pickerWidth);
			first.GetData(baseColors);

			UIElement colorChoice = UIElement.GetImage(first, Color.White, "CPB_MainPicker", positioner: UIHelper.GetTopLeft(size + margin, 0, mainPickerWidth, pickerHeight - margin - pickerWidth)).WithInteractivity(click: (_, _, _, _, element) =>
			{
				colorChoiceMade = new Vector2(Game1.getMouseX() - element.Bounds.X, Game1.getMouseY() - element.Bounds.Y);
				colorPickPointer.UpdateBounds();
			});

			UIElement colorPickerBase = UIElement.GetImage(cp, Color.White, "CPB_BasePicker", 1, 2, UIHelper.GetTopRight(0, 0, pickerWidth, 1f)).WithInteractivity(click: (_, _, _, _, element) =>
			{
				pickPosition = Game1.getMouseY() - element.Bounds.Y;
				colorPickerBasePick.UpdateBounds();
				baseValue = (int)((float)pickPosition / element.Bounds.Height * 360f);
			});


			UIElement colorPickerAlphaPick = UIElement.GetImage(UIHelper.ArrowUp, Color.Wheat, "CPB_Pick", positioner: (t, p) =>
			{
				int h = p.Bounds.Height;
				float s = (float)h / t.Theme.Height;
				int w = (int)(s * t.Theme.Height);
				return new Rectangle(p.Bounds.X - w / 2 + pickAlphaPosition, p.Bounds.Bottom - h / 3, w, h);
			});

			UIElement colorPickerAlpha = UIElement.GetImage(ap, Color.White, "CPB_AlphaPicker", 1, 2, UIHelper.GetBottomLeft(size + margin + pickerWidth, 0, mainPickerWidth - pickerWidth, pickerWidth)).WithInteractivity(click: (_, _, _, _, element) =>
			{
				pickAlphaPosition = Game1.getMouseX() - element.Bounds.X;
				colorPickerAlphaPick.UpdateBounds();
				currentAlpha = 1f - pickAlphaPosition / (float)element.Bounds.Width;
			});

			UIElement alphaReset = UIElement.GetImage(UIHelper.PlainTheme, Color.LightGray, "ResetAlpha", 1, 1, UIHelper.GetBottomLeft(size + margin, 0, pickerWidth, pickerWidth)).WithInteractivity(click: (_, _, _, _, _) =>
			{
				colorPreview.Color = new Color(colorPreview.Color.R, colorPreview.Color.G, colorPreview.Color.B);
				pickAlphaPosition = 0;
				currentAlpha = 1f;
				colorPickerAlphaPick.UpdateBounds();
			});

			colorPickerBase.Add(colorPickerBasePick);
			colorPickerAlpha.Add(colorPickerAlphaPick);
			colorChoice.Add(colorPickPointer);
			canvas.Add(alphaReset);
			canvas.Add(colorPickerBase);
			canvas.Add(colorPickerAlpha);

			canvas.Add(colorPreview);
			canvas.Add(colorOld);
			canvas.Add(colorChoice);
			box.Add(canvas);
			container.Add(box);


			box.WithInteractivity(draw: (_, _) =>
			{
				currentColor = ColorFromHSV(baseValue, (double)colorChoiceMade.X / colorChoice.Bounds.Width, 1d - (double)colorChoiceMade.Y / colorChoice.Bounds.Height);
				currentColor.A = (byte)(int)(255f * currentAlpha);
				colorPreview.Color = currentColor;

				if (baseValue != lastBaseValue && Mouse.GetState().LeftButton == ButtonState.Released)
				{
					lastBaseValue = baseValue;
					colorChoice.Theme = getMainColorPickerFromBase(baseValue, mainPickerWidth, pickerHeight - margin - pickerWidth);
					colorChoice.Theme.GetData(baseColors);
				}

				if (container.Color != Color.Transparent)
				{
					float newA = container.Color.A / 255f;

					ColorToHSV(container.Color, out double newH, out double newS, out double newV);
					container.Color = Color.Transparent;

					baseValue = (int)newH;
					pickPosition = (int)(baseValue / 360f * pickerHeight);
					pickAlphaPosition = (int)((1f - newA) * (mainPickerWidth - pickerWidth));
					currentAlpha = newA;
					colorChoiceMade = new Vector2((float)(newS * mainPickerWidth), (float)((1d - newV) * (pickerHeight - margin - pickerWidth) - 1f));
				}

				if (currentColor != lastColor)
				{
					lastColor = currentColor;
					handler?.Invoke(colorPreview.Color);
				}
			});

			return container;
		}
		private static Texture2D getMainColorPickerFromBase(int b, int witdh, int height)
		{
			return PyDraw.getRectangle(witdh, height, (x, y, w, h) => ColorFromHSV(b, (double)x / w, 1d - (double)y / h));
		}

		private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
		{
			int max = Math.Max(color.R, Math.Max(color.G, color.B));
			int min = Math.Min(color.R, Math.Min(color.G, color.B));

			hue = GetHue(color.R, color.G, color.B);
			saturation = max == 0 ? 0 : 1d - 1d * min / max;
			value = max / 255d;
		}

		private static double GetHue(float r, float g, float b)
		{
			double h;
			float num1 = Math.Min(Math.Min(r, g), b);
			float num2 = Math.Max(Math.Max(r, g), b);
			float num3 = num2 - num1;
			if (num2 != 0.0)
			{
				h = !(Math.Abs(r - (double)num2) > 1e-6) ? (g - b) / num3 : Math.Abs(g - (double)num2) > 1e-6 ? (float)(4.0 + (r - (double)g) / num3) : (float)(2.0 + (b - (double)r) / num3);
				h *= 60f;
				if (h >= 0.0)
					return h;
				h += 360f;
			}
			else
				h = 0f;

			return h;
		}

		private static Color ColorFromHSV(double h, double s, double v)
		{
			return PortraitureMod.helper.Reflection.GetMethod(new ColorPicker("converter", 0, 0), "HsvToRgb").Invoke<Color>(h, s, v);
		}
	}
}
