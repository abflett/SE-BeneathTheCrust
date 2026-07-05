using System;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace WkKn
{
    [MyTextSurfaceScript(WkKnSession.IdentityTextSurfaceScriptId, "Working Knowledge Identity")]
    public class WkKnIdentityTextSurfaceScript : WkKnTextSurfaceScriptBase
    {
        private float iconPaddingLeft;
        private float iconSizeScale = 1f;

        public override ScriptUpdate NeedsUpdate
        {
            get { return ScriptUpdate.Update100; }
        }

        public WkKnIdentityTextSurfaceScript(Sandbox.ModAPI.Ingame.IMyTextSurface surface, VRage.Game.ModAPI.Ingame.IMyCubeBlock block, Vector2 size)
            : base(surface, block, size)
        {
        }

        public override void Run()
        {
            base.Run();

            var snapshot = WkKnSession.GetIdentityDisplaySnapshot();
            var surfaceSize = GetDrawableSize(new Vector2(512f, 256f));
            var settings = WkKnSession.GetWkKnLcdSettings(
                m_block,
                m_surface,
                surfaceSize,
                WkKnSession.CreateWkKnLcdSettings(surfaceSize, "0,0,0,0", 180, 7, 11, 1f, 0f, 1f));
            var size = settings.Layout.Size;
            drawOffset = settings.Layout.Offset;
            textSize = settings.TextSize;
            iconPaddingLeft = settings.IconPaddingLeft;
            iconSizeScale = settings.IconSize;
            layoutScale = GetLayoutScale(size) * settings.LayoutScale;
            var padding = ClampLayout(Math.Min(size.X, size.Y) * 0.14f, 12f, 28f);
            var accent = GetReadableForegroundColor(settings);
            var dim = new Color(
                (byte)Math.Max(35, (int)(accent.R * 0.55f)),
                (byte)Math.Max(45, (int)(accent.G * 0.55f)),
                (byte)Math.Max(40, (int)(accent.B * 0.55f)));
            var background = GetDisplayBackgroundColor(settings);

            using (var frame = m_surface.DrawFrame())
            {
                AddRawRectangle(frame, surfaceSize * 0.5f, surfaceSize, background);

                if (!string.IsNullOrWhiteSpace(snapshot.Message))
                {
                    DrawMessage(frame, snapshot.Message, size, padding, accent, dim);
                    return;
                }

                DrawIdentity(frame, snapshot, size, padding, accent, dim);
            }
        }




        private void DrawIdentity(MySpriteDrawFrame frame, IdentityDisplaySnapshot snapshot, Vector2 size, float padding, Color accent, Color dim)
        {
            var iconSize = Math.Min(size.Y - padding * 2f, size.X * 0.24f);
            iconSize = ClampFloat(iconSize * iconSizeScale, Scaled(12f), size.Y * 0.90f);
            var iconCenter = new Vector2(padding + iconPaddingLeft + iconSize * 0.5f, size.Y * 0.5f);
            DrawFactionIcon(frame, snapshot, iconCenter, iconSize, accent, dim);

            var textX = padding + iconPaddingLeft + iconSize + padding;
            var textWidth = Math.Max(40f, size.X - textX - padding);
            var name = string.IsNullOrWhiteSpace(snapshot.FactionName) ? "No Faction" : snapshot.FactionName;
            var tag = "[" + (string.IsNullOrWhiteSpace(snapshot.FactionTag) ? "INDEPENDENT" : snapshot.FactionTag) + "]";
            var player = string.IsNullOrWhiteSpace(snapshot.PlayerName) ? "Local Player" : snapshot.PlayerName;

            var nameScale = ScaleText(ClampText(size.Y / 280f, 0.62f, 0.88f));
            var nameLines = WrapText(name, textWidth, nameScale, 2);
            var tagScale = FitTextScale(tag, textWidth, ScaleText(ClampText(size.Y / 420f, 0.42f, 0.58f)), ScaleText(Scaled(0.32f)));
            var playerScale = FitTextScale(player, textWidth, ScaleText(ClampText(size.Y / 390f, 0.46f, 0.64f)), ScaleText(Scaled(0.34f)));
            var nameLineHeight = MeasureTextHeight("Faction", nameScale, 28f) + Scaled(2f);
            var nameHeight = nameLines.Count * nameLineHeight;
            var tagHeight = MeasureTextHeight(tag, tagScale, 18f);
            var playerHeight = MeasureTextHeight(player, playerScale, 20f);
            var gap = ClampLayout(size.Y * 0.04f, 4f, 10f);
            var totalHeight = nameHeight + tagHeight + playerHeight + gap * 2f;
            var y = size.Y * 0.5f - totalHeight * 0.5f;

            for (var i = 0; i < nameLines.Count; i++)
            {
                AddText(frame, nameLines[i], new Vector2(textX, y), nameScale, accent, TextAlignment.LEFT);
                y += nameLineHeight;
            }
            y += gap;
            AddText(frame, TrimToWidth(tag, textWidth, tagScale), new Vector2(textX, y), tagScale, accent, TextAlignment.LEFT);
            y += tagHeight + gap;
            AddText(frame, TrimToWidth(player, textWidth, playerScale), new Vector2(textX, y), playerScale, accent, TextAlignment.LEFT);
        }

        private void DrawFactionIcon(MySpriteDrawFrame frame, IdentityDisplaySnapshot snapshot, Vector2 center, float size, Color accent, Color dim)
        {
            var backing = MySprite.CreateSprite("SquareSimple", center + drawOffset, new Vector2(size, size));
            backing.Color = snapshot.HasFaction ? snapshot.FactionBackgroundColor : new Color(dim, 0.32f);
            frame.Add(backing);

            if (!string.IsNullOrWhiteSpace(snapshot.FactionIconName))
            {
                var icon = MySprite.CreateSprite(snapshot.FactionIconName, center + drawOffset, new Vector2(size * 0.72f, size * 0.72f));
                icon.Color = snapshot.FactionIconColor;
                frame.Add(icon);
                return;
            }

            var fallback = snapshot.HasFaction && !string.IsNullOrWhiteSpace(snapshot.FactionTag) ? snapshot.FactionTag : "WK";
            fallback = TrimText(fallback, 4);
            var scale = FitTextScale(fallback, size * 0.72f, ScaleText(ClampText(size / 125f, 0.48f, 0.88f)), ScaleText(Scaled(0.32f)));
            AddText(frame, fallback, new Vector2(center.X, center.Y - MeasureTextHeight(fallback, scale, 20f) * 0.5f), scale, snapshot.HasFaction ? snapshot.FactionIconColor : accent, TextAlignment.CENTER);
        }

        private void DrawMessage(MySpriteDrawFrame frame, string message, Vector2 size, float padding, Color accent, Color dim)
        {
            message = string.IsNullOrWhiteSpace(message) ? "No identity data available." : message;
            var scale = FitTextScale(message, size.X - padding * 2f, ScaleText(ClampText(size.Y / 360f, 0.48f, 0.70f)), ScaleText(Scaled(0.34f)));
            AddText(frame, TrimToWidth(message, size.X - padding * 2f, scale), new Vector2(size.X * 0.5f, size.Y * 0.5f - MeasureTextHeight(message, scale, 24f) * 0.5f), scale, accent, TextAlignment.CENTER);
        }
    }
}
