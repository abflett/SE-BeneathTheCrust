namespace WkKn
{
    internal struct ResearchProgressTuning
    {
        public readonly double RequiredProgress;
        public readonly double ProgressPrecision;

        public ResearchProgressTuning(
            double requiredProgress,
            double progressPrecision)
        {
            RequiredProgress = requiredProgress;
            ProgressPrecision = progressPrecision;
        }
    }
}
