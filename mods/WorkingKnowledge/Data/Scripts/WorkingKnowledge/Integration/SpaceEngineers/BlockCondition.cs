using System;
using Sandbox.Definitions;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal static class BlockCondition
    {
        internal static double GetBuildRatio(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.ComponentStack == null)
                return 1.0;

            var maxIntegrity = slimBlock.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= 0f)
                return 1.0;

            return RatioMath.Clamp01(slimBlock.ComponentStack.BuildIntegrity / maxIntegrity);
        }

        internal static double GetIntegrityRatio(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.ComponentStack == null)
                return 1.0;

            var maxIntegrity = slimBlock.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= 0f)
                return 1.0;

            return RatioMath.Clamp01(slimBlock.ComponentStack.Integrity / maxIntegrity);
        }

        internal static double GetProficiencyBuildConditionCap(IMySlimBlock slimBlock, double proficiency, double buildCapExponent)
        {
            var functionalThreshold = GetFunctionalBuildRatio(slimBlock);
            var cappedProficiency = Math.Pow(RatioMath.Clamp01(proficiency), buildCapExponent);
            return RatioMath.Clamp01(functionalThreshold + ((1.0 - functionalThreshold) * cappedProficiency));
        }

        internal static double GetFunctionalBuildRatio(IMySlimBlock slimBlock)
        {
            var definition = slimBlock != null ? slimBlock.BlockDefinition as MyCubeBlockDefinition : null;
            if (definition == null)
                return 0.0;

            if (definition.CriticalIntegrityRatio > 0f)
                return RatioMath.Clamp01(definition.CriticalIntegrityRatio);

            if (definition.Components != null && definition.Components.Length > 0)
            {
                var criticalGroup = Math.Min(definition.Components.Length - 1, Math.Max(0, (int)definition.CriticalGroup));
                var functionalIntegrity = 0.0;
                var totalIntegrity = 0.0;

                for (var i = 0; i < definition.Components.Length; i++)
                {
                    var component = definition.Components[i];
                    if (component.Definition == null)
                        continue;

                    var componentIntegrity = Math.Max(0.0, component.Definition.MaxIntegrity) * Math.Max(0, component.Count);
                    totalIntegrity += componentIntegrity;
                    if (i <= criticalGroup)
                        functionalIntegrity += componentIntegrity;
                }

                if (totalIntegrity > 0.0 && functionalIntegrity > 0.0)
                    return RatioMath.Clamp01(functionalIntegrity / totalIntegrity);
            }

            return RatioMath.Clamp01(definition.CriticalIntegrityRatio);
        }

    }
}
