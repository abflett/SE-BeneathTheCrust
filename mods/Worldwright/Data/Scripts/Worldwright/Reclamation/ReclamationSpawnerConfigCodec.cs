using System;
using System.Globalization;
using System.Text;
using VRageMath;

namespace Worldwright
{
    internal static class ReclamationSpawnerConfigCodec
    {
        private const string SectionName = "Worldwright.BlockSpawner";
        private const string SectionHeader = "[" + SectionName + "]";

        internal static ReclamationSpawnerConfig Read(string customData)
        {
            var config = new ReclamationSpawnerConfig();
            if (string.IsNullOrWhiteSpace(customData))
                return config;

            var inSection = false;
            var lines = customData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    var name = line.Substring(1, line.Length - 2).Trim();
                    inSection = name.Equals(SectionName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inSection || line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal) || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                var separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                var key = line.Substring(0, separator).Trim();
                var value = line.Substring(separator + 1).Trim();
                if (key.Equals("entry", StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Length > 0)
                        config.Entries.Add(value);
                }
                else if (key.Equals("mode", StringComparison.OrdinalIgnoreCase))
                {
                    ReclamationSequenceMode mode;
                    if (Enum.TryParse(value, true, out mode))
                        config.Mode = mode;
                }
                else if (key.Equals("velocity", StringComparison.OrdinalIgnoreCase))
                {
                    float velocity;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out velocity))
                        config.OutwardVelocity = velocity;
                }
                else if (key.Equals("interval", StringComparison.OrdinalIgnoreCase))
                {
                    float interval;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out interval))
                        config.AutomaticIntervalSeconds = interval;
                }
                else if (key.Equals("rotationVariance", StringComparison.OrdinalIgnoreCase))
                {
                    float variance;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out variance))
                        config.RotationVariance = variance;
                }
                else if (key.Equals("minimumIntegrity", StringComparison.OrdinalIgnoreCase))
                {
                    float minimumIntegrity;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out minimumIntegrity))
                        config.MinimumIntegrity = minimumIntegrity;
                }
                else if (key.Equals("maximumIntegrity", StringComparison.OrdinalIgnoreCase))
                {
                    float maximumIntegrity;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out maximumIntegrity))
                        config.MaximumIntegrity = maximumIntegrity;
                }
                else if (key.Equals("appearance", StringComparison.OrdinalIgnoreCase))
                {
                    ReclamationAppearancePreset preset;
                    if (TryReadAppearance(value, out preset))
                        config.AppearancePresets.Add(preset);
                }
                else if (key.Equals("cursor", StringComparison.OrdinalIgnoreCase))
                {
                    int cursor;
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out cursor))
                        config.Cursor = cursor;
                }
                else if (key.Equals("completed", StringComparison.OrdinalIgnoreCase))
                {
                    bool completed;
                    if (bool.TryParse(value, out completed))
                        config.Completed = completed;
                }
            }

            config.Normalize();
            return config;
        }

        internal static string Write(string customData, ReclamationSpawnerConfig config)
        {
            config = config ?? new ReclamationSpawnerConfig();
            config.Normalize();

            var source = customData ?? string.Empty;
            var lines = source.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var output = new StringBuilder();
            var skippingSection = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    var name = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    if (name.Equals(SectionName, StringComparison.OrdinalIgnoreCase))
                    {
                        skippingSection = true;
                        continue;
                    }

                    skippingSection = false;
                }

                if (!skippingSection)
                    AppendLine(output, lines[i]);
            }

            while (output.Length > 0 && (output[output.Length - 1] == '\r' || output[output.Length - 1] == '\n'))
                output.Length--;

            if (output.Length > 0)
                output.AppendLine().AppendLine();

            output.AppendLine(SectionHeader);
            output.Append("mode=").AppendLine(config.Mode.ToString());
            output.Append("velocity=").AppendLine(config.OutwardVelocity.ToString("0.###", CultureInfo.InvariantCulture));
            output.Append("interval=").AppendLine(config.AutomaticIntervalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
            output.Append("rotationVariance=").AppendLine(config.RotationVariance.ToString("0.###", CultureInfo.InvariantCulture));
            output.Append("minimumIntegrity=").AppendLine(config.MinimumIntegrity.ToString("0.###", CultureInfo.InvariantCulture));
            output.Append("maximumIntegrity=").AppendLine(config.MaximumIntegrity.ToString("0.###", CultureInfo.InvariantCulture));
            output.Append("cursor=").AppendLine(config.Cursor.ToString(CultureInfo.InvariantCulture));
            output.Append("completed=").AppendLine(config.Completed ? "true" : "false");
            for (var i = 0; i < config.Entries.Count; i++)
                output.Append("entry=").AppendLine(config.Entries[i]);
            for (var i = 0; i < config.AppearancePresets.Count; i++)
                output.Append("appearance=").AppendLine(WriteAppearance(config.AppearancePresets[i]));

            return output.ToString().TrimEnd('\r', '\n');
        }

        private static void AppendLine(StringBuilder output, string line)
        {
            if (output.Length > 0)
                output.AppendLine();

            output.Append(line);
        }

        private static bool TryReadAppearance(string value, out ReclamationAppearancePreset preset)
        {
            preset = null;
            var separator = value.IndexOf('|');
            var colorText = separator >= 0 ? value.Substring(0, separator) : value;
            var skin = separator >= 0 ? value.Substring(separator + 1) : string.Empty;
            var components = colorText.Split(',');
            if (components.Length != 3)
                return false;

            float hue;
            float saturation;
            float valueOffset;
            if (!float.TryParse(components[0], NumberStyles.Float, CultureInfo.InvariantCulture, out hue) ||
                !float.TryParse(components[1], NumberStyles.Float, CultureInfo.InvariantCulture, out saturation) ||
                !float.TryParse(components[2], NumberStyles.Float, CultureInfo.InvariantCulture, out valueOffset))
                return false;

            preset = new ReclamationAppearancePreset
            {
                ColorMaskHsv = new Vector3(hue, saturation, valueOffset),
                SkinSubtypeId = skin,
            };
            return true;
        }

        private static string WriteAppearance(ReclamationAppearancePreset preset)
        {
            preset = preset ?? new ReclamationAppearancePreset();
            return preset.ColorMaskHsv.X.ToString("R", CultureInfo.InvariantCulture) + "," +
                   preset.ColorMaskHsv.Y.ToString("R", CultureInfo.InvariantCulture) + "," +
                   preset.ColorMaskHsv.Z.ToString("R", CultureInfo.InvariantCulture) + "|" +
                   (preset.SkinSubtypeId ?? string.Empty);
        }
    }
}
