using System;

namespace WkKn
{
    internal delegate bool WkConfigSettingSetter(WkConfig config, string value, out string error);

    internal sealed class WkConfigSettingDefinition
    {
        internal readonly string Setting;
        internal readonly string Title;
        internal readonly string Category;
        internal readonly string ValueHint;
        internal readonly string Description;
        internal readonly string[] Aliases;
        internal readonly bool Editable;
        private readonly Func<WkConfig, string> getter;
        private readonly WkConfigSettingSetter setter;

        internal WkConfigSettingDefinition(
            string setting,
            string title,
            string category,
            string valueHint,
            string description,
            bool editable,
            Func<WkConfig, string> getter,
            WkConfigSettingSetter setter,
            string[] aliases)
        {
            Setting = setting;
            Title = title;
            Category = category;
            ValueHint = valueHint;
            Description = description;
            Editable = editable;
            this.getter = getter;
            this.setter = setter;
            Aliases = aliases ?? new string[0];
        }

        internal string GetLine(WkConfig config)
        {
            return Setting + " = " + (getter == null ? string.Empty : getter(config));
        }

        internal bool TrySetValue(WkConfig config, string value, out string error)
        {
            if (!Editable || setter == null)
            {
                error = Setting + " is set by commands, not direct config edits.";
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
