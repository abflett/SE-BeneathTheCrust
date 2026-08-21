using System;

namespace WkKn
{
    internal enum WkSettingControlKind
    {
        ReadOnly,
        Boolean,
        Number,
        Integer,
        Choice,
        Text,
        DefaultableNumber,
    }

    internal enum WkSettingSliderScale
    {
        Linear,
        LogarithmicWithZero,
    }

    // UI-only numeric presentation. Values remain canonical at the command boundary and this
    // metadata is never serialized into world persistence.
    internal sealed class WkSettingPresentation
    {
        internal readonly double Minimum;
        internal readonly double Maximum;
        internal readonly double Step;
        internal readonly int DecimalPlaces;
        internal readonly double DisplayMultiplier;
        internal readonly string Suffix;
        internal readonly WkSettingSliderScale SliderScale;
        internal readonly double LogarithmicFloor;

        internal WkSettingPresentation(
            double minimum,
            double maximum,
            double step,
            int decimalPlaces,
            double displayMultiplier,
            string suffix,
            WkSettingSliderScale sliderScale = WkSettingSliderScale.Linear,
            double logarithmicFloor = 0.01)
        {
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
            DecimalPlaces = decimalPlaces;
            DisplayMultiplier = displayMultiplier;
            Suffix = suffix ?? string.Empty;
            SliderScale = sliderScale;
            LogarithmicFloor = logarithmicFloor;
        }

        internal static WkSettingPresentation ForSetting(
            string setting,
            WkSettingControlKind controlKind,
            double minimum,
            double maximum)
        {
            if (controlKind == WkSettingControlKind.Integer)
                return new WkSettingPresentation(minimum, maximum, 1.0, 0, 1.0, string.Empty);

            var key = WkConfigStore.NormalizeKey(setting);
            if (IsPercentRatio(key))
            {
                var step = key.IndexOf("segmentrate", StringComparison.Ordinal) >= 0 ? 0.001 : 0.01;
                var decimals = step < 0.01 ? 1 : 0;
                return new WkSettingPresentation(0.0, 1.0, step, decimals, 100.0, "%");
            }

            if (IsMultiplier(key))
            {
                return new WkSettingPresentation(
                    0.0,
                    10.0,
                    0.01,
                    2,
                    1.0,
                    "x",
                    WkSettingSliderScale.LogarithmicWithZero,
                    0.01);
            }

            if (key == "weldbotchpostfunctionalpressure" || key == "weldbotchsoftcappressure")
                return new WkSettingPresentation(0.0, 10.0, 0.1, 1, 1.0, string.Empty);

            if (key.IndexOf("seconds", StringComparison.Ordinal) >= 0 || key.IndexOf("cooldown", StringComparison.Ordinal) >= 0)
                return new WkSettingPresentation(minimum, maximum, 0.1, 1, 1.0, " s");

            if (key == "weldbotchsoundrange")
                return new WkSettingPresentation(minimum, maximum, 5.0, 0, 1.0, " m");

            if (key == "progresshudoffsetx" || key == "progresshudoffsety")
                return new WkSettingPresentation(minimum, maximum, 0.05, 2, 1.0, string.Empty);

            return new WkSettingPresentation(minimum, maximum, 0.01, 2, 1.0, string.Empty);
        }

        private static bool IsPercentRatio(string key)
        {
            return key.IndexOf("suppressionpercent", StringComparison.Ordinal) >= 0 ||
                   key == "salvagescrapyield" ||
                   key == "fundamentalsproficiencyprogress" ||
                   key == "proficiencyfirstthreshold" ||
                   key == "proficiencysecondthreshold" ||
                   key == "proficiencyfirstsegmentrate" ||
                   key == "proficiencysecondsegmentrate" ||
                   key == "proficiencyfinalsegmentrate" ||
                   key == "weldbotchbasechance" ||
                   key == "weldbotchmaxchance";
        }

        private static bool IsMultiplier(string key)
        {
            return key == "researchscale" ||
                   key == "researchgrindinggainscale" ||
                   key == "datafragmentrewardscale" ||
                   key == "datafragmentlootscale" ||
                   key == "researchefficiencystart" ||
                   key == "researchefficiencyend" ||
                   key == "salvagescale" ||
                   key == "proficiencygainscale" ||
                   key == "proficiencygrindinggainscale" ||
                   key == "proficiencyweldinggainscale" ||
                   key == "weldbotchchancescale" ||
                   key == "weldbotchpressurescale" ||
                   key == "weldbotchrawlossratio" ||
                   key == "weldbotchforgivenessscale";
        }
    }

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
        internal readonly WkSettingControlKind ControlKind;
        internal readonly double Minimum;
        internal readonly double Maximum;
        internal readonly string[] Choices;
        internal readonly WkSettingPresentation Presentation;
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
            string[] aliases,
            WkSettingControlKind controlKind,
            double minimum,
            double maximum,
            string[] choices)
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
            ControlKind = controlKind;
            Minimum = minimum;
            Maximum = maximum;
            Choices = choices ?? new string[0];
            Presentation = WkSettingPresentation.ForSetting(setting, controlKind, minimum, maximum);
        }

        internal string GetValue(WkConfig config)
        {
            return getter == null ? string.Empty : getter(config);
        }

        internal string GetLine(WkConfig config)
        {
            return Setting + " = " + GetValue(config);
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
