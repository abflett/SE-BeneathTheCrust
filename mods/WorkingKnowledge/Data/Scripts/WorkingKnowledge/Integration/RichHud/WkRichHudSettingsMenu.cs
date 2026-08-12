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
        private ControlPage worldPage;
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

            playerPage = new ControlPage { Name = "Player Settings" };
            AddPlayerCategories(playerPage);
            RichHudTerminal.Root.Add(playerPage);

            worldPage = new ControlPage { Name = "Server Settings" };
            AddDifficultyCategory(worldPage);
            foreach (var categoryName in worldStore.GetDisplayCategories())
            {
                if (categoryName.Equals("Difficulty", StringComparison.OrdinalIgnoreCase))
                    continue;

                var category = new ControlCategory
                {
                    HeaderText = categoryName,
                    SubheaderText = "Authoritative world settings. Administrator access is required.",
                };

                var definitions = worldStore.Settings;
                for (var i = 0; i < definitions.Count; i++)
                {
                    if (definitions[i].Category.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                        category.Add(CreateWorldTile(definitions[i]));
                }

                worldPage.Add(category);
            }

            RichHudTerminal.Root.Add(worldPage);

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
            if (worldPage != null)
                worldPage.Enabled = true;

            for (var i = 0; i < worldControls.Count; i++)
                worldControls[i].Enabled = canEditWorld;
        }

        internal void Close()
        {
            live = false;
            worldControls.Clear();
            playerPage = null;
            worldPage = null;
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

        private void AddPlayerCategories(ControlPage page)
        {
            var display = new ControlCategory
            {
                HeaderText = "Progress Display",
                SubheaderText = "Configure the Rich HUD progress overlay.",
            };
            var feedback = new ControlCategory
            {
                HeaderText = "Feedback",
                SubheaderText = "Configure personal chat, toast, and sound feedback.",
            };

            var definitions = playerStore.Settings;
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition.Setting.StartsWith("progressHud", StringComparison.OrdinalIgnoreCase))
                    display.Add(CreatePlayerTile(definition));
                else
                    feedback.Add(CreatePlayerTile(definition));
            }

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
            feedback.Add(new ControlTile { ControlContainer = { resetButton } });

            page.Add(display);
            page.Add(feedback);
        }

        private void AddDifficultyCategory(ControlPage page)
        {
            var category = new ControlCategory
            {
                HeaderText = "Difficulty",
                SubheaderText = "Apply a complete preset or reset every world setting.",
            };
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
            category.Add(new ControlTile { ControlContainer = { dropdown } });

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
            category.Add(new ControlTile { ControlContainer = { resetButton } });
            page.Add(category);
        }

        private ControlTile CreatePlayerTile(WkPlayerConfigSettingDefinition definition)
        {
            return CreateTile(
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

        private ControlTile CreateWorldTile(WkConfigSettingDefinition definition)
        {
            var tile = CreateTile(
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
            return tile;
        }

        private ControlTile CreateTile(
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
            var tile = new ControlTile();
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
                return tile;
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
                return tile;
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
                return tile;
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
            return tile;
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
