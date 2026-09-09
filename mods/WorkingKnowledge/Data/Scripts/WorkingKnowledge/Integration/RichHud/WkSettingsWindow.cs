using System;
using System.Collections.Generic;
using System.Globalization;
using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using Sandbox.Game;
using VRageMath;

namespace WkKn
{
    /// <summary>
    /// Working Knowledge's compact settings surface. This deliberately uses RHF's public HUD
    /// primitives instead of the terminal's fixed-size ControlTile layout.
    /// </summary>
    internal sealed class WkSettingsWindow : WindowBase
    {
        private const float SidebarWidth = 238f;
        private readonly ScaledSpaceNode responsiveRoot;
        private const float WindowMargin = 24f;
        private const float ColumnGap = 18f;

        private readonly WkPlayerConfigStore playerStore;
        private readonly WkConfigStore worldStore;
        private readonly Func<WkPlayerConfigRecord> getPlayerConfig;
        private readonly Func<WkConfig> getWorldConfig;
        private readonly Func<string, Func<string>, string> getEffectiveValue;
        private readonly Action<string, string, string, bool> applyValue;
        private readonly Action<string, string> applyDifficulty;
        private readonly Action<bool> resetConfig;
        private readonly List<SettingsPage> pages = new List<SettingsPage>();
        private readonly List<SettingRow> rows = new List<SettingRow>();
        private readonly List<SettingRow> worldRows = new List<SettingRow>();
        private readonly List<BorderedButton> navigationButtons = new List<BorderedButton>();
        private readonly EmptyHudElement navigation;
        private readonly List<HudElementBase> navigationItems = new List<HudElementBase>();
        private readonly WkSettingsDraft draft = new WkSettingsDraft();
        private readonly EmptyHudElement footer;
        private readonly BorderedButton applyButton;
        private readonly Label footerStatus;
        private int? previousHudState;
        private readonly TexturedBox columnDivider;
        private readonly BorderedButton closeButton;
        private SettingsPage selectedPage;
        private Color bodyBaseColor;
        private bool live;
        private bool canEditWorld;

        internal WkSettingsWindow(
            WkPlayerConfigStore playerStore,
            WkConfigStore worldStore,
            Func<WkPlayerConfigRecord> getPlayerConfig,
            Func<WkConfig> getWorldConfig,
            Func<string, Func<string>, string> getEffectiveValue,
            Action<string, string, string, bool> applyValue,
            Action<string, string> applyDifficulty,
            Action<bool> resetConfig)
            : base(new ScaledSpaceNode(HudMain.HighDpiRoot))
        {
            responsiveRoot = (ScaledSpaceNode)Parent;
            responsiveRoot.UpdateScaleFunc = GetWindowScale;
            this.playerStore = playerStore;
            this.worldStore = worldStore;
            this.getPlayerConfig = getPlayerConfig;
            this.getWorldConfig = getWorldConfig;
            this.getEffectiveValue = delegate(string key, Func<string> current)
            { return draft.GetValue(key, delegate { return getEffectiveValue(key, current); }); };
            this.applyValue = applyValue;
            this.applyDifficulty = applyDifficulty;
            this.resetConfig = resetConfig;

            HeaderBuilder.Format = TerminalFormatting.HeaderFormat;
            HeaderText = new RichText("Working Knowledge Settings");
            header.Background.Visible = false;
            header.Height = 40f;

            footer = new EmptyHudElement(body);
            closeButton = new BorderedButton(footer)
            {
                Text = "Exit",
                Size = new Vector2(110f, 38f),
                Padding = Vector2.Zero,
                TextPadding = Vector2.Zero,
            };
            closeButton.MouseInput.LeftClicked += delegate { Hide(); };
            applyButton = new BorderedButton(footer)
            {
                Text = "Apply Changes",
                Size = new Vector2(190f, 38f),
                Padding = Vector2.Zero,
                TextPadding = Vector2.Zero,
            };
            applyButton.MouseInput.LeftClicked += delegate { ApplyChanges(); };
            footerStatus = new Label(footer)
            {
                Text = "Exit discards unapplied edits.",
                AutoResize = false,
                BuilderMode = TextBuilderModes.Wrapped,
                Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .8f),
            };
            header.MouseInput.RequestCursor = true;
            navigation = new EmptyHudElement(body);

            columnDivider = new TexturedBox(body)
            {
                Color = TerminalFormatting.LimedSpruce,
                Width = 1f,
            };

            MouseInput.RequestCursor = true;
            bodyBaseColor = new Color(27, 35, 41, 242);
            BodyColor = bodyBaseColor;
            BorderColor = new Color(84, 98, 107);
            MinimumSize = new Vector2(920f, 680f);
            Size = new Vector2(1120f, 820f);
            AllowResizing = true;
            CanDrag = true;
            Visible = false;

            BuildPages();
            SharedBinds.Escape.NewPressed += OnEscapePressed;
        }

        internal bool IsOpen
        {
            get { return Visible; }
        }

        internal void Show()
        {
            RichHudTerminal.CloseMenu();
            if (!Visible)
            {
                DiscardChanges();
                if (MyAPIGateway.Session != null && MyAPIGateway.Session.Config != null)
                {
                    previousHudState = MyAPIGateway.Session.Config.HudState;
                    MyVisualScriptLogicProvider.SetHudState(0, 0);
                }
                FitToScreen();
                Offset = Vector2.Zero;
                Visible = true;
            }

            live = true;
            GetWindowFocus();
            Refresh(canEditWorld);
        }

