namespace WkKn
{
    internal sealed class WorkingKnowledgeLayerGroup
    {
        internal readonly ResearchCatalogEntry Entry;
        internal readonly string ModName;
        internal readonly int LineNumber;

        internal WorkingKnowledgeLayerGroup(ResearchCatalogEntry entry, string modName, int lineNumber)
        {
            Entry = entry;
            ModName = modName;
            LineNumber = lineNumber;
        }

        internal string Source
        {
            get { return ModName + " line " + LineNumber; }
        }
    }
}
