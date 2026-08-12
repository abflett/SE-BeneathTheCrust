using System;
using System.Collections.Generic;
using System.Globalization;
using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;

namespace WkKn
{
    internal sealed class WkRichHudSettingsMenu
    {
        private readonly WkPlayerConfigStore playerStore;
        private readonly WkConfigStore worldStore;
        private readonly Func<WkPlayerConfigRecord> getPlayerConfig;
        private readonly Func<WkConfig> getWorldConfig;
        private readonly Action<string, string, bool> setValue;
        private readonly Action<string> setDifficulty;
        private readonly Action<bool> resetConfig;
        private readonly List<TerminalControlBase> worldControls = new List<TerminalControlBase>();
        private readonly Dictionary<string, string> pendingValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> pendingCommands = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private ControlPage playerPage;
        private TerminalPageCategory serverCategory;
        private bool live;

        internal WkRichHudSettingsMenu(
            WkPlayerConfigStore playerStore,
            WkConfigStore worldStore,
            Func<WkPlayerConfigRecord> getPlayerConfig,
            Func<WkConfig> getWorldConfig,
            Action<string, string, bool> setValue,
            Action<string> setDifficulty,
            Action<bool> resetConfig)
        {
            this.playerStore = playerStore;
            this.worldStore = worldStore;
            this.getPlayerConfig = getPlayerConfig;
            this.getWorldConfig = getWorldConfig;
            this.setValue = setValue;
            this.setDifficulty = setDifficulty;
            this.resetConfig = resetConfig;
        }

        internal void Build()
        {
            live = false;
            RichHudTerminal.Root.Enabled = true;

            var playerCategory = new TerminalPageCategory { Name = "Player Settings", Enabled = true };
            playerPage = new ControlPage { Name = "Progress HUD", Enabled = true };
            AddPlayerProgressGroups(playerPage);
            playerCategory.Add(playerPage);

            var feedbackPage = new ControlPage { Name = "Feedback", Enabled = true };
            AddPlayerFeedbackGroups(feedbackPage);
            playerCategory.Add(feedbackPage);
            RichHudTerminal.Root.Add(playerCategory);

            serverCategory = new TerminalPageCategory { Name = "Server Settings", Enabled = true };
            var difficultyPage = new ControlPage { Name = "Difficulty", Enabled = true };
            AddDifficultyCategory(difficultyPage);
            serverCategory.Add(difficultyPage);
            foreach (var categoryName in worldStore.GetDisplayCategories())
            {
                if (categoryName.Equals("Difficulty", StringComparison.OrdinalIgnoreCase))
                    continue;

                var page = new ControlPage { Name = categoryName, Enabled = true };
                AddWorldGroups(page, categoryName);
                serverCategory.Add(page);
            }

            RichHudTerminal.Root.Add(serverCategory);

            var help = new TextPage
            {
                Name = "Help",
                HeaderText = new RichText("Working Knowledge Settings"),
                SubHeaderText = new RichText("Rich HUD Master integration"),
                Text = new RichText(
                    "Player settings affect only you. Server settings are authoritative and are available to administrators.\n\n" +
                    "Open this page with /wk settings. Existing /wk config and /wk difficulty commands remain fully supported.\n\n" +
                    "Numeric controls use the same parsing, validation, clamping, saving, and runtime update path as chat commands."),
            };
            RichHudTerminal.Root.Add(help);

            live = true;
        }

        internal void Open()
        {
            if (playerPage != null)
                RichHudTerminal.OpenToPage(playerPage);
        }

        internal void UpdateAccess(bool canEditWorld)
        {
            if (serverCategory != null)
                serverCategory.Enabled = true;

            for (var i = 0; i < worldControls.Count; i++)
                worldControls[i].Enabled = canEditWorld;
        }

        internal void Close()
        {
            live = false;
            worldControls.Clear();
            playerPage = null;
            serverCategory = null;
            pendingValues.Clear();
            pendingCommands.Clear();
        }

        internal void AcknowledgeCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            string matchedSetting = null;
            foreach (var pair in pendingCommands)
            {
                if (string.Equals(pair.Value, command, StringComparison.Ordinal))
                {
                    matchedSetting = pair.Key;
                    break;
                }
            }

