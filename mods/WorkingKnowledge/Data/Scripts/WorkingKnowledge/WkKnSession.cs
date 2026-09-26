using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders;

namespace WkKn
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    public partial class WkKnSession : MySessionComponentBase
    {
        private bool runtimeActive;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (runtimeActive)
                return;

            runtimeActive = true;
            RegisterGameEventHandlers();
            InitializeRuntimeModules(sessionComponent);
        }

        public override void LoadData()
        {
            LoadRuntimeData();
        }

        public override void BeforeStart()
        {
            StartRuntime();
        }

        public override void SaveData()
        {
            SaveRuntimeData();
        }

        public override void UpdateBeforeSimulation()
        {
            UpdateRuntimeBeforeSimulation();
        }

        public override void UpdateAfterSimulation()
        {
            UpdateRuntimeAfterSimulation();
        }

        protected override void UnloadData()
        {
            if (!runtimeActive)
                return;

            runtimeActive = false;
            WkCleanup.Run("game event handlers", UnregisterGameEventHandlers);
            WkCleanup.Run("runtime modules", UnloadRuntimeModules);
        }
    }
}
