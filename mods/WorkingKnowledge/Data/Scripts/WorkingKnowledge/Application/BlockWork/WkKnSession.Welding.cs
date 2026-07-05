using System;

namespace WkKn
{
    public partial class WkKnSession
    {
        private bool ApplyWeldOperation(WeldOperation operation)
        {
            if (operation == null || operation.Block == null || operation.Inventory == null || operation.IdentityId == 0)
                return false;

            var afterBuildRatio = BlockCondition.GetBuildRatio(operation.Block);
            var afterIntegrityRatio = BlockCondition.GetIntegrityRatio(operation.Block);
            var buildProgress = Math.Max(0.0, afterBuildRatio - operation.BeforeBuildRatio);
            var repairProgress = Math.Max(0.0, afterIntegrityRatio - operation.BeforeIntegrityRatio);
            var maxIntegrity = operation.Block.ComponentStack == null ? 0f : operation.Block.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= 0f)
                return false;

            var integrityRawDelta = operation.Block.ComponentStack.Integrity - operation.BeforeIntegrity;
            if (integrityRawDelta <= WeldCapTolerance)
                return false;

            var workScale = Math.Max(buildProgress, repairProgress);
            if (workScale <= WeldCapTolerance)
                workScale = integrityRawDelta / maxIntegrity;

            if (workScale <= WeldCapTolerance)
                return false;

            var botched = config.ProficiencyEnabled &&
                           config.ProficiencyWeldingLossEnabled &&
                           ApplyWeldBotchOperation(operation, workScale, integrityRawDelta);

            if (!botched)
                AwardProficiencyFromHandsOnWork(operation.IdentityId, operation.ResearchId, workScale * GetFullWorkReward(operation.Block, operation.ResearchId), "welding");

            return botched;
        }
    }
}
