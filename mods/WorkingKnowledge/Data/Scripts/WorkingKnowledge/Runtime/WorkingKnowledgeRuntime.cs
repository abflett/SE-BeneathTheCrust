using VRage.Game;
using VRage.Game.ObjectBuilders;

namespace WkKn
{
    internal sealed class WorkingKnowledgeRuntime
    {
        private readonly RuntimeServices services;

        private WorkingKnowledgeRuntime(RuntimeServices services)
        {
            this.services = services;
        }

        internal static WorkingKnowledgeRuntime Create(WkKnSession session)
        {
            return new WorkingKnowledgeRuntime(RuntimeServices.Create(session));
        }

        internal void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            services.GameEvents.Register();
            services.Session.InitializeRuntimeModules(sessionComponent);
        }

        internal void LoadData()
        {
            services.Session.LoadRuntimeData();
        }

        internal void BeforeStart()
        {
            services.Session.StartRuntime();
        }

        internal void SaveData()
        {
            services.Session.SaveRuntimeData();
        }

        internal void UpdateBeforeSimulation()
        {
            services.TickScheduler.UpdateBeforeSimulation();
        }

        internal void UpdateAfterSimulation()
        {
            services.TickScheduler.UpdateAfterSimulation();
        }

        internal void Unload()
        {
            services.GameEvents.Unregister();
            services.Session.UnloadRuntimeModules();
        }
    }
}
