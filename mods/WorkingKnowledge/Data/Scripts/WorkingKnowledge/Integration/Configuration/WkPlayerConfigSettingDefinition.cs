using System;

namespace WkKn
{
    internal delegate bool WkPlayerConfigSettingSetter(WkPlayerConfigRecord config, string value, out string error);

    internal sealed class WkPlayerConfigSettingDefinition
    {
        internal readonly string Setting;
        internal readonly string Title;
        internal readonly string ValueHint;
        internal readonly string Description;
        internal readonly string[] Aliases;
        internal readonly WkSettingControlKind ControlKind;
        internal readonly double Minimum;
        internal readonly double Maximum;
        internal readonly string[] Choices;
        private readonly Func<WkPlayerConfigRecord, string> getter;
        private readonly WkPlayerConfigSettingSetter setter;

        internal WkPlayerConfigSettingDefinition(
            string setting,
            string title,
            string valueHint,
            string description,
            Func<WkPlayerConfigRecord, string> getter,
            WkPlayerConfigSettingSetter setter,
            string[] aliases,
            WkSettingControlKind controlKind,
            double minimum,
            double maximum,
            string[] choices)
        {
            Setting = setting;
            Title = title;
            ValueHint = valueHint;
            Description = description;
            this.getter = getter;
            this.setter = setter;
            Aliases = aliases ?? new string[0];
            ControlKind = controlKind;
            Minimum = minimum;
            Maximum = maximum;
            Choices = choices ?? new string[0];
        }

        internal string GetValue(WkPlayerConfigRecord config)
        {
            return getter == null ? string.Empty : getter(config);
        }

        internal string GetLine(WkPlayerConfigRecord config)
        {
            return Setting + " = " + GetValue(config);
        }

        internal bool TrySetValue(WkPlayerConfigRecord config, string value, out string error)
        {
            if (setter == null)
            {
                error = Setting + " cannot be changed.";
                return false;
            }

            return setter(config, value, out error);
        }

        internal bool Matches(string normalizedKey)
        {
            if (normalizedKey == WkConfigStore.NormalizeKey(Setting))
                return true;

            for (var i = 0; i < Aliases.Length; i++)
            {
                if (normalizedKey == WkConfigStore.NormalizeKey(Aliases[i]))
                    return true;
            }

            return false;
        }
    }
}
