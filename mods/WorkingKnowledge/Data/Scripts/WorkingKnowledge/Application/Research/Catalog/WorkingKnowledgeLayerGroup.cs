namespace WkKn
{
    internal sealed class WorkingKnowledgeLayerGroup
    {
        internal readonly ResearchCatalogEntry Entry;
        internal readonly string ModName;
        internal readonly int LineNumber;
        internal readonly int LoadIndex;

        internal WorkingKnowledgeLayerGroup(ResearchCatalogEntry entry, string modName, int lineNumber, int loadIndex)
        {
            Entry = entry;
            ModName = modName;
            LineNumber = lineNumber;
            LoadIndex = loadIndex;
        }

        internal string Source
        {
            get { return ModName + " (priority " + (LoadIndex + 1) + ") line " + LineNumber; }
        }
    }
}
