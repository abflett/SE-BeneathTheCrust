using System;
using System.Collections.Generic;
using System.Text;
using Draygo.API;
using Sandbox.ModAPI;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace WkKn
{
    internal sealed class WkProgressHudOverlay
    {
        private const int MaxRows = 5;
        private const int MaxLabelLength = 32;
        private const long HoldTicks = 360;
        private const long FadeTicks = 180;
        private const double LabelScale = 1.05;
        private const double RowSpacing = 0.095;
        private const double AnchorX = 0.57;
        private const double AnchorY = 0.94;
        private const double BarLeftX = 0.57;
        private const double ResearchBarY = -0.028;
        private const double ProficiencyBarY = -0.041;
        private const double BarWidth = 0.40;
        private const double BarHeight = 0.016;
        private const double ProficiencyBarHeight = 0.0112;
        private const double BarBackgroundPaddingX = 0.002;
        private const double BarBackgroundPaddingY = 0.003;
        private const double BarBackgroundWidth = BarWidth + (BarBackgroundPaddingX * 2.0);
        private const double BarBackgroundHeight = (ResearchBarY + (BarHeight * 0.5)) - (ProficiencyBarY - (ProficiencyBarHeight * 0.5)) + (BarBackgroundPaddingY * 2.0);
        private const double BarBackgroundY = ((ResearchBarY + (BarHeight * 0.5)) + (ProficiencyBarY - (ProficiencyBarHeight * 0.5))) * 0.5;
        private const double BarScale = 0.20;
        private const double ShadowOffsetX = 0.0015;
        private const double ShadowOffsetY = 0.0015;
        private static readonly Color LabelColor = new Color(225, 236, 240);
        private static readonly Color LabelShadowColor = new Color(0, 0, 0);
        private static readonly Color BarBackColor = new Color(24, 34, 38);
        private static readonly MyStringId BarMaterial = MyStringId.GetOrCompute("Square");

        private readonly List<Entry> entries = new List<Entry>(MaxRows);
        private readonly Row[] rows = new Row[MaxRows];
        private readonly Color researchColor;
        private readonly Color proficiencyColor;
        private HudAPIv2 api;
        private bool apiReady;

        internal WkProgressHudOverlay(Color researchColor, Color proficiencyColor)
        {
            this.researchColor = researchColor;
            this.proficiencyColor = proficiencyColor;
        }

        internal void Initialize()
        {
            if (api != null || MyAPIGateway.Utilities == null || MyAPIGateway.Utilities.IsDedicated)
                return;

            api = new HudAPIv2();
        }

        internal void Update(long currentTick)
        {
            EnsureRowsCreated();
            if (!apiReady)
                return;

            TrimExpired(currentTick);
            RenderRows(currentTick);
        }

        internal void Clear()
        {
            entries.Clear();
            HideRows();
        }

        internal void Close()
        {
            Clear();

            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                {
                    rows[i].Close();
                    rows[i] = null;
                }
            }

            apiReady = false;
            if (api != null)
            {
                api.Close();
                api = null;
            }
        }

        internal void UpdateResearch(long identityId, string progressId, string displayName, double progress, long currentTick)
        {
            UpdateEntry(identityId, progressId, displayName, progress, null, currentTick);
        }

        internal void UpdateProficiency(long identityId, string progressId, string displayName, double progress, long currentTick)
        {
            UpdateEntry(identityId, progressId, displayName, null, progress, currentTick);
        }

        internal void UpdateCombined(long identityId, string progressId, string displayName, double researchProgress, double proficiencyProgress, long currentTick)
        {
            UpdateEntry(identityId, progressId, displayName, researchProgress, proficiencyProgress, currentTick);
        }

        private void EnsureRowsCreated()
        {
            if (apiReady || api == null || !api.Heartbeat)
                return;

            apiReady = true;
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null)
                    rows[i] = Row.Create();
            }

            HideRows();
        }

        private void UpdateEntry(long identityId, string progressId, string displayName, double? researchProgress, double? proficiencyProgress, long currentTick)
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
            if (researchProgress.HasValue)
                entry.ResearchProgress = Clamp01(researchProgress.Value);
            if (proficiencyProgress.HasValue)
                entry.ProficiencyProgress = Clamp01(proficiencyProgress.Value);
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

        private void TrimExpired(long currentTick)
        {
            for (var i = entries.Count - 1; i >= 0; i--)
            {
                if (currentTick - entries[i].LastUpdatedTick > HoldTicks + FadeTicks)
                    entries.RemoveAt(i);
            }
        }

        private void RenderRows(long currentTick)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (i >= entries.Count)
                {
                    rows[i].SetVisible(false);
                    continue;
                }

                var entry = entries[entries.Count - 1 - i];
                rows[i].Update(
                    entry.DisplayName,
                    entry.ResearchProgress,
                    entry.ProficiencyProgress,
                    GetAlpha(currentTick, entry.LastUpdatedTick),
                    AnchorY - (RowSpacing * i),
                    researchColor,
                    proficiencyColor);
            }
        }

        private static byte GetAlpha(long currentTick, long lastUpdatedTick)
        {
            var age = currentTick - lastUpdatedTick;
            if (age <= HoldTicks)
                return 255;

            var fadeAge = age - HoldTicks;
            var alpha = 1.0 - Math.Min(1.0, Math.Max(0.0, fadeAge / (double)FadeTicks));
            return (byte)Math.Round(255.0 * alpha);
        }

        private void HideRows()
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                    rows[i].SetVisible(false);
            }
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
            private readonly StringBuilder labelBuilder;
            private readonly StringBuilder labelShadowUpBuilder;
            private readonly StringBuilder labelShadowDownBuilder;
            private readonly StringBuilder labelShadowCrossBuilder;
            private readonly HudAPIv2.HUDMessage labelShadowUp;
            private readonly HudAPIv2.HUDMessage labelShadowDown;
            private readonly HudAPIv2.HUDMessage labelShadowCross;
            private readonly HudAPIv2.HUDMessage label;
            private readonly HudAPIv2.BillBoardHUDMessage barBack;
            private readonly HudAPIv2.BillBoardHUDMessage researchFill;
            private readonly HudAPIv2.BillBoardHUDMessage proficiencyFill;

            private Row(
                StringBuilder labelBuilder,
                StringBuilder labelShadowUpBuilder,
                StringBuilder labelShadowDownBuilder,
                StringBuilder labelShadowCrossBuilder,
                HudAPIv2.HUDMessage labelShadowUp,
                HudAPIv2.HUDMessage labelShadowDown,
                HudAPIv2.HUDMessage labelShadowCross,
                HudAPIv2.HUDMessage label,
                HudAPIv2.BillBoardHUDMessage barBack,
                HudAPIv2.BillBoardHUDMessage researchFill,
                HudAPIv2.BillBoardHUDMessage proficiencyFill)
            {
                this.labelBuilder = labelBuilder;
                this.labelShadowUpBuilder = labelShadowUpBuilder;
                this.labelShadowDownBuilder = labelShadowDownBuilder;
                this.labelShadowCrossBuilder = labelShadowCrossBuilder;
                this.labelShadowUp = labelShadowUp;
                this.labelShadowDown = labelShadowDown;
                this.labelShadowCross = labelShadowCross;
                this.label = label;
                this.barBack = barBack;
                this.researchFill = researchFill;
                this.proficiencyFill = proficiencyFill;
            }

            internal static Row Create()
            {
                var barBack = CreateBar(BarBackColor, BarBackgroundHeight);
                var researchFill = CreateBar(WkProgressHudOverlay.researchColorFallback);
                var proficiencyFill = CreateBar(WkProgressHudOverlay.proficiencyColorFallback);

                var labelShadowUpBuilder = new StringBuilder(MaxLabelLength);
                var labelShadowUp = new HudAPIv2.HUDMessage(labelShadowUpBuilder, Vector2D.Zero, null, -1, LabelScale, true, false, null, BlendTypeEnum.PostPP);
                labelShadowUp.Visible = false;

                var labelShadowDownBuilder = new StringBuilder(MaxLabelLength);
                var labelShadowDown = new HudAPIv2.HUDMessage(labelShadowDownBuilder, Vector2D.Zero, null, -1, LabelScale, true, false, null, BlendTypeEnum.PostPP);
                labelShadowDown.Visible = false;

                var labelShadowCrossBuilder = new StringBuilder(MaxLabelLength);
                var labelShadowCross = new HudAPIv2.HUDMessage(labelShadowCrossBuilder, Vector2D.Zero, null, -1, LabelScale, true, false, null, BlendTypeEnum.PostPP);
                labelShadowCross.Visible = false;

                var labelBuilder = new StringBuilder(MaxLabelLength);
                var label = new HudAPIv2.HUDMessage(labelBuilder, Vector2D.Zero, null, -1, LabelScale, true, false, null, BlendTypeEnum.PostPP);
                label.Visible = false;

                return new Row(labelBuilder, labelShadowUpBuilder, labelShadowDownBuilder, labelShadowCrossBuilder, labelShadowUp, labelShadowDown, labelShadowCross, label, barBack, researchFill, proficiencyFill);
            }

            internal void Update(string labelText, double researchProgress, double proficiencyProgress, byte alpha, double rowY, Color researchColor, Color proficiencyColor)
            {
                UpdateLabel(labelShadowUpBuilder, labelShadowUp, labelText, LabelShadowColor, alpha, AnchorX - ShadowOffsetX, rowY + ShadowOffsetY);
                UpdateLabel(labelShadowDownBuilder, labelShadowDown, labelText, LabelShadowColor, alpha, AnchorX + ShadowOffsetX, rowY - ShadowOffsetY);
                UpdateLabel(labelShadowCrossBuilder, labelShadowCross, labelText, LabelShadowColor, alpha, AnchorX - ShadowOffsetX, rowY - ShadowOffsetY);

                UpdateLabel(labelBuilder, label, labelText, LabelColor, alpha, AnchorX, rowY);

                UpdateBar(barBack, BarLeftX - BarBackgroundPaddingX, rowY + BarBackgroundY, BarBackgroundWidth, BarBackgroundHeight, WithAlpha(BarBackColor, (byte)(alpha * 130 / 255)));
                UpdateBar(researchFill, BarLeftX, rowY + ResearchBarY, BarWidth * Clamp01(researchProgress), BarHeight, WithAlpha(researchColor, alpha));
                UpdateBar(proficiencyFill, BarLeftX, rowY + ProficiencyBarY, BarWidth * Clamp01(proficiencyProgress), ProficiencyBarHeight, WithAlpha(proficiencyColor, alpha));
            }

            internal void SetVisible(bool visible)
            {
                labelShadowUp.Visible = visible;
                labelShadowDown.Visible = visible;
                labelShadowCross.Visible = visible;
                label.Visible = visible;
                barBack.Visible = visible;
                researchFill.Visible = visible;
                proficiencyFill.Visible = visible;
            }

            internal void Close()
            {
                labelShadowUp.DeleteMessage();
                labelShadowDown.DeleteMessage();
                labelShadowCross.DeleteMessage();
                label.DeleteMessage();
                barBack.DeleteMessage();
                researchFill.DeleteMessage();
                proficiencyFill.DeleteMessage();
            }

            private static HudAPIv2.BillBoardHUDMessage CreateBar(Color color)
            {
                return CreateBar(color, BarHeight);
            }

            private static HudAPIv2.BillBoardHUDMessage CreateBar(Color color, double height)
            {
                var bar = new HudAPIv2.BillBoardHUDMessage(BarMaterial, Vector2D.Zero, color, null, -1, BarScale, 1f, (float)(height / BarScale), 0f, true, false, BlendTypeEnum.PostPP);
                bar.Visible = false;
                return bar;
            }

            private static void UpdateBar(HudAPIv2.BillBoardHUDMessage bar, double leftX, double y, double width, double height, Color color)
            {
                width = Math.Max(0.001, width);
                bar.Origin = new Vector2D(leftX + (width * 0.5), y);
                bar.Width = (float)(width / BarScale);
                bar.Height = (float)(height / BarScale);
                bar.BillBoardColor = color;
                bar.Visible = color.A > 0;
            }

            private static void UpdateLabel(StringBuilder builder, HudAPIv2.HUDMessage message, string labelText, Color color, byte alpha, double x, double y)
            {
                builder.Clear();
                AppendColorTag(builder, color, alpha);
                builder.Append(labelText);
                message.Origin = new Vector2D(x, y);
                message.Visible = alpha > 0;
            }

            private static void AppendColorTag(StringBuilder builder, Color color, byte alpha)
            {
                builder.Append("<color=")
                    .Append(color.R)
                    .Append(',')
                    .Append(color.G)
                    .Append(',')
                    .Append(color.B)
                    .Append(',')
                    .Append(alpha)
                    .Append('>');
            }
        }

        private static readonly Color researchColorFallback = new Color(140, 220, 95);
        private static readonly Color proficiencyColorFallback = new Color(95, 205, 230);
    }
}
