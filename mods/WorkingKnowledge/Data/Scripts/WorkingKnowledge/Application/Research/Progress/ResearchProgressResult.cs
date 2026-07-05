namespace WkKn
{
    internal struct ResearchProgressResult
    {
        public static readonly ResearchProgressResult NoChange = new ResearchProgressResult();

        public bool Changed;
        public double AddedProgress;
        public double TotalProgress;
        public bool Unlocked;
    }
}
