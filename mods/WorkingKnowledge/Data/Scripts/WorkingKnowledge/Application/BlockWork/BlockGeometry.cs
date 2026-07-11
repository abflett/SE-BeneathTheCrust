using System;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal static class BlockGeometry
    {
        internal static double GetWorldRadius(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.CubeGrid == null)
                return 0.0;

            var x = Math.Max(1, slimBlock.Max.X - slimBlock.Min.X + 1);
            var y = Math.Max(1, slimBlock.Max.Y - slimBlock.Min.Y + 1);
            var z = Math.Max(1, slimBlock.Max.Z - slimBlock.Min.Z + 1);
            return slimBlock.CubeGrid.GridSize * Math.Sqrt((x * x) + (y * y) + (z * z)) * 0.5;
        }
    }
}
