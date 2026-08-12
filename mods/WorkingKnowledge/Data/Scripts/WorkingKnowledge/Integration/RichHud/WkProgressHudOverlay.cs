using System;
using System.Collections.Generic;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using VRageMath;

namespace WkKn
{
    internal sealed class WkProgressHudOverlay
    {
        private const int MaxRows = 10;
        private const int MaxLabelLength = 32;
        private const long FadeOutTicks = 180;
        private const float OverlayWidth = 400f;
        private const float RowHeight = 32f;
        private const float RowSpacing = 19f;
        private const float ScreenMargin = 28f;
        private const float LabelHeight = 25f;
        private const float BarBackgroundHeight = 19f;
        private const float BarInset = 2f;
        private const float ResearchBarHeight = 10f;
        private const float ProficiencyBarHeight = 5f;
        private const float LabelInset = 4f;
        private const float LabelTextSize = 1.02f;
        private static readonly Color LabelColor = new Color(225, 236, 240);
        private static readonly Color LabelShadowColor = new Color(0, 0, 0);
        private static readonly Color BarBackColor = new Color(24, 34, 38);

        private readonly List<Entry> entries = new List<Entry>(MaxRows);
        private readonly Row[] rows = new Row[MaxRows];
        private readonly Color researchColor;
        private readonly Color proficiencyColor;
        private HudChain rowChain;

        internal WkProgressHudOverlay(Color researchColor, Color proficiencyColor)
        {
            this.researchColor = researchColor;
            this.proficiencyColor = proficiencyColor;
        }

        internal void Attach(HudParentBase parent)
        {
            if (rowChain != null || parent == null)
                return;

            rowChain = new HudChain(true, parent)
            {
                Spacing = RowSpacing,
                SizingMode = HudChainSizingModes.FitChainBoth,
                Visible = false,
            };

            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = Row.Create();
                rowChain.Add(rows[i].Container);
            }
        }

        internal void Detach()
        {
            if (rowChain != null)
            {
                rowChain.Visible = false;
                rowChain.Unregister();
                rowChain = null;
            }

            for (var i = 0; i < rows.Length; i++)
                rows[i] = null;
        }

        internal void Update(long currentTick, WkProgressHudSettings settings)
        {
            settings = NormalizeSettings(settings);
            TrimExpired(currentTick, settings);

            if (rowChain == null || !settings.Enabled)
            {
                HideRows();
                return;
            }

            RenderRows(currentTick, settings);
        }

        internal void Clear()
        {
            entries.Clear();
            HideRows();
        }

        internal void Close()
        {
            Clear();
            Detach();
        }

        internal void UpdateCombined(long identityId, string progressId, string displayName, double researchProgress, double proficiencyProgress, long currentTick)
        {
            UpdateEntry(identityId, progressId, displayName, researchProgress, proficiencyProgress, currentTick);
        }

