using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.Game.Entities;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace WkKn
{
    internal struct ResearchDamageOperation
    {
        public long AttackerId;
        public IMySlimBlock Block;
        public ResearchUnlockTarget Target;
        public long IdentityId;
        public double BeforeBuildRatio;
        public long Tick;
    }

    internal struct ProficiencyDamageOperation
    {
        public long AttackerId;
        public IMySlimBlock Block;
        public long IdentityId;
        public string ResearchId;
        public string DisplayName;
        public double BeforeBuildRatio;
        public long Tick;
    }

    internal class SalvageOperation
    {
        public long AttackerId;
        public IMySlimBlock Block;
        public MyInventory Inventory;
        public string ResearchId;
        public string DisplayName;
        public int IntactPercent;
        public long Tick;
        public Dictionary<MyDefinitionId, MyFixedPoint> BeforeComponents;
    }

    internal class WeldOperation
    {
        public long AttackerId;
        public IMySlimBlock Block;
        public MyInventory Inventory;
        public long IdentityId;
        public string ResearchId;
        public string DisplayName;
        public double Proficiency;
        public double BeforeBuildRatio;
        public double BeforeIntegrityRatio;
        public float BeforeBuildIntegrity;
        public float BeforeIntegrity;
    }

    internal struct WeldBotchResult
    {
        public float DamageAmount;
        public string LostComponentsText;
        public string RecoveredComponentsText;
    }

    internal sealed class WeldBotchNotificationAccumulator
    {
        public long IdentityId;
        public string ResearchId;
        public string BlockDisplayName;
        public float DamageAmount;
        public float MaxIntegrity;
        public long LastTick;
        public readonly Dictionary<string, int> LostComponentsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, int> RecoveredComponentsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    internal struct WeldBotchSoundEvent
    {
        public Vector3D Position;
        public string SoundSubtype;
        public double Range;
    }
}
