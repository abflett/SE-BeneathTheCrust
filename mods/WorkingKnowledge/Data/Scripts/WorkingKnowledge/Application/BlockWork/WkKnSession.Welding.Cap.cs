using System.Globalization;
using VRage.Game.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private double GetActiveWeldBuildConditionCap(IMySlimBlock slimBlock, long identityId, string researchId)
        {
            return weldBuildCapPolicy.GetActiveCap(slimBlock, GetPlayerProficiency(identityId, researchId), config.ProficiencyBuildCapEnabled);
        }

        private static string GetWeldBlockKey(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.CubeGrid == null)
                return null;

            return slimBlock.CubeGrid.EntityId.ToString(CultureInfo.InvariantCulture) + ":" +
                   slimBlock.Min.X.ToString(CultureInfo.InvariantCulture) + "," +
                   slimBlock.Min.Y.ToString(CultureInfo.InvariantCulture) + "," +
                   slimBlock.Min.Z.ToString(CultureInfo.InvariantCulture);
        }
    }
}
