using System;
using System.Collections.Generic;
using System.Globalization;

namespace WkKn
{
    internal sealed class WkPlayerConfigStore
    {
        private static readonly WkPlayerConfigSettingDefinition[] settings = CreateSettings();
        private WkPlayerConfigSaveData data = new WkPlayerConfigSaveData();

        internal WkPlayerConfigSaveData Data
        {
            get { return data; }
        }

        internal bool IsDirty { get; private set; }

        internal void Reset()
        {
            data = new WkPlayerConfigSaveData();
            IsDirty = false;
        }

        internal void SetData(WkPlayerConfigSaveData loaded)
        {
            data = loaded ?? new WkPlayerConfigSaveData();
            Normalize();
            IsDirty = false;
        }

        internal void MarkClean()
        {
            IsDirty = false;
        }

        internal void Normalize()
        {
            if (data == null)
                data = new WkPlayerConfigSaveData();

            if (data.Players == null)
                data.Players = new List<WkPlayerConfigRecord>();

            for (var i = data.Players.Count - 1; i >= 0; i--)
            {
                var player = data.Players[i];
                if (player == null || string.IsNullOrWhiteSpace(player.Id))
                {
                    data.Players.RemoveAt(i);
                    continue;
                }

                NormalizeRecord(player);
            }
        }

        internal WkPlayerConfigRecord GetOrCreatePlayer(string playerId)
        {
            Normalize();
            var player = FindPlayer(playerId);
            if (player != null)
                return player;

            player = CreateDefault(playerId);
            data.Players.Add(player);
            IsDirty = true;
            return player;
        }

        internal WkPlayerConfigRecord GetPlayerOrDefault(string playerId)
        {
            Normalize();
            var player = FindPlayer(playerId);
            return player ?? CreateDefault(playerId);
        }

        internal bool ResetPlayer(string playerId)
        {
            Normalize();
            for (var i = 0; i < data.Players.Count; i++)
            {
                if (string.Equals(data.Players[i].Id, playerId, StringComparison.OrdinalIgnoreCase))
                {
                    data.Players.RemoveAt(i);
                    IsDirty = true;
                    return true;
                }
            }

            return false;
        }

        internal bool IsSetting(string setting)
        {
            return FindSetting(setting) != null;
        }

        internal bool TrySetValue(string playerId, string setting, string value, out string error)
        {
            var definition = FindSetting(setting);
            if (definition == null)
            {
                error = "Unknown player config setting: " + setting;
                return false;
            }

            var player = GetOrCreatePlayer(playerId);
            if (!definition.TrySetValue(player, value, out error))
                return false;

            NormalizeRecord(player);
            IsDirty = true;
            error = null;
            return true;
        }

        internal bool TryGetSettingHelp(string setting, string playerId, out List<string> lines)
        {
            lines = null;
            var definition = FindSetting(setting);
            if (definition == null)
                return false;

            var player = GetPlayerOrDefault(playerId);
            lines = new List<string>();
            lines.Add(definition.Title);
            lines.Add(definition.GetLine(player));
            lines.Add("Value: " + definition.ValueHint);
            lines.Add(definition.Description);
            if (definition.Aliases != null && definition.Aliases.Length > 0)
                lines.Add("Aliases: " + string.Join(", ", definition.Aliases));

            return true;
        }

        internal IEnumerable<string> GetDisplayLines(string playerId)
        {
            var player = GetPlayerOrDefault(playerId);
            for (var i = 0; i < settings.Length; i++)
                yield return settings[i].GetLine(player);
        }

        internal string GetLine(string playerId, string setting)
        {
            var definition = FindSetting(setting);
            return definition == null ? setting + " = unknown" : definition.GetLine(GetPlayerOrDefault(playerId));
        }

        internal IEnumerable<string> GetSettingLines()
        {
            for (var i = 0; i < settings.Length; i++)
                yield return settings[i].Setting;
        }

