using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;



namespace WkKn
{
    public partial class WkKnSession
    {
        private void QueueResearchNotification(long identityId, ResearchUnlockTarget target, ResearchProgressResult progress, string source)
        {
            researchNotificationService.Queue(identityId, target, progress, simulationTick, IsProgressComboEligibleSource(source));
        }

        private long GetNotificationDelayTicks()
        {
            return (long)Math.Round(Math.Max(0.1, config.NotificationDelaySeconds) * 60.0);
        }

        private bool IsLocalIdentity(long identityId)
        {
            return identityId != 0 &&
                   MyAPIGateway.Session != null &&
                   MyAPIGateway.Session.Player != null &&
                   MyAPIGateway.Session.Player.IdentityId == identityId;
        }

    }
}
