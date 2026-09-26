using System;
using VRage.Utils;

namespace WkKn
{
    // Teardown only: a failed external cleanup must not prevent the remaining releases.
    internal static class WkCleanup
    {
        internal static void Run(string operation, Action cleanup)
        {
            try
            {
                cleanup();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " cleanup failed (" + operation + "): " + exception);
            }
        }
    }
}