        private WkPlayerConfigRecord FindPlayer(string playerId)
        {
            if (data == null || data.Players == null || string.IsNullOrWhiteSpace(playerId))
                return null;

            for (var i = 0; i < data.Players.Count; i++)
            {
                var player = data.Players[i];
                if (player != null && string.Equals(player.Id, playerId, StringComparison.OrdinalIgnoreCase))
                    return player;
            }

            return null;
        }

        private static WkPlayerConfigRecord CreateDefault(string playerId)
        {
            return new WkPlayerConfigRecord
            {
                Id = playerId ?? "",
                ProgressChatEnabled = false,
                ProgressToastEnabled = false,
                ProgressHudEnabled = true,
                ProgressHudRows = 5,
                ProgressHudOrder = "descending",
                ProgressHudPosition = "topRight",
                ProgressHudOffsetX = 0.0,
                ProgressHudOffsetY = 0.0,
                ProgressHudFadeSeconds = 6.0,
                ResearchChatSuppressionPercent = 0.0,
                ProficiencyChatSuppressionPercent = 0.0,
                ResearchToastSuppressionPercent = 0.0,
                ProficiencyToastSuppressionPercent = 0.0,
                CompletionSoundEnabled = true,
                WeldBotchSoundEnabled = true,
                WeldBotchWarningCooldownSeconds = -1.0,
            };
        }

        private static void NormalizeRecord(WkPlayerConfigRecord player)
        {
            if (player == null)
                return;

            player.Id = player.Id == null ? "" : player.Id.Trim();
            player.ProgressHudRows = player.ProgressHudRows <= 0 ? 5 : (int)RatioMath.Clamp(player.ProgressHudRows, 1, 10);
            player.ProgressHudOrder = NormalizeChoice(player.ProgressHudOrder, "descending", "ascending", "descending");
            player.ProgressHudPosition = NormalizeChoice(player.ProgressHudPosition, "topRight", "topLeft", "topRight", "bottomLeft", "bottomRight", "center");
            player.ProgressHudOffsetX = RatioMath.Clamp(player.ProgressHudOffsetX, -2.0, 2.0);
            player.ProgressHudOffsetY = RatioMath.Clamp(player.ProgressHudOffsetY, -2.0, 2.0);
            player.ProgressHudFadeSeconds = player.ProgressHudFadeSeconds < 0.0 ? 6.0 : RatioMath.Clamp(player.ProgressHudFadeSeconds, 0.0, 60.0);
            player.ResearchChatSuppressionPercent = RatioMath.Clamp(player.ResearchChatSuppressionPercent, 0.0, 100.0);
            player.ProficiencyChatSuppressionPercent = RatioMath.Clamp(player.ProficiencyChatSuppressionPercent, 0.0, 100.0);
            player.ResearchToastSuppressionPercent = RatioMath.Clamp(player.ResearchToastSuppressionPercent, 0.0, 100.0);
            player.ProficiencyToastSuppressionPercent = RatioMath.Clamp(player.ProficiencyToastSuppressionPercent, 0.0, 100.0);
            player.WeldBotchWarningCooldownSeconds = player.WeldBotchWarningCooldownSeconds < 0.0 ? -1.0 : RatioMath.Clamp(player.WeldBotchWarningCooldownSeconds, 0.0, 30.0);
        }

        private static WkPlayerConfigSettingDefinition FindSetting(string setting)
        {
            var key = WkConfigStore.NormalizeKey(setting);
            for (var i = 0; i < settings.Length; i++)
            {
                if (settings[i].Matches(key))
                    return settings[i];
            }

            return null;
        }

