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
        private void PopulatePlayerResearchList(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> items, List<MyTerminalControlListBoxItem> selected)
        {
            ClearResearchList(items, selected);
            if (!IsResearchPedestal(block) || items == null)
                return;

            var identityId = ResolveTerminalIdentityId(block);
            var playerScope = FindScope(researchStore.PlayerScopes, identityId.ToString());
            PopulateResearchScopeList(items, playerScope, GetResearchPedestalViewForBlock(block), "No matching player research.");
        }

        private void PopulateFactionResearchList(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> items, List<MyTerminalControlListBoxItem> selected)
        {
            ClearResearchList(items, selected);
            if (!IsResearchPedestal(block) || items == null)
                return;

            var identityId = ResolveTerminalIdentityId(block);
            IMyFaction faction;
            if (!TryGetPlayerFaction(identityId, out faction))
            {
                AddResearchListMessage(items, "No faction archive available.");
                return;
            }

            var factionScope = FindScope(researchStore.FactionScopes, faction.FactionId.ToString());
            PopulateResearchScopeList(items, factionScope, GetResearchPedestalViewForBlock(block), "No matching faction research.");
        }

        private static void ClearResearchList(List<MyTerminalControlListBoxItem> items, List<MyTerminalControlListBoxItem> selected)
        {
            if (items != null)
                items.Clear();

            if (selected != null)
                selected.Clear();
        }

        private void PopulateResearchScopeList(List<MyTerminalControlListBoxItem> items, ResearchScopeRecord scope, ResearchPedestalView view, string emptyMessage)
        {
            var entries = BuildResearchListEntries(scope, view);
            if (entries.Count == 0)
            {
                AddResearchListMessage(items, emptyMessage);
                return;
            }

            foreach (var schematic in entries)
            {
                var text = FormatTerminalProgress(schematic.Progress) + "  " + schematic.DisplayName;
                var tooltip = schematic.DisplayName + "\n" + schematic.ResearchId + "\n" + FormatProgress(schematic.Progress) + (schematic.Unlocked ? " unlocked" : " researched");
                items.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(text), MyStringId.GetOrCompute(tooltip), schematic.ResearchId));
            }
        }

        private List<ResearchListEntry> BuildResearchListEntries(ResearchScopeRecord scope, ResearchPedestalView view)
        {
            return researchTerminalService.BuildListEntries(scope, view, schematicCatalog.Targets, IsResearchVisibleToLocalPlayer, GetSchematicDisplayName);
        }

        private bool IsResearchVisibleToLocalPlayer(string researchId)
        {
            if (string.IsNullOrWhiteSpace(researchId) ||
                MyAPIGateway.DLC == null ||
                MyAPIGateway.Session == null ||
                MyAPIGateway.Session.Player == null ||
                MyAPIGateway.Session.Player.SteamUserId == 0)
                return true;

            List<MyDefinitionId> blockIds;
            if (!schematicCatalog.TryGetBlockIds(researchId, out blockIds) || blockIds == null || blockIds.Count == 0)
                return true;

            var steamId = MyAPIGateway.Session.Player.SteamUserId;
            foreach (var blockId in blockIds)
            {
                if (MyAPIGateway.DLC.HasDefinitionDLC(blockId, steamId))
                    return true;
            }

            return false;
        }

        private static void AddResearchListMessage(List<MyTerminalControlListBoxItem> items, string message)
        {
            if (items == null)
                return;

            items.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(message), MyStringId.GetOrCompute(message), null));
        }

        private static string FormatTerminalProgress(double progress)
        {
            return progress >= RequiredResearchProgress ? "100%" : Math.Round(Clamp01(progress) * 100.0, 0).ToString("0", CultureInfo.InvariantCulture) + "%";
        }

    }
}
