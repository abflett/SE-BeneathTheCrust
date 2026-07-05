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
        private readonly Func<WkPlayerConfigRecord, string> getter;
        private readonly WkPlayerConfigSettingSetter setter;

        internal WkPlayerConfigSettingDefinition(
            string setting,
            string title,
            string valueHint,
            string description,
            Func<WkPlayerConfigRecord, string> getter,
            WkPlayerConfigSettingSetter setter,
            string[] aliases)
        {
            Setting = setting;
            Title = title;
            ValueHint = valueHint;
            Description = description;
            this.getter = getter;
            this.setter = setter;
            Aliases = aliases ?? new string[0];
        }

        internal string GetLine(WkPlayerConfigRecord config)
        {
            return Setting + " = " + (getter == null ? string.Empty : getter(config));
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
