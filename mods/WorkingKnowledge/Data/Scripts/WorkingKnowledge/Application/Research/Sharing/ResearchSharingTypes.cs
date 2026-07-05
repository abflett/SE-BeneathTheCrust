namespace WkKn
{
    internal struct ResearchContributionEntry
    {
        public string Token;
        public double Progress;
    }

    internal struct ResearchSyncResult
    {
        public int ChangedSchematics;
        public int CompletedSchematics;
        public double AddedProgress;

        public int TotalChanges
        {
            get { return ChangedSchematics; }
        }

        public void Add(ResearchProgressResult progress)
        {
            if (!progress.Changed)
                return;

            ChangedSchematics++;
            AddedProgress += progress.AddedProgress;
            if (progress.Unlocked)
                CompletedSchematics++;
        }

        public void Add(ResearchSyncResult result)
        {
            ChangedSchematics += result.ChangedSchematics;
            CompletedSchematics += result.CompletedSchematics;
            AddedProgress += result.AddedProgress;
        }
    }

    internal struct ResearchSharingResult
    {
        public ResearchSyncResult UploadedToFaction;
        public ResearchSyncResult DownloadedToPlayer;

        public int TotalChanges
        {
            get { return UploadedToFaction.TotalChanges + DownloadedToPlayer.TotalChanges; }
        }
    }
}
