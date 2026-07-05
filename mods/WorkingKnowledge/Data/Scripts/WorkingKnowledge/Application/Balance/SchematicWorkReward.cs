namespace WkKn
{
    internal struct SchematicWorkReward
    {
        public readonly string ResearchId;
        public readonly double LargeGridBaseWorkReward;
        public readonly double SmallGridBaseWorkReward;
        public readonly double ReferenceBuildSeconds;

        public SchematicWorkReward(string researchId, double largeGridBaseWorkReward, double smallGridBaseWorkReward, double referenceBuildSeconds)
        {
            ResearchId = researchId;
            LargeGridBaseWorkReward = largeGridBaseWorkReward;
            SmallGridBaseWorkReward = smallGridBaseWorkReward;
            ReferenceBuildSeconds = referenceBuildSeconds;
        }
    }
}
