using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders;

namespace WkKn
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    public partial class WkKnSession : MySessionComponentBase
    {
        private WorkingKnowledgeRuntime runtime;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            Runtime.Init(sessionComponent);
        }

        public override void LoadData()
        {
            Runtime.LoadData();
        }

        public override void BeforeStart()
        {
            Runtime.BeforeStart();
        }

        public override void SaveData()
        {
            Runtime.SaveData();
        }

        public override void UpdateBeforeSimulation()
        {
            Runtime.UpdateBeforeSimulation();
        }

        public override void UpdateAfterSimulation()
        {
            Runtime.UpdateAfterSimulation();
        }

        protected override void UnloadData()
        {
            if (runtime != null)
            {
                runtime.Unload();
                runtime = null;
            }
        }

        private WorkingKnowledgeRuntime Runtime
        {
            get
            {
                if (runtime == null)
                    runtime = WorkingKnowledgeRuntime.Create(this);

                return runtime;
            }
        }
    }
}
