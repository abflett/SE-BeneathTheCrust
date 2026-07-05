using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using VRage.Utils;
using VRageMath;


namespace WkKn
{
    public partial class WkKnSession
    {
        internal static bool IsResearchSciFiTerminalDisplayBlock(VRage.Game.ModAPI.Ingame.IMyCubeBlock block)
        {
            return block != null &&
                   block.BlockDefinition.SubtypeName.Equals(ResearchSciFiTerminalSubtype, StringComparison.OrdinalIgnoreCase);
        }

        internal static WkKnLcdSettings CreateWkKnLcdSettings(
            Vector2 surfaceSize,
            string inset,
            long pageTicks,
            int rowsFirstPage,
            int rowsOtherPages,
            float textSize,
            float iconPaddingLeft,
            float iconSize)
        {
            float left;
            float top;
            float right;
            float bottom;
            if (!TryParseDisplayInset(inset, surfaceSize, out left, out top, out right, out bottom))
                left = top = right = bottom = 0f;

            return BuildWkKnLcdSettings(surfaceSize, left, top, right, bottom, pageTicks, rowsFirstPage, rowsOtherPages, 1f, textSize, iconPaddingLeft, iconSize);
        }

        internal static WkKnLcdSettings GetWkKnLcdSettings(
            VRage.Game.ModAPI.Ingame.IMyCubeBlock block,
            Sandbox.ModAPI.Ingame.IMyTextSurface surface,
            Vector2 surfaceSize,
            WkKnLcdSettings defaults)
        {
            EnsureWkKnDisplayCustomData(block);

            var settings = BuildWkKnLcdSettings(
                surfaceSize,
                defaults.Layout.Left,
                defaults.Layout.Top,
                defaults.Layout.Right,
                defaults.Layout.Bottom,
                defaults.PageTicks,
                defaults.RowsFirstPage,
                defaults.RowsOtherPages,
                defaults.LayoutScale,
                defaults.TextSize,
                defaults.IconPaddingLeft,
                defaults.IconSize);

            var terminalBlock = block as Sandbox.ModAPI.Ingame.IMyTerminalBlock;
            if (terminalBlock == null || string.IsNullOrWhiteSpace(terminalBlock.CustomData))
                return settings;

            var sections = ParseLcdConfigSections(terminalBlock.CustomData);
            for (var i = 0; i < sections.Count; i++)
            {
                if (IsRootLcdSection(sections[i].Name))
                    ApplyLcdConfigSection(sections[i], surfaceSize, ref settings);
            }

            var displayName = GetSurfaceDisplayName(surface);
            var surfaceName = GetSurfaceName(surface);
            for (var i = 0; i < sections.Count; i++)
            {
                if (!IsDisplayLcdSection(sections[i].Name))
                    continue;

                string displayValue;
                if (!sections[i].Values.TryGetValue(DisplayKey, out displayValue))
                    continue;

                displayValue = Unquote(displayValue);
                if (DisplayNameMatches(displayValue, displayName) || DisplayNameMatches(displayValue, surfaceName))
                    ApplyLcdConfigSection(sections[i], surfaceSize, ref settings);
            }

            return settings;
        }

        private static WkKnLcdSettings BuildWkKnLcdSettings(
            Vector2 surfaceSize,
            float left,
            float top,
            float right,
            float bottom,
            long pageTicks,
            int rowsFirstPage,
            int rowsOtherPages,
            float layoutScale,
            float textSize,
            float iconPaddingLeft,
            float iconSize)
        {
            left = ClampInset(left, surfaceSize.X);
            top = ClampInset(top, surfaceSize.Y);
            right = ClampInset(right, Math.Max(1f, surfaceSize.X - left));
            bottom = ClampInset(bottom, Math.Max(1f, surfaceSize.Y - top));

            return new WkKnLcdSettings
            {
                Layout = new DisplayLayoutBounds
                {
                    Offset = new Vector2(left, top),
                    Size = new Vector2(Math.Max(1f, surfaceSize.X - left - right), Math.Max(1f, surfaceSize.Y - top - bottom)),
                    Left = left,
                    Top = top,
                    Right = right,
                    Bottom = bottom,
                },
                PageTicks = Math.Max(1L, pageTicks),
                RowsFirstPage = Math.Max(1, rowsFirstPage),
                RowsOtherPages = Math.Max(1, rowsOtherPages),
                LayoutScale = ClampSettingFloat(layoutScale, 0.5f, 1.5f, 1f),
                TextSize = ClampSettingFloat(textSize, 0.2f, 3.0f, 1f),
                IconPaddingLeft = ClampSettingFloat(iconPaddingLeft, -1000f, 1000f, 0f),
                IconSize = ClampSettingFloat(iconSize, 0.1f, 5.0f, 1f),
            };
        }

