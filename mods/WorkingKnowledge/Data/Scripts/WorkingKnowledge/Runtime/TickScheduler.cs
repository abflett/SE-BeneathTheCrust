namespace WkKn
{
    internal sealed class TickScheduler
    {
        private readonly WkKnSession session;

        internal TickScheduler(WkKnSession session)
        {
            this.session = session;
        }

        internal void UpdateBeforeSimulation()
        {
            session.UpdateRuntimeBeforeSimulation();
        }

        internal void UpdateAfterSimulation()
        {
            session.UpdateRuntimeAfterSimulation();
        }
    }
}
