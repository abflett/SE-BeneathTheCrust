using Sandbox.ModAPI;
using Sandbox.Game;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const string ControlInterfaceResearchId = "control.interfaces";

        private bool EnforceResearchPlacement(IMySlimBlock slimBlock)
        {
            if (slimBlock == null ||
                slimBlock.BlockDefinition == null ||
                slimBlock.CubeGrid == null ||
                MyAPIGateway.Session == null ||
                !MyAPIGateway.Session.IsServer)
                return false;

            var builderIdentityId = ResolveBlockBuilderIdentity(slimBlock);
            var builderPlayer = FindPlayerByIdentity(builderIdentityId);
            if (builderIdentityId == 0 || builderPlayer == null || ShouldBypassResearchPlacement(slimBlock, builderPlayer))
                return false;

            ResearchUnlockTarget target;
            if (!TryGetPlacementResearchTarget(slimBlock, out target))
                return false;

            if (GetPlayerResearchProgress(builderIdentityId, target.ResearchId) >= RequiredResearchProgress)
                return false;

            slimBlock.CubeGrid.RemoveBlock(slimBlock, true);
            NotifyPlacementDenied(builderIdentityId, target.DisplayName);
            return true;
        }

        private bool TryGetPlacementResearchTarget(IMySlimBlock slimBlock, out ResearchUnlockTarget target)
        {
            target = default(ResearchUnlockTarget);
            if (slimBlock == null || slimBlock.BlockDefinition == null)
                return false;

            if (schematicCatalog.TryGetTargetByBlock(slimBlock.BlockDefinition.Id, out target))
                return true;

            var terminalBlock = slimBlock.FatBlock as IMyTerminalBlock;
            return researchTerminalAdapter.IsResearchTerminal(terminalBlock) &&
                   schematicCatalog.TryGetTargetByResearchId(ControlInterfaceResearchId, out target);
        }

        private static long ResolveBlockBuilderIdentity(IMySlimBlock slimBlock)
        {
            if (slimBlock == null)
                return 0;

            if (slimBlock.BuiltBy != 0)
                return slimBlock.BuiltBy;

            var cubeBlock = slimBlock.FatBlock as IMyCubeBlock;
            return cubeBlock != null ? cubeBlock.OwnerId : 0;
        }

        private static bool ShouldBypassResearchPlacement(IMySlimBlock slimBlock, IMyPlayer player)
        {
            var session = MyAPIGateway.Session;
            if (session == null)
                return false;

            if (session.CreativeMode)
                return true;

            if (player == null || player.SteamUserId == 0)
                return false;

            return player.PromoteLevel >= MyPromoteLevel.SpaceMaster &&
                   BlockCondition.GetBuildRatio(slimBlock) >= RequiredResearchProgress;
        }

        private void NotifyPlacementDenied(long identityId, string displayName)
        {
            if (!IsLocalPlayerIdentity(identityId))
                return;

            MyVisualScriptLogicProvider.ShowNotification("Schematic required: " + displayName, 3500, MyFontEnum.Red);
        }
    }
}
