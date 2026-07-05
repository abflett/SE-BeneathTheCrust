using System.Collections.Generic;

namespace WkKn
{
    internal class ResearchDisplaySnapshot
    {
        public string Message;
        public long UpdatedTick;
        public List<ResearchDisplayScopeSnapshot> Scopes = new List<ResearchDisplayScopeSnapshot>();

        public static ResearchDisplaySnapshot CreateMessage(string message)
        {
            return new ResearchDisplaySnapshot
            {
                Message = message,
                Scopes = new List<ResearchDisplayScopeSnapshot>(),
            };
        }

        public ResearchDisplaySnapshot Clone()
        {
            var clone = new ResearchDisplaySnapshot
            {
                Message = Message,
                UpdatedTick = UpdatedTick,
                Scopes = new List<ResearchDisplayScopeSnapshot>(),
            };

            if (Scopes != null)
            {
                foreach (var scope in Scopes)
                {
                    if (scope != null)
                        clone.Scopes.Add(scope.Clone());
                }
            }

            return clone;
        }
    }

    internal class ResearchDisplayScopeSnapshot
    {
        public string Header;
        public string OwnerName;
        public string FactionTag;
        public bool IsFaction;
        public int TrackedCount;
        public int ActiveCount;
        public int CompletedCount;
        public List<ResearchDisplayEntry> Entries = new List<ResearchDisplayEntry>();

        public ResearchDisplayScopeSnapshot Clone()
        {
            return new ResearchDisplayScopeSnapshot
            {
                Header = Header,
                OwnerName = OwnerName,
                FactionTag = FactionTag,
                IsFaction = IsFaction,
                TrackedCount = TrackedCount,
                ActiveCount = ActiveCount,
                CompletedCount = CompletedCount,
                Entries = Entries == null ? new List<ResearchDisplayEntry>() : new List<ResearchDisplayEntry>(Entries),
            };
        }
    }

    internal struct ResearchDisplayEntry
    {
        public string ResearchId;
        public string DisplayName;
        public double Progress;
        public bool Unlocked;
    }

    internal class IdentityDisplaySnapshot
    {
        public string Message;
        public string PlayerName;
        public string FactionName;
        public string FactionTag;
        public string FactionIconName;
        public bool HasFaction;
        public VRageMath.Color FactionIconColor;
        public VRageMath.Color FactionBackgroundColor;
    }
}
