using System.Collections.Generic;

namespace WkKn
{
    public partial class WkKnSession
    {
        private ResearchScopeRecord FindScope(List<ResearchScopeRecord> scopes, string scopeId)
        {
            return researchStore.FindScope(scopes, scopeId);
        }
    }
}
