using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ObjectBuilders;

namespace WkKn
{
    public partial class WkKnSession
    {
        internal const string LogPrefix = "[Working Knowledge]";
        private const string UnlockerBlockPrefix = "WkKnUnlocker";
        private const string FundamentalsResearchId = "fundamentals";
        private const string ResearchPedestalSubtype = "WkKnResearchPedestal";
        internal const string ResearchSciFiTerminalSubtype = "WkKnResearchSciFiTerminal";
        private const string ControlInterfaceResearchGroupSubtype = "WkKn_control_interfaces";
        private const string ResearchStorageFile = "WkKnResearch.xml";
        private const string ProficiencyStorageFile = "WkKnProficiency.xml";
        private const string ConfigStorageFile = "WkKnConfig.xml";
        private const string PlayerConfigStorageFile = "WkKnPlayerConfig.xml";
        private const double RequiredResearchProgress = 1.0;
        private const double ResearchProgressPrecision = 0.0001;
        private const double ProficiencyBuildCapExponent = 2.0;
        private const double ActiveHandWelderRange = 6.0;
        private const double ShipWelderAttributionSearchRadius = 12.0;
        private const double ShipWelderAttributionPadding = 1.0;
        private const double ShipWelderFallbackSensorRadius = 2.5;
        private const double WeldCapTolerance = 0.001;
        private const long BlockIntegrityScanDiscoveryIntervalTicks = 600;
        private const int BlockIntegrityMaxGridScansPerTick = 1;
        private const double BlockIntegrityScanDiscoveryRadius = 10000.0;
        private const long ResearchAutosaveTicks = 300;
        internal const string ResearchTextSurfaceScriptId = "TSS_WkKnResearch";
        internal const string IdentityTextSurfaceScriptId = "TSS_WkKnIdentity";
        internal const string CalibratorTextSurfaceScriptId = "TSS_WkKnCalibrator";
        private const ushort ResearchDisplayNetworkMessageId = 49218;
        private const ushort ResearchTerminalSyncNetworkMessageId = 49219;
        private const ushort LocalFeedbackNetworkMessageId = 49220;
        private const long ResearchDisplayRefreshTicks = 120;
        private const long ResearchDisplayNetworkRequestTicks = 600;
        private const string ResearchDisplaySyncRequest = "Request";
        private const string ResearchDisplaySyncResponse = "Response";
        private const string ResearchTerminalSyncRequest = "Request";
        private const string ResearchTerminalSyncResponse = "Response";
        internal const string ProficiencyTextSurfaceScriptId = "TSS_WkKnProficiency";
        private const ushort ProficiencyDisplayNetworkMessageId = 49217;
        private const long ProficiencyDisplayRefreshTicks = 120;
        private const long ProficiencyDisplayNetworkRequestTicks = 600;
        private const string ProficiencyDisplaySyncRequest = "Request";
        private const string ProficiencyDisplaySyncResponse = "Response";
        private const string CommonFragmentSubtype = "WkKnFragmentCommon";
        private const string UncommonFragmentSubtype = "WkKnFragmentUncommon";
        private const string RareFragmentSubtype = "WkKnFragmentRare";
        private const string PrototechFragmentSubtype = "WkKnFragmentPrototech";
        private const string ResearchSchematicSubtypePrefix = "WkKnSchematic_";
        private static readonly MyDefinitionId ScrapOreId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap");
        private static readonly MyDefinitionId CommonFragmentId = new MyDefinitionId(typeof(MyObjectBuilder_ConsumableItem), CommonFragmentSubtype);
        private static readonly MyDefinitionId UncommonFragmentId = new MyDefinitionId(typeof(MyObjectBuilder_ConsumableItem), UncommonFragmentSubtype);
        private static readonly MyDefinitionId RareFragmentId = new MyDefinitionId(typeof(MyObjectBuilder_ConsumableItem), RareFragmentSubtype);
        private static readonly MyDefinitionId PrototechFragmentId = new MyDefinitionId(typeof(MyObjectBuilder_ConsumableItem), PrototechFragmentSubtype);
        private static readonly object ResearchDisplaySnapshotLock = new object();
        private static ResearchDisplaySnapshot currentResearchDisplaySnapshot = ResearchDisplaySnapshot.CreateMessage("Working Knowledge is loading.");
        private static readonly object ProficiencyDisplaySnapshotLock = new object();
        private static ProficiencyDisplaySnapshot currentProficiencyDisplaySnapshot = ProficiencyDisplaySnapshot.CreateMessage("Working Knowledge is loading.");
        private static WkKnSession activeSession;
        private readonly Dictionary<long, string> progressChatHeaderByIdentity = new Dictionary<long, string>();
        private readonly Dictionary<string, double> progressChatLastShownByKey = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, double> progressToastLastShownByKey = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> weldBotchWarningLastShownByKey = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private readonly WkProgressHudOverlay progressHudOverlay = new WkProgressHudOverlay(new VRageMath.Color(165, 255, 35), new VRageMath.Color(80, 255, 210));
        private readonly SchematicCatalog schematicCatalog = new SchematicCatalog();
        private readonly ResearchService researchService = new ResearchService(new ResearchProgressTuning(RequiredResearchProgress, ResearchProgressPrecision));
        private readonly ResearchSharingService researchSharingService = new ResearchSharingService(RequiredResearchProgress);
        private readonly ResearchDisplayService researchDisplayService = new ResearchDisplayService(RequiredResearchProgress);
        private readonly ResearchSummaryService researchSummaryService = new ResearchSummaryService();
        private readonly ResearchNotificationService researchNotificationService = new ResearchNotificationService();
        private readonly ResearchTerminalService researchTerminalService = new ResearchTerminalService(RequiredResearchProgress);
        private readonly ResearchTerminalAdapter researchTerminalAdapter = new ResearchTerminalAdapter(ResearchPedestalSubtype, ResearchSciFiTerminalSubtype);
        private readonly PlayerIdentityAdapter playerIdentityAdapter = new PlayerIdentityAdapter();
        private readonly DataFragmentResearchSource dataFragmentResearchSource = new DataFragmentResearchSource();
        private readonly VanillaResearchMirror vanillaResearchMirror = new VanillaResearchMirror();
        private readonly ProficiencyService proficiencyService = new ProficiencyService(RequiredResearchProgress);
        private readonly ProficiencyDisplayService proficiencyDisplayService = new ProficiencyDisplayService(RequiredResearchProgress);
        private readonly ProficiencySummaryService proficiencySummaryService = new ProficiencySummaryService(RequiredResearchProgress);
        private readonly ProficiencyNotificationService proficiencyNotificationService = new ProficiencyNotificationService();
        private readonly SalvageRecoveryPolicy salvageRecoveryPolicy = new SalvageRecoveryPolicy(RequiredResearchProgress);
        private readonly WeldBuildCapPolicy weldBuildCapPolicy = new WeldBuildCapPolicy(RequiredResearchProgress, ProficiencyBuildCapExponent, WeldCapTolerance);
        private readonly BlockIntegrityMonitor blockIntegrityMonitor = new BlockIntegrityMonitor(WeldCapTolerance);
        private readonly BlockWorkRuntimeState blockWorkState = new BlockWorkRuntimeState();
        private readonly Dictionary<long, ResearchPedestalView> researchPedestalViewsByBlock = new Dictionary<long, ResearchPedestalView>();
        private readonly List<IMyTerminalControl> researchPedestalControls = new List<IMyTerminalControl>();
        private readonly Random random = new Random();
        private IMyTerminalControlListbox playerResearchListControl;
        private IMyTerminalControlListbox factionResearchListControl;
        private MyPhysicalItemDefinition scrapDefinition;
        private WkConfig config
        {
            get { return configStore.Data; }
        }

        private readonly WkConfigStore configStore = new WkConfigStore();
        private readonly WkPlayerConfigStore playerConfigStore = new WkPlayerConfigStore();
        private readonly ResearchStore researchStore = new ResearchStore();
        private readonly ProficiencyStore proficiencyStore = new ProficiencyStore();
        private DisplaySyncTransport<ResearchDisplaySyncMessage> researchDisplaySyncTransport;
        private DisplaySyncTransport<ProficiencyDisplaySyncMessage> proficiencyDisplaySyncTransport;
        private bool researchTerminalSyncRegistered;
        private bool localFeedbackNetworkRegistered;
        private ResearchDisplaySyncMessage syncedLocalResearchDisplay;
        private ProficiencyScopeRecord syncedLocalProficiencyScope;
        private bool researchDataFragmentLootInjected;
        private long lastResearchDisplayRefreshTick = -ResearchDisplayRefreshTicks;
        private long lastResearchDisplayNetworkRequestTick = -ResearchDisplayNetworkRequestTicks;
        private long lastProficiencyDisplayRefreshTick = -ProficiencyDisplayRefreshTicks;
        private long lastProficiencyDisplayNetworkRequestTick = -ProficiencyDisplayNetworkRequestTicks;
        private long simulationTick;
    }
}
