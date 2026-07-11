namespace WkKn
{
    public partial class WkKnSession
    {
        private void RebuildResearchDefinitions()
        {
            layerAudit = VanillaResearchDefinitionBinder.Rebuild(
                schematicCatalog,
                UnlockerBlockPrefix,
                ResearchPedestalSubtype,
                ResearchSciFiTerminalSubtype,
                ControlInterfaceResearchGroupSubtype);
            PublishLayerAudit();
        }
    }
}
