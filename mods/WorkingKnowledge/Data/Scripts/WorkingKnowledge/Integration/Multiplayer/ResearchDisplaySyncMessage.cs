using System.Collections.Generic;

namespace WkKn
{
    // Network DTO for research display snapshot requests and responses.
    // Keep public field names stable while the display sync protocol is in use.
    public class ResearchDisplaySyncMessage
    {
        public string Kind;
        public long IdentityId;
        public List<ResearchDisplayScopeRecord> Scopes = new List<ResearchDisplayScopeRecord>();
    }

    public class ResearchDisplayScopeRecord
    {
        public string ScopeId;
        public string Header;
        public string OwnerName;
        public string FactionTag;
        public bool IsFaction;
        public List<ResearchDisplaySchematicRecord> Schematics = new List<ResearchDisplaySchematicRecord>();
    }

    public class ResearchDisplaySchematicRecord
    {
        public string ResearchId;
        public double Progress;
        public bool Unlocked;
    }
}