        internal void Hide()
        {
            Visible = false;
            DiscardChanges();
            if (previousHudState.HasValue)
            {
                MyVisualScriptLogicProvider.SetHudState(previousHudState.Value, 0);
                previousHudState = null;
            }
            HudMain.EnableCursor = false;
        }

        internal void Refresh(bool allowWorldChanges)
        {
            canEditWorld = allowWorldChanges;
            if (!Visible)
                return;

            for (var i = 0; i < worldRows.Count; i++)
                worldRows[i].SetEditable(allowWorldChanges);

            for (var i = 0; i < rows.Count; i++)
                rows[i].Refresh();
        }

        internal void Dispose()
        {
            live = false;
            SharedBinds.Escape.NewPressed -= OnEscapePressed;
            Hide();
            Unregister();
            responsiveRoot.Unregister();
            pages.Clear();
            rows.Clear();
            worldRows.Clear();
            navigationButtons.Clear();
            selectedPage = null;
        }

        protected override void Layout()
        {
            FitToScreen();
            base.Layout();

            BodyColor = bodyBaseColor;
            const float footerHeight = 64f;
            var contentHeight = body.UnpaddedSize.Y - footerHeight - (WindowMargin * 2f);
            var contentWidth = body.UnpaddedSize.X - SidebarWidth - (WindowMargin * 2f) - ColumnGap;

            navigation.Size = new Vector2(SidebarWidth, contentHeight);
            navigation.Offset = new Vector2(-body.UnpaddedSize.X * .5f + WindowMargin + SidebarWidth * .5f, footerHeight * .5f);
            var navY = contentHeight * .5f;
            for (var i = 0; i < navigationItems.Count; i++)
            {
                var item = navigationItems[i];
                item.Width = SidebarWidth;
                item.Offset = new Vector2(0f, navY - item.Height * .5f);
                navY -= item.Height + 4f;
            }

            columnDivider.Height = contentHeight;
            columnDivider.ParentAlignment = ParentAlignments.Center;
            columnDivider.Offset = new Vector2(navigation.Offset.X + SidebarWidth * .5f + ColumnGap * .5f, footerHeight * .5f);
            for (var i = 0; i < pages.Count; i++)
            {
                pages[i].Size = new Vector2(contentWidth, contentHeight);
                pages[i].ParentAlignment = ParentAlignments.Center;
                pages[i].Offset = new Vector2(body.UnpaddedSize.X * .5f - WindowMargin - contentWidth * .5f, footerHeight * .5f);
            }
            footer.Size = new Vector2(body.UnpaddedSize.X - WindowMargin * 2f, footerHeight);
            footer.Offset = new Vector2(0f, -body.UnpaddedSize.Y * .5f + footerHeight * .5f);
            closeButton.Offset = new Vector2(footer.Width * .5f - 55f, 0f);
            applyButton.Offset = new Vector2(footer.Width * .5f - 220f, 0f);
            footerStatus.Size = new Vector2(footer.Width - 340f, 46f);
            footerStatus.LineWrapWidth = footerStatus.Width;
            footerStatus.Offset = new Vector2(-170f, 0f);
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (MyAPIGateway.Gui != null && MyAPIGateway.Gui.IsCursorVisible)
                Hide();

            base.HandleInput(cursorPos);

            FitToScreen();
        }

        private static float GetWindowScale()
        {
            var screen = HudMain.ScreenDimHighDPI;
            return Math.Max(.1f, Math.Min(1f, Math.Min((screen.X - 48f) / 920f, (screen.Y - 48f) / 680f)));
        }

        private void FitToScreen()
        {
            var available = (HudMain.ScreenDimHighDPI - new Vector2(48f)) / GetWindowScale();
            available = Vector2.Max(available, MinimumSize);
            Size = Vector2.Clamp(Size, MinimumSize, available);
            var travel = Vector2.Max(Vector2.Zero, (available - Size) * .5f);
            Offset = Vector2.Clamp(Offset, -travel, travel);
        }