        private void UpdateEntry(long identityId, string progressId, string displayName, double researchProgress, double proficiencyProgress, long currentTick)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(progressId) || !IsLocalIdentity(identityId))
                return;

            Entry entry;
            var index = FindEntryIndex(progressId);
            if (index >= 0)
            {
                entry = entries[index];
                entries.RemoveAt(index);
            }
            else
            {
                entry = new Entry { ProgressId = progressId };
            }

            entry.DisplayName = TruncateLabel(string.IsNullOrWhiteSpace(displayName) ? progressId : displayName);
            entry.ResearchProgress = Clamp01(researchProgress);
            entry.ProficiencyProgress = Clamp01(proficiencyProgress);
            entry.LastUpdatedTick = currentTick;

            entries.Insert(0, entry);
            while (entries.Count > MaxRows)
                entries.RemoveAt(entries.Count - 1);
        }

        private int FindEntryIndex(string progressId)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].ProgressId, progressId, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private void TrimExpired(long currentTick, WkProgressHudSettings settings)
        {
            if (settings.FadeSeconds <= 0.0)
                return;

            var holdTicks = GetHoldTicks(settings);
            for (var i = entries.Count - 1; i >= 0; i--)
            {
                if (currentTick - entries[i].LastUpdatedTick > holdTicks + FadeOutTicks)
                    entries.RemoveAt(i);
            }
        }

        private void RenderRows(long currentTick, WkProgressHudSettings settings)
        {
            var visibleCount = Math.Min(settings.RowCount, entries.Count);
            ApplyPlacement(settings);

            for (var i = 0; i < rows.Length; i++)
            {
                if (i >= visibleCount)
                {
                    rows[i].SetVisible(false);
                    continue;
                }

                var entryIndex = IsAscending(settings.Order) ? i : visibleCount - 1 - i;
                var entry = entries[entryIndex];
                rows[i].Update(
                    entry.DisplayName,
                    entry.ResearchProgress,
                    entry.ProficiencyProgress,
                    GetAlpha(currentTick, entry.LastUpdatedTick, settings),
                    researchColor,
                    proficiencyColor);
            }

            rowChain.Visible = visibleCount > 0;
        }

        private void ApplyPlacement(WkProgressHudSettings settings)
        {
            var xOffset = (float)(settings.OffsetX * HudMain.ScreenDimHighDPI.X * 0.5);
            var yOffset = (float)(settings.OffsetY * HudMain.ScreenDimHighDPI.Y * 0.5);

            switch (NormalizePosition(settings.Position))
            {
                case "topLeft":
                    rowChain.ParentAlignment = ParentAlignments.InnerTopLeft;
                    rowChain.Offset = new Vector2(ScreenMargin + xOffset, -ScreenMargin + yOffset);
                    break;
                case "bottomLeft":
                    rowChain.ParentAlignment = ParentAlignments.InnerBottomLeft;
                    rowChain.Offset = new Vector2(ScreenMargin + xOffset, ScreenMargin + yOffset);
                    break;
                case "bottomRight":
                    rowChain.ParentAlignment = ParentAlignments.InnerBottomRight;
                    rowChain.Offset = new Vector2(-ScreenMargin + xOffset, ScreenMargin + yOffset);
                    break;
                case "center":
                    rowChain.ParentAlignment = ParentAlignments.Center;
                    rowChain.Offset = new Vector2(xOffset, yOffset);
                    break;
                default:
                    rowChain.ParentAlignment = ParentAlignments.InnerTopRight;
                    rowChain.Offset = new Vector2(-ScreenMargin + xOffset, -ScreenMargin + yOffset);
                    break;
            }
        }

        private static byte GetAlpha(long currentTick, long lastUpdatedTick, WkProgressHudSettings settings)
        {
            if (settings.FadeSeconds <= 0.0)
                return 255;

            var age = currentTick - lastUpdatedTick;
            var holdTicks = GetHoldTicks(settings);
            if (age <= holdTicks)
                return 255;

            var fadeAge = age - holdTicks;
            var alpha = 1.0 - Math.Min(1.0, Math.Max(0.0, fadeAge / (double)FadeOutTicks));
            return (byte)Math.Round(255.0 * alpha);
        }

        private static long GetHoldTicks(WkProgressHudSettings settings)
        {
            return (long)Math.Round(RatioMath.Clamp(settings.FadeSeconds, 0.0, 60.0) * 60.0);
        }

        private static WkProgressHudSettings NormalizeSettings(WkProgressHudSettings settings)
        {
            settings.RowCount = settings.RowCount <= 0 ? 5 : (int)RatioMath.Clamp(settings.RowCount, 1, MaxRows);
            settings.Order = NormalizeOrder(settings.Order);
            settings.Position = NormalizePosition(settings.Position);
            settings.OffsetX = RatioMath.Clamp(settings.OffsetX, -2.0, 2.0);
            settings.OffsetY = RatioMath.Clamp(settings.OffsetY, -2.0, 2.0);
            settings.FadeSeconds = settings.FadeSeconds < 0.0 ? 6.0 : RatioMath.Clamp(settings.FadeSeconds, 0.0, 60.0);
            return settings;
        }

        private static string NormalizeOrder(string order)
        {
            return string.Equals(order, "ascending", StringComparison.OrdinalIgnoreCase) ? "ascending" : "descending";
        }

        private static string NormalizePosition(string position)
        {
            if (string.Equals(position, "topLeft", StringComparison.OrdinalIgnoreCase))
                return "topLeft";
            if (string.Equals(position, "bottomLeft", StringComparison.OrdinalIgnoreCase))
                return "bottomLeft";
            if (string.Equals(position, "bottomRight", StringComparison.OrdinalIgnoreCase))
                return "bottomRight";
            if (string.Equals(position, "center", StringComparison.OrdinalIgnoreCase))
                return "center";

            return "topRight";
        }

        private static bool IsAscending(string order)
        {
            return string.Equals(order, "ascending", StringComparison.OrdinalIgnoreCase);
        }

        private void HideRows()
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                    rows[i].SetVisible(false);
            }

            if (rowChain != null)
                rowChain.Visible = false;
        }

        private static bool IsLocalIdentity(long identityId)
        {
            return MyAPIGateway.Session != null &&
                   MyAPIGateway.Session.Player != null &&
                   MyAPIGateway.Session.Player.IdentityId == identityId;
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;
            if (value > 1.0)
                return 1.0;
            return value;
        }

        private static string TruncateLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return "Progress";

            var trimmed = label.Trim();
            if (trimmed.Length <= MaxLabelLength)
                return trimmed;

            return trimmed.Substring(0, MaxLabelLength - 3) + "...";
        }

        private static Color WithAlpha(Color color, byte alpha)
        {
            return new Color(color.R, color.G, color.B, alpha);
        }

        private sealed class Entry
        {
            internal string ProgressId;
            internal string DisplayName;
            internal double ResearchProgress;
            internal double ProficiencyProgress;
            internal long LastUpdatedTick;
        }

        private sealed class Row
        {
            private readonly Label labelShadowUp;
            private readonly Label labelShadowDown;
            private readonly Label labelShadowCross;
            private readonly Label label;
            private readonly TexturedBox barBackground;
            private readonly TexturedBox researchFill;
            private readonly TexturedBox proficiencyFill;
            private string currentLabel;
            private byte currentAlpha;

            private Row(EmptyHudElement container)
            {
                Container = container;

                labelShadowUp = CreateLabelShadow(container, new Vector2(LabelInset - 1.5f, 1.5f));
                labelShadowDown = CreateLabelShadow(container, new Vector2(LabelInset + 1.5f, -1.5f));
                labelShadowCross = CreateLabelShadow(container, new Vector2(LabelInset - 1.5f, -1.5f));

                label = new Label(container)
                {
                    AutoResize = false,
                    VertCenterText = true,
                    Size = new Vector2(OverlayWidth, LabelHeight),
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Offset = new Vector2(LabelInset, 0f),
                    BuilderMode = TextBuilderModes.Unlined,
                    ZOffset = 2,
                };

                barBackground = new TexturedBox(container)
                {
                    Size = new Vector2(OverlayWidth, BarBackgroundHeight),
                    ParentAlignment = ParentAlignments.InnerBottomLeft,
                    Color = WithAlpha(BarBackColor, 130),
                };

                researchFill = new TexturedBox(barBackground)
                {
                    Size = new Vector2(OverlayWidth - (BarInset * 2f), ResearchBarHeight),
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Offset = new Vector2(BarInset, -BarInset),
                };

                proficiencyFill = new TexturedBox(barBackground)
                {
                    Size = new Vector2(OverlayWidth - (BarInset * 2f), ProficiencyBarHeight),
                    ParentAlignment = ParentAlignments.InnerBottomLeft,
                    Offset = new Vector2(BarInset, BarInset),
                };

                SetVisible(false);
            }

            internal EmptyHudElement Container { get; private set; }

            internal static Row Create()
            {
                return new Row(new EmptyHudElement
                {
                    Size = new Vector2(OverlayWidth, RowHeight),
                    ParentAlignment = ParentAlignments.InnerLeft,
                    Visible = false,
                });
            }

            internal void Update(string labelText, double researchProgress, double proficiencyProgress, byte alpha, Color researchColor, Color proficiencyColor)
            {
                var labelFormat = new GlyphFormat(WithAlpha(LabelColor, alpha), TextAlignment.Left, LabelTextSize);
                var shadowAlpha = (byte)(alpha * 240 / 255);
                var shadowFormat = new GlyphFormat(WithAlpha(LabelShadowColor, shadowAlpha), TextAlignment.Left, LabelTextSize);

                if (!string.Equals(currentLabel, labelText, StringComparison.Ordinal))
                {
                    label.TextBoard.SetText(labelText, labelFormat);
                    labelShadowUp.TextBoard.SetText(labelText, shadowFormat);
                    labelShadowDown.TextBoard.SetText(labelText, shadowFormat);
                    labelShadowCross.TextBoard.SetText(labelText, shadowFormat);
                    currentLabel = labelText;
                }
                else if (currentAlpha != alpha)
                {
                    label.TextBoard.SetFormatting(labelFormat);
                    labelShadowUp.TextBoard.SetFormatting(shadowFormat);
                    labelShadowDown.TextBoard.SetFormatting(shadowFormat);
                    labelShadowCross.TextBoard.SetFormatting(shadowFormat);
                }

                var innerWidth = OverlayWidth - (BarInset * 2f);
                researchFill.Width = Math.Max(0.5f, innerWidth * (float)Clamp01(researchProgress));
                proficiencyFill.Width = Math.Max(0.5f, innerWidth * (float)Clamp01(proficiencyProgress));
                researchFill.Color = WithAlpha(researchColor, alpha);
                proficiencyFill.Color = WithAlpha(proficiencyColor, alpha);
                barBackground.Color = WithAlpha(BarBackColor, (byte)(alpha * 130 / 255));
                currentAlpha = alpha;
                SetVisible(alpha > 0);
            }

            private static Label CreateLabelShadow(EmptyHudElement container, Vector2 offset)
            {
                return new Label(container)
                {
                    AutoResize = false,
                    VertCenterText = true,
                    Size = new Vector2(OverlayWidth, LabelHeight),
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Offset = offset,
                    BuilderMode = TextBuilderModes.Unlined,
                    ZOffset = 1,
                };
            }

            internal void SetVisible(bool visible)
            {
                Container.Visible = visible;
            }
        }
    }
}
