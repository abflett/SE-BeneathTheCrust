namespace WkKn
{
    internal struct ProficiencyProgressResult
    {
        public static readonly ProficiencyProgressResult NoChange = new ProficiencyProgressResult();

        public bool Changed;
        public double AddedProgress;
        public double TotalProgress;
    }
}