        private void BuildPages()
        {
            AddNavigationHeading("PLAYER SETTINGS");

            var progressPage = AddPage(
                "Progress HUD",
                "Progress HUD",
                "Display and position recent research and Proficiency activity.");
            AddPlayerSection(progressPage, "Display", "Visibility, history size, timing, and row order.",
                "progressHudEnabled", "progressHudRows", "progressHudFadeSeconds", "progressHudOrder");
            AddPlayerSection(progressPage, "Placement", "Choose an anchor and adjust its final screen position.",
                "progressHudPosition", "progressHudOffsetX", "progressHudOffsetY");

            var feedbackPage = AddPage(
                "Feedback",
                "Player Feedback",
                "Choose how Working Knowledge reports your personal progress.");
            AddPlayerSection(feedbackPage, "Notifications", "Personal chat and popup feedback.",
                "progressChatEnabled", "progressToastEnabled");
            AddPlayerSection(feedbackPage, "Chat Thresholds", "Minimum accumulated progress before another chat update.",
                "researchChatSuppressionPercent", "proficiencyChatSuppressionPercent");
            AddPlayerSection(feedbackPage, "Toast Thresholds", "Minimum accumulated progress before another popup update.",
                "researchToastSuppressionPercent", "proficiencyToastSuppressionPercent");
            AddPlayerSection(feedbackPage, "Sounds and Warnings", "Completion audio and construction-botch feedback.",
                "completionSoundEnabled", "weldBotchSoundEnabled", "weldBotchWarningCooldownSeconds");
            feedbackPage.AddSection("Reset", "Restore all personal settings to their defaults.");
            feedbackPage.AddRow(CreateActionRow(
                "Reset Player Settings",
                "Removes your overrides and restores the current defaults.",
                "Reset Player Settings",
                delegate { if (live) StageReset(false); },
                false));

            AddNavigationHeading("SERVER SETTINGS");

            var difficultyPage = AddPage(
                "Difficulty",
                "Server Difficulty",
                "Preset bundles and authoritative world configuration. Administrator access is required.");
            difficultyPage.AddSection("Preset", "Apply a complete balance preset or reset all server settings.");
            difficultyPage.AddRow(CreateDifficultyRow());
            difficultyPage.AddRow(CreateActionRow(
                "Reset Server Settings",
                "Restores the complete easy preset and all authoritative defaults.",
                "Reset Server Settings",
                delegate { if (live && canEditWorld) StageReset(true); },
                true));

            foreach (var category in worldStore.GetDisplayCategories())
            {
                if (category.Equals("Difficulty", StringComparison.OrdinalIgnoreCase))
                    continue;

                var displayName = category.Equals("Feedback", StringComparison.OrdinalIgnoreCase)
                    ? "Feedback Defaults"
                    : category.Equals("Defaults", StringComparison.OrdinalIgnoreCase) ? "New Player Defaults" : category;
                var page = AddPage(
                    displayName,
                    displayName,
                    "Authoritative " + category.ToLowerInvariant() + " settings for this world. Administrator access is required.");
                page.AddSection(category, GetCategoryDescription(category));
                AddWorldDefinitions(page, category);
            }

            AddNavigationHeading("INFORMATION");
            var helpPage = AddPage(
                "Help",
                "Working Knowledge Settings",
                "A compact Rich HUD interface backed by the existing server-authoritative command system.");
            helpPage.AddSection("Using This Window", "Settings are grouped by ownership and purpose.");
            helpPage.AddRow(new InformationRow(
                "Player settings affect only you. Server settings remain visible to everyone but can only be changed by an administrator. " +
                "Every change uses the same validation, saving, and multiplayer path as the /wk commands."));
            helpPage.AddSection("Commands Remain Available", "The graphical interface is optional.");
            helpPage.AddRow(new InformationRow(
                "Use /wk settings to reopen this window. Existing /wk config, /wk difficulty, and category help commands remain fully supported."));

            SelectPage(progressPage, navigationButtons[0]);
        }

