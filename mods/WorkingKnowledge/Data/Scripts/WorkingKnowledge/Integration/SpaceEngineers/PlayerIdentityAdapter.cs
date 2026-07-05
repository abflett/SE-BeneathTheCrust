using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal sealed class PlayerIdentityAdapter
    {
        internal long ResolveIdentityId(long playerId)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player == null)
                    continue;

                if (player.IdentityId == playerId || (long)player.SteamUserId == playerId)
                    return player.IdentityId;
            }

            return playerId;
        }

        internal long ResolveIdentityId(ulong sender)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player == null)
                    continue;

                if (player.SteamUserId == sender)
                    return player.IdentityId;
            }

            return sender <= long.MaxValue ? (long)sender : 0;
        }

        internal long ResolveIdentityId(IMyCharacter character)
        {
            if (character == null)
                return 0;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player == null || player.Character == null)
                    continue;

                if (player.Character == character)
                    return player.IdentityId;
            }

            return 0;
        }

        internal IMyPlayer FindPlayerByIdentity(long identityId)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player != null && player.IdentityId == identityId)
                    return player;
            }

            return null;
        }

        internal bool TryGetPlayerFaction(long identityId, out IMyFaction faction)
        {
            faction = MyAPIGateway.Session != null && MyAPIGateway.Session.Factions != null
                ? MyAPIGateway.Session.Factions.TryGetPlayerFaction(identityId)
                : null;

            return faction != null;
        }

        internal long ResolveShipToolOperator(IMyCubeBlock toolBlock)
        {
            if (toolBlock == null || toolBlock.CubeGrid == null)
                return 0;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (var player in players)
            {
                if (player == null || player.Controller == null || player.Controller.ControlledEntity == null)
                    continue;

                var controller = player.Controller.ControlledEntity.Entity as IMyShipController;
                if (controller == null || controller.CubeGrid == null)
                    continue;

                if (controller.CubeGrid.EntityId == toolBlock.CubeGrid.EntityId && controller.IsUnderControl)
                    return player.IdentityId;
            }

            return toolBlock.OwnerId;
        }
    }
}
