using System.Collections.Generic;

namespace WkKn
{
    // Network DTO for proficiency display snapshot requests and responses.
    // Keep public field names stable while the display sync protocol is in use.
    public class ProficiencyDisplaySyncMessage
    {
        public string Kind;
        public long IdentityId;
        public List<ProficiencyRecord> Proficiencies = new List<ProficiencyRecord>();
    }
}
