using System;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal sealed class WeldBuildCapPolicy
    {
        private readonly double requiredProgress;
        private readonly double buildCapExponent;
        private readonly double tolerance;

        internal WeldBuildCapPolicy(double requiredProgress, double buildCapExponent, double tolerance)
        {
            this.requiredProgress = requiredProgress;
            this.buildCapExponent = buildCapExponent;
            this.tolerance = tolerance;
        }

        internal double GetActiveCap(IMySlimBlock slimBlock, double proficiency, bool buildCapEnabled)
        {
            if (!buildCapEnabled)
                return requiredProgress;

            var theoreticalCap = BlockCondition.GetProficiencyBuildConditionCap(slimBlock, proficiency, buildCapExponent);
            return GetSnappedCap(slimBlock, theoreticalCap);
        }

        private double GetSnappedCap(IMySlimBlock slimBlock, double theoreticalCap)
        {
            var cap = RatioMath.Clamp01(theoreticalCap);
            if (cap >= requiredProgress - tolerance)
                return requiredProgress;

            var definition = slimBlock != null ? slimBlock.BlockDefinition as MyCubeBlockDefinition : null;
            if (definition == null || definition.Components == null || definition.Components.Length == 0)
                return cap;

            var totalIntegrity = 0.0;
            for (var i = 0; i < definition.Components.Length; i++)
            {
                var component = definition.Components[i];
                if (component.Definition == null)
                    continue;

                totalIntegrity += Math.Max(0.0, component.Definition.MaxIntegrity) * Math.Max(0, component.Count);
            }

            if (totalIntegrity <= 0.0)
                return cap;

            var functionalMinimum = BlockCondition.GetFunctionalBuildRatio(slimBlock);
            var bestRatio = 0.0;
            var functionalStepRatio = 0.0;
            var cumulativeIntegrity = 0.0;

            for (var i = 0; i < definition.Components.Length; i++)
            {
                var component = definition.Components[i];
                if (component.Definition == null)
                    continue;

                var componentIntegrity = Math.Max(0.0, component.Definition.MaxIntegrity);
                var componentCount = Math.Max(0, component.Count);
                if (componentIntegrity <= 0.0 || componentCount <= 0)
                    continue;

                for (var index = 0; index < componentCount; index++)
                {
                    cumulativeIntegrity += componentIntegrity;
                    var ratio = RatioMath.Clamp01(cumulativeIntegrity / totalIntegrity);
                    if (functionalStepRatio <= 0.0 && ratio >= functionalMinimum - tolerance)
                        functionalStepRatio = ratio;

                    if (ratio <= cap + tolerance && ratio >= functionalMinimum - tolerance)
                        bestRatio = ratio;
                }
            }

            if (bestRatio > 0.0)
                return RatioMath.Clamp01(bestRatio);

            if (functionalStepRatio > 0.0)
                return RatioMath.Clamp01(functionalStepRatio);

            return cap;
        }
    }
}
