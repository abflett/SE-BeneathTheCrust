using System;
using System.Globalization;

namespace WkKn
{
    internal static class WkSettingNumericInput
    {
        internal static bool TryParse(string text, double displayMultiplier, string suffix,
            double minimum, double maximum, bool integer, out string canonical, out string error)
        {
            canonical = null;
            error = null;
            var token = (text ?? string.Empty).Trim();
            var unit = (suffix ?? string.Empty).Trim();
            if (unit.Length > 0 && token.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
                token = token.Substring(0, token.Length - unit.Length).Trim();

            double displayed;
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out displayed) ||
                double.IsNaN(displayed) || double.IsInfinity(displayed))
            {
                error = "Enter a finite number in the displayed units.";
                return false;
            }

            var value = displayed / displayMultiplier;
            if (value < minimum || value > maximum || (integer && value != Math.Truncate(value)))
            {
                error = "Enter " + (integer ? "a whole number" : "a value") + " from " +
                    (minimum * displayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + " to " +
                    (maximum * displayMultiplier).ToString("0.######", CultureInfo.InvariantCulture) + suffix + ".";
                return false;
            }

            canonical = value.ToString("0.######", CultureInfo.InvariantCulture);
            return true;
        }
    }
}
