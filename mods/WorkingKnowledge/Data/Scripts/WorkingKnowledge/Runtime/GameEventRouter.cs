namespace WkKn
{
    internal sealed class GameEventRouter
    {
        private readonly WkKnSession session;
        private bool registered;

        internal GameEventRouter(WkKnSession session)
        {
            this.session = session;
        }

        internal void Register()
        {
            if (registered)
                return;

            session.RegisterGameEventHandlers();
            registered = true;
        }

        internal void Unregister()
        {
            if (!registered)
                return;

            session.UnregisterGameEventHandlers();
            registered = false;
        }
    }
}