            if (matchedSetting != null)
            {
                pendingCommands.Remove(matchedSetting);
                pendingValues.Remove(matchedSetting);
            }

            if (command.Equals("/wk config reset", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("/wk config world reset", StringComparison.OrdinalIgnoreCase))
            {
                pendingCommands.Clear();
                pendingValues.Clear();
            }
        }

        private void AddPlayerProgressGroups(ControlPage page)
        {
            AddPlayerGroup(
                page,
                "Display",
                "Visibility, history size, and fade timing.",
                "progressHudEnabled",
                "progressHudRows",
                "progressHudFadeSeconds");
            AddPlayerGroup(
                page,
                "Placement",
                "Screen anchor and fine position offsets.",
                "progressHudPosition",
                "progressHudOffsetX",
                "progressHudOffsetY");
            AddPlayerGroup(
                page,
                "Row Order",
                "Choose how recent schematic rows are arranged.",
                "progressHudOrder");
        }

        private void AddPlayerFeedbackGroups(ControlPage page)
        {
            AddPlayerGroup(
                page,
                "Notifications",
                "Enable personal chat and popup progress feedback.",
                "progressChatEnabled",
                "progressToastEnabled");
            AddPlayerGroup(
                page,
                "Chat Thresholds",
                "Minimum accumulated progress before another chat update.",
                "researchChatSuppressionPercent",
                "proficiencyChatSuppressionPercent");
            AddPlayerGroup(
                page,
                "Toast Thresholds",
                "Minimum accumulated progress before another popup update.",
                "researchToastSuppressionPercent",
                "proficiencyToastSuppressionPercent");
            AddPlayerGroup(
                page,
                "Sounds",
                "Personal completion and construction-botch audio.",
                "completionSoundEnabled",
                "weldBotchSoundEnabled");
            AddPlayerGroup(
                page,
                "Botch Warning",
                "Personal repeat-warning cooldown; default follows the world setting.",
                "weldBotchWarningCooldownSeconds");

            var resetButton = new TerminalButton
            {
                Name = "Reset Player Settings",
                ToolTip = "Restore your Working Knowledge player settings to their defaults.",
            };
            resetButton.ControlChangedHandler = delegate
            {
                if (live)
                    resetConfig(false);
            };
            AddControlGroup(page, "Reset", "Restore all personal settings.", new ControlTile { ControlContainer = { resetButton } });
        }

