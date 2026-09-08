using Sandbox.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private WkTextHudSettingsLauncher textHudSettingsLauncher;

        private void OnRichHudReady()
        {
            richHudSettingsReady = true;
        }

        private void OnRichHudReset()
        {
            richHudSettingsReady = false;
            richHudOpenRequested = false;
            richHudPendingSettingsCommands.Clear();
            richHudSettingsStateRequested = false;
            richHudWorldConfigSnapshot = null;
            if (richHudSettingsMenu != null)
                richHudSettingsMenu.Close();
            richHudSettingsMenu = null;
        }

        private void UpdateRichHudSettings()
        {
            if (MyAPIGateway.Utilities != null && !MyAPIGateway.Utilities.IsDedicated)
            {
                if (textHudSettingsLauncher == null)
                    textHudSettingsLauncher = new WkTextHudSettingsLauncher(OpenRichHudSettings);
                textHudSettingsLauncher.Update();
            }
            FlushRichHudSettingsCommands();
            if (!richHudSettingsReady)
                return;

            if (richHudSettingsMenu == null)
            {
                if (MyAPIGateway.Multiplayer != null &&
                    !MyAPIGateway.Multiplayer.IsServer &&
                    richHudWorldConfigSnapshot == null)
                {
                    if (!richHudSettingsStateRequested)
                        RefreshRichHudSettingsState();
                    return;
                }

                richHudSettingsMenu = new WkRichHudSettingsMenu(
                    playerConfigStore,
                    configStore,
                    GetRichHudPlayerConfig,
                    GetRichHudWorldConfig,
                    SetRichHudConfigValue,
                    SetRichHudDifficulty,
                    ResetRichHudConfig);
                richHudSettingsMenu.Build();
                RefreshRichHudSettingsState();
            }

            richHudSettingsMenu.UpdateAccess(GetRichHudCanEditWorldConfig());
            if (richHudOpenRequested)
            {
                richHudOpenRequested = false;
                richHudSettingsMenu.Open();
            }
        }

        private void OpenRichHudSettings()
        {
            if (!richHudIntegration.IsReady)
            {
                ShowWkWarningMessage("Rich HUD Master is not ready. Ensure it is enabled, then try /wk settings again.");
                return;
            }

            richHudOpenRequested = true;
            RefreshRichHudSettingsState();
            if (richHudSettingsMenu != null)
            {
                richHudOpenRequested = false;
                richHudSettingsMenu.Open();
            }
        }

        private void RefreshRichHudSettingsState()
        {
            if (MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.IsServer)
            {
                richHudSettingsStateRequested = true;
                if (!SendSettingsRequestToServer())
                {
                    richHudSettingsStateRequested = false;
                    ShowWkWarningMessage("Working Knowledge could not request settings from the server.");
                }
                return;
            }

            richHudWorldConfigSnapshot = config;
            richHudCanEditWorldConfig = GetRichHudCanEditWorldConfig();
        }

        private WkPlayerConfigRecord GetRichHudPlayerConfig()
        {
            var identityId = MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null
                ? MyAPIGateway.Session.Player.IdentityId
                : 0;
            return GetPlayerConfig(identityId);
        }

        private WkConfig GetRichHudWorldConfig()
        {
            if (MyAPIGateway.Multiplayer == null || MyAPIGateway.Multiplayer.IsServer)
                return config;

            return richHudWorldConfigSnapshot;
        }

        private bool GetRichHudCanEditWorldConfig()
        {
            if (MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.IsServer)
                return richHudCanEditWorldConfig;

            var steamId = MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null
                ? MyAPIGateway.Session.Player.SteamUserId
                : 0;
            return CanEditConfig(steamId);
        }

        private void SetRichHudConfigValue(string setting, string value, bool isWorld)
        {
            richHudPendingSettingsCommands[setting] = "/wk config " + setting + " " + value;
            richHudPendingSettingsDueTick = simulationTick + 15;
        }

        private void SetRichHudDifficulty(string preset)
        {
            richHudPendingSettingsCommands.Clear();
            ExecuteRichHudSettingsCommand("/wk difficulty " + preset);
        }

        private void ResetRichHudConfig(bool world)
        {
            richHudPendingSettingsCommands.Clear();
            ExecuteRichHudSettingsCommand(world ? "/wk config world reset" : "/wk config reset");
        }

        private void FlushRichHudSettingsCommands()
        {
            if (richHudPendingSettingsCommands.Count == 0 || simulationTick < richHudPendingSettingsDueTick)
                return;

            var commands = new string[richHudPendingSettingsCommands.Count];
            richHudPendingSettingsCommands.Values.CopyTo(commands, 0);
            richHudPendingSettingsCommands.Clear();
            for (var i = 0; i < commands.Length; i++)
                ExecuteRichHudSettingsCommand(commands[i]);
        }

        private void ExecuteRichHudSettingsCommand(string command)
        {
            if (MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.IsServer)
            {
                if (!SendCommandRequestToServer(command, true))
                    ShowWkWarningMessage("Working Knowledge could not send that setting to the server.");
                return;
            }

            var player = MyAPIGateway.Session != null ? MyAPIGateway.Session.Player : null;
            if (player == null)
                return;

            var previousSuppressFeedback = suppressWkCommandFeedback;
            suppressWkCommandFeedback = true;
            try
            {
                ExecuteWorkingKnowledgeCommand(player.SteamUserId, command, player.IdentityId);
            }
            finally
            {
                suppressWkCommandFeedback = previousSuppressFeedback;
            }

            RefreshRichHudSettingsState();
            if (richHudSettingsMenu != null)
                richHudSettingsMenu.AcknowledgeCommand(command);
        }
    }
}