        private SettingsPage AddPage(string navigationName, string title, string subtitle)
        {
            var page = new SettingsPage(body, title, subtitle)
            {
                Visible = false,
            };
            pages.Add(page);

            var button = new BorderedButton(navigation)
            {
                Text = navigationName,
                Size = new Vector2(SidebarWidth - 34f, 38f),
                Padding = Vector2.Zero,
                TextPadding = new Vector2(18f, 0f),
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Left),
                FocusColor = TerminalFormatting.DullMint,
                FocusTextColor = TerminalFormatting.Mercury,
                UseFocusFormatting = false,
            };
            button.MouseInput.LeftClicked += delegate { SelectPage(page, button); };
            navigationItems.Add(button);
            navigationButtons.Add(button);
            return page;
        }

        private void AddNavigationHeading(string text)
        {
            var heading = new LabelBox(navigation)
            {
                Text = text,
                AutoResize = false,
                Size = new Vector2(SidebarWidth - 34f, 26f),
                TextPadding = new Vector2(10f, 0f),
                Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .78f),
                Color = new Color(0, 0, 0, 0),
            };
            navigationItems.Add(heading);
        }

        private void SelectPage(SettingsPage page, BorderedButton button)
        {
            if (selectedPage != null)
                selectedPage.Visible = false;

            for (var i = 0; i < navigationButtons.Count; i++)
            {
                navigationButtons[i].Color = new Color(0, 0, 0, 0);
                navigationButtons[i].BorderThickness = 0f;
                navigationButtons[i].HighlightColor = TerminalFormatting.Atomic;
            }

            selectedPage = page;
            selectedPage.Visible = true;
            button.Color = TerminalFormatting.DullMint;
            button.HighlightColor = TerminalFormatting.DullMint;
            Refresh(canEditWorld);
        }

        private void AddPlayerSection(SettingsPage page, string title, string description, params string[] settings)
        {
            page.AddSection(title, description);
            for (var i = 0; i < settings.Length; i++)
            {
                var definition = FindPlayerSetting(settings[i]);
                if (definition == null)
                    continue;

                var row = CreatePlayerRow(definition);
                page.AddRow(row);
                rows.Add(row);
            }
        }

        private void AddWorldDefinitions(SettingsPage page, string category)
        {
            var definitions = worldStore.Settings;
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (!definition.Category.Equals(category, StringComparison.OrdinalIgnoreCase) ||
                    definition.Setting.Equals("difficultyPreset", StringComparison.OrdinalIgnoreCase))
                    continue;

                var row = CreateWorldRow(definition);
                page.AddRow(row);
                rows.Add(row);
                worldRows.Add(row);
            }
        }

        private WkPlayerConfigSettingDefinition FindPlayerSetting(string setting)
        {
            var definitions = playerStore.Settings;
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Setting.Equals(setting, StringComparison.OrdinalIgnoreCase))
                    return definitions[i];
            }

            return null;
        }

        private SettingRow CreatePlayerRow(WkPlayerConfigSettingDefinition definition)
        {
            return CreateSettingRow(
                definition.Setting,
                definition.Title,
                definition.Description,
                definition.ValueHint,
                definition.ControlKind,
                definition.Presentation,
                definition.Minimum,
                definition.Maximum,
                definition.Choices,
                delegate
                {
                    var config = getPlayerConfig();
                    return getEffectiveValue(definition.Setting, delegate { return config == null ? string.Empty : definition.GetValue(config); });
                },
                false);
        }

        private SettingRow CreateWorldRow(WkConfigSettingDefinition definition)
        {
            return CreateSettingRow(
                definition.Setting,
                definition.Title,
                definition.Description,
                definition.ValueHint,
                definition.ControlKind,
                definition.Presentation,
                definition.Minimum,
                definition.Maximum,
                definition.Choices,
                delegate
                {
                    var config = getWorldConfig();
                    return getEffectiveValue(definition.Setting, delegate { return config == null ? string.Empty : definition.GetValue(config); });
                },
                true);
        }

        private SettingRow CreateSettingRow(
            string setting,
            string title,
            string description,
            string valueHint,
            WkSettingControlKind kind,
            WkSettingPresentation presentation,
            double minimum,
            double maximum,
            string[] choices,
            Func<string> getValue,
            bool isWorld)
        {
            var tooltip = description + "\nValue: " + valueHint;
            Action<string> changed = delegate(string value)
            {
                if (!live || (isWorld && !canEditWorld))
                    return;

                var command = "/wk config " + setting + " " + value;
                draft.Stage(setting, value, delegate { if (!isWorld || canEditWorld) applyValue(setting, value, command, isWorld); });
                UpdateDraftStatus();
            };

            SettingRow row;
            if (kind == WkSettingControlKind.Boolean)
                row = new BooleanSettingRow(title, description, tooltip, getValue, changed);
            else if (kind == WkSettingControlKind.Number || kind == WkSettingControlKind.Integer)
                row = new SliderSettingRow(title, description, tooltip, getValue, changed, presentation, kind == WkSettingControlKind.Integer, minimum, maximum);
            else if (kind == WkSettingControlKind.Choice)
                row = new ChoiceSettingRow(title, description, tooltip, getValue, changed, choices);
            else if (kind == WkSettingControlKind.ReadOnly)
                row = new ReadOnlySettingRow(title, description, getValue);
            else
                row = new TextSettingRow(title, description, tooltip, getValue, changed, kind == WkSettingControlKind.DefaultableNumber);

            if (isWorld)
                row.SetEditable(false);
            return row;
        }

        private SettingRow CreateDifficultyRow()
        {
            var choices = new List<string>();
            foreach (var preset in worldStore.GetDifficultyPresetNames())
                choices.Add(preset);
            choices.Add("custom");

            var row = new ChoiceSettingRow(
                "Difficulty Preset",
                "Apply a complete balance preset. Manual server-setting changes are reported as Custom.",
                "Applies a complete Working Knowledge difficulty preset.",
                delegate
                {
                    var config = getWorldConfig();
                    return getEffectiveValue("difficultyPreset", delegate { return config == null ? string.Empty : config.DifficultyPreset; });
                },
                delegate(string value)
                {
                    if (!live || !canEditWorld || value.Equals("custom", StringComparison.OrdinalIgnoreCase))
                        return;

                    DiscardChanges();
                    draft.Stage("difficultyPreset", value, delegate { if (canEditWorld) applyDifficulty(value, "/wk difficulty " + value); });
                    footerStatus.Text = "Preset queued. Apply Changes to update the world.";
                },
                choices.ToArray(),
                delegate(string value) { return !value.Equals("custom", StringComparison.OrdinalIgnoreCase); });
            row.SetEditable(false);
            rows.Add(row);
            worldRows.Add(row);
            return row;
        }

        private SettingRow CreateActionRow(string title, string description, string buttonText, Action action, bool isWorld)
        {
            var row = new ActionSettingRow(title, description, buttonText, action);
            rows.Add(row);
            if (isWorld)
            {
                row.SetEditable(false);
                worldRows.Add(row);
            }
            return row;
        }

        private void UpdateDraftStatus()
        {
            footerStatus.Text = "Unapplied edits. Apply Changes to keep them; Exit discards them.";
        }

        private void DiscardChanges()
        {
            draft.Clear();
            for (var i = 0; i < rows.Count; i++)
                rows[i].DiscardDraft();
            footerStatus.Text = "Exit discards unapplied edits. Apply Changes to keep them.";
        }

        private void StageReset(bool world)
        {
            DiscardChanges();
            draft.Stage("reset", string.Empty, delegate { if (!world || canEditWorld) resetConfig(world); });
            footerStatus.Text = "Reset queued. Apply Changes to restore defaults.";
        }

        private void ApplyChanges()
        {
            // Validate every typed field before submitting any of the staged changes.
            for (var i = 0; i < rows.Count; i++)
            {
                string error;
                if (!rows[i].ValidateDraft(out error))
                {
                    footerStatus.Text = error;
                    return;
                }
            }
            for (var i = 0; i < rows.Count; i++)
                rows[i].StageDraft();
            if (draft.Count == 0)
            {
                footerStatus.Text = "No unapplied changes.";
                return;
            }
            draft.Apply();
            footerStatus.Text = "Changes submitted. Exit closes the window.";
        }

        private void OnEscapePressed(object sender, EventArgs args)
        {
            if (Visible)
                Hide();
        }

        private static string GetCategoryDescription(string category)
        {
            if (category.Equals("Research", StringComparison.OrdinalIgnoreCase))
                return "Discovery gains, research efficiency, and data-item behavior.";
            if (category.Equals("Proficiency", StringComparison.OrdinalIgnoreCase))
                return "Hands-on skill gains, thresholds, and work consequences.";
            if (category.Equals("Botches", StringComparison.OrdinalIgnoreCase))
                return "Construction failure chance, pressure, losses, forgiveness, and audio.";
            if (category.Equals("Salvage", StringComparison.OrdinalIgnoreCase))
                return "Grinding recovery and low-Proficiency scrap conversion.";
            if (category.Equals("Feedback", StringComparison.OrdinalIgnoreCase))
                return "World defaults for new players and shared notification behavior.";
            if (category.Equals("Defaults", StringComparison.OrdinalIgnoreCase))
                return "Starting research and Proficiency granted to new or joining players.";
            return "Authoritative settings for this world.";
        }

        private static bool ParseBool(string value)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) && parsed;
        }

        private static double ParseNumber(string value)
        {
            double parsed;
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) &&
                   !double.IsNaN(parsed) &&
                   !double.IsInfinity(parsed)
                ? parsed
                : 0f;
        }

        private static string FormatCanonicalValue(double value, bool integer)
        {
            return integer
                ? ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture)
                : value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static double Quantize(double value, WkSettingPresentation presentation)
        {
            var clamped = Math.Max(presentation.Minimum, Math.Min(presentation.Maximum, value));
            if (presentation.Step <= 0.0)
                return clamped;

            var rounded = Math.Round(clamped / presentation.Step) * presentation.Step;
            return Math.Max(presentation.Minimum, Math.Min(presentation.Maximum, rounded));
        }

        private static float ToSliderValue(double canonicalValue, WkSettingPresentation presentation)
        {
            if (presentation.SliderScale != WkSettingSliderScale.LogarithmicWithZero)
                return (float)Math.Max(presentation.Minimum, Math.Min(presentation.Maximum, canonicalValue));

            if (canonicalValue <= 0.0)
                return 0f;

            const double zeroSlot = 0.02;
            var floor = Math.Max(0.000001, presentation.LogarithmicFloor);
            var maximum = Math.Max(floor, presentation.Maximum);
            var clamped = Math.Max(floor, Math.Min(maximum, canonicalValue));
            var normalized = Math.Log(clamped / floor) / Math.Log(maximum / floor);
            return (float)(zeroSlot + ((1.0 - zeroSlot) * normalized));
        }

        private static double FromSliderValue(float sliderValue, WkSettingPresentation presentation)
        {
            if (presentation.SliderScale != WkSettingSliderScale.LogarithmicWithZero)
                return sliderValue;

            const double zeroSlot = 0.02;
            if (sliderValue <= zeroSlot * 0.5)
                return 0.0;

            var floor = Math.Max(0.000001, presentation.LogarithmicFloor);
            var maximum = Math.Max(floor, presentation.Maximum);
            var normalized = Math.Max(0.0, Math.Min(1.0, (sliderValue - zeroSlot) / (1.0 - zeroSlot)));
            return floor * Math.Exp(Math.Log(maximum / floor) * normalized);
        }

        private static string ToDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var result = value.Substring(0, 1).ToUpperInvariant();
            for (var i = 1; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && !char.IsWhiteSpace(value[i - 1]))
                    result += " ";
                result += value[i];
            }

            return result;
        }

        private sealed class SettingsPage : HudElementBase
        {
            private readonly Label title;
            private readonly Label subtitle;
            private readonly ScrollBox content;

            internal SettingsPage(HudParentBase parent, string heading, string description) : base(parent)
            {
                title = new Label(this)
                {
                    Text = heading,
                    AutoResize = false,
                    Format = new GlyphFormat(TerminalFormatting.Mercury, TextAlignment.Left, 1.25f),
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Size = new Vector2(600f, 34f),
                };
                subtitle = new Label(this)
                {
                    Text = description,
                    AutoResize = false,
                    Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .86f),
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Offset = new Vector2(0f, -32f),
                    Size = new Vector2(600f, 28f),
                };
                content = new ScrollBox(true, this)
                {
                    Color = new Color(0, 0, 0, 0),
                    Padding = new Vector2(40f, 10f),
                    Spacing = 0f,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                };
                content.ScrollBar.Padding = new Vector2(12f, 8f);
                content.ScrollBar.Width = 6f;
            }

            internal void AddSection(string heading, string description)
            {
                content.Add(new SectionHeading(heading, description));
            }

            internal void AddRow(SettingRow row)
            {
                content.Add(row);
            }

            protected override void Layout()
            {
                title.Width = UnpaddedSize.X;
                subtitle.Width = UnpaddedSize.X;
                content.Size = new Vector2(UnpaddedSize.X, Math.Max(120f, UnpaddedSize.Y - 72f));
                content.ParentAlignment = ParentAlignments.InnerBottom;
            }
        }

        private sealed class SectionHeading : HudElementBase
        {
            private readonly Label title;
            private readonly Label description;
            private readonly TexturedBox divider;

            internal SectionHeading(string heading, string subheading) : base(null)
            {
                Size = new Vector2(620f, 68f);
                title = new Label(this)
                {
                    Text = heading,
                    AutoResize = false,
                    Format = new GlyphFormat(TerminalFormatting.Mercury, TextAlignment.Left, 1.05f),
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Offset = new Vector2(12f, -8f),
                    Size = new Vector2(500f, 26f),
                };
                description = new Label(this)
                {
                    Text = subheading,
                    AutoResize = false,
                    Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .75f),
                    ParentAlignment = ParentAlignments.InnerBottomLeft,
                    Offset = new Vector2(12f, 9f),
                    Size = new Vector2(500f, 24f),
                };
                divider = new TexturedBox(this)
                {
                    Color = TerminalFormatting.LimedSpruce,
                    ParentAlignment = ParentAlignments.InnerBottom,
                    Height = 1f,
                };
            }

            protected override void Layout()
            {
                title.Width = Math.Max(100f, UnpaddedSize.X - 24f);
                description.Width = title.Width;
                divider.Width = UnpaddedSize.X;
            }
        }

        private abstract class SettingRow : HudElementBase
        {
            protected readonly Label title;
            protected readonly Label description;
            private readonly TexturedBox divider;
            private bool editable = true;

            protected SettingRow(string heading, string detail, float height = 128f) : base(null)
            {
                Size = new Vector2(620f, height);
                title = new Label(this)
                {
                    Text = heading,
                    AutoResize = false,
                    Format = TerminalFormatting.ControlFormat,
                    ParentAlignment = ParentAlignments.InnerTopLeft,
                    Offset = new Vector2(14f, -10f),
                    Size = new Vector2(310f, 25f),
                };
                description = new Label(this)
                {
                    Text = detail,
                    AutoResize = false,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .8f),
                    ParentAlignment = ParentAlignments.InnerBottomLeft,
                    Offset = new Vector2(14f, 8f),
                    Size = new Vector2(310f, height - 35f),
                };
                divider = new TexturedBox(this)
                {
                    Color = new Color(61, 70, 78, 135),
                    ParentAlignment = ParentAlignments.InnerBottom,
                    Height = 1f,
                };
            }

            internal bool Editable
            {
                get { return editable; }
            }

            internal virtual void SetEditable(bool value)
            {
                editable = value;
                InputEnabled = value;
                title.Format = value
                    ? TerminalFormatting.ControlFormat
                    : new GlyphFormat(TerminalFormatting.MidGrey, TextAlignment.Left, 1.08f);
            }

            internal abstract void Refresh();

            internal virtual bool ValidateDraft(out string error) { error = null; return true; }
            internal virtual void StageDraft() { }
            internal virtual void DiscardDraft() { }

            protected override void Layout()
            {
                var width = Math.Max(200f, UnpaddedSize.X - 28f);
                title.Size = new Vector2(width, 28f);
                title.ParentAlignment = ParentAlignments.InnerTopLeft;
                title.Offset = new Vector2(14f, -8f);
                description.Width = width;
                description.LineWrapWidth = width;
                description.Height = Math.Max(24f, description.TextBoard.TextSize.Y);
                description.VertCenterText = false;
                description.ParentAlignment = ParentAlignments.InnerTopLeft;
                description.Offset = new Vector2(14f, -40f);
                Height = 40f + description.Height + 12f + 38f + 16f;
                divider.Width = UnpaddedSize.X;
                LayoutControl(width);
            }

            protected void PlaceControl(HudElementBase element, float left, float width, float height = 38f)
            {
                element.ParentAlignment = ParentAlignments.Center;
                element.Size = new Vector2(width, height);
                element.Offset = new Vector2(-UnpaddedSize.X * .5f + 14f + left + width * .5f,
                    -Height * .5f + 16f + 19f);
            }

            protected abstract void LayoutControl(float width);
        }

        private sealed class BooleanSettingRow : SettingRow
        {
            private readonly BorderedCheckBox control;
            private readonly Func<string> getter;

            internal BooleanSettingRow(string title, string description, string tooltip, Func<string> getter, Action<string> changed)
                : base(title, description)
            {
                this.getter = getter;
                control = new BorderedCheckBox(this)
                {
                    ParentAlignment = ParentAlignments.InnerRight,
                };
                control.MouseInput.ToolTip = tooltip;
                control.ValueChanged += delegate
                {
                    var value = control.Value ? "true" : "false";
                    if (Editable && !string.Equals(value, getter(), StringComparison.OrdinalIgnoreCase))
                        changed(value);
                };
            }

            internal override void Refresh()
            {
                control.Value = ParseBool(getter());
            }

            protected override void LayoutControl(float width)
            {
                PlaceControl(control, 0f, 32f, 32f);
            }
        }

        private sealed class SliderSettingRow : SettingRow
        {
            private readonly SliderBox control;
            private readonly WkSettingsTextField valueField;
            private readonly double minimum, maximum;
            private string lastFieldText = string.Empty;
            private readonly Func<string> getter;
            private readonly Action<string> changed;
            private readonly bool integer;
            private readonly WkSettingPresentation presentation;
            private bool refreshing;

            internal SliderSettingRow(string title, string description, string tooltip, Func<string> getter, Action<string> changed, WkSettingPresentation presentation, bool integer, double minimum, double maximum)
                : base(title, description, 128f)
            {
                this.getter = getter;
                this.changed = changed;
                this.integer = integer;
                this.presentation = presentation;
                this.minimum = minimum;
                this.maximum = maximum;
                valueField = new WkSettingsTextField(this)
                {
                    Text = string.Empty,
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(190f, 32f),
                    Offset = new Vector2(-100f, 20f),
                };
                valueField.MouseInput.ToolTip = "Enter a value, then use Apply Changes below. Valid range: " +
                    (minimum * presentation.DisplayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + " to " +
                    (maximum * presentation.DisplayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + presentation.Suffix +
                    ". The unit suffix is optional.\nSlider range: " +
                    (presentation.Minimum * presentation.DisplayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + " to " +
                    (presentation.Maximum * presentation.DisplayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + presentation.Suffix + ".\n" + description;
                control = new SliderBox(this)
                {
                    Min = presentation.SliderScale == WkSettingSliderScale.LogarithmicWithZero ? 0f : (float)presentation.Minimum,
                    Max = presentation.SliderScale == WkSettingSliderScale.LogarithmicWithZero ? 1f : (float)presentation.Maximum,
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(280f, 34f),
                    Offset = new Vector2(-20f, -10f),
                };
                control.MouseInput.ToolTip = tooltip;
                control.ValueChanged += OnValueChanged;
            }

            internal override void SetEditable(bool value)
            {
                base.SetEditable(value);
                valueField.EnableEditing = value;
                if (!value)
                    valueField.StopEditing();
            }

            internal override void Refresh()
            {
                var canonicalValue = ParseNumber(getter());
                refreshing = true;
                control.Value = ToSliderValue(canonicalValue, presentation);
                refreshing = false;
                if (!valueField.FocusHandler.HasFocus && valueField.Value.ToString() == lastFieldText)
                    SetFieldValue(canonicalValue);
            }

            protected override void LayoutControl(float width)
            {
                const float fieldWidth = 160f;
                PlaceControl(control, 0f, width - fieldWidth - 16f, 34f);
                PlaceControl(valueField, width - fieldWidth, fieldWidth);
            }

            private void SetFieldValue(double canonicalValue)
            {
                // Keep typed precision; slider steps apply only to slider gestures.
                lastFieldText = (canonicalValue * presentation.DisplayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + presentation.Suffix;
                valueField.Text = lastFieldText;
            }

            internal override bool ValidateDraft(out string error)
            {
                error = null;
                if (!Editable || valueField.Value.ToString() == lastFieldText)
                    return true;
                string value;
                if (WkSettingNumericInput.TryParse(valueField.Value.ToString(), presentation.DisplayMultiplier,
                    presentation.Suffix, minimum, maximum, integer, out value, out error))
                    return true;
                error = title.Text.ToString() + ": " + error;
                return false;
            }

            internal override void StageDraft()
            {
                if (!Editable || valueField.Value.ToString() == lastFieldText)
                    return;
                string value, error;
                if (WkSettingNumericInput.TryParse(valueField.Value.ToString(), presentation.DisplayMultiplier,
                    presentation.Suffix, minimum, maximum, integer, out value, out error))
                {
                    lastFieldText = valueField.Value.ToString();
                    changed(value);
                }
            }

            internal override void DiscardDraft()
            {
                valueField.StopEditing();
                SetFieldValue(ParseNumber(getter()));
            }

            private void OnValueChanged(object sender, EventArgs args)
            {
                if (refreshing)
                    return;

                var canonicalValue = Quantize(FromSliderValue(control.Value, presentation), presentation);
                var value = FormatCanonicalValue(canonicalValue, integer);
                SetFieldValue(canonicalValue);
                var authoritative = ParseNumber(getter());
                if (!Editable || (integer
                    ? (int)Math.Round(canonicalValue) == (int)Math.Round(authoritative)
                    : Math.Abs(canonicalValue - authoritative) < 0.0000005))
                    return;

                changed(value);
            }
        }

        private sealed class ChoiceSettingRow : SettingRow
        {
            private readonly Dropdown<string> control;
            private readonly Func<string> getter;
            private readonly Action<string> changed;

            internal ChoiceSettingRow(
                string title,
                string description,
                string tooltip,
                Func<string> getter,
                Action<string> changed,
                string[] choices,
                Func<string, bool> enabled = null)
                : base(title, description)
            {
                this.getter = getter;
                this.changed = changed;
                control = new Dropdown<string>(this)
                {
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(280f, 42f),
                    Offset = new Vector2(-20f, 0f),
                    DropdownHeight = 170f,
                };
                control.MouseInput.ToolTip = tooltip;
                for (var i = 0; i < choices.Length; i++)
                {
                    var choice = choices[i];
                    control.Add(new RichText(ToDisplayName(choice)), choice, enabled == null || enabled(choice));
                }
                control.ValueChanged += OnValueChanged;
            }

            internal override void Refresh()
            {
                var value = getter();
                if (control.Value == null || !string.Equals(control.Value.AssocMember, value, StringComparison.OrdinalIgnoreCase))
                    control.SetSelection(value);
            }

            protected override void LayoutControl(float width)
            {
                PlaceControl(control, 0f, Math.Min(360f, width));
            }

            private void OnValueChanged(object sender, EventArgs args)
            {
                if (!Editable || control.Value == null)
                    return;

                var value = control.Value.AssocMember;
                if (!string.Equals(value, getter(), StringComparison.OrdinalIgnoreCase))
                    changed(value);
            }
        }

        private sealed class TextSettingRow : SettingRow
        {
            private readonly WkSettingsTextField field;
            private readonly Func<string> getter;
            private readonly Action<string> changed;
            private readonly bool defaultableNumber;
            private string lastFieldText = string.Empty;

            internal TextSettingRow(string title, string description, string tooltip, Func<string> getter, Action<string> changed, bool defaultableNumber)
                : base(title, description)
            {
                this.getter = getter;
                this.changed = changed;
                this.defaultableNumber = defaultableNumber;
                field = new WkSettingsTextField(this) { Text = string.Empty };
                field.MouseInput.ToolTip = tooltip;
            }

            internal override void SetEditable(bool value)
            {
                base.SetEditable(value);
                field.EnableEditing = value;
                if (!value)
                    field.StopEditing();
            }

            internal override void Refresh()
            {
                if (!field.FocusHandler.HasFocus && field.Value.ToString() == lastFieldText)
                    DiscardDraft();
            }

            internal override void DiscardDraft()
            {
                field.StopEditing();
                lastFieldText = getter();
                field.Text = lastFieldText;
            }

            internal override bool ValidateDraft(out string error)
            {
                error = null;
                if (Editable && string.IsNullOrWhiteSpace(field.Value.ToString()))
                {
                    error = title.Text.ToString() + ": enter a value.";
                    return false;
                }
                if (Editable && defaultableNumber && !field.Value.ToString().Trim().Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    string value;
                    if (!WkSettingNumericInput.TryParse(field.Value.ToString(), 1.0, " s", 0.0, 30.0, false, out value, out error))
                    {
                        error = title.Text.ToString() + ": " + error;
                        return false;
                    }
                }
                return true;
            }

            internal override void StageDraft()
            {
                if (Editable && field.Value.ToString() != lastFieldText)
                {
                    lastFieldText = field.Value.ToString();
                    var value = lastFieldText.Trim();
                    if (defaultableNumber && !value.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        string error;
                        WkSettingNumericInput.TryParse(value, 1.0, " s", 0.0, 30.0, false, out value, out error);
                    }
                    changed(value);
                }
            }

            protected override void LayoutControl(float width)
            {
                PlaceControl(field, 0f, Math.Min(420f, width));
            }
        }

        private sealed class ReadOnlySettingRow : SettingRow
        {
            private readonly Label value;
            private readonly Func<string> getter;

            internal ReadOnlySettingRow(string title, string description, Func<string> getter)
                : base(title, description)
            {
                this.getter = getter;
                value = new Label(this)
                {
                    AutoResize = false,
                    Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Right),
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(280f, 32f),
                    Offset = new Vector2(-20f, 0f),
                };
            }

            internal override void Refresh()
            {
                value.Text = ToDisplayName(getter());
            }

            protected override void LayoutControl(float width)
            {
                PlaceControl(value, 0f, width);
            }
        }

        private sealed class ActionSettingRow : SettingRow
        {
            private readonly BorderedButton button;

            internal ActionSettingRow(string title, string description, string buttonText, Action action)
                : base(title, description)
            {
                button = new BorderedButton(this)
                {
                    Text = buttonText,
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(280f, 42f),
                    Padding = Vector2.Zero,
                    Offset = new Vector2(-20f, 0f),
                };
                button.MouseInput.LeftClicked += delegate { if (Editable) action(); };
            }

            internal override void Refresh()
            {
            }

            protected override void LayoutControl(float width)
            {
                PlaceControl(button, 0f, Math.Min(360f, width));
            }
        }

        private sealed class InformationRow : SettingRow
        {
            internal InformationRow(string text) : base("", text, 94f)
            {
                title.Visible = false;
                description.Offset = new Vector2(14f, 0f);
                description.Height = 80f;
                description.Format = new GlyphFormat(TerminalFormatting.Mercury, TextAlignment.Left, .82f);
            }

            internal override void Refresh()
            {
            }

            protected override void LayoutControl(float width)
            {
                description.Offset = new Vector2(14f, -10f);
                Height = description.Height + 24f;
            }
        }
    }
}
