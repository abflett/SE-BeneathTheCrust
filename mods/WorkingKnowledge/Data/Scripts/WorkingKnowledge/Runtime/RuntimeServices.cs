namespace WkKn
{
    internal sealed class RuntimeServices
    {
        private RuntimeServices(WkKnSession session)
        {
            Session = session;
            GameEvents = new GameEventRouter(session);
            TickScheduler = new TickScheduler(session);
        }

        internal WkKnSession Session { get; private set; }

        internal GameEventRouter GameEvents { get; private set; }

        internal TickScheduler TickScheduler { get; private set; }

        internal static RuntimeServices Create(WkKnSession session)
        {
            return new RuntimeServices(session);
        }
    }
}
