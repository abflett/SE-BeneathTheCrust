using System;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal sealed class ResearchTerminalAdapter
    {
        private readonly string pedestalSubtype;
        private readonly string sciFiTerminalSubtype;

        internal ResearchTerminalAdapter(string pedestalSubtype, string sciFiTerminalSubtype)
        {
            this.pedestalSubtype = pedestalSubtype;
            this.sciFiTerminalSubtype = sciFiTerminalSubtype;
        }

        internal bool IsResearchTerminal(IMyTerminalBlock block)
        {
            return block != null &&
                   ((block.BlockDefinition.TypeIdString == "MyObjectBuilder_TerminalBlock" &&
                     block.BlockDefinition.SubtypeName.Equals(pedestalSubtype, StringComparison.OrdinalIgnoreCase)) ||
                    (block.BlockDefinition.TypeIdString == "MyObjectBuilder_MyProgrammableBlock" &&
                     block.BlockDefinition.SubtypeName.Equals(sciFiTerminalSubtype, StringComparison.OrdinalIgnoreCase)));
        }

        internal long ResolveIdentityId(IMyTerminalBlock block)
        {
            if (MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
                return MyAPIGateway.Session.Player.IdentityId;

            return block != null ? block.OwnerId : 0;
        }

        internal bool TryGetPlayerFaction(long identityId, out IMyFaction faction)
        {
            faction = MyAPIGateway.Session != null && MyAPIGateway.Session.Factions != null
                ? MyAPIGateway.Session.Factions.TryGetPlayerFaction(identityId)
                : null;

            return faction != null;
        }

        internal ResearchTerminalValidationResult ValidateSync(IMyTerminalBlock block, long identityId)
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer)
                return ResearchTerminalValidationResult.Failed("Research terminal sync needs the server message bridge in multiplayer.");

            if (!IsResearchTerminal(block) || identityId == 0)
                return ResearchTerminalValidationResult.Failed("Research terminal is not ready.");

            if (!block.HasPlayerAccess(identityId, MyRelationsBetweenPlayerAndBlock.NoOwnership))
                return ResearchTerminalValidationResult.Failed("You do not have access to this Research Terminal.");

            IMyFaction faction;
            if (!TryGetPlayerFaction(identityId, out faction))
                return ResearchTerminalValidationResult.Failed("Join or create a faction to use faction archive sync.");

            return ResearchTerminalValidationResult.Success(faction);
        }
    }

    internal struct ResearchTerminalValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public IMyFaction Faction;

        internal static ResearchTerminalValidationResult Success(IMyFaction faction)
        {
            return new ResearchTerminalValidationResult
            {
                IsValid = true,
                Faction = faction,
            };
        }

        internal static ResearchTerminalValidationResult Failed(string errorMessage)
        {
            return new ResearchTerminalValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
            };
        }
    }
}
