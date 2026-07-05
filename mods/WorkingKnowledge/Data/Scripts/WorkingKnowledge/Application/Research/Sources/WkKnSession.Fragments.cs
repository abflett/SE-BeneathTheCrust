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
        private void OnPlayerConnected(long playerId)
        {
            var identityId = ResolveIdentityId(playerId);
            if (identityId == 0)
                return;

            ApplyFundamentalsDefaultsForIdentity(identityId);
            SyncCompletedResearchForIdentity(identityId);
            SendResearchDisplayResponseToIdentity(identityId);
            SendProficiencyDisplayResponseToIdentity(identityId);
        }

        private void OnItemConsumed(IMyCharacter character, MyDefinitionId consumedItem)
        {
            if (!MyAPIGateway.Session.IsServer || character == null)
                return;

            ResearchFragmentTier tier;
            string fragmentSubtype;
            if (TryGetResearchFragmentTier(consumedItem, out tier, out fragmentSubtype))
            {
                OnDataFragmentConsumed(character, tier, fragmentSubtype);
                return;
            }

            string schematicSubtype;
            if (TryGetResearchSchematicSubtype(consumedItem, out schematicSubtype))
                OnResearchSchematicConsumed(character, schematicSubtype);
        }

        private void OnDataFragmentConsumed(IMyCharacter character, ResearchFragmentTier tier, string fragmentSubtype)
        {
            if (!config.DataFragmentsEnabled)
            {
                RefundConsumable(character, fragmentSubtype);
                MyVisualScriptLogicProvider.ShowNotification("Data fragments are disabled by world settings.", 3500, MyFontEnum.Red);
                return;
            }

            var identityId = ResolveIdentityId(character);
            if (identityId == 0)
                return;

            ResearchFragmentReward reward;
            if (!dataFragmentResearchSource.TrySelectReward(schematicCatalog, tier, random, config.DataFragmentRewardScale, out reward))
            {
                MyVisualScriptLogicProvider.ShowNotification("No compatible schematic data found.", 3500, MyFontEnum.Red);
                return;
            }

            if (GetPlayerResearchProgress(identityId, reward.Target.ResearchId) >= RequiredResearchProgress)
            {
                MyVisualScriptLogicProvider.ShowNotification("Duplicate schematic data recovered.", 3500, MyFontEnum.Red);
                return;
            }

            AwardResearchProgress(identityId, reward.Target, reward.Progress, fragmentSubtype);
        }

        private void OnResearchSchematicConsumed(IMyCharacter character, string schematicSubtype)
        {
            RefundConsumable(character, schematicSubtype);

            if (!config.DataFragmentsEnabled)
            {
                MyVisualScriptLogicProvider.ShowNotification("Research data items are disabled by world settings.", 3500, MyFontEnum.Red);
                return;
            }

            var identityId = ResolveIdentityId(character);
            if (identityId == 0)
                return;

            ResearchUnlockTarget target;
            if (!TryGetResearchSchematicTarget(schematicSubtype, out target))
            {
                MyVisualScriptLogicProvider.ShowNotification("Unknown schematic data.", 3500, MyFontEnum.Red);
                return;
            }

            if (GetPlayerResearchProgress(identityId, target.ResearchId) >= RequiredResearchProgress)
            {
                MyVisualScriptLogicProvider.ShowNotification(target.DisplayName + " already known.", 3500, MyFontEnum.Red);
                return;
            }

            AwardResearchProgress(identityId, target, RequiredResearchProgress, schematicSubtype);
        }

        private static bool TryGetResearchFragmentTier(MyDefinitionId consumedItem, out ResearchFragmentTier tier, out string fragmentSubtype)
        {
            if (consumedItem == CommonFragmentId)
            {
                tier = ResearchFragmentTier.Common;
                fragmentSubtype = CommonFragmentSubtype;
                return true;
            }

            if (consumedItem == UncommonFragmentId)
            {
                tier = ResearchFragmentTier.Uncommon;
                fragmentSubtype = UncommonFragmentSubtype;
                return true;
            }

            if (consumedItem == RareFragmentId)
            {
                tier = ResearchFragmentTier.Rare;
                fragmentSubtype = RareFragmentSubtype;
                return true;
            }

            if (consumedItem == PrototechFragmentId)
            {
                tier = ResearchFragmentTier.Prototech;
                fragmentSubtype = PrototechFragmentSubtype;
                return true;
            }

            tier = ResearchFragmentTier.Common;
            fragmentSubtype = null;
            return false;
        }

        private static bool TryGetResearchSchematicSubtype(MyDefinitionId consumedItem, out string schematicSubtype)
        {
            schematicSubtype = consumedItem.SubtypeName;
            return !string.IsNullOrWhiteSpace(schematicSubtype) &&
                   schematicSubtype.StartsWith(ResearchSchematicSubtypePrefix, StringComparison.OrdinalIgnoreCase);
        }

        private bool TryGetResearchSchematicTarget(string schematicSubtype, out ResearchUnlockTarget target)
        {
            foreach (var candidate in schematicCatalog.Targets)
            {
                if (string.Equals(GetResearchSchematicSubtype(candidate.ResearchId), schematicSubtype, StringComparison.OrdinalIgnoreCase))
                {
                    target = candidate;
                    return true;
                }
            }

            target = default(ResearchUnlockTarget);
            return false;
        }

        private static string GetResearchSchematicSubtype(string researchId)
        {
            return ResearchSchematicSubtypePrefix + GetSafeSubtypeToken(researchId);
        }

        private static string GetSafeSubtypeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            var builder = new StringBuilder();
            var lastWasSeparator = false;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                    lastWasSeparator = false;
                    continue;
                }

                if (builder.Length > 0 && !lastWasSeparator)
                {
                    builder.Append('_');
                    lastWasSeparator = true;
                }
            }

            while (builder.Length > 0 && builder[builder.Length - 1] == '_')
                builder.Length--;

            return builder.Length == 0 ? "unknown" : builder.ToString();
        }

        private static void RefundConsumable(IMyCharacter character, string subtype)
        {
            var inventory = character.GetInventory() as MyInventory;
            if (inventory == null || string.IsNullOrWhiteSpace(subtype))
                return;

            var item = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConsumableItem>(subtype);
            inventory.AddItems((MyFixedPoint)1, item);
        }

    }
}
