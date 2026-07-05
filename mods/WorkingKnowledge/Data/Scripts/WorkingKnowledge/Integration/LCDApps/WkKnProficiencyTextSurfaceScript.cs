using System;
using System.Globalization;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace WkKn
{
    [MyTextSurfaceScript(WkKnSession.ProficiencyTextSurfaceScriptId, "Working Knowledge Proficiency")]
    public class WkKnProficiencyTextSurfaceScript : WkKnTextSurfaceScriptBase
    {
        private const long DisplayPageTicks = 180;
        private const int MaximumRowsPerPage = 7;
        private const int MaximumRowsWithoutHeader = 11;
        private const float HeaderEntryGap = 32f;
        private long displayPageTicks = DisplayPageTicks;
        private int maximumRowsPerPage = MaximumRowsPerPage;
        private int maximumRowsWithoutHeader = MaximumRowsWithoutHeader;

        private struct EntryPageLayout
        {
            public int Page;
            public int PageCount;
            public int Start;
            public int End;
            public bool ShowHeader;
        }

        public override ScriptUpdate NeedsUpdate
        {
            get { return ScriptUpdate.Update100; }
        }

        public WkKnProficiencyTextSurfaceScript(Sandbox.ModAPI.Ingame.IMyTextSurface surface, VRage.Game.ModAPI.Ingame.IMyCubeBlock block, Vector2 size)
            : base(surface, block, size)
        {
        }

        public override void Run()
        {
            base.Run();

            var snapshot = WkKnSession.GetProficiencyDisplaySnapshot();
            var surfaceSize = GetDrawableSize(new Vector2(512f, 512f));
            var settings = WkKnSession.GetWkKnLcdSettings(
                m_block,
                m_surface,
                surfaceSize,
                WkKnSession.CreateWkKnLcdSettings(surfaceSize, "0,0,0,0", DisplayPageTicks, MaximumRowsPerPage, MaximumRowsWithoutHeader, 1f, 0f, 1f));
            var size = settings.Layout.Size;
            drawOffset = settings.Layout.Offset;
            displayPageTicks = settings.PageTicks;
            maximumRowsPerPage = settings.RowsFirstPage;
            maximumRowsWithoutHeader = settings.RowsOtherPages;
            textSize = settings.TextSize;
            layoutScale = GetLayoutScale(size) * settings.LayoutScale;
            var padding = ClampLayout(Math.Min(size.X, size.Y) * 0.055f, 18f, 38f);
            var background = GetDisplayBackgroundColor(settings);
            var accent = GetColorShade(GetReadableForegroundColor(settings), background, 0);
            var dim = GetColorShade(accent, background, 4);
            var muted = GetColorShade(accent, background, 7);

            using (var frame = m_surface.DrawFrame())
            {
                AddRawRectangle(frame, surfaceSize * 0.5f, surfaceSize, background);

                if (snapshot.Entries == null || snapshot.Entries.Count == 0)
                {
                    var emptyY = DrawHeader(frame, snapshot, size, padding, padding, accent, background);
                    DrawEmptyState(frame, snapshot, size, padding, emptyY, accent, dim);
                    DrawFooter(frame, snapshot, new EntryPageLayout { Page = 0, PageCount = 1 }, size, padding, accent, background);
                    return;
                }

                var pageLayout = GetEntryPageLayout(snapshot, size, padding);
                var y = padding;
                if (pageLayout.ShowHeader)
                    y = DrawHeader(frame, snapshot, size, padding, y, accent, background);

                DrawEntries(frame, snapshot, pageLayout, size, padding, y, accent, dim, muted, background);
            }
        }




        private float DrawHeader(MySpriteDrawFrame frame, ProficiencyDisplaySnapshot snapshot, Vector2 size, float padding, float y, Color accent, Color background)
        {
            const string title = "Schematic Proficiencies";
            var headerTop = y;
            var statsScale = ScaleText(ClampText(size.Y / 960f, 0.34f, 0.46f));
            var statsPadding = ClampLayout(size.Y * 0.012f, 5f, 9f);
            var statsLineHeight = MeasureTextHeight("Mastered", statsScale, 16f) + Scaled(2f);
            var statsBoxWidth = ClampLayout(size.X * 0.22f, 122f, 184f);
            var statsBoxHeight = statsLineHeight * 3f + statsPadding * 2f;
            var statsLeft = size.X - padding - statsBoxWidth;
            var titleMaxWidth = Math.Max(Scaled(90f), statsLeft - padding - Scaled(14f));
            var titleScale = FitTextScale(title, titleMaxWidth, ScaleText(ClampText(size.Y / 590f, 0.74f, 1.0f)), ScaleText(Scaled(0.52f)));
            var playerScale = ScaleText(ClampText(size.Y / 780f, 0.42f, 0.60f));
            var titleHeight = MeasureTextHeight(title, titleScale, 34f);

            DrawStatsPanel(frame, snapshot, new Vector2(statsLeft, headerTop), new Vector2(statsBoxWidth, statsBoxHeight), statsPadding, statsScale, accent, background);
            AddText(frame, title, new Vector2(padding, y), titleScale, accent, TextAlignment.LEFT);
            y += titleHeight + Scaled(6f);

            var playerName = TrimToWidth(BuildPlayerName(snapshot), titleMaxWidth, playerScale);
            AddText(frame, playerName, new Vector2(padding, y), playerScale, accent, TextAlignment.LEFT);
            y += MeasureTextHeight(playerName, playerScale, 18f);

            y = Math.Max(y, headerTop + statsBoxHeight) + Scaled(12f);
            return y + Scaled(HeaderEntryGap);
        }

        private float MeasureHeaderBottom(ProficiencyDisplaySnapshot snapshot, Vector2 size, float padding, float y)
        {
            const string title = "Schematic Proficiencies";
            var headerTop = y;
            var statsScale = ScaleText(ClampText(size.Y / 960f, 0.34f, 0.46f));
            var statsPadding = ClampLayout(size.Y * 0.012f, 5f, 9f);
            var statsLineHeight = MeasureTextHeight("Mastered", statsScale, 16f) + Scaled(2f);
            var statsBoxWidth = ClampLayout(size.X * 0.22f, 122f, 184f);
            var statsBoxHeight = statsLineHeight * 3f + statsPadding * 2f;
            var statsLeft = size.X - padding - statsBoxWidth;
            var titleMaxWidth = Math.Max(Scaled(90f), statsLeft - padding - Scaled(14f));
            var titleScale = FitTextScale(title, titleMaxWidth, ScaleText(ClampText(size.Y / 590f, 0.74f, 1.0f)), ScaleText(Scaled(0.52f)));
            var playerScale = ScaleText(ClampText(size.Y / 780f, 0.42f, 0.60f));

            y += MeasureTextHeight(title, titleScale, 34f) + Scaled(6f);
            y += MeasureTextHeight(TrimToWidth(BuildPlayerName(snapshot), titleMaxWidth, playerScale), playerScale, 18f);
            y = Math.Max(y, headerTop + statsBoxHeight) + Scaled(12f);
            return y + Scaled(HeaderEntryGap);
        }

        private static string BuildPlayerName(ProficiencyDisplaySnapshot snapshot)
        {
            return snapshot == null || string.IsNullOrWhiteSpace(snapshot.PlayerName) ? "Local Player" : snapshot.PlayerName;
        }

        private void DrawEmptyState(MySpriteDrawFrame frame, ProficiencyDisplaySnapshot snapshot, Vector2 size, float padding, float y, Color accent, Color dim)
        {
            var message = string.IsNullOrWhiteSpace(snapshot.Message) ? "No active Proficiency in progress." : snapshot.Message;
            var scale = FitTextScale(message, size.X - padding * 2f, ScaleText(ClampText(size.Y / 690f, 0.52f, 0.74f)), ScaleText(Scaled(0.36f)));
            var lines = WrapText(message, size.X - padding * 2f, scale, 3);
            var lineHeight = MeasureTextHeight("Proficiency", scale, 24f) + Scaled(4f);
            var blockHeight = lines.Count * lineHeight;
            var startY = Math.Max(y, size.Y * 0.5f - blockHeight * 0.5f);

            for (var i = 0; i < lines.Count; i++)
                AddText(frame, lines[i], new Vector2(size.X * 0.5f, startY + i * lineHeight), scale, i == 0 ? accent : dim, TextAlignment.CENTER);
        }

        private EntryPageLayout GetEntryPageLayout(ProficiencyDisplaySnapshot snapshot, Vector2 size, float padding)
        {
            var footerScale = ScaleText(ClampText(size.Y / 900f, 0.36f, 0.50f));
            var footerHeight = MeasureTextHeight("Page", footerScale, 16f) + padding;
            var rowHeight = GetEntryRowHeight(size);
            var headerBottom = MeasureHeaderBottom(snapshot, size, padding, padding);
            var firstPageHeight = Math.Max(rowHeight, size.Y - headerBottom - footerHeight - padding);
            var laterPageHeight = Math.Max(rowHeight, size.Y - padding - footerHeight - padding);
            var firstPageRows = Math.Max(1, Math.Min(maximumRowsPerPage, (int)(firstPageHeight / rowHeight)));
            var laterPageRows = Math.Max(1, Math.Min(maximumRowsWithoutHeader, (int)(laterPageHeight / rowHeight)));
            var remainingRows = Math.Max(0, snapshot.Entries.Count - firstPageRows);
            var pageCount = 1 + ((remainingRows + laterPageRows - 1) / laterPageRows);
            var cycleTick = WkKnSession.GetProficiencyDisplayCycleTick();
            var page = (int)((cycleTick / displayPageTicks) % pageCount);

            if (page == 0)
            {
                return new EntryPageLayout
                {
                    Page = page,
                    PageCount = pageCount,
                    Start = 0,
                    End = Math.Min(snapshot.Entries.Count, firstPageRows),
                    ShowHeader = true,
                };
            }

            var start = firstPageRows + ((page - 1) * laterPageRows);
            return new EntryPageLayout
            {
                Page = page,
                PageCount = pageCount,
                Start = start,
                End = Math.Min(snapshot.Entries.Count, start + laterPageRows),
                ShowHeader = false,
            };
        }

        private void DrawEntries(MySpriteDrawFrame frame, ProficiencyDisplaySnapshot snapshot, EntryPageLayout pageLayout, Vector2 size, float padding, float y, Color accent, Color dim, Color muted, Color background)
        {
            var rowScale = ScaleText(ClampText(size.Y / 875f, 0.38f, 0.53f));
            var percentScale = ScaleText(ClampText(size.Y / 900f, 0.36f, 0.50f));
            var rowHeight = GetEntryRowHeight(size);
            var start = pageLayout.Start;
            var end = pageLayout.End;
            var barWidth = size.X - padding * 2f;
            var barHeight = ClampLayout(size.Y / 122f, 3f, 6f);
            var percentWidth = Math.Max(Scaled(58f), size.X * 0.16f);
            var nameWidth = Math.Max(Scaled(60f), barWidth - percentWidth - Scaled(12f));

            for (var i = start; i < end; i++)
            {
                var entry = snapshot.Entries[i];
                var rowTop = y + (i - start) * rowHeight;
                var percent = FormatPercent(entry.Progress);
                var name = TrimToWidth(string.IsNullOrWhiteSpace(entry.DisplayName) ? entry.ProficiencyId : entry.DisplayName, nameWidth, rowScale);
                var barFill = GetColorShade(accent, background, 2);
                var textY = rowTop + Scaled(2f);

                AddText(frame, name, new Vector2(padding, textY), rowScale, accent, TextAlignment.LEFT);
                AddText(frame, percent, new Vector2(size.X - padding, textY), percentScale, accent, TextAlignment.RIGHT);

                var barCenter = new Vector2(size.X * 0.5f, rowTop + MeasureTextHeight("Skill", rowScale, 20f) + Scaled(13f));
                AddProgressBar(frame, barCenter, new Vector2(barWidth, barHeight), (float)entry.Progress, muted, barFill);
            }

            DrawFooter(frame, snapshot, pageLayout, size, padding, accent, background);
        }

        private void DrawFooter(MySpriteDrawFrame frame, ProficiencyDisplaySnapshot snapshot, EntryPageLayout pageLayout, Vector2 size, float padding, Color accent, Color background)
        {
            var footerScale = ScaleText(ClampText(size.Y / 930f, 0.34f, 0.48f));
            var footerPage = (pageLayout.Page + 1).ToString(CultureInfo.InvariantCulture) +
                             "/" +
                             Math.Max(1, pageLayout.PageCount).ToString(CultureInfo.InvariantCulture);
            var footerTextHeight = MeasureTextHeight(footerPage, footerScale, 16f);
            var footerBarHeight = ClampLayout(footerTextHeight + Scaled(10f), 22f, 34f);
            var footerBarCenter = new Vector2(size.X * 0.5f, size.Y - footerBarHeight * 0.5f);
            var footerY = footerBarCenter.Y - footerTextHeight * 0.5f - Scaled(1f);
            var leftWidth = Math.Max(Scaled(64f), size.X * 0.55f);
            var rightWidth = Math.Max(Scaled(48f), size.X - padding * 2f - leftWidth - Scaled(12f));
            var left = TrimToWidth(BuildPlayerName(snapshot), leftWidth, footerScale);
            var right = TrimToWidth(footerPage, rightWidth, footerScale);
            var footerFillColor = GetColorShade(accent, background, 3);
            var footerTextColor = GetColorShade(accent, background, 9);

            AddRectangle(frame, footerBarCenter, new Vector2(size.X, footerBarHeight), footerFillColor);
            AddText(frame, left, new Vector2(padding + Scaled(8f), footerY), footerScale, footerTextColor, TextAlignment.LEFT);
            AddText(frame, right, new Vector2(size.X - padding - Scaled(8f), footerY), footerScale, footerTextColor, TextAlignment.RIGHT);
        }


        private void DrawStatsPanel(MySpriteDrawFrame frame, ProficiencyDisplaySnapshot snapshot, Vector2 topLeft, Vector2 size, float padding, float scale, Color accent, Color background)
        {
            var center = topLeft + size * 0.5f;
            var lineHeight = MeasureTextHeight("Mastered", scale, 16f) + Scaled(2f);
            var labelX = topLeft.X + padding;
            var numberX = topLeft.X + size.X - padding;
            var y = topLeft.Y + padding;
            var panelFillColor = GetColorShade(accent, background, 3);
            var panelTextColor = GetColorShade(accent, background, 9);

            AddRectangle(frame, center, size, panelFillColor);
            AddText(frame, "Active", new Vector2(labelX, y), scale, panelTextColor, TextAlignment.LEFT);
            AddText(frame, (snapshot == null || snapshot.Entries == null ? 0 : snapshot.Entries.Count).ToString(CultureInfo.InvariantCulture), new Vector2(numberX, y), scale, panelTextColor, TextAlignment.RIGHT);
            y += lineHeight;
            AddText(frame, "Mastered", new Vector2(labelX, y), scale, panelTextColor, TextAlignment.LEFT);
            AddText(frame, (snapshot == null ? 0 : snapshot.MasteredCount).ToString(CultureInfo.InvariantCulture), new Vector2(numberX, y), scale, panelTextColor, TextAlignment.RIGHT);
            y += lineHeight;
            AddText(frame, "Tracked", new Vector2(labelX, y), scale, panelTextColor, TextAlignment.LEFT);
            AddText(frame, (snapshot == null ? 0 : snapshot.TrackedCount).ToString(CultureInfo.InvariantCulture), new Vector2(numberX, y), scale, panelTextColor, TextAlignment.RIGHT);
        }

        private float GetEntryRowHeight(Vector2 size)
        {
            return ClampLayout(size.Y / 8.55f, 48f, 66f);
        }
    }
}
