using System.Collections.Generic;

namespace WkKn
{
    // Proficiency persistence DTOs. Legacy worlds may still contain this shape in WkKnProficiency.xml.
    public class ProficiencySaveData
    {
        public List<ProficiencyScopeRecord> Players = new List<ProficiencyScopeRecord>();
    }

    public class ProficiencyScopeRecord
    {
        public string Id;
        public List<ProficiencyRecord> Proficiencies = new List<ProficiencyRecord>();
    }

    public class ProficiencyRecord
    {
        public string ProficiencyId;
        public double Progress;
        public long LastTouchedTick;
    }
}
