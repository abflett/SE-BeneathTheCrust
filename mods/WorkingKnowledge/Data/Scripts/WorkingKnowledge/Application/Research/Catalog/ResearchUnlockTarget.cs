using VRage.Game;

namespace WkKn
{
    internal struct ResearchUnlockTarget
    {
        public readonly string ResearchId;
        public readonly string DisplayName;
        public readonly MyDefinitionId UnlockerId;
        public readonly SchematicTier Tier;

        public ResearchUnlockTarget(string researchId, string displayName, MyDefinitionId unlockerId, SchematicTier tier)
        {
            ResearchId = researchId;
            DisplayName = displayName;
            UnlockerId = unlockerId;
            Tier = tier;
        }
    }
}