        private static WkPlayerConfigSettingDefinition[] CreateSettings()
        {
            return new WkPlayerConfigSettingDefinition[]
            {
                Bool("progressChatEnabled", "Progress Chat", "true/false", "Shows your delayed research and Proficiency progress chat messages when world settings allow them.", delegate(WkPlayerConfigRecord c) { return c.ProgressChatEnabled; }, delegate(WkPlayerConfigRecord c, bool v) { c.ProgressChatEnabled = v; }, "chatenabled", "chatnotifications"),
                Bool("progressToastEnabled", "Progress Toast", "true/false", "Shows popup progress notifications and botch toasts when world settings allow them. The Text HUD progress bars are separate.", delegate(WkPlayerConfigRecord c) { return c.ProgressToastEnabled; }, delegate(WkPlayerConfigRecord c, bool v) { c.ProgressToastEnabled = v; }, "toastenabled", "toastnotifications"),
                Bool("progressHudEnabled", "Progress HUD Bars", "true/false", "Shows the Text HUD API progress bar overlay for recent research and Proficiency updates.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudEnabled; }, delegate(WkPlayerConfigRecord c, bool v) { c.ProgressHudEnabled = v; }, "barsenabled", "hudbars", "progressbars"),
                Integer("progressHudRows", "Progress HUD Rows", "1 to 10", "Maximum recent schematic rows shown in the progress bar overlay.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudRows; }, delegate(WkPlayerConfigRecord c, int v) { c.ProgressHudRows = v; }, 1, 10, "hudrows", "progressrows"),
                Choice("progressHudOrder", "Progress HUD Order", "ascending or descending", "Row ordering for the progress bar overlay. The default descending order keeps the newest row at the end of the stack.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudOrder; }, delegate(WkPlayerConfigRecord c, string v) { c.ProgressHudOrder = v; }, new[] { "ascending", "descending" }, "hudorder", "progressorder"),
                Choice("progressHudPosition", "Progress HUD Position", "topLeft, topRight, bottomLeft, bottomRight, or center", "Position preset for the progress bar overlay.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudPosition; }, delegate(WkPlayerConfigRecord c, string v) { c.ProgressHudPosition = v; }, new[] { "topLeft", "topRight", "bottomLeft", "bottomRight", "center" }, "hudposition", "progressposition", "hudpreset"),
                Number("progressHudOffsetX", "Progress HUD X Offset", "-2.0 to 2.0", "Horizontal offset added to the progress bar position preset. Positive values move right.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudOffsetX; }, delegate(WkPlayerConfigRecord c, double v) { c.ProgressHudOffsetX = v; }, -2.0, 2.0, "hudx", "hudoffsetx", "progressx"),
                Number("progressHudOffsetY", "Progress HUD Y Offset", "-2.0 to 2.0", "Vertical offset added to the progress bar position preset. Positive values move up.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudOffsetY; }, delegate(WkPlayerConfigRecord c, double v) { c.ProgressHudOffsetY = v; }, -2.0, 2.0, "hudy", "hudoffsety", "progressy"),
                Number("progressHudFadeSeconds", "Progress HUD Fade Seconds", "0.0 to 60.0 seconds", "Seconds a progress bar row stays active before fading. Use 0 to keep recent history visible until replaced.", delegate(WkPlayerConfigRecord c) { return c.ProgressHudFadeSeconds; }, delegate(WkPlayerConfigRecord c, double v) { c.ProgressHudFadeSeconds = v; }, 0.0, 60.0, "hudfade", "progressfade", "barfade"),
                Percent("researchChatSuppressionPercent", "Research Chat Threshold", "Minimum accumulated research percent before another research chat update is shown for you.", delegate(WkPlayerConfigRecord c) { return c.ResearchChatSuppressionPercent; }, delegate(WkPlayerConfigRecord c, double v) { c.ResearchChatSuppressionPercent = v; }, "reschatsuppression", "researchchatthreshold", "reschatthreshold"),
                Percent("proficiencyChatSuppressionPercent", "Proficiency Chat Threshold", "Minimum accumulated Proficiency percent before another Proficiency chat update is shown for you.", delegate(WkPlayerConfigRecord c) { return c.ProficiencyChatSuppressionPercent; }, delegate(WkPlayerConfigRecord c, double v) { c.ProficiencyChatSuppressionPercent = v; }, "profchatsuppression", "proficiencychatthreshold", "profchatthreshold"),
                Percent("researchToastSuppressionPercent", "Research Toast Threshold", "Minimum accumulated research percent before another research toast appears for you.", delegate(WkPlayerConfigRecord c) { return c.ResearchToastSuppressionPercent; }, delegate(WkPlayerConfigRecord c, double v) { c.ResearchToastSuppressionPercent = v; }, "restoastsuppression", "researchtoastthreshold", "restoastthreshold"),
                Percent("proficiencyToastSuppressionPercent", "Proficiency Toast Threshold", "Minimum accumulated Proficiency percent before another Proficiency toast appears for you.", delegate(WkPlayerConfigRecord c) { return c.ProficiencyToastSuppressionPercent; }, delegate(WkPlayerConfigRecord c, double v) { c.ProficiencyToastSuppressionPercent = v; }, "proftoastsuppression", "proficiencytoastthreshold", "proftoastthreshold"),
                Bool("completionSoundEnabled", "Completion Sound", "true/false", "Plays your research and Proficiency completion sounds when world settings allow them.", delegate(WkPlayerConfigRecord c) { return c.CompletionSoundEnabled; }, delegate(WkPlayerConfigRecord c, bool v) { c.CompletionSoundEnabled = v; }),
                Bool("weldBotchSoundEnabled", "Botch Sound", "true/false", "Plays positional botch sounds for you when world settings allow them.", delegate(WkPlayerConfigRecord c) { return c.WeldBotchSoundEnabled; }, delegate(WkPlayerConfigRecord c, bool v) { c.WeldBotchSoundEnabled = v; }),
                DefaultableNumber("weldBotchWarningCooldownSeconds", "Botch Warning Cooldown", "default or 0.0 to 30.0 seconds", "Minimum time before repeating the same botch warning for you. Use default to follow world settings.", delegate(WkPlayerConfigRecord c) { return c.WeldBotchWarningCooldownSeconds; }, delegate(WkPlayerConfigRecord c, double v) { c.WeldBotchWarningCooldownSeconds = v; }),
            };
        }

        private static WkPlayerConfigSettingDefinition Bool(string setting, string title, string valueHint, string description, Func<WkPlayerConfigRecord, bool> getter, Action<WkPlayerConfigRecord, bool> setter, params string[] aliases)
        {
            return new WkPlayerConfigSettingDefinition(
                setting,
                title,
                valueHint,
                description,
                delegate(WkPlayerConfigRecord config) { return getter(config).ToString(); },
                delegate(WkPlayerConfigRecord config, string value, out string error)
                {
                    bool parsed;
                    if (!TryParseBool(value, out parsed))
                    {
                        error = "Use true/false for " + setting + ".";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkPlayerConfigSettingDefinition Percent(string setting, string title, string description, Func<WkPlayerConfigRecord, double> getter, Action<WkPlayerConfigRecord, double> setter, params string[] aliases)
        {
            return new WkPlayerConfigSettingDefinition(
                setting,
                title,
                "0.0 to 100.0 percent, such as 1 or 10%",
                description,
                delegate(WkPlayerConfigRecord config) { return FormatNumber(getter(config)); },
                delegate(WkPlayerConfigRecord config, string value, out string error)
                {
                    double parsed;
                    if (!TryParsePercentNumber(value, out parsed))
                    {
                        error = "Use a percent value for " + setting + ", such as 0, 1, or 10%.";
                        return false;
                    }

                    setter(config, RatioMath.Clamp(parsed, 0.0, 100.0));
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkPlayerConfigSettingDefinition Integer(string setting, string title, string valueHint, string description, Func<WkPlayerConfigRecord, int> getter, Action<WkPlayerConfigRecord, int> setter, int min, int max, params string[] aliases)
        {
            return new WkPlayerConfigSettingDefinition(
                setting,
                title,
                valueHint,
                description,
                delegate(WkPlayerConfigRecord config) { return getter(config).ToString(CultureInfo.InvariantCulture); },
                delegate(WkPlayerConfigRecord config, string value, out string error)
                {
                    int parsed;
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    {
                        error = "Use a whole number for " + setting + ".";
                        return false;
                    }

                    setter(config, (int)RatioMath.Clamp(parsed, min, max));
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkPlayerConfigSettingDefinition Number(string setting, string title, string valueHint, string description, Func<WkPlayerConfigRecord, double> getter, Action<WkPlayerConfigRecord, double> setter, double min, double max, params string[] aliases)
        {
            return new WkPlayerConfigSettingDefinition(
                setting,
                title,
                valueHint,
                description,
                delegate(WkPlayerConfigRecord config) { return FormatNumber(getter(config)); },
                delegate(WkPlayerConfigRecord config, string value, out string error)
                {
                    double parsed;
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                    {
                        error = "Use a number for " + setting + ".";
                        return false;
                    }

                    setter(config, RatioMath.Clamp(parsed, min, max));
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkPlayerConfigSettingDefinition Choice(string setting, string title, string valueHint, string description, Func<WkPlayerConfigRecord, string> getter, Action<WkPlayerConfigRecord, string> setter, string[] choices, params string[] aliases)
        {
            return new WkPlayerConfigSettingDefinition(
                setting,
                title,
                valueHint,
                description,
                delegate(WkPlayerConfigRecord config) { return getter(config); },
                delegate(WkPlayerConfigRecord config, string value, out string error)
                {
                    string normalized;
                    if (!TryNormalizeChoice(value, choices, out normalized))
                    {
                        error = "Use one of these values for " + setting + ": " + string.Join(", ", choices) + ".";
                        return false;
                    }

                    setter(config, normalized);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkPlayerConfigSettingDefinition DefaultableNumber(string setting, string title, string valueHint, string description, Func<WkPlayerConfigRecord, double> getter, Action<WkPlayerConfigRecord, double> setter, params string[] aliases)
        {
            return new WkPlayerConfigSettingDefinition(
                setting,
                title,
                valueHint,
                description,
                delegate(WkPlayerConfigRecord config)
                {
                    var value = getter(config);
                    return value < 0.0 ? "default" : FormatNumber(value);
                },
                delegate(WkPlayerConfigRecord config, string value, out string error)
                {
                    if (IsDefaultValue(value))
                    {
                        setter(config, -1.0);
                        error = null;
                        return true;
                    }

                    double parsed;
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                    {
                        error = "Use a number or default for " + setting + ".";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static bool IsDefaultValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.Equals("default", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeChoice(string value, string fallback, params string[] choices)
        {
            string normalized;
            return TryNormalizeChoice(value, choices, out normalized) ? normalized : fallback;
        }

        private static bool TryNormalizeChoice(string value, string[] choices, out string normalized)
        {
            normalized = null;
            if (choices == null || choices.Length == 0 || string.IsNullOrWhiteSpace(value))
                return false;

            var key = WkConfigStore.NormalizeKey(value);
            for (var i = 0; i < choices.Length; i++)
            {
                if (key == WkConfigStore.NormalizeKey(choices[i]))
                {
                    normalized = choices[i];
                    return true;
                }
            }

            return false;
        }

        private static bool TryParsePercentNumber(string value, out double parsed)
        {
            parsed = 0.0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim();
            if (normalized.EndsWith("%", StringComparison.Ordinal))
                normalized = normalized.Substring(0, normalized.Length - 1);

            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
        }

        private static bool TryParseBool(string value, out bool parsed)
        {
            parsed = false;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                parsed = true;
                return true;
            }

            if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("0", StringComparison.OrdinalIgnoreCase))
            {
                parsed = false;
                return true;
            }

            return false;
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