        private static void ApplyLcdConfigSection(LcdCustomDataSection section, Vector2 surfaceSize, ref WkKnLcdSettings settings)
        {
            var left = settings.Layout.Left;
            var top = settings.Layout.Top;
            var right = settings.Layout.Right;
            var bottom = settings.Layout.Bottom;
            var pageTicks = settings.PageTicks;
            var rowsFirstPage = settings.RowsFirstPage;
            var rowsOtherPages = settings.RowsOtherPages;
            var layoutScale = settings.LayoutScale;
            var textSize = settings.TextSize;
            var hasTextColor = settings.HasTextColor;
            var textColor = settings.TextColor;
            var hasBackgroundColor = settings.HasBackgroundColor;
            var backgroundColor = settings.BackgroundColor;
            var iconPaddingLeft = settings.IconPaddingLeft;
            var iconSize = settings.IconSize;

            string value;
            float parsedLeft;
            float parsedTop;
            float parsedRight;
            float parsedBottom;
            if (section.Values.TryGetValue(InsetKey, out value) &&
                TryParseDisplayInset(value, surfaceSize, out parsedLeft, out parsedTop, out parsedRight, out parsedBottom))
            {
                left = parsedLeft;
                top = parsedTop;
                right = parsedRight;
                bottom = parsedBottom;
            }

            TryApplyInsetSide(section, InsetLeftKey, surfaceSize.X, ref left);
            TryApplyInsetSide(section, InsetTopKey, surfaceSize.Y, ref top);
            TryApplyInsetSide(section, InsetRightKey, surfaceSize.X, ref right);
            TryApplyInsetSide(section, InsetBottomKey, surfaceSize.Y, ref bottom);
            TryApplyLong(section, PageTicksKey, 1L, 24000L, ref pageTicks);
            TryApplyInt(section, RowsFirstPageKey, 1, 100, ref rowsFirstPage);
            TryApplyInt(section, RowsOtherPagesKey, 1, 100, ref rowsOtherPages);
            TryApplyFloat(section, LayoutScaleKey, 0.5f, 1.5f, ref layoutScale);
            TryApplyFloat(section, TextSizeKey, 0.2f, 3.0f, ref textSize);
            TryApplyColor(section, TextColorKey, ref hasTextColor, ref textColor);
            TryApplyColor(section, BackgroundColorKey, ref hasBackgroundColor, ref backgroundColor);
            TryApplyFloat(section, IconPaddingLeftKey, -1000f, 1000f, ref iconPaddingLeft);
            TryApplyFloat(section, IconSizeKey, 0.1f, 5.0f, ref iconSize);

            settings = BuildWkKnLcdSettings(surfaceSize, left, top, right, bottom, pageTicks, rowsFirstPage, rowsOtherPages, layoutScale, textSize, iconPaddingLeft, iconSize);
            settings.HasTextColor = hasTextColor;
            settings.TextColor = textColor;
            settings.HasBackgroundColor = hasBackgroundColor;
            settings.BackgroundColor = backgroundColor;
        }

        private static void TryApplyInsetSide(LcdCustomDataSection section, string key, float axisSize, ref float value)
        {
            string text;
            float parsed;
            if (section.Values.TryGetValue(key, out text) && TryParseDisplayInsetPart(text, axisSize, out parsed))
                value = parsed;
        }

        private static void TryApplyLong(LcdCustomDataSection section, string key, long min, long max, ref long value)
        {
            string text;
            long parsed;
            if (section.Values.TryGetValue(key, out text) && long.TryParse(Unquote(text), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                value = Math.Max(min, Math.Min(max, parsed));
        }

        private static void TryApplyInt(LcdCustomDataSection section, string key, int min, int max, ref int value)
        {
            string text;
            int parsed;
            if (section.Values.TryGetValue(key, out text) && int.TryParse(Unquote(text), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                value = Math.Max(min, Math.Min(max, parsed));
        }

        private static void TryApplyFloat(LcdCustomDataSection section, string key, float min, float max, ref float value)
        {
            string text;
            double parsed;
            if (section.Values.TryGetValue(key, out text) && double.TryParse(Unquote(text), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                value = ClampSettingFloat((float)parsed, min, max, value);
        }

        private static void TryApplyColor(LcdCustomDataSection section, string key, ref bool hasValue, ref Color value)
        {
            string text;
            Color parsed;
            if (section.Values.TryGetValue(key, out text) && TryParseDisplayColor(text, out parsed))
            {
                hasValue = true;
                value = parsed;
            }
        }

        private static float ClampInset(float value, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                return 0f;

            return Math.Min(value, Math.Max(0f, max - 1f));
        }

        private static float ClampSettingFloat(float value, float min, float max, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return fallback;

            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

    }
}
