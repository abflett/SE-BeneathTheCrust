using System.Collections.Generic;

namespace WkKn
{
    // World-storage DTOs for WkKnProficiency.xml. Early development can break this shape.
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
