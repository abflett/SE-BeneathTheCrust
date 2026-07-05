namespace WkKn
{
    internal struct ProficiencyProgressTuning
    {
        public readonly double FirstThreshold;
        public readonly double SecondThreshold;
        public readonly double FirstSegmentRate;
        public readonly double SecondSegmentRate;
        public readonly double FinalSegmentRate;
        public readonly double RequiredProgress;

        public ProficiencyProgressTuning(
            double firstThreshold,
            double secondThreshold,
            double firstSegmentRate,
            double secondSegmentRate,
            double finalSegmentRate,
            double requiredProgress)
        {
            FirstThreshold = firstThreshold;
            SecondThreshold = secondThreshold;
            FirstSegmentRate = firstSegmentRate;
            SecondSegmentRate = secondSegmentRate;
            FinalSegmentRate = finalSegmentRate;
            RequiredProgress = requiredProgress;
        }
    }
}
