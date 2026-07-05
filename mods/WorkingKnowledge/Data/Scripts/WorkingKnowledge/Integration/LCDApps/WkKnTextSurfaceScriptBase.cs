using System;
using System.Globalization;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace WkKn
{
    public abstract class WkKnTextSurfaceScriptBase : MyTSSCommon
    {
        protected const string Font = "Monospace";
        protected Vector2 drawOffset;
        protected float layoutScale = 1f;
        protected float textSize = 1f;

        protected WkKnTextSurfaceScriptBase(Sandbox.ModAPI.Ingame.IMyTextSurface surface, VRage.Game.ModAPI.Ingame.IMyCubeBlock block, Vector2 size)
            : base(surface, block, size)
        {
        }

        protected Vector2 GetDrawableSize(Vector2 fallback)
        {
            var size = m_size;
            if (size.X <= 0f || size.Y <= 0f)
                size = m_surface.TextureSize;

            if (size.X <= 0f || size.Y <= 0f)
                size = m_surface.SurfaceSize;

            if (size.X <= 0f || size.Y <= 0f)
                size = fallback;

            return size;
        }

        protected Color GetReadableForegroundColor(WkKnSession.WkKnLcdSettings settings)
        {
            if (settings.HasTextColor)
                return settings.TextColor;

            var color = m_foregroundColor;
            if (color.R + color.G + color.B < 90)
                return new Color(180, 255, 200);

            return color;
        }

        protected Color GetDisplayBackgroundColor(WkKnSession.WkKnLcdSettings settings)
        {
            if (settings.HasBackgroundColor)
                return settings.BackgroundColor;

            var color = m_backgroundColor;
            return new Color(color.R, color.G, color.B);
        }

        protected void AddProgressBar(MySpriteDrawFrame frame, Vector2 center, Vector2 size, float ratio, Color background, Color foreground)
        {
            ratio = ClampFloat(ratio, 0f, 1f);
            AddRectangle(frame, center, size, background);
            if (ratio <= 0f)
                return;

            var filledSize = new Vector2(size.X * ratio, size.Y);
            var filledCenter = new Vector2(center.X - size.X * 0.5f + filledSize.X * 0.5f, center.Y);
            AddRectangle(frame, filledCenter, filledSize, foreground);
        }

        protected static Color GetColorShade(Color text, Color background, int shade)
        {
            if (shade < 0)
                shade = 0;

            if (shade > 9)
                shade = 9;

            var ratio = shade / 9f;
            return new Color(
                ClampByte((int)Math.Round(text.R + (background.R - text.R) * ratio)),
                ClampByte((int)Math.Round(text.G + (background.G - text.G) * ratio)),
                ClampByte((int)Math.Round(text.B + (background.B - text.B) * ratio)));
        }

        private static byte ClampByte(int value)
        {
            if (value < 0)
                return 0;

            if (value > 255)
                return 255;

            return (byte)value;
        }

        protected void AddRectangle(MySpriteDrawFrame frame, Vector2 center, Vector2 size, Color color)
        {
            var sprite = MySprite.CreateSprite("SquareSimple", center + drawOffset, size);
            sprite.Color = color;
            frame.Add(sprite);
        }

        protected static void AddRawRectangle(MySpriteDrawFrame frame, Vector2 center, Vector2 size, Color color)
        {
            var sprite = MySprite.CreateSprite("SquareSimple", center, size);
            sprite.Color = color;
            frame.Add(sprite);
        }

        protected void AddText(MySpriteDrawFrame frame, string text, Vector2 position, float scale, Color color, TextAlignment alignment)
        {
            var sprite = MySprite.CreateText(text ?? string.Empty, Font, color, scale, alignment);
            sprite.Position = position + drawOffset;
            frame.Add(sprite);
        }

        protected float FitTextScale(string text, float maxWidth, float desiredScale, float minimumScale)
        {
            var width = MeasureTextWidth(text, desiredScale);
            if (width <= 0f || width <= maxWidth)
                return desiredScale;

            return Math.Max(minimumScale, desiredScale * (maxWidth / width));
        }

        protected float ScaleText(float scale)
        {
            return ClampFloat(scale * textSize, 0.12f, 3.0f);
        }

        protected static float GetLayoutScale(Vector2 size)
        {
            return ClampFloat(Math.Min(size.X, size.Y) / 512f, 0.30f, 1.50f);
        }

        protected float Scaled(float value)
        {
            return value * layoutScale;
        }

        protected float ClampLayout(float value, float min, float max)
        {
            return ClampFloat(value, Scaled(min), Scaled(max));
        }

        protected float ClampText(float value, float min, float max)
        {
            return ClampFloat(value, Math.Min(Scaled(min), max), max);
        }

        protected string TrimToWidth(string text, float maxWidth, float scale)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (MeasureTextWidth(text, scale) <= maxWidth)
                return text;

            const string suffix = "...";
            var suffixWidth = MeasureTextWidth(suffix, scale);
            var allowedWidth = Math.Max(0f, maxWidth - suffixWidth);
            for (var length = text.Length - 1; length > 0; length--)
            {
                var candidate = text.Substring(0, length).TrimEnd();
                if (MeasureTextWidth(candidate, scale) <= allowedWidth)
                    return candidate + suffix;
            }

            return suffix;
        }

        protected System.Collections.Generic.List<string> WrapText(string text, float maxWidth, float scale, int maxLines)
        {
            var lines = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return lines;

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var current = new StringBuilder();
            foreach (var word in words)
            {
                var candidate = current.Length == 0 ? word : current.ToString() + " " + word;
                if (MeasureTextWidth(candidate, scale) <= maxWidth)
                {
                    current.Clear();
                    current.Append(candidate);
                    continue;
                }

                if (current.Length > 0)
                {
                    lines.Add(current.ToString());
                    current.Clear();
                }

                current.Append(TrimToWidth(word, maxWidth, scale));
                if (lines.Count >= maxLines - 1)
                    break;
            }

            if (current.Length > 0 && lines.Count < maxLines)
                lines.Add(current.ToString());

            return lines;
        }

        protected static string TrimText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.Length <= maxLength ? text : text.Substring(0, maxLength);
        }

        protected float MeasureTextWidth(string text, float scale)
        {
            return m_surface.MeasureStringInPixels(new StringBuilder(text ?? string.Empty), Font, scale).X;
        }

        protected float MeasureTextHeight(string text, float scale, float fallback)
        {
            var height = m_surface.MeasureStringInPixels(new StringBuilder(text ?? string.Empty), Font, scale).Y;
            return height > 0f ? height : fallback;
        }

        protected static string FormatPercent(double progress)
        {
            progress = Clamp01(progress);
            return progress >= 1.0 ? "100%" : (Math.Round(progress * 1000.0) / 10.0).ToString("0.#", CultureInfo.InvariantCulture) + "%";
        }

        protected static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;

            if (value > 1.0)
                return 1.0;

            return value;
        }

        protected static float ClampFloat(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
