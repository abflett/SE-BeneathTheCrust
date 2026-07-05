using System;
using System.Collections.Generic;
using System.Globalization;
using VRageMath;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const string LcdRootSectionName = "WkKnLCD";
        private const string LcdRootSectionHeader = "[WkKnLCD]";
        private const string DisplayKey = "Display";
        private const string InsetKey = "Inset";
        private const string InsetLeftKey = "InsetLeft";
        private const string InsetTopKey = "InsetTop";
        private const string InsetRightKey = "InsetRight";
        private const string InsetBottomKey = "InsetBottom";
        private const string PageTicksKey = "PageTicks";
        private const string RowsFirstPageKey = "RowsFirstPage";
        private const string RowsOtherPagesKey = "RowsOtherPages";
        private const string LayoutScaleKey = "LayoutScale";
        private const string TextSizeKey = "TextSize";
        private const string TextColorKey = "TextColor";
        private const string BackgroundColorKey = "BackgroundColor";
        private const string IconPaddingLeftKey = "IconPaddingLeft";
        private const string IconSizeKey = "IconSize";
        private const string ResearchTerminalLargeDisplayName = "Large Display";
        private const string ResearchTerminalKeyboardDisplayName = "Keyboard";
        private const string ResearchTerminalLargeDisplayInset = "1%,13%,1%,13%";
        private const string ResearchTerminalLargeDisplayTextSize = "1.1";
        private const string ResearchTerminalKeyboardInset = "14,28,6,29";
        private const string ResearchTerminalKeyboardIconPaddingLeft = "20";
        private const string ResearchTerminalKeyboardIconSize = "1";

        public struct DisplayLayoutBounds
        {
            public Vector2 Offset;
            public Vector2 Size;
            public float Left;
            public float Top;
            public float Right;
            public float Bottom;

            public string FormatInset()
            {
                return "L " + Left.ToString("0.#", CultureInfo.InvariantCulture) +
                       "  T " + Top.ToString("0.#", CultureInfo.InvariantCulture) +
                       "  R " + Right.ToString("0.#", CultureInfo.InvariantCulture) +
                       "  B " + Bottom.ToString("0.#", CultureInfo.InvariantCulture);
            }
        }

        public struct WkKnLcdSettings
        {
            public DisplayLayoutBounds Layout;
            public long PageTicks;
            public int RowsFirstPage;
            public int RowsOtherPages;
            public float LayoutScale;
            public float TextSize;
            public bool HasTextColor;
            public Color TextColor;
            public bool HasBackgroundColor;
            public Color BackgroundColor;
            public float IconPaddingLeft;
            public float IconSize;
        }

        private sealed class LcdCustomDataSection
        {
            public string Name;
            public Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
