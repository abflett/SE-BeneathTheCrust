using System;
using Sandbox.ModAPI;
using VRage;
using VRage.Utils;

namespace WkKn
{
    // Optional Text HUD API menu bridge. Protocol members match the installed HudAPIv2
    // MenuItem and MenuRootCategory public client API; no rendering dependency is added.
    internal sealed class WkTextHudSettingsLauncher
    {
        private const long RegistrationId = 573804956;
        private const int MenuItemType = 20, MenuRootType = 22;
        private const int TextMember = 0, InteractableMember = 1;
        private const int CallbackMember = 100, ParentMember = 101;
        private const int HeaderMember = 100, MenuFlagMember = 200;
        private Action<object, int, object> setter;
        private object root, item;
        private readonly Action open;
        private bool requested;
        private int inputReleasedTicks;
        private bool closed;

        internal WkTextHudSettingsLauncher(Action open)
        {
            this.open = open;
            MyAPIGateway.Utilities.RegisterMessageHandler(RegistrationId, Register);
        }

        private void Register(object message)
        {
            if (closed || setter != null || !(message is MyTuple<Func<int, object>, Action<object, int, object>, Func<object, int, object>, Action<object>>))
                return;

            var api = (MyTuple<Func<int, object>, Action<object, int, object>, Func<object, int, object>, Action<object>>)message;
            setter = api.Item2;
            try
            {
                root = api.Item1(MenuRootType);
                item = api.Item1(MenuItemType);
                setter(root, TextMember, "Working Knowledge");
                setter(root, HeaderMember, "Working Knowledge Settings");
                setter(item, TextMember, "Open settings (close chat to continue)");
                setter(item, ParentMember, root);
                setter(item, CallbackMember, (Action)delegate { if (!closed) requested = true; });
                setter(item, InteractableMember, true);
                setter(root, MenuFlagMember, 1); // Player menu; server permissions still apply in WK.
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " optional Text HUD settings launcher unavailable: " + exception.Message);
            }
        }

        internal void Update()
        {
            // Text HUD owns its menu while chat is open. Hand off after it releases input.
            if (requested && MyAPIGateway.Gui != null && !MyAPIGateway.Gui.ChatEntryVisible && !MyAPIGateway.Gui.IsCursorVisible)
            {
                // Let the Escape press used to close chat finish before the new window opens.
                if (++inputReleasedTicks >= 3)
                {
                    requested = false;
                    inputReleasedTicks = 0;
                    open();
                }
            }
            else
                inputReleasedTicks = 0;
        }

        internal void Close()
        {
            closed = true;
            requested = false;
            MyAPIGateway.Utilities.UnregisterMessageHandler(RegistrationId, Register);
            try
            {
                if (setter != null && root != null)
                    setter(root, InteractableMember, false);
                if (setter != null && item != null)
                    setter(item, CallbackMember, null);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " optional Text HUD launcher cleanup: " + exception.Message);
            }
            setter = null;
            root = item = null;
        }
    }
}
