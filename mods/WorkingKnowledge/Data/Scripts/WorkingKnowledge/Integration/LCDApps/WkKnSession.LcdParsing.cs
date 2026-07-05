using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using VRage.Utils;
using VRageMath;


namespace WkKn
{
    public partial class WkKnSession
    {
        private static List<LcdCustomDataSection> ParseLcdConfigSections(string customData)
        {
            var sections = new List<LcdCustomDataSection>();
            if (string.IsNullOrWhiteSpace(customData))
                return sections;

            LcdCustomDataSection current = null;
            var lines = customData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal))
                    continue;

                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    var sectionName = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    if (IsRootLcdSection(sectionName) || IsDisplayLcdSection(sectionName))
                    {
                        current = new LcdCustomDataSection { Name = sectionName };
                        sections.Add(current);
                    }
                    else
                    {
                        current = null;
                    }

                    continue;
                }

                if (current == null)
                    continue;

                var equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex <= 0)
                    continue;

                var key = trimmed.Substring(0, equalsIndex).Trim();
                var value = trimmed.Substring(equalsIndex + 1).Trim();
                current.Values[key] = value;
            }

            return sections;
        }

        private static bool IsRootLcdSection(string sectionName)
        {
            return string.Equals(sectionName, LcdRootSectionName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDisplayLcdSection(string sectionName)
        {
            return !string.IsNullOrWhiteSpace(sectionName) &&
                   sectionName.StartsWith(LcdRootSectionName + ".", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSurfaceDisplayName(Sandbox.ModAPI.Ingame.IMyTextSurface surface)
        {
            return surface == null || surface.DisplayName == null ? string.Empty : surface.DisplayName.Trim();
        }

        private static string GetSurfaceName(Sandbox.ModAPI.Ingame.IMyTextSurface surface)
        {
            return surface == null || surface.Name == null ? string.Empty : surface.Name.Trim();
        }

        private static bool DisplayNameMatches(string configured, string actual)
        {
            return !string.IsNullOrWhiteSpace(configured) &&
                   !string.IsNullOrWhiteSpace(actual) &&
                   string.Equals(configured.Trim(), actual.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Unquote(string value)
        {
            if (value == null)
                return string.Empty;

            value = value.Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[value.Length - 1] == '"') || (value[0] == '\'' && value[value.Length - 1] == '\'')))
                return value.Substring(1, value.Length - 2).Trim();

            return value;
        }

        private static bool TryParseDisplayInset(string value, Vector2 surfaceSize, out float left, out float top, out float right, out float bottom)
        {
            left = top = right = bottom = 0f;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var parts = Unquote(value).Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                return false;

            return TryParseDisplayInsetPart(parts[0], surfaceSize.X, out left) &&
                   TryParseDisplayInsetPart(parts[1], surfaceSize.Y, out top) &&
                   TryParseDisplayInsetPart(parts[2], surfaceSize.X, out right) &&
                   TryParseDisplayInsetPart(parts[3], surfaceSize.Y, out bottom);
        }

        private static bool TryParseDisplayInsetPart(string value, float axisSize, out float pixels)
        {
            pixels = 0f;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = Unquote(value);
            var isPercent = value.EndsWith("%", StringComparison.Ordinal);
            if (isPercent)
                value = value.Substring(0, value.Length - 1);
            else if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(0, value.Length - 2);

            double numericValue;
            if (!double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out numericValue))
                return false;

            pixels = (float)(isPercent ? axisSize * numericValue / 100.0 : numericValue);
            return true;
        }

        private static bool TryParseDisplayColor(string value, out Color color)
        {
            color = Color.Black;
            value = Unquote(value);
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (TryParseNamedDisplayColor(value, out color))
                return true;

            if (value[0] == '#')
                return TryParseHexDisplayColor(value.Substring(1), out color);

            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return TryParseHexDisplayColor(value.Substring(2), out color);

            var parts = value.Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return false;

            byte red;
            byte green;
            byte blue;
            if (!TryParseColorChannel(parts[0], out red) ||
                !TryParseColorChannel(parts[1], out green) ||
                !TryParseColorChannel(parts[2], out blue))
                return false;

            color = new Color(red, green, blue);
            return true;
        }

        private static bool TryParseNamedDisplayColor(string value, out Color color)
        {
            color = Color.Black;

            if (value.Equals("Black", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.Black;
                return true;
            }

            if (value.Equals("White", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.White;
                return true;
            }

            if (value.Equals("Red", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.Red;
                return true;
            }

            if (value.Equals("Green", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.Green;
                return true;
            }

            if (value.Equals("Blue", StringComparison.OrdinalIgnoreCase))
            {
                color = Color.Blue;
                return true;
            }

            return false;
        }

        private static bool TryParseHexDisplayColor(string value, out Color color)
        {
            color = Color.Black;
            if (string.IsNullOrWhiteSpace(value) || value.Length != 6)
                return false;

            byte red;
            byte green;
            byte blue;
            if (!TryParseHexByte(value.Substring(0, 2), out red) ||
                !TryParseHexByte(value.Substring(2, 2), out green) ||
                !TryParseHexByte(value.Substring(4, 2), out blue))
                return false;

            color = new Color(red, green, blue);
            return true;
        }

        private static bool TryParseHexByte(string value, out byte result)
        {
            result = 0;
            int parsed;
            if (!int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsed))
                return false;

            result = (byte)Math.Max(0, Math.Min(255, parsed));
            return true;
        }

        private static bool TryParseColorChannel(string value, out byte channel)
        {
            channel = 0;
            value = Unquote(value);
            double parsed;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return false;

            if (parsed >= 0.0 && parsed <= 1.0 && value.IndexOf('.') >= 0)
                parsed *= 255.0;

            channel = (byte)Math.Max(0, Math.Min(255, (int)Math.Round(parsed)));
            return true;
        }

    }
}
