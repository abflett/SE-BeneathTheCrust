namespace WkKn
{
    internal sealed class WkSettingsTypingDelay
    {
        private string observed = string.Empty;
        private double due;
        private bool pending;

        internal void Reset(string text)
        {
            observed = text;
            pending = false;
        }

        internal bool IsReady(string text, double nowMilliseconds)
        {
            if (text != observed)
            {
                observed = text;
                due = nowMilliseconds + 350.0;
                pending = true;
            }
            if (!pending || nowMilliseconds < due)
                return false;
            pending = false;
            return true;
        }
    }
}
