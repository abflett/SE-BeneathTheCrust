using System;
using System.Collections.Generic;
using System.Globalization;
using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
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
        private readonly ScrollBox navigation;
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
            : base(HudMain.HighDpiRoot)
        {
            this.playerStore = playerStore;
            this.worldStore = worldStore;
            this.getPlayerConfig = getPlayerConfig;
            this.getWorldConfig = getWorldConfig;
            this.getEffectiveValue = getEffectiveValue;
            this.applyValue = applyValue;
            this.applyDifficulty = applyDifficulty;
            this.resetConfig = resetConfig;

            HeaderBuilder.Format = TerminalFormatting.HeaderFormat;
            HeaderText = new RichText("Working Knowledge Settings");
            header.Background.Visible = false;
            header.Height = 58f;

            closeButton = new BorderedButton(header)
            {
                Text = "X",
                Size = new Vector2(42f, 34f),
                Padding = Vector2.Zero,
                TextPadding = Vector2.Zero,
                ParentAlignment = ParentAlignments.InnerTopRight,
                Offset = new Vector2(-18f, -12f),
            };
            closeButton.MouseInput.LeftClicked += delegate { Hide(); };
            header.MouseInput.RequestCursor = true;

            navigation = new ScrollBox(true, body)
            {
                Color = TerminalFormatting.DarkSlateGrey,
                Padding = new Vector2(10f, 12f),
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Spacing = 3f,
            };
            navigation.ScrollBar.Padding = new Vector2(12f, 10f);
            navigation.ScrollBar.Width = 6f;

            columnDivider = new TexturedBox(body)
            {
                Color = TerminalFormatting.LimedSpruce,
                Width = 1f,
            };

            MouseInput.RequestCursor = true;
            bodyBaseColor = new Color(27, 35, 41, 242);
            BodyColor = bodyBaseColor;
            BorderColor = new Color(84, 98, 107);
            MinimumSize = new Vector2(920f, 560f);
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
                var screen = HudMain.ScreenDimHighDPI;
                Offset = (screen - Size) * .5f - new Vector2(40f);
                Visible = true;
            }

            live = true;
            GetWindowFocus();
            Refresh(canEditWorld);
        }

        internal void Hide()
        {
            Visible = false;
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
            Visible = false;
            Unregister();
            pages.Clear();
            rows.Clear();
            worldRows.Clear();
            navigationButtons.Clear();
            selectedPage = null;
        }

        protected override void Layout()
        {
            base.Layout();

            BodyColor = bodyBaseColor.SetAlphaPct(HudMain.UiBkOpacity);
            var contentHeight = Math.Max(300f, body.UnpaddedSize.Y - (WindowMargin * 2f));
            var contentWidth = Math.Max(420f, body.UnpaddedSize.X - SidebarWidth - (WindowMargin * 2f) - ColumnGap);

            navigation.Size = new Vector2(SidebarWidth, contentHeight);
            navigation.ParentAlignment = ParentAlignments.InnerLeft;
            navigation.Offset = new Vector2(WindowMargin, 0f);

            columnDivider.Height = contentHeight;
            columnDivider.ParentAlignment = ParentAlignments.InnerLeft;
            columnDivider.Offset = new Vector2(WindowMargin + SidebarWidth + (ColumnGap * .5f), 0f);

            for (var i = 0; i < pages.Count; i++)
            {
                pages[i].Size = new Vector2(contentWidth, contentHeight);
                pages[i].ParentAlignment = ParentAlignments.InnerRight;
                pages[i].Offset = new Vector2(-WindowMargin, 0f);
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (MyAPIGateway.Gui != null && MyAPIGateway.Gui.IsCursorVisible)
                Hide();

            base.HandleInput(cursorPos);

            var halfScreen = HudMain.ScreenDimHighDPI * .5f;
            Offset = Vector2.Clamp(Offset, -halfScreen, halfScreen);
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
                delegate { if (live) resetConfig(false); },
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
                delegate { if (live && canEditWorld) resetConfig(true); },
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

            var button = new BorderedButton
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
            navigation.Add(button);
            navigationButtons.Add(button);
            return page;
        }

        private void AddNavigationHeading(string text)
        {
            var heading = new LabelBox
            {
                Text = text,
                AutoResize = false,
                Size = new Vector2(SidebarWidth - 34f, 32f),
                TextPadding = new Vector2(10f, 0f),
                Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .78f),
                Color = new Color(0, 0, 0, 0),
            };
            navigation.Add(heading);
        }

        private void SelectPage(SettingsPage page, BorderedButton button)
        {
            if (selectedPage != null)
                selectedPage.Visible = false;

            for (var i = 0; i < navigationButtons.Count; i++)
            {
                navigationButtons[i].Color = TerminalFormatting.OuterSpace;
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
                applyValue(setting, value, command, isWorld);
            };

            SettingRow row;
            if (kind == WkSettingControlKind.Boolean)
                row = new BooleanSettingRow(title, description, tooltip, getValue, changed);
            else if (kind == WkSettingControlKind.Number || kind == WkSettingControlKind.Integer)
                row = new SliderSettingRow(title, description, tooltip, getValue, changed, presentation, kind == WkSettingControlKind.Integer);
            else if (kind == WkSettingControlKind.Choice)
                row = new ChoiceSettingRow(title, description, tooltip, getValue, changed, choices);
            else if (kind == WkSettingControlKind.ReadOnly)
                row = new ReadOnlySettingRow(title, description, getValue);
            else
                row = new TextSettingRow(title, description, tooltip, getValue, changed);

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

                    applyDifficulty(value, "/wk difficulty " + value);
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

        private static float ParseFloat(string value)
        {
            float parsed;
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) &&
                   !float.IsNaN(parsed) &&
                   !float.IsInfinity(parsed)
                ? parsed
                : 0f;
        }

        private static string FormatCanonicalValue(double value, bool integer)
        {
            return integer
                ? ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture)
                : value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string FormatDisplayValue(double canonicalValue, bool integer, WkSettingPresentation presentation)
        {
            if (integer)
                return ((int)Math.Round(canonicalValue)).ToString(CultureInfo.InvariantCulture) + presentation.Suffix;

            var displayed = canonicalValue * presentation.DisplayMultiplier;
            var format = presentation.DecimalPlaces <= 0
                ? "0"
                : "0." + new string('#', presentation.DecimalPlaces);
            return displayed.ToString(format, CultureInfo.InvariantCulture) + presentation.Suffix;
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

            protected SettingRow(string heading, string detail, float height = 92f) : base(null)
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
                    Format = new GlyphFormat(TerminalFormatting.MistBlue, TextAlignment.Left, .68f),
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

            protected override void Layout()
            {
                var controlWidth = Math.Min(300f, Math.Max(220f, UnpaddedSize.X * .38f));
                var textWidth = Math.Max(180f, UnpaddedSize.X - controlWidth - 48f);
                title.Width = textWidth;
                description.Width = textWidth;
                description.TextBoard.LineWrapWidth = textWidth;
                divider.Width = UnpaddedSize.X;
                LayoutControl(controlWidth);
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
                control.Offset = new Vector2(-24f, 0f);
            }
        }

        private sealed class SliderSettingRow : SettingRow
        {
            private readonly SliderBox control;
            private readonly Label valueLabel;
            private readonly Func<string> getter;
            private readonly Action<string> changed;
            private readonly bool integer;
            private readonly WkSettingPresentation presentation;
            private bool refreshing;

            internal SliderSettingRow(string title, string description, string tooltip, Func<string> getter, Action<string> changed, WkSettingPresentation presentation, bool integer)
                : base(title, description, 98f)
            {
                this.getter = getter;
                this.changed = changed;
                this.integer = integer;
                this.presentation = presentation;
                valueLabel = new Label(this)
                {
                    AutoResize = false,
                    Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Right),
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(280f, 24f),
                    Offset = new Vector2(-20f, 17f),
                };
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

            internal override void Refresh()
            {
                var canonicalValue = ParseFloat(getter());
                refreshing = true;
                control.Value = ToSliderValue(canonicalValue, presentation);
                refreshing = false;
                valueLabel.Text = FormatDisplayValue(canonicalValue, integer, presentation);
            }

            protected override void LayoutControl(float width)
            {
                control.Width = width;
                valueLabel.Width = width;
            }

            private void OnValueChanged(object sender, EventArgs args)
            {
                if (refreshing)
                    return;

                var canonicalValue = Quantize(FromSliderValue(control.Value, presentation), presentation);
                var value = FormatCanonicalValue(canonicalValue, integer);
                valueLabel.Text = FormatDisplayValue(canonicalValue, integer, presentation);
                var authoritative = ParseFloat(getter());
                if (!Editable || (integer
                    ? (int)Math.Round(canonicalValue) == (int)Math.Round(authoritative)
                    : Math.Abs(canonicalValue - authoritative) < Math.Max(.000001, presentation.Step * .25)))
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
                control.Width = width;
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
            private readonly TextField field;
            private readonly BorderedButton apply;
            private readonly Func<string> getter;

            internal TextSettingRow(string title, string description, string tooltip, Func<string> getter, Action<string> changed)
                : base(title, description, 98f)
            {
                this.getter = getter;
                field = new TextField(this)
                {
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(190f, 40f),
                    Offset = new Vector2(-110f, 0f),
                };
                field.MouseInput.ToolTip = tooltip;
                apply = new BorderedButton(this)
                {
                    Text = "Apply",
                    ParentAlignment = ParentAlignments.InnerRight,
                    Size = new Vector2(92f, 40f),
                    Padding = Vector2.Zero,
                    TextPadding = Vector2.Zero,
                    Offset = new Vector2(-18f, 0f),
                };
                apply.MouseInput.LeftClicked += delegate
                {
                    if (Editable)
                        changed(field.Value.ToString());
                };
            }

            internal override void Refresh()
            {
                if (!field.FocusHandler.HasFocus)
                {
                    var value = getter();
                    if (!string.Equals(field.Value.ToString(), value, StringComparison.Ordinal))
                        field.Text = value;
                }
            }

            protected override void LayoutControl(float width)
            {
                apply.Width = 92f;
                field.Width = Math.Max(110f, width - apply.Width - 10f);
                field.Offset = new Vector2(-(apply.Width + 30f), 0f);
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
                value.Width = width;
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
                button.Width = width;
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
                description.Width = Math.Max(100f, UnpaddedSize.X - 28f);
                description.TextBoard.LineWrapWidth = description.Width;
            }
        }
    }
}
