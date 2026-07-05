namespace WkKn
{
    internal enum ResearchPedestalView : long
    {
        Default = 0,
        Researching = 1,
        Completed = 2,
        All = 3,
    }

    internal struct ResearchListEntry
    {
        public string ResearchId;
        public string DisplayName;
        public double Progress;
        public bool Unlocked;
    }
}
