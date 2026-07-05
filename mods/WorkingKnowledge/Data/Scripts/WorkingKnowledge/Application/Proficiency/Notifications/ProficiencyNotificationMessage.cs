namespace WkKn
{
    internal struct ProficiencyNotificationMessage
    {
        public long IdentityId;
        public string ProficiencyId;
        public string DisplayName;
        public double AddedProgress;
        public double TotalProgress;
        public bool ComboEligible;
    }
}
