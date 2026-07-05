using System;
using System.Globalization;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace WkKn
{
    [MyTextSurfaceScript(WkKnSession.ResearchTextSurfaceScriptId, "Working Knowledge Research")]
    public class WkKnResearchTextSurfaceScript : WkKnTextSurfaceScriptBase
    {
        private const long DisplayPageTicks = 180;
        private const int MaximumRowsWithHeader = 7;
        private const int MaximumRowsWithoutHeader = 11;
        private const float HeaderEntryGap = 32f;
        private long displayPageTicks = DisplayPageTicks;
        private int maximumRowsWithHeader = MaximumRowsWithHeader;
        private int maximumRowsWithoutHeader = MaximumRowsWithoutHeader;

        private struct DisplayPage
        {
            public ResearchDisplayScopeSnapshot Scope;
            public int ScopeIndex;
            public int ScopeCount;
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

        public WkKnResearchTextSurfaceScript(Sandbox.ModAPI.Ingame.IMyTextSurface surface, VRage.Game.ModAPI.Ingame.IMyCubeBlock block, Vector2 size)
            : base(surface, block, size)
        {
        }

        public override void Run()
        {
            base.Run();

            var snapshot = WkKnSession.GetResearchDisplaySnapshot();
            var surfaceSize = GetDrawableSize(new Vector2(512f, 512f));
            var settings = WkKnSession.GetWkKnLcdSettings(
                m_block,
                m_surface,
                surfaceSize,
                WkKnSession.CreateWkKnLcdSettings(surfaceSize, "0,0,0,0", DisplayPageTicks, MaximumRowsWithHeader, MaximumRowsWithoutHeader, 1f, 0f, 1f));
            var size = settings.Layout.Size;
            drawOffset = settings.Layout.Offset;
            displayPageTicks = settings.PageTicks;
            maximumRowsWithHeader = settings.RowsFirstPage;
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

                if (snapshot.Scopes == null || snapshot.Scopes.Count == 0)
                {
                    DrawMessage(frame, string.IsNullOrWhiteSpace(snapshot.Message) ? "No research archive available." : snapshot.Message, size, padding, accent, dim);
                    return;
                }

                var page = GetDisplayPage(snapshot, size, padding);
                if (page.Scope == null)
                {
                    DrawMessage(frame, "No research archive available.", size, padding, accent, dim);
                    return;
                }

                var y = padding;
                if (page.ShowHeader)
                    y = DrawHeader(frame, page.Scope, size, padding, y, accent, dim, background);

                if (page.Scope.Entries == null || page.Scope.Entries.Count == 0)
                {
                    DrawEmptyState(frame, "No research progress recorded.", size, padding, y, accent, dim);
                    DrawFooter(frame, page, size, padding, accent, dim, background);
                    return;
                }

                DrawEntries(frame, page, size, padding, y, accent, dim, muted, background);
            }
        }




        private DisplayPage GetDisplayPage(ResearchDisplaySnapshot snapshot, Vector2 size, float padding)
        {
            var pagesByScope = new int[snapshot.Scopes.Count];
            var totalPages = 0;
            for (var i = 0; i < snapshot.Scopes.Count; i++)
            {
                pagesByScope[i] = GetScopePageCount(snapshot.Scopes[i], size, padding);
                totalPages += pagesByScope[i];
            }

            var pageIndex = (int)((WkKnSession.GetResearchDisplayCycleTick() / displayPageTicks) % Math.Max(1, totalPages));
            for (var scopeIndex = 0; scopeIndex < snapshot.Scopes.Count; scopeIndex++)
            {
                var scopePageCount = pagesByScope[scopeIndex];
                if (pageIndex < scopePageCount)
                    return GetScopeDisplayPage(snapshot.Scopes[scopeIndex], scopeIndex, snapshot.Scopes.Count, pageIndex, scopePageCount, size, padding);

                pageIndex -= scopePageCount;
            }

            return GetScopeDisplayPage(snapshot.Scopes[0], 0, snapshot.Scopes.Count, 0, pagesByScope[0], size, padding);
        }

        private int GetScopePageCount(ResearchDisplayScopeSnapshot scope, Vector2 size, float padding)
        {
            if (scope == null || scope.Entries == null || scope.Entries.Count == 0)
                return 1;

            int firstPageRows;
            int laterPageRows;
            GetRowCounts(scope, size, padding, out firstPageRows, out laterPageRows);
            var remaining = Math.Max(0, scope.Entries.Count - firstPageRows);
            return 1 + ((remaining + laterPageRows - 1) / laterPageRows);
        }

        private DisplayPage GetScopeDisplayPage(ResearchDisplayScopeSnapshot scope, int scopeIndex, int scopeCount, int page, int pageCount, Vector2 size, float padding)
        {
            if (scope == null || scope.Entries == null || scope.Entries.Count == 0)
            {
                return new DisplayPage
                {
                    Scope = scope,
                    ScopeIndex = scopeIndex,
                    ScopeCount = scopeCount,
                    Page = 0,
                    PageCount = Math.Max(1, pageCount),
                    Start = 0,
                    End = 0,
                    ShowHeader = true,
                };
            }

            int firstPageRows;
            int laterPageRows;
            GetRowCounts(scope, size, padding, out firstPageRows, out laterPageRows);

            if (page == 0)
            {
                return new DisplayPage
                {
                    Scope = scope,
                    ScopeIndex = scopeIndex,
                    ScopeCount = scopeCount,
                    Page = page,
                    PageCount = Math.Max(1, pageCount),
                    Start = 0,
                    End = Math.Min(scope.Entries.Count, firstPageRows),
                    ShowHeader = true,
                };
            }

            var start = firstPageRows + ((page - 1) * laterPageRows);
            return new DisplayPage
            {
                Scope = scope,
                ScopeIndex = scopeIndex,
                ScopeCount = scopeCount,
                Page = page,
                PageCount = Math.Max(1, pageCount),
                Start = start,
                End = Math.Min(scope.Entries.Count, start + laterPageRows),
                ShowHeader = false,
            };
        }

        private void GetRowCounts(ResearchDisplayScopeSnapshot scope, Vector2 size, float padding, out int firstPageRows, out int laterPageRows)
        {
            var footerScale = ScaleText(ClampText(size.Y / 900f, 0.36f, 0.50f));
            var footerHeight = MeasureTextHeight("Page", footerScale, 16f) + padding;
            var rowHeight = GetEntryRowHeight(size);
            var headerBottom = MeasureHeaderBottom(scope, size, padding, padding);
            var firstPageHeight = Math.Max(rowHeight, size.Y - headerBottom - footerHeight - padding);
            var laterPageHeight = Math.Max(rowHeight, size.Y - padding - footerHeight - padding);
            firstPageRows = Math.Max(1, Math.Min(maximumRowsWithHeader, (int)(firstPageHeight / rowHeight)));
            laterPageRows = Math.Max(1, Math.Min(maximumRowsWithoutHeader, (int)(laterPageHeight / rowHeight)));
        }

        private float DrawHeader(MySpriteDrawFrame frame, ResearchDisplayScopeSnapshot scope, Vector2 size, float padding, float y, Color accent, Color dim, Color background)
        {
            const string title = "Schematic Research";
            var headerTop = y;
            var statsScale = ScaleText(ClampText(size.Y / 960f, 0.34f, 0.46f));
            var statsPadding = ClampLayout(size.Y * 0.012f, 5f, 9f);
            var statsLineHeight = MeasureTextHeight("Complete", statsScale, 16f) + Scaled(2f);
            var statsBoxWidth = ClampLayout(size.X * 0.22f, 122f, 184f);
            var statsBoxHeight = statsLineHeight * 3f + statsPadding * 2f;
            var statsLeft = size.X - padding - statsBoxWidth;
            var titleMaxWidth = Math.Max(Scaled(90f), statsLeft - padding - Scaled(14f));
            var titleScale = FitTextScale(title, titleMaxWidth, ScaleText(ClampText(size.Y / 590f, 0.74f, 1.0f)), ScaleText(Scaled(0.52f)));
            var ownerScale = ScaleText(ClampText(size.Y / 780f, 0.42f, 0.60f));
            var titleHeight = MeasureTextHeight(title, titleScale, 34f);

            DrawStatsPanel(frame, scope, new Vector2(statsLeft, headerTop), new Vector2(statsBoxWidth, statsBoxHeight), statsPadding, statsScale, accent, background);
            AddText(frame, title, new Vector2(padding, y), titleScale, accent, TextAlignment.LEFT);
            y += titleHeight + Scaled(6f);

            var ownerLines = WrapText(BuildHeaderOwnerLine(scope), titleMaxWidth, ownerScale, 2);
            var ownerLineHeight = MeasureTextHeight("Owner", ownerScale, 18f) + Scaled(2f);
            for (var i = 0; i < ownerLines.Count; i++)
            {
                AddText(frame, ownerLines[i], new Vector2(padding, y), ownerScale, accent, TextAlignment.LEFT);
                y += ownerLineHeight;
            }

            y = Math.Max(y, headerTop + statsBoxHeight) + Scaled(12f);
            return y + Scaled(HeaderEntryGap);
        }

        private float MeasureHeaderBottom(ResearchDisplayScopeSnapshot scope, Vector2 size, float padding, float y)
        {
            const string title = "Schematic Research";
            var headerTop = y;
            var statsScale = ScaleText(ClampText(size.Y / 960f, 0.34f, 0.46f));
            var statsPadding = ClampLayout(size.Y * 0.012f, 5f, 9f);
            var statsLineHeight = MeasureTextHeight("Complete", statsScale, 16f) + Scaled(2f);
            var statsBoxWidth = ClampLayout(size.X * 0.22f, 122f, 184f);
            var statsBoxHeight = statsLineHeight * 3f + statsPadding * 2f;
            var statsLeft = size.X - padding - statsBoxWidth;
            var titleMaxWidth = Math.Max(Scaled(90f), statsLeft - padding - Scaled(14f));
            var titleScale = FitTextScale(title, titleMaxWidth, ScaleText(ClampText(size.Y / 590f, 0.74f, 1.0f)), ScaleText(Scaled(0.52f)));
            var ownerScale = ScaleText(ClampText(size.Y / 780f, 0.42f, 0.60f));
            var ownerLineHeight = MeasureTextHeight("Owner", ownerScale, 18f) + Scaled(2f);
            y += MeasureTextHeight(title, titleScale, 34f) + Scaled(6f);
            y += WrapText(BuildHeaderOwnerLine(scope), titleMaxWidth, ownerScale, 2).Count * ownerLineHeight;
            y = Math.Max(y, headerTop + statsBoxHeight) + Scaled(12f);
            return y + Scaled(HeaderEntryGap);
        }

        private static string BuildHeaderOwnerLine(ResearchDisplayScopeSnapshot scope)
        {
            var ownerName = scope == null || string.IsNullOrWhiteSpace(scope.OwnerName) ? "Unknown" : scope.OwnerName;
            if (scope != null && !string.IsNullOrWhiteSpace(scope.FactionTag))
                return ownerName + " [" + scope.FactionTag + "]";

            return ownerName;
        }

        private static string BuildFooterName(ResearchDisplayScopeSnapshot scope)
        {
            if (scope == null || string.IsNullOrWhiteSpace(scope.OwnerName))
                return "Unknown";

            if (scope.IsFaction && !string.IsNullOrWhiteSpace(scope.FactionTag))
                return "[" + scope.FactionTag + "]";

            return scope.OwnerName;
        }

        private void DrawEntries(MySpriteDrawFrame frame, DisplayPage page, Vector2 size, float padding, float y, Color accent, Color dim, Color muted, Color background)
        {
            var rowScale = ScaleText(ClampText(size.Y / 875f, 0.38f, 0.53f));
            var percentScale = ScaleText(ClampText(size.Y / 900f, 0.36f, 0.50f));
            var rowHeight = GetEntryRowHeight(size);
            var barWidth = size.X - padding * 2f;
            var barHeight = ClampLayout(size.Y / 122f, 3f, 6f);
            var percentWidth = Math.Max(Scaled(58f), size.X * 0.16f);
            var nameWidth = Math.Max(Scaled(60f), barWidth - percentWidth - Scaled(12f));

            for (var i = page.Start; i < page.End; i++)
            {
                var entry = page.Scope.Entries[i];
                var localIndex = i - page.Start;
                var rowTop = y + localIndex * rowHeight;
                var percent = FormatPercent(entry.Progress);
                var name = TrimToWidth(string.IsNullOrWhiteSpace(entry.DisplayName) ? entry.ResearchId : entry.DisplayName, nameWidth, rowScale);
                var barFill = entry.Unlocked ? accent : GetColorShade(accent, background, 2);
                var rowText = accent;
                var percentText = accent;
                var textY = rowTop + Scaled(2f);

                AddText(frame, name, new Vector2(padding, textY), rowScale, rowText, TextAlignment.LEFT);
                AddText(frame, percent, new Vector2(size.X - padding, textY), percentScale, percentText, TextAlignment.RIGHT);

                var barCenter = new Vector2(size.X * 0.5f, rowTop + MeasureTextHeight("Research", rowScale, 20f) + Scaled(13f));
                AddProgressBar(frame, barCenter, new Vector2(barWidth, barHeight), (float)entry.Progress, muted, barFill);
            }

            DrawFooter(frame, page, size, padding, accent, dim, background);
        }

        private void DrawFooter(MySpriteDrawFrame frame, DisplayPage page, Vector2 size, float padding, Color accent, Color dim, Color background)
        {
            var footerScale = ScaleText(ClampText(size.Y / 930f, 0.34f, 0.48f));
            var scopeLabel = page.Scope != null && page.Scope.IsFaction ? "Faction" : "Player";
            var footerName = BuildFooterName(page.Scope) +
                             " " +
                             (page.Page + 1).ToString(CultureInfo.InvariantCulture) +
                             "/" +
                             page.PageCount.ToString(CultureInfo.InvariantCulture);
            var footerTextHeight = MeasureTextHeight(footerName, footerScale, 16f);
            var footerBarHeight = ClampLayout(footerTextHeight + Scaled(10f), 22f, 34f);
            var footerBarCenter = new Vector2(size.X * 0.5f, size.Y - footerBarHeight * 0.5f);
            var footerY = footerBarCenter.Y - footerTextHeight * 0.5f - Scaled(1f);
            var leftWidth = Math.Max(Scaled(64f), size.X * 0.34f);
            var rightWidth = Math.Max(Scaled(64f), size.X - padding * 2f - leftWidth - Scaled(12f));
            var left = TrimToWidth(scopeLabel, leftWidth, footerScale);
            var right = TrimToWidth(footerName, rightWidth, footerScale);
            var footerFillColor = GetColorShade(accent, background, 3);
            var footerTextColor = GetColorShade(accent, background, 9);

            AddRectangle(frame, footerBarCenter, new Vector2(size.X, footerBarHeight), footerFillColor);
            AddText(frame, left, new Vector2(padding + Scaled(8f), footerY), footerScale, footerTextColor, TextAlignment.LEFT);
            AddText(frame, right, new Vector2(size.X - padding - Scaled(8f), footerY), footerScale, footerTextColor, TextAlignment.RIGHT);
        }

        private void DrawMessage(MySpriteDrawFrame frame, string message, Vector2 size, float padding, Color accent, Color dim)
        {
            var y = DrawStaticTitle(frame, size, padding, accent, dim);
            DrawEmptyState(frame, message, size, padding, y, accent, dim);
        }

        private float DrawStaticTitle(MySpriteDrawFrame frame, Vector2 size, float padding, Color accent, Color dim)
        {
            const string title = "Schematic Research";
            var titleScale = FitTextScale(title, size.X - padding * 2f, ScaleText(ClampText(size.Y / 560f, 0.76f, 1.04f)), ScaleText(Scaled(0.52f)));
            var y = padding;
            AddText(frame, title, new Vector2(padding, y), titleScale, accent, TextAlignment.LEFT);
            return y + MeasureTextHeight(title, titleScale, 34f) + Scaled(12f);
        }

        private void DrawEmptyState(MySpriteDrawFrame frame, string message, Vector2 size, float padding, float y, Color accent, Color dim)
        {
            message = string.IsNullOrWhiteSpace(message) ? "No research progress recorded." : message;
            var scale = FitTextScale(message, size.X - padding * 2f, ScaleText(ClampText(size.Y / 690f, 0.52f, 0.74f)), ScaleText(Scaled(0.36f)));
            var lines = WrapText(message, size.X - padding * 2f, scale, 3);
            var lineHeight = MeasureTextHeight("Research", scale, 24f) + Scaled(4f);
            var blockHeight = lines.Count * lineHeight;
            var startY = Math.Max(y, size.Y * 0.5f - blockHeight * 0.5f);

            for (var i = 0; i < lines.Count; i++)
                AddText(frame, lines[i], new Vector2(size.X * 0.5f, startY + i * lineHeight), scale, i == 0 ? accent : dim, TextAlignment.CENTER);
        }


        private void DrawStatsPanel(MySpriteDrawFrame frame, ResearchDisplayScopeSnapshot scope, Vector2 topLeft, Vector2 size, float padding, float scale, Color accent, Color background)
        {
            var center = topLeft + size * 0.5f;
            var lineHeight = MeasureTextHeight("Complete", scale, 16f) + Scaled(2f);
            var labelX = topLeft.X + padding;
            var numberX = topLeft.X + size.X - padding;
            var y = topLeft.Y + padding;
            var panelFillColor = GetColorShade(accent, background, 3);
            var panelTextColor = GetColorShade(accent, background, 9);

            AddRectangle(frame, center, size, panelFillColor);
            AddText(frame, "Active", new Vector2(labelX, y), scale, panelTextColor, TextAlignment.LEFT);
            AddText(frame, (scope == null ? 0 : scope.ActiveCount).ToString(CultureInfo.InvariantCulture), new Vector2(numberX, y), scale, panelTextColor, TextAlignment.RIGHT);
            y += lineHeight;
            AddText(frame, "Complete", new Vector2(labelX, y), scale, panelTextColor, TextAlignment.LEFT);
            AddText(frame, (scope == null ? 0 : scope.CompletedCount).ToString(CultureInfo.InvariantCulture), new Vector2(numberX, y), scale, panelTextColor, TextAlignment.RIGHT);
            y += lineHeight;
            AddText(frame, "Tracked", new Vector2(labelX, y), scale, panelTextColor, TextAlignment.LEFT);
            AddText(frame, (scope == null ? 0 : scope.TrackedCount).ToString(CultureInfo.InvariantCulture), new Vector2(numberX, y), scale, panelTextColor, TextAlignment.RIGHT);
        }

        private float GetEntryRowHeight(Vector2 size)
        {
            return ClampLayout(size.Y / 8.55f, 48f, 66f);
        }
    }
}
