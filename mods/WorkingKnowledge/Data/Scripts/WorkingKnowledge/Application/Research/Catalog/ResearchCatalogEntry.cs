namespace WkKn
{
    internal struct ResearchCatalogEntry
    {
        public readonly string BlockKey;
        public readonly string ResearchId;
        public readonly string DisplayName;
        public readonly string GroupSubtype;
        public readonly string UnlockerSubtype;
        public readonly SchematicTier Tier;

        public ResearchCatalogEntry(string blockKey, string researchId, string displayName, string groupSubtype, string unlockerSubtype, SchematicTier tier)
        {
            BlockKey = blockKey;
            ResearchId = researchId;
            DisplayName = displayName;
            GroupSubtype = groupSubtype;
            UnlockerSubtype = unlockerSubtype;
            Tier = tier;
        }
    }
}
