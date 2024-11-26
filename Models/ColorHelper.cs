﻿using System.Windows.Media;

namespace MVVMPaintApp.Models
{
    internal class ColorHelper
    {
        public static Color CalculateColorFromPosition(Color baseColor, double normalizedY, double blackToColorPoint, double colorToWhitePoint)
        {
            if (normalizedY < blackToColorPoint)
            {
                double blackAmount = 1.0 - (normalizedY / blackToColorPoint);
                return MixColors(baseColor, Colors.Black, blackAmount);
            }

            if (normalizedY > colorToWhitePoint)
            {
                double whiteAmount = (normalizedY - colorToWhitePoint) / (1.0 - colorToWhitePoint);
                return MixColors(baseColor, Colors.White, whiteAmount);
            }

            return baseColor;
        }

        public static bool TryParseRgbValues(string r, string g, string b, out Color color)
        {
            color = Colors.Black;
            if (byte.TryParse(r, out byte _r) &&
                byte.TryParse(g, out byte _g) &&
                byte.TryParse(b, out byte _b))
            {
                color = Color.FromRgb(_r, _g, _b);
                return true;
            }
            return false;
        }

        public static Color GetRainbowColor(double position)
        {
            var (r, g, b) = CalculateRainbowComponents(position * 6);
            return Color.FromRgb(r, g, b);
        }

        public static (byte r, byte g, byte b) CalculateRainbowComponents(double position)
        {
            int index = (int)position;
            double remainder = position - index;

            return index switch
            {
                0 => (255, (byte)(255 * remainder), 0),                    // Red to Yellow
                1 => ((byte)(255 * (1 - remainder)), 255, 0),             // Yellow to Green
                2 => (0, 255, (byte)(255 * remainder)),                   // Green to Cyan
                3 => (0, (byte)(255 * (1 - remainder)), 255),            // Cyan to Blue
                4 => ((byte)(255 * remainder), 0, 255),                   // Blue to Magenta
                _ => (255, 0, (byte)(255 * (1 - remainder)))             // Magenta to Red
            };
        }

        public static Color MixColors(Color color1, Color color2, double amount)
        {
            return Color.FromRgb(
                (byte)(color1.R * (1 - amount) + color2.R * amount),
                (byte)(color1.G * (1 - amount) + color2.G * amount),
                (byte)(color1.B * (1 - amount) + color2.B * amount)
            );
        }

        public static double GetHue(Color color)
        {
            var (h, _, _) = RgbToHsv(color);
            return h;
        }

        public static (double h, double s, double v) RgbToHsv(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue = delta == 0 ? 0 :
                max == r ? 60 * ((g - b) / delta) :
                max == g ? 60 * (2 + (b - r) / delta) :
                          60 * (4 + (r - g) / delta);

            if (hue < 0) hue += 360;

            double saturation = max == 0 ? 0 : delta / max;
            double value = max;

            return (hue, saturation, value);
        }

        public static double GetRelativeBrightness(Color color, Color baseColor)
        {
            double colorBrightness = CalculateBrightness(color);
            double baseBrightness = CalculateBrightness(baseColor);

            if (colorBrightness < baseBrightness)
                return (colorBrightness / baseBrightness) - 1;
            if (colorBrightness > baseBrightness)
                return (colorBrightness - baseBrightness) / (1.0 - baseBrightness);
            return 0;
        }

        public static double CalculateBrightness(Color color) =>
            (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255.0;
    }
}