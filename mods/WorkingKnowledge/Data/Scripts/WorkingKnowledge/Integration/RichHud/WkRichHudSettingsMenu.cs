using System;
using System.Collections.Generic;
using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;

namespace WkKn
{
    /// <summary>
    /// Connects the custom Working Knowledge settings window to the Rich HUD Terminal launcher
    /// and the existing server-authoritative command path.
    /// </summary>
    internal sealed class WkRichHudSettingsMenu
    {
        private readonly WkPlayerConfigStore playerStore;
        private readonly WkConfigStore worldStore;
        private readonly Func<WkPlayerConfigRecord> getPlayerConfig;
        private readonly Func<WkConfig> getWorldConfig;
        private readonly Action<string, string, bool> setValue;
        private readonly Action<string> setDifficulty;
        private readonly Action<bool> resetConfig;
        private readonly Dictionary<string, string> pendingValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> pendingCommands = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private WkSettingsWindow settingsWindow;
        private bool live;
        private bool canEditWorld;

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

            var launcherPage = new ControlPage { Name = "Settings", Enabled = true };
            AddSettingsLauncher(launcherPage);
            RichHudTerminal.Root.Add(launcherPage);

            var help = new TextPage
            {
                Name = "Help",
                HeaderText = new RichText("Working Knowledge Settings"),
                SubHeaderText = new RichText("Rich HUD Master integration"),
                Text = new RichText(
                    "Player settings affect only you. Server settings are authoritative and are available to administrators.\n\n" +
                    "Open the compact Working Knowledge window with /wk settings or the Settings launcher. Existing /wk config and /wk difficulty commands remain fully supported.\n\n" +
                    "Every control uses the same parsing, validation, clamping, saving, and multiplayer path as the chat commands."),
            };
            RichHudTerminal.Root.Add(help);

            settingsWindow = new WkSettingsWindow(
                playerStore,
                worldStore,
                getPlayerConfig,
                getWorldConfig,
                GetEffectiveValue,
                ApplyWindowValue,
                ApplyWindowDifficulty,
                resetConfig);

            live = true;
        }

        internal void Open()
        {
            if (settingsWindow != null)
                settingsWindow.Show();
        }

        internal void UpdateAccess(bool canEditWorld)
        {
            this.canEditWorld = canEditWorld;
            if (settingsWindow != null)
                settingsWindow.Refresh(canEditWorld);
        }

        internal void Close()
        {
            live = false;
            if (settingsWindow != null)
                settingsWindow.Dispose();

            settingsWindow = null;
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

            if (settingsWindow != null)
                settingsWindow.Refresh(canEditWorld);
        }

        private void AddSettingsLauncher(ControlPage page)
        {
            var openButton = new TerminalButton
            {
                Name = "Open Working Knowledge Settings",
                ToolTip = "Open the compact vertically scrolling Working Knowledge settings window.",
            };
            openButton.ControlChangedHandler = delegate
            {
                if (live && settingsWindow != null)
                    settingsWindow.Show();
            };

            var tile = new ControlTile { ControlContainer = { openButton } };
            var category = new ControlCategory
            {
                HeaderText = "Working Knowledge Settings",
                SubheaderText = "Opens the compact player and administrator settings window.",
            };
            category.Add(tile);
            page.Add(category);
        }

        private void ApplyWindowValue(string setting, string value, string command, bool isWorld)
        {
            SetPendingValue(setting, value, command);
            setValue(setting, value, isWorld);
        }

        private void ApplyWindowDifficulty(string value, string command)
        {
            SetPendingValue("difficultyPreset", value, command);
            setDifficulty(value);
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
    }
}