        private void AddPlayerGroup(ControlPage page, string header, string description, params string[] settingNames)
        {
            var tile = new ControlTile();
            for (var i = 0; i < settingNames.Length; i++)
            {
                var definition = FindPlayerSetting(settingNames[i]);
                if (definition != null)
                    AddPlayerControl(tile, definition);
            }

            AddControlGroup(page, header, description, tile);
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

        private void AddWorldGroups(ControlPage page, string categoryName)
        {
            var group = new List<WkConfigSettingDefinition>();
            var controlCount = 0;
            var groupNumber = 1;
            var definitions = worldStore.Settings;
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (!definition.Category.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var weight = definition.ControlKind == WkSettingControlKind.Text ? 2 : 1;
                if (group.Count > 0 && controlCount + weight > 3)
                {
                    AddWorldGroup(page, categoryName, groupNumber++, group);
                    group = new List<WkConfigSettingDefinition>();
                    controlCount = 0;
                }

                group.Add(definition);
                controlCount += weight;
            }

            if (group.Count > 0)
                AddWorldGroup(page, categoryName, groupNumber, group);
        }

        private void AddWorldGroup(ControlPage page, string categoryName, int groupNumber, List<WkConfigSettingDefinition> definitions)
        {
            var tile = new ControlTile();
            for (var i = 0; i < definitions.Count; i++)
                AddWorldControl(tile, definitions[i]);

            var header = groupNumber == 1
                ? categoryName + " Settings"
                : categoryName + " Settings — Continued";
            AddControlGroup(page, header, "Authoritative world settings. Administrator access is required.", tile);
        }

        private static void AddControlGroup(ControlPage page, string header, string description, ControlTile tile)
        {
            var category = new ControlCategory
            {
                HeaderText = header,
                SubheaderText = description,
            };
            category.Add(tile);
            page.Add(category);
        }

        private void AddDifficultyCategory(ControlPage page)
        {
            var tile = new ControlTile();
            var dropdown = new TerminalDropdown<string>
            {
                Name = "Difficulty Preset",
                ToolTip = "Applies the selected Working Knowledge difficulty preset.",
            };
            foreach (var preset in worldStore.GetDifficultyPresetNames())
                dropdown.List.Add(new RichText(ToDisplayName(preset)), preset);
            dropdown.List.Add(new RichText("Custom"), "custom");
            dropdown.List[dropdown.List.Count - 1].Enabled = false;
            dropdown.CustomValueGetter = delegate { return FindChoice(dropdown, GetEffectiveValue("difficultyPreset", delegate { return GetWorldValue("difficultyPreset"); })); };
            dropdown.ControlChangedHandler = delegate
            {
                if (live && dropdown.Value != null &&
                    !string.Equals(dropdown.Value.AssocObject, GetEffectiveValue("difficultyPreset", delegate { return GetWorldValue("difficultyPreset"); }), StringComparison.OrdinalIgnoreCase))
                {
                    SetPendingValue("difficultyPreset", dropdown.Value.AssocObject, "/wk difficulty " + dropdown.Value.AssocObject);
                    setDifficulty(dropdown.Value.AssocObject);
                }
            };
            worldControls.Add(dropdown);
            tile.Add(dropdown);

            var resetButton = new TerminalButton
            {
                Name = "Reset Server Settings",
                ToolTip = "Restore all authoritative world settings to the default medium preset.",
            };
            resetButton.ControlChangedHandler = delegate
            {
                if (live)
                    resetConfig(true);
            };
            worldControls.Add(resetButton);
            tile.Add(resetButton);
            AddControlGroup(page, "Difficulty", "Apply a complete preset or reset every world setting.", tile);
        }

        private void AddPlayerControl(ControlTile tile, WkPlayerConfigSettingDefinition definition)
        {
            AddControl(
                tile,
                definition.Setting,
                definition.Title,
                definition.ValueHint,
                definition.Description,
                definition.ControlKind,
                definition.Minimum,
                definition.Maximum,
                definition.Choices,
                delegate { var config = getPlayerConfig(); return GetEffectiveValue(definition.Setting, delegate { return config == null ? string.Empty : definition.GetValue(config); }); },
                false);
        }

        private void AddWorldControl(ControlTile tile, WkConfigSettingDefinition definition)
        {
            AddControl(
                tile,
                definition.Setting,
                definition.Title,
                definition.ValueHint,
                definition.Description,
                definition.ControlKind,
                definition.Minimum,
                definition.Maximum,
                definition.Choices,
                delegate { var config = getWorldConfig(); return GetEffectiveValue(definition.Setting, delegate { return config == null ? string.Empty : definition.GetValue(config); }); },
                true);
        }

        private void AddControl(
            ControlTile tile,
            string setting,
            string title,
            string valueHint,
            string description,
            WkSettingControlKind kind,
            double minimum,
            double maximum,
            string[] choices,
            Func<string> getValue,
            bool isWorld)
        {
            var tooltip = description + "\nValue: " + valueHint;

            if (kind == WkSettingControlKind.Boolean)
            {
                var toggle = new TerminalOnOffButton
                {
                    Name = title,
                    ToolTip = tooltip,
                    CustomValueGetter = delegate { return ParseBool(getValue()); },
                };
                toggle.ControlChangedHandler = delegate
                {
                    if (live && toggle.Value != ParseBool(getValue()))
                    {
                        var value = toggle.Value ? "true" : "false";
                        SetPendingValue(setting, value, "/wk config " + setting + " " + value);
                        setValue(setting, value, isWorld);
                    }
                };
                tile.Add(toggle);
                TrackWorldControl(toggle, isWorld);
                return;
            }

            if (kind == WkSettingControlKind.Number || kind == WkSettingControlKind.Integer)
            {
                var slider = new TerminalSlider
                {
                    Name = title,
                    ToolTip = tooltip,
                    Min = (float)minimum,
                    Max = (float)maximum,
                    CustomValueGetter = delegate { return ParseFloat(getValue()); },
                };
                slider.ControlChangedHandler = delegate
                {
                    if (!live)
                        return;

                    var authoritativeValue = ParseFloat(getValue());
                    if (kind == WkSettingControlKind.Integer)
                    {
                        if ((int)Math.Round(slider.Value) == (int)Math.Round(authoritativeValue))
                            return;
                    }
                    else if (Math.Abs(slider.Value - authoritativeValue) < 0.0001f)
                    {
                        return;
                    }

                    var value = kind == WkSettingControlKind.Integer
                        ? ((int)Math.Round(slider.Value)).ToString(CultureInfo.InvariantCulture)
                        : slider.Value.ToString("0.####", CultureInfo.InvariantCulture);
                    SetPendingValue(setting, value, "/wk config " + setting + " " + value);
                    setValue(setting, value, isWorld);
                };
                tile.Add(slider);
                TrackWorldControl(slider, isWorld);
                return;
            }

            if (kind == WkSettingControlKind.Choice)
            {
                var dropdown = new TerminalDropdown<string>
                {
                    Name = title,
                    ToolTip = tooltip,
                };
                for (var i = 0; i < choices.Length; i++)
                    dropdown.List.Add(new RichText(ToDisplayName(choices[i])), choices[i]);
                dropdown.CustomValueGetter = delegate { return FindChoice(dropdown, getValue()); };
                dropdown.ControlChangedHandler = delegate
                {
                    if (live && dropdown.Value != null &&
                        !string.Equals(dropdown.Value.AssocObject, getValue(), StringComparison.OrdinalIgnoreCase))
                    {
                        var value = dropdown.Value.AssocObject;
                        SetPendingValue(setting, value, "/wk config " + setting + " " + value);
                        setValue(setting, value, isWorld);
                    }
                };
                tile.Add(dropdown);
                TrackWorldControl(dropdown, isWorld);
                return;
            }

            var field = new TerminalTextField
            {
                Name = title,
                ToolTip = tooltip,
                CustomValueGetter = getValue,
            };
            var apply = new TerminalButton
            {
                Name = "Apply " + title,
                ToolTip = "Validate and apply the value entered above.",
            };
            apply.ControlChangedHandler = delegate
            {
                if (live)
                {
                    SetPendingValue(setting, field.Value, "/wk config " + setting + " " + field.Value);
                    setValue(setting, field.Value, isWorld);
                }
            };
            tile.Add(field);
            tile.Add(apply);
            TrackWorldControl(field, isWorld);
            TrackWorldControl(apply, isWorld);
        }

        private string GetWorldValue(string setting)
        {
            var config = getWorldConfig();
            var definitions = worldStore.Settings;
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Setting.Equals(setting, StringComparison.OrdinalIgnoreCase))
                    return config == null ? string.Empty : definitions[i].GetValue(config);
            }

            return string.Empty;
        }

        private void TrackWorldControl(TerminalControlBase control, bool isWorld)
        {
            if (isWorld)
                worldControls.Add(control);
        }

        private string GetEffectiveValue(string setting, Func<string> authoritativeGetter)
        {
            string pending;
            return pendingValues.TryGetValue(setting, out pending)
                ? pending
                : authoritativeGetter();
        }

        private void SetPendingValue(string setting, string value, string command)
        {
            pendingValues[setting] = value;
            pendingCommands[setting] = command;
        }

        private static EntryData<string> FindChoice(TerminalDropdown<string> dropdown, string value)
        {
            for (var i = 0; i < dropdown.List.Count; i++)
            {
                if (string.Equals(dropdown.List[i].AssocObject, value, StringComparison.OrdinalIgnoreCase))
                    return dropdown.List[i];
            }

            return dropdown.List.Count > 0 ? dropdown.List[0] : null;
        }

        private static bool ParseBool(string value)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) && parsed;
        }

        private static float ParseFloat(string value)
        {
            float parsed;
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ? parsed : 0f;
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
    }
}
