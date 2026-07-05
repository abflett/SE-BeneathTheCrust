using System.Collections.Generic;

namespace WkKn
{
    internal class ProficiencyDisplaySnapshot
    {
        public string PlayerName;
        public string Message;
        public int TrackedCount;
        public int MasteredCount;
        public long UpdatedTick;
        public List<ProficiencyDisplayEntry> Entries = new List<ProficiencyDisplayEntry>();

        public static ProficiencyDisplaySnapshot CreateMessage(string message)
        {
            return new ProficiencyDisplaySnapshot
            {
                Message = message,
                Entries = new List<ProficiencyDisplayEntry>(),
            };
        }

        public ProficiencyDisplaySnapshot Clone()
        {
            return new ProficiencyDisplaySnapshot
            {
                PlayerName = PlayerName,
                Message = Message,
                TrackedCount = TrackedCount,
                MasteredCount = MasteredCount,
                UpdatedTick = UpdatedTick,
                Entries = Entries == null ? new List<ProficiencyDisplayEntry>() : new List<ProficiencyDisplayEntry>(Entries),
            };
        }
    }

    internal struct ProficiencyDisplayEntry
    {
        public string ProficiencyId;
        public string DisplayName;
        public double Progress;
    }
}
