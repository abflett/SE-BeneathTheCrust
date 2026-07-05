using System;
using System.Globalization;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace WkKn
{
    [MyTextSurfaceScript(WkKnSession.CalibratorTextSurfaceScriptId, "Working Knowledge Calibrator")]
    public class WkKnCalibratorTextSurfaceScript : MyTSSCommon
    {
        private const string Font = "Monospace";

        public override ScriptUpdate NeedsUpdate
        {
            get { return ScriptUpdate.Update100; }
        }

        public WkKnCalibratorTextSurfaceScript(Sandbox.ModAPI.Ingame.IMyTextSurface surface, VRage.Game.ModAPI.Ingame.IMyCubeBlock block, Vector2 size)
            : base(surface, block, size)
        {
        }

        public override void Run()
        {
            base.Run();

            var size = GetDrawableSize();
            var settings = WkKnSession.GetWkKnLcdSettings(
                m_block,
                m_surface,
                size,
                WkKnSession.CreateWkKnLcdSettings(size, "0,0,0,0", 180, 7, 11, 1f, 0f, 1f));
            var layout = settings.Layout;
            var background = Color.Black;
            var major = Color.White;
            var minor = new Color(95, 95, 95);
            var border = Color.Red;
            var dim = new Color(190, 190, 190);

            using (var frame = m_surface.DrawFrame())
            {
                AddRectangle(frame, size * 0.5f, size, background);
                DrawGrid(frame, size, minor, major);
                DrawSafeArea(frame, layout, border);
                DrawInfo(frame, size, GetDisplayName(), layout, major, dim);
            }
        }

        private Vector2 GetDrawableSize()
        {
            var size = m_size;
            if (size.X <= 0f || size.Y <= 0f)
                size = m_surface.TextureSize;

            if (size.X <= 0f || size.Y <= 0f)
                size = m_surface.SurfaceSize;

            if (size.X <= 0f || size.Y <= 0f)
                size = new Vector2(512f, 512f);

            return size;
        }

        private void DrawGrid(MySpriteDrawFrame frame, Vector2 size, Color minor, Color major)
        {
            var maxX = (int)Math.Floor(size.X);
            var maxY = (int)Math.Floor(size.Y);
            for (var x = 0; x <= maxX; x += 10)
            {
                var isMajor = x % 50 == 0;
                AddRectangle(frame, new Vector2(x, size.Y * 0.5f), new Vector2(isMajor ? 2f : 1f, size.Y), isMajor ? major : minor);
                if (isMajor)
                    AddGridLabel(frame, x.ToString(CultureInfo.InvariantCulture), new Vector2(x + 3f, 3f), 0.30f, major, TextAlignment.LEFT);
            }

            for (var y = 0; y <= maxY; y += 10)
            {
                var isMajor = y % 50 == 0;
                AddRectangle(frame, new Vector2(size.X * 0.5f, y), new Vector2(size.X, isMajor ? 2f : 1f), isMajor ? major : minor);
                if (isMajor)
                    AddGridLabel(frame, y.ToString(CultureInfo.InvariantCulture), new Vector2(3f, y + 3f), 0.30f, major, TextAlignment.LEFT);
            }

            AddRectangle(frame, new Vector2(size.X * 0.5f, 0f), new Vector2(size.X, 3f), major);
            AddRectangle(frame, new Vector2(size.X * 0.5f, size.Y), new Vector2(size.X, 3f), major);
            AddRectangle(frame, new Vector2(0f, size.Y * 0.5f), new Vector2(3f, size.Y), major);
            AddRectangle(frame, new Vector2(size.X, size.Y * 0.5f), new Vector2(3f, size.Y), major);
        }

        private void DrawSafeArea(MySpriteDrawFrame frame, WkKnSession.DisplayLayoutBounds layout, Color color)
        {
            var center = layout.Offset + layout.Size * 0.5f;
            var left = layout.Offset.X;
            var top = layout.Offset.Y;
            var right = layout.Offset.X + layout.Size.X;
            var bottom = layout.Offset.Y + layout.Size.Y;
            AddRectangle(frame, new Vector2(center.X, top), new Vector2(layout.Size.X, 3f), color);
            AddRectangle(frame, new Vector2(center.X, bottom), new Vector2(layout.Size.X, 3f), color);
            AddRectangle(frame, new Vector2(left, center.Y), new Vector2(3f, layout.Size.Y), color);
            AddRectangle(frame, new Vector2(right, center.Y), new Vector2(3f, layout.Size.Y), color);
        }

        private string GetDisplayName()
        {
            var displayName = m_surface.DisplayName;
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName.Trim();

            displayName = m_surface.Name;
            return string.IsNullOrWhiteSpace(displayName) ? "Display" : displayName.Trim();
        }

        private void DrawInfo(MySpriteDrawFrame frame, Vector2 size, string displayName, WkKnSession.DisplayLayoutBounds layout, Color accent, Color dim)
        {
            var scale = Math.Max(0.32f, Math.Min(0.52f, size.Y / 760f));
            var lineHeight = MeasureTextHeight("Calibration", scale, 18f) + 3f;
            var y = Math.Max(6f, size.Y - lineHeight * 2f - 8f);
            var surface = ((int)Math.Round(size.X)).ToString(CultureInfo.InvariantCulture) + "x" + ((int)Math.Round(size.Y)).ToString(CultureInfo.InvariantCulture);
            AddText(frame, "WK CALIBRATOR  " + surface, new Vector2(8f, y), scale, accent, TextAlignment.LEFT);
            y += lineHeight;
            AddText(frame, displayName + "  " + layout.FormatInset(), new Vector2(8f, y), scale, dim, TextAlignment.LEFT);
        }

        private static void AddRectangle(MySpriteDrawFrame frame, Vector2 center, Vector2 size, Color color)
        {
            var sprite = MySprite.CreateSprite("SquareSimple", center, size);
            sprite.Color = color;
            frame.Add(sprite);
        }

        private static void AddText(MySpriteDrawFrame frame, string text, Vector2 position, float scale, Color color, TextAlignment alignment)
        {
            var sprite = MySprite.CreateText(text ?? string.Empty, Font, color, scale, alignment);
            sprite.Position = position;
            frame.Add(sprite);
        }

        private static void AddGridLabel(MySpriteDrawFrame frame, string text, Vector2 position, float scale, Color color, TextAlignment alignment)
        {
            var sprite = MySprite.CreateText(text ?? string.Empty, Font, color, scale, alignment);
            sprite.Position = position;
            frame.Add(sprite);
        }

        private float MeasureTextHeight(string text, float scale, float fallback)
        {
            var height = m_surface.MeasureStringInPixels(new StringBuilder(text ?? string.Empty), Font, scale).Y;
            return height > 0f ? height : fallback;
        }

    }
}
