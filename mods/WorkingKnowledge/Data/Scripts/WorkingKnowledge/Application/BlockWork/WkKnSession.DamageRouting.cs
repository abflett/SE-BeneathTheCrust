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
        private void BeforeDamage(object target, ref MyDamageInformation damage)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            if (damage.Type == MyDamageType.Grind)
            {
                BeforeGrindDamage(target, ref damage);
            }
        }

        private void AfterDamage(object target, MyDamageInformation damage)
        {
            if (!MyAPIGateway.Session.IsServer)
                return;

            if (damage.Type == MyDamageType.Grind)
            {
                AfterGrindDamage(target, damage);
            }
        }

    }
}
