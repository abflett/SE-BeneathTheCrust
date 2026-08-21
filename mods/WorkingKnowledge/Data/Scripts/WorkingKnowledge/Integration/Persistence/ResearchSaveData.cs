using System.Collections.Generic;

namespace WkKn
{
    // Research persistence DTOs. Legacy worlds may still contain this shape in WkKnResearch.xml.
    public class ResearchSaveData
    {
        public List<ResearchScopeRecord> Players = new List<ResearchScopeRecord>();
        public List<ResearchScopeRecord> Factions = new List<ResearchScopeRecord>();
    }

    public class ResearchScopeRecord
    {
        public string Id;
        public List<ResearchSchematicRecord> Schematics = new List<ResearchSchematicRecord>();
    }

    public class ResearchSchematicRecord
    {
        public string ResearchId;
        public double Progress;
        public bool Unlocked;
        public string UnlockerSubtype;
        public string ActiveToken;
        public double ActiveProgress;
        public List<ResearchLedgerEntry> Ledger = new List<ResearchLedgerEntry>();
    }

    public class ResearchLedgerEntry
    {
        public string Token;
        public double Progress;
    }
}
