using VRage.Game;

namespace WkKn
{
    internal struct ResearchUnlockTarget
    {
        public readonly string ResearchId;
        public readonly string DisplayName;
        public readonly string Description;
        public readonly MyDefinitionId UnlockerId;
        public readonly SchematicTier Tier;

        public ResearchUnlockTarget(string researchId, string displayName, string description, MyDefinitionId unlockerId, SchematicTier tier)
        {
            ResearchId = researchId;
            DisplayName = displayName;
            Description = description;
            UnlockerId = unlockerId;
            Tier = tier;
        }
    }
}
