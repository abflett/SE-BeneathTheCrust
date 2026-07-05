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
        private void SyncResearch(IMyTerminalBlock block)
        {
            if (block == null)
                return;

            if (MyAPIGateway.Multiplayer != null && !MyAPIGateway.Multiplayer.IsServer)
            {
                SendResearchTerminalSyncRequest(block);
                return;
            }

            string responseText;
            ExecuteResearchTerminalSync(block, ResolveTerminalIdentityId(block), true, out responseText);
        }

        private bool ExecuteResearchTerminalSync(IMyTerminalBlock block, long identityId, bool showLocalMessages, out string responseText)
        {
            responseText = null;

            var validation = researchTerminalAdapter.ValidateSync(block, identityId);
            if (!validation.IsValid)
            {
                responseText = validation.ErrorMessage;
                if (showLocalMessages)
                    ShowWkChatSection("Research Sync", responseText);

                return false;
            }

            var faction = validation.Faction;
            var playerScopeId = identityId.ToString();
            var factionScopeId = faction.FactionId.ToString();
            var sync = researchSharingService.SyncPlayerAndFaction(researchStore, researchService, schematicCatalog, playerScopeId, factionScopeId);

            if (sync.DownloadedToPlayer.CompletedSchematics > 0)
                SyncCompletedResearchForIdentity(identityId);

            var changed = sync.TotalChanges;
            if (changed > 0)
            {
                responseText = "Synced research. Faction " + FormatSyncResult(sync.UploadedToFaction) + ", player " + FormatSyncResult(sync.DownloadedToPlayer) + ".";
                FinishTerminalSync(block, true, responseText, showLocalMessages);
                NotifyResearchDisplayChanged(identityId);
                NotifyFactionResearchDisplayChanged(faction.FactionId);
                return true;
            }
            else
            {
                responseText = "No research progress to sync.";
                FinishTerminalSync(block, false, responseText, showLocalMessages);
                return false;
            }
        }

        private static string FormatSyncResult(ResearchSyncResult result)
        {
            return "+" + FormatProgressDelta(result.AddedProgress) + " (" + result.ChangedSchematics + " updates)";
        }

        private void FinishTerminalSync(IMyTerminalBlock block, bool changed, string message, bool showLocalMessages)
        {
            if (changed)
                SaveResearchStore();

            if (showLocalMessages)
                ShowWkChatSection("Research Sync", message);

            RequestResearchPedestalRefresh(block);
        }

        private long ResolveTerminalIdentityId(IMyTerminalBlock block)
        {
            return researchTerminalAdapter.ResolveIdentityId(block);
        }

        private bool TryGetPlayerFaction(long identityId, out IMyFaction faction)
        {
            return playerIdentityAdapter.TryGetPlayerFaction(identityId, out faction);
        }


    }
}
