using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchSharingService
    {
        private readonly double requiredResearchProgress;

        internal ResearchSharingService(double requiredResearchProgress)
        {
            this.requiredResearchProgress = requiredResearchProgress;
        }

        internal ResearchSharingResult SyncPlayerAndFaction(
            ResearchStore store,
            ResearchService researchService,
            SchematicCatalog schematicCatalog,
            string playerScopeId,
            string factionScopeId)
        {
            var result = new ResearchSharingResult
            {
                UploadedToFaction = CopyResearchSchematics(store, researchService, schematicCatalog, store.PlayerScopes, playerScopeId, store.FactionScopes, factionScopeId),
                DownloadedToPlayer = CopyResearchSchematics(store, researchService, schematicCatalog, store.FactionScopes, factionScopeId, store.PlayerScopes, playerScopeId),
            };

            if (result.TotalChanges > 0)
                store.MarkDirty();

            return result;
        }

        private ResearchSyncResult CopyResearchSchematics(
            ResearchStore store,
            ResearchService researchService,
            SchematicCatalog schematicCatalog,
            List<ResearchScopeRecord> sourceScopes,
            string sourceScopeId,
            List<ResearchScopeRecord> targetScopes,
            string targetScopeId)
        {
            var result = new ResearchSyncResult();
            var sourceScope = store.FindScope(sourceScopes, sourceScopeId);
            if (sourceScope == null || sourceScope.Schematics == null)
                return result;

            foreach (var schematic in sourceScope.Schematics)
                result.Add(CopyResearchSchematic(store, researchService, schematicCatalog, targetScopes, targetScopeId, schematic));

            return result;
        }

        private ResearchSyncResult CopyResearchSchematic(
            ResearchStore store,
            ResearchService researchService,
            SchematicCatalog schematicCatalog,
            List<ResearchScopeRecord> targetScopes,
            string targetScopeId,
            ResearchSchematicRecord source)
        {
            var result = new ResearchSyncResult();
            if (source == null || string.IsNullOrWhiteSpace(source.ResearchId))
                return result;

            if (source.Unlocked || source.Progress >= requiredResearchProgress)
            {
                result.Add(CopyCompletedSchematic(store, researchService, schematicCatalog, targetScopes, targetScopeId, source));
                return result;
            }

            var contributions = GetSchematicContributions(source);
            foreach (var contribution in contributions)
            {
                var progress = ApplySyncedResearchContribution(store, targetScopes, targetScopeId, source.ResearchId, GetStoredUnlockerSubtype(schematicCatalog, source), contribution.Token, contribution.Progress);
                result.Add(progress);
            }

            return result;
        }

        private ResearchProgressResult CopyCompletedSchematic(
            ResearchStore store,
            ResearchService researchService,
            SchematicCatalog schematicCatalog,
            List<ResearchScopeRecord> targetScopes,
            string targetScopeId,
            ResearchSchematicRecord source)
        {
            if (source == null || (!source.Unlocked && source.Progress < requiredResearchProgress) || string.IsNullOrWhiteSpace(source.ResearchId))
                return ResearchProgressResult.NoChange;

            var unlockerSubtype = GetStoredUnlockerSubtype(schematicCatalog, source);
            return researchService.RecordProgress(store, targetScopes, targetScopeId, source.ResearchId, unlockerSubtype, requiredResearchProgress);
        }

        private List<ResearchContributionEntry> GetSchematicContributions(ResearchSchematicRecord schematic)
        {
            var contributionsByToken = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            if (schematic == null)
                return new List<ResearchContributionEntry>();

            if (schematic.Ledger != null)
            {
                foreach (var entry in schematic.Ledger)
                {
                    if (entry != null)
                        AddContributionProgress(contributionsByToken, entry.Token, entry.Progress);
                }
            }

            if (!string.IsNullOrWhiteSpace(schematic.ActiveToken) && schematic.ActiveProgress > 0.0)
                AddContributionProgress(contributionsByToken, schematic.ActiveToken, schematic.ActiveProgress);

            var contributions = new List<ResearchContributionEntry>();
            foreach (var entry in contributionsByToken)
            {
                contributions.Add(new ResearchContributionEntry
                {
                    Token = entry.Key,
                    Progress = Clamp01(entry.Value),
                });
            }

            return contributions;
        }

        private static void AddContributionProgress(Dictionary<string, double> contributionsByToken, string token, double progress)
        {
            if (string.IsNullOrWhiteSpace(token) || progress <= 0.0)
                return;

            progress = Clamp01(progress);
            double existingProgress;
            if (contributionsByToken.TryGetValue(token, out existingProgress))
            {
                if (progress > existingProgress)
                    contributionsByToken[token] = progress;
            }
            else
            {
                contributionsByToken.Add(token, progress);
            }
        }

        private ResearchProgressResult ApplySyncedResearchContribution(ResearchStore store, List<ResearchScopeRecord> targetScopes, string targetScopeId, string researchId, string unlockerSubtype, string token, double sourceProgress)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(researchId) || sourceProgress <= 0.0)
                return ResearchProgressResult.NoChange;

            var scope = store.GetOrCreateScope(targetScopes, targetScopeId);
            var schematic = store.GetOrCreateSchematic(scope, researchId);
            if (schematic.Unlocked)
                return ResearchProgressResult.NoChange;

            var result = new ResearchProgressResult();
            var targetContributionProgress = GetContributionProgress(schematic, token);
            var delta = Clamp01(sourceProgress) - targetContributionProgress;
            if (delta > 0.0)
            {
                var acceptedProgress = Math.Min(delta, requiredResearchProgress - schematic.Progress);
                if (acceptedProgress > 0.0)
                {
                    schematic.Progress += acceptedProgress;
                    SetContributionProgress(schematic, token, targetContributionProgress + acceptedProgress);
                    result.AddedProgress = acceptedProgress;
                    result.Changed = true;
                }
            }

            if (schematic.Progress >= requiredResearchProgress)
            {
                schematic.Progress = requiredResearchProgress;
                schematic.Unlocked = true;
                schematic.ActiveToken = null;
                schematic.ActiveProgress = 0.0;
                schematic.Ledger.Clear();
                result.Changed = true;
                result.Unlocked = true;
            }

            if (!string.Equals(schematic.UnlockerSubtype, unlockerSubtype, StringComparison.OrdinalIgnoreCase))
            {
                schematic.UnlockerSubtype = unlockerSubtype;
                result.Changed = true;
            }

            if (result.Changed)
                store.MarkDirty();

            result.TotalProgress = schematic.Progress;
            return result;
        }

        private static double GetContributionProgress(ResearchSchematicRecord schematic, string token)
        {
            if (schematic == null || string.IsNullOrWhiteSpace(token))
                return 0.0;

            if (string.Equals(schematic.ActiveToken, token, StringComparison.OrdinalIgnoreCase))
                return Clamp01(schematic.ActiveProgress);

            if (schematic.Ledger != null)
            {
                foreach (var entry in schematic.Ledger)
                {
                    if (entry != null && string.Equals(entry.Token, token, StringComparison.OrdinalIgnoreCase))
                        return Clamp01(entry.Progress);
                }
            }

            return 0.0;
        }

        private static void SetContributionProgress(ResearchSchematicRecord schematic, string token, double progress)
        {
            if (schematic == null || string.IsNullOrWhiteSpace(token))
                return;

            progress = Clamp01(progress);
            if (string.Equals(schematic.ActiveToken, token, StringComparison.OrdinalIgnoreCase))
            {
                schematic.ActiveProgress = progress;
                return;
            }

            if (schematic.Ledger == null)
                schematic.Ledger = new List<ResearchLedgerEntry>();

            foreach (var entry in schematic.Ledger)
            {
                if (entry != null && string.Equals(entry.Token, token, StringComparison.OrdinalIgnoreCase))
                {
                    entry.Progress = progress;
                    return;
                }
            }

            schematic.Ledger.Add(new ResearchLedgerEntry
            {
                Token = token,
                Progress = progress,
            });
        }

        private static string GetStoredUnlockerSubtype(SchematicCatalog schematicCatalog, ResearchSchematicRecord schematic)
        {
            if (schematic == null)
                return null;

            if (!string.IsNullOrWhiteSpace(schematic.UnlockerSubtype) && !string.Equals(schematic.UnlockerSubtype, schematic.ResearchId, StringComparison.OrdinalIgnoreCase))
                return schematic.UnlockerSubtype;

            ResearchUnlockTarget target;
            if (schematicCatalog.TryGetTargetByResearchId(schematic.ResearchId, out target))
                return target.UnlockerId.SubtypeName;

            return schematic.UnlockerSubtype;
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;
            if (value > 1.0)
                return 1.0;
            return value;
        }
    }
}
