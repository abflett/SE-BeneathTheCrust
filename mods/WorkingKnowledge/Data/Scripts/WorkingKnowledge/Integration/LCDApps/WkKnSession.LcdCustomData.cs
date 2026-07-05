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
        private static void EnsureWkKnDisplayCustomData(VRage.Game.ModAPI.Ingame.IMyCubeBlock block)
        {
            var terminalBlock = block as Sandbox.ModAPI.Ingame.IMyTerminalBlock;
            if (terminalBlock == null)
                return;

            var customData = terminalBlock.CustomData ?? string.Empty;
            var sections = ParseLcdConfigSections(customData);
            var hasRootSection = HasLcdSection(sections, LcdRootSectionName);

            if (!IsResearchSciFiTerminalDisplayBlock(block))
            {
                if (hasRootSection)
                    return;

                var defaultBuilder = new StringBuilder(customData);
                AppendSpacer(defaultBuilder, customData);
                AppendRootConfig(defaultBuilder);
                defaultBuilder.AppendLine();
                AppendCommentedLcdExampleTemplate(defaultBuilder);
                terminalBlock.CustomData = defaultBuilder.ToString();
                return;
            }

            var hasLargeDisplay = HasDisplaySpecificSection(sections, ResearchTerminalLargeDisplayName);
            var hasKeyboard = HasDisplaySpecificSection(sections, ResearchTerminalKeyboardDisplayName);
            if (hasRootSection && hasLargeDisplay && hasKeyboard)
                return;

            var builder = new StringBuilder(customData);
            AppendSpacer(builder, customData);

            var addedRootSection = false;
            if (!hasRootSection)
            {
                AppendRootConfig(builder);
                builder.AppendLine();
                addedRootSection = true;
            }

            var nextConfig = GetNextConfigNumber(sections);
            if (!hasLargeDisplay)
            {
                AppendDisplayConfig(builder, nextConfig++, ResearchTerminalLargeDisplayName, ResearchTerminalLargeDisplayInset);
                builder.AppendLine(TextSizeKey + "=" + ResearchTerminalLargeDisplayTextSize);
                builder.AppendLine();
            }

            if (!hasKeyboard)
            {
                AppendDisplayConfig(builder, nextConfig, ResearchTerminalKeyboardDisplayName, ResearchTerminalKeyboardInset);
                builder.AppendLine(IconPaddingLeftKey + "=" + ResearchTerminalKeyboardIconPaddingLeft);
                builder.AppendLine(IconSizeKey + "=" + ResearchTerminalKeyboardIconSize);
            }

            if (addedRootSection)
            {
                builder.AppendLine();
                AppendCommentedLcdExampleTemplate(builder);
            }

            terminalBlock.CustomData = builder.ToString();
        }

        private static void AppendRootConfig(StringBuilder builder)
        {
            builder.AppendLine(LcdRootSectionHeader);
            builder.AppendLine(InsetKey + "=0,0,0,0");
            builder.AppendLine(PageTicksKey + "=180");
            builder.AppendLine(RowsFirstPageKey + "=7");
            builder.AppendLine(RowsOtherPagesKey + "=11");
            builder.AppendLine(LayoutScaleKey + "=1");
            builder.AppendLine(TextSizeKey + "=1");
            builder.AppendLine("// " + TextColorKey + "=180,255,220");
            builder.AppendLine("// " + BackgroundColorKey + "=0,0,0");
        }

        private static void AppendDisplayConfig(StringBuilder builder, int configNumber, string displayName, string inset)
        {
            builder.AppendLine("[" + LcdRootSectionName + ".Config" + configNumber.ToString(CultureInfo.InvariantCulture) + "]");
            builder.AppendLine(DisplayKey + "=" + displayName);
            builder.AppendLine(InsetKey + "=" + inset);
        }

        private static void AppendCommentedLcdExampleTemplate(StringBuilder builder)
        {
            builder.AppendLine("// --- Optional Working Knowledge LCD examples ---");
            builder.AppendLine("// App selection happens in the LCD Content dropdown, not in Custom Data.");
            builder.AppendLine("// Uncomment a section, edit Display to match the screen name shown by the Calibrator, then tune values.");
            builder.AppendLine("// Pretend three-screen block:");
            builder.AppendLine("//   Screen 1: select Working Knowledge Research");
            builder.AppendLine("//   Screen 2: select Working Knowledge Proficiency");
            builder.AppendLine("//   Screen 3: select Working Knowledge Identity");
            builder.AppendLine("//");
            builder.AppendLine("// Setting notes:");
            builder.AppendLine("//   Display: screen name to match, such as Screen 1, Left, Center, Large Display, or Keyboard.");
            builder.AppendLine("//   Inset: safe area as left,top,right,bottom. Values can be pixels or percentages.");
            builder.AppendLine("//   InsetLeft/Top/Right/Bottom: optional one-side overrides for fine tuning.");
            builder.AppendLine("//   PageTicks: page speed for Research and Proficiency. 60 ticks is about one second.");
            builder.AppendLine("//   RowsFirstPage/RowsOtherPages: max rows on first and later pages.");
            builder.AppendLine("//   LayoutScale: global spacing/layout multiplier from 0.5 to 1.5.");
            builder.AppendLine("//   TextSize: text-only multiplier from 0.2 to 3.0.");
            builder.AppendLine("//   TextColor/BackgroundColor: R,G,B, #RRGGBB, or Black/White/Red/Green/Blue.");
            builder.AppendLine("//   IconPaddingLeft/IconSize: Identity icon position and size.");
            builder.AppendLine("//");
            AppendCommentedLcdExampleSection(builder, "Screen1Research", "Screen 1", true, true, false);
            builder.AppendLine("//");
            AppendCommentedLcdExampleSection(builder, "Screen2Proficiency", "Screen 2", true, true, false);
            builder.AppendLine("//");
            AppendCommentedLcdExampleSection(builder, "Screen3Identity", "Screen 3", true, false, true);
        }

        private static void AppendCommentedLcdExampleSection(
            StringBuilder builder,
            string sectionSuffix,
            string displayName,
            bool includeRows,
            bool includePaging,
            bool includeIcon)
        {
            builder.AppendLine("// [" + LcdRootSectionName + "." + sectionSuffix + "]");
            builder.AppendLine("// " + DisplayKey + "=" + displayName);
            builder.AppendLine("// " + InsetKey + "=0,0,0,0");
            builder.AppendLine("// " + InsetLeftKey + "=0");
            builder.AppendLine("// " + InsetTopKey + "=0");
            builder.AppendLine("// " + InsetRightKey + "=0");
            builder.AppendLine("// " + InsetBottomKey + "=0");

            if (includePaging)
                builder.AppendLine("// " + PageTicksKey + "=180");

            if (includeRows)
            {
                builder.AppendLine("// " + RowsFirstPageKey + "=7");
                builder.AppendLine("// " + RowsOtherPagesKey + "=11");
            }

            builder.AppendLine("// " + LayoutScaleKey + "=1");
            builder.AppendLine("// " + TextSizeKey + "=1");
            builder.AppendLine("// " + TextColorKey + "=180,255,220");
            builder.AppendLine("// " + BackgroundColorKey + "=0,0,0");

            if (includeIcon)
            {
                builder.AppendLine("// " + IconPaddingLeftKey + "=0");
                builder.AppendLine("// " + IconSizeKey + "=1");
            }
        }

        private static void AppendSpacer(StringBuilder builder, string original)
        {
            if (builder.Length == 0)
                return;

            if (!original.EndsWith("\n", StringComparison.Ordinal))
                builder.AppendLine();

            builder.AppendLine();
        }

        private static int GetNextConfigNumber(List<LcdCustomDataSection> sections)
        {
            var next = 1;
            for (var i = 0; i < sections.Count; i++)
            {
                var name = sections[i].Name ?? string.Empty;
                if (!name.StartsWith(LcdRootSectionName + ".Config", StringComparison.OrdinalIgnoreCase))
                    continue;

                var suffix = name.Substring((LcdRootSectionName + ".Config").Length);
                int parsed;
                if (int.TryParse(suffix, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    next = Math.Max(next, parsed + 1);
            }

            return next;
        }

        private static bool HasLcdSection(List<LcdCustomDataSection> sections, string sectionName)
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (string.Equals(sections[i].Name, sectionName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool HasDisplaySpecificSection(List<LcdCustomDataSection> sections, string displayName)
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (!IsDisplayLcdSection(sections[i].Name))
                    continue;

                string value;
                if (sections[i].Values.TryGetValue(DisplayKey, out value) && DisplayNameMatches(Unquote(value), displayName))
                    return true;
            }

            return false;
        }

    }
}
