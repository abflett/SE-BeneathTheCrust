using System;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal struct WeldBotchChanceTuning
    {
        internal readonly bool BuildCapEnabled;
        internal readonly double BaseChance;
        internal readonly double ChanceScale;
        internal readonly double MaxChance;
        internal readonly double PostFunctionalPressure;
        internal readonly double SoftCapPressure;
        internal readonly double PressureScale;

        internal WeldBotchChanceTuning(
            bool buildCapEnabled,
            double baseChance,
            double chanceScale,
            double maxChance,
            double postFunctionalPressure,
            double softCapPressure,
            double pressureScale)
        {
            BuildCapEnabled = buildCapEnabled;
            BaseChance = baseChance;
            ChanceScale = chanceScale;
            MaxChance = maxChance;
            PostFunctionalPressure = postFunctionalPressure;
            SoftCapPressure = softCapPressure;
            PressureScale = pressureScale;
        }
    }

    internal sealed class WeldBotchChancePolicy
    {
        private readonly double requiredProgress;
        private readonly double tolerance;

        internal WeldBotchChancePolicy(double requiredProgress, double tolerance)
        {
            this.requiredProgress = requiredProgress;
            this.tolerance = tolerance;
        }

        internal double GetChance(IMySlimBlock block, double proficiency, WeldBotchChanceTuning tuning)
        {
            if (block == null || block.ComponentStack == null)
                return 0.0;

            return GetChance(
                BlockCondition.GetIntegrityRatio(block),
                BlockCondition.GetFunctionalBuildRatio(block),
                proficiency,
                tuning);
        }

        internal double GetChance(
            double currentIntegrityRatio,
            double functionalRatio,
            double proficiency,
            WeldBotchChanceTuning tuning)
        {
            proficiency = RatioMath.Clamp01(proficiency);
            if (proficiency >= requiredProgress - tolerance)
                return 0.0;

            if (IsAtCap(currentIntegrityRatio, functionalRatio, proficiency, tuning.BuildCapEnabled))
                return requiredProgress;

            var functionalChance = RatioMath.Clamp01(
                (1.0 - proficiency) * GetBaseChanceScale(tuning));
            var functional = RatioMath.Clamp01(functionalRatio);
            if (functional >= requiredProgress - tolerance ||
                currentIntegrityRatio < functional - tolerance)
            {
                return ClampChance(
                    GetPreFunctionalChance(currentIntegrityRatio, functional, functionalChance),
                    tuning.MaxChance);
            }

            return GetBotchZoneChance(
                currentIntegrityRatio,
                functional,
                proficiency,
                functionalChance,
                tuning);
        }

        private bool IsAtCap(double currentIntegrityRatio, double functionalRatio, double proficiency, bool buildCapEnabled)
        {
            if (!buildCapEnabled)
                return false;

            var cap = GetCapIntegrityRatio(functionalRatio, proficiency);
            return RatioMath.Clamp01(currentIntegrityRatio) >= cap - tolerance;
        }

        internal double GetCapIntegrityRatio(double functionalRatio, double proficiency)
        {
            var functional = RatioMath.Clamp01(functionalRatio);
            var proficiencyCap = RatioMath.Clamp01(proficiency);
            if (functional >= requiredProgress - tolerance)
                return proficiencyCap;

            return functional + ((requiredProgress - functional) * proficiencyCap);
        }

        private static double GetBaseChanceScale(WeldBotchChanceTuning tuning)
        {
            return (Math.Max(0.0, tuning.BaseChance) / 0.5) * Math.Max(0.0, tuning.ChanceScale);
        }

        private double GetPreFunctionalChance(double currentIntegrityRatio, double functionalRatio, double functionalChance)
        {
            var current = RatioMath.Clamp01(currentIntegrityRatio);
            var functional = RatioMath.Clamp01(functionalRatio);
            if (functional <= tolerance)
                return current > tolerance ? functionalChance : 0.0;

            var preFunctionalProgress = RatioMath.Clamp01(current / functional);
            if (preFunctionalProgress <= 0.5 + tolerance)
            {
                var earlyRamp = RatioMath.Clamp01(preFunctionalProgress / 0.5);
                return functionalChance * 0.02 * earlyRamp * earlyRamp;
            }

            var lateRamp = RatioMath.Clamp01((preFunctionalProgress - 0.5) / 0.5);
            return functionalChance * (0.02 + (0.98 * lateRamp));
        }

        private double GetBotchZoneChance(
            double currentIntegrityRatio,
            double functionalRatio,
            double proficiency,
            double functionalChance,
            WeldBotchChanceTuning tuning)
        {
            if (functionalRatio >= requiredProgress - tolerance)
                return ClampChance(functionalChance, tuning.MaxChance);

            var botchZoneProgress = RatioMath.Clamp01(
                (RatioMath.Clamp01(currentIntegrityRatio) - functionalRatio) /
                (requiredProgress - functionalRatio));
            var botchZoneCap = tuning.BuildCapEnabled
                ? RatioMath.Clamp01(proficiency)
                : requiredProgress;
            if (botchZoneCap <= tolerance || botchZoneProgress >= botchZoneCap - tolerance)
                return requiredProgress;

            var capProgress = RatioMath.Clamp01(botchZoneProgress / botchZoneCap);
            var climb = Math.Pow(capProgress, GetBotchZoneCurveExponent(tuning));
            var chance = functionalChance + ((requiredProgress - functionalChance) * climb);
            return ClampChance(chance, tuning.MaxChance);
        }

        private static double GetBotchZoneCurveExponent(WeldBotchChanceTuning tuning)
        {
            var pressure = Math.Max(
                0.0,
                (tuning.PostFunctionalPressure + tuning.SoftCapPressure) *
                0.5 *
                Math.Max(0.0, tuning.PressureScale));
            return 1.0 / (1.0 + (pressure * 0.4));
        }

        private double ClampChance(double chance, double maxChance)
        {
            return RatioMath.Clamp(
                chance,
                0.0,
                RatioMath.Clamp(maxChance, 0.0, requiredProgress));
        }
    }
}
