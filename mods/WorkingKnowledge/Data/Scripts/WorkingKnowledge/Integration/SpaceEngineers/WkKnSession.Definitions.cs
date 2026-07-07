namespace WkKn
{
    public partial class WkKnSession
    {
        private void RebuildResearchDefinitions()
        {
            VanillaResearchDefinitionBinder.Rebuild(
                schematicCatalog,
                UnlockerBlockPrefix,
                ResearchPedestalSubtype,
                ResearchSciFiTerminalSubtype,
                ControlInterfaceResearchGroupSubtype);
        }
    }
}
