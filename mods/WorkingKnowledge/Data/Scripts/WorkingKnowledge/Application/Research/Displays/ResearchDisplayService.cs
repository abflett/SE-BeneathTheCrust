using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchDisplayService
    {
        private readonly double requiredProgress;

        internal ResearchDisplayService(double requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        internal ResearchDisplaySnapshot BuildSnapshot(
            long updatedTick,
            ResearchDisplaySyncMessage syncData,
            ResearchDisplayScopeRecord localPlayerScope,
            ResearchDisplayScopeRecord localFactionScope,
            Func<string, bool> isVisible,
            Func<string, string> getDisplayName)
        {
            var snapshot = new ResearchDisplaySnapshot
            {
                UpdatedTick = updatedTick,
                Scopes = new List<ResearchDisplayScopeSnapshot>(),
            };

            if (syncData == null || syncData.Scopes == null || syncData.Scopes.Count == 0)
            {
                if (localPlayerScope != null)
                    snapshot.Scopes.Add(BuildScopeSnapshot(localPlayerScope, isVisible, getDisplayName));

                if (localFactionScope != null)
                    snapshot.Scopes.Add(BuildScopeSnapshot(localFactionScope, isVisible, getDisplayName));
            }
            else
            {
                foreach (var scope in syncData.Scopes)
                {
                    if (scope != null)
                        snapshot.Scopes.Add(BuildScopeSnapshot(scope, isVisible, getDisplayName));
                }
            }

            if (snapshot.Scopes.Count == 0)
                snapshot.Message = "No research archive available.";

            return snapshot;
        }

        internal ResearchDisplaySyncMessage CreateSyncMessage(
            string responseKind,
            long identityId,
            string playerName,
            ResearchScopeRecord playerScope,
            string factionName,
            string factionTag,
            ResearchScopeRecord factionScope)
        {
            var message = new ResearchDisplaySyncMessage
            {
                Kind = responseKind,
                IdentityId = identityId,
                Scopes = new List<ResearchDisplayScopeRecord>(),
            };

            message.Scopes.Add(CreatePlayerScope(playerName, playerScope));
            if (factionScope != null)
                message.Scopes.Add(CreateFactionScope(factionName, factionTag, factionScope));

            return message;
        }

        internal ResearchDisplayScopeRecord CreatePlayerScope(string playerName, ResearchScopeRecord sourceScope)
        {
            return CreateScope("PLAYER ARCHIVE", string.IsNullOrWhiteSpace(playerName) ? "Local Player" : playerName, null, false, sourceScope);
        }

        internal ResearchDisplayScopeRecord CreateFactionScope(string factionName, string factionTag, ResearchScopeRecord sourceScope)
        {
            var ownerName = !string.IsNullOrWhiteSpace(factionName) ? factionName : (!string.IsNullOrWhiteSpace(factionTag) ? factionTag : "Faction");
            return CreateScope("FACTION ARCHIVE", ownerName, factionTag, true, sourceScope);
        }

        internal void NormalizeSyncMessage(ResearchDisplaySyncMessage message)
        {
            if (message == null)
                return;

            if (message.Scopes == null)
                message.Scopes = new List<ResearchDisplayScopeRecord>();

            foreach (var scope in message.Scopes)
            {
                if (scope == null)
                    continue;

                if (scope.Schematics == null)
                    scope.Schematics = new List<ResearchDisplaySchematicRecord>();

                foreach (var schematic in scope.Schematics)
                {
                    if (schematic != null)
                        schematic.Progress = RatioMath.Clamp01(schematic.Progress);
                }
            }
        }

        private ResearchDisplayScopeSnapshot BuildScopeSnapshot(
            ResearchDisplayScopeRecord scope,
            Func<string, bool> isVisible,
            Func<string, string> getDisplayName)
        {
            var snapshot = new ResearchDisplayScopeSnapshot
            {
                Header = string.IsNullOrWhiteSpace(scope.Header) ? "RESEARCH ARCHIVE" : scope.Header,
                OwnerName = string.IsNullOrWhiteSpace(scope.OwnerName) ? "Unknown" : scope.OwnerName,
                FactionTag = scope.FactionTag,
                IsFaction = scope.IsFaction,
                Entries = new List<ResearchDisplayEntry>(),
            };

            if (scope.Schematics == null)
                return snapshot;

            foreach (var schematic in scope.Schematics)
            {
                if (schematic == null || string.IsNullOrWhiteSpace(schematic.ResearchId))
                    continue;

                if (!IsVisible(isVisible, schematic.ResearchId))
                    continue;

                var progress = schematic.Unlocked ? requiredProgress : RatioMath.Clamp01(schematic.Progress);
                snapshot.TrackedCount++;
                if (schematic.Unlocked || progress >= requiredProgress)
                    snapshot.CompletedCount++;
                else if (progress > 0.0)
                    snapshot.ActiveCount++;

                if (progress <= 0.0)
                    continue;

                snapshot.Entries.Add(new ResearchDisplayEntry
                {
                    ResearchId = schematic.ResearchId,
                    DisplayName = GetDisplayName(getDisplayName, schematic.ResearchId),
                    Progress = progress,
                    Unlocked = schematic.Unlocked || progress >= requiredProgress,
                });
            }

            snapshot.Entries.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
            return snapshot;
        }

        private ResearchDisplayScopeRecord CreateScope(string header, string ownerName, string factionTag, bool isFaction, ResearchScopeRecord sourceScope)
        {
            return new ResearchDisplayScopeRecord
            {
                ScopeId = sourceScope != null ? sourceScope.Id : null,
                Header = header,
                OwnerName = ownerName,
                FactionTag = factionTag,
                IsFaction = isFaction,
                Schematics = CopySchematics(sourceScope),
            };
        }

        private List<ResearchDisplaySchematicRecord> CopySchematics(ResearchScopeRecord scope)
        {
            var records = new List<ResearchDisplaySchematicRecord>();
            if (scope == null || scope.Schematics == null)
                return records;

            foreach (var schematic in scope.Schematics)
            {
                if (schematic == null || string.IsNullOrWhiteSpace(schematic.ResearchId))
                    continue;

                records.Add(new ResearchDisplaySchematicRecord
                {
                    ResearchId = schematic.ResearchId,
                    Progress = schematic.Unlocked ? requiredProgress : schematic.Progress,
                    Unlocked = schematic.Unlocked,
                });
            }

            return records;
        }

        private static bool IsVisible(Func<string, bool> isVisible, string researchId)
        {
            return isVisible == null || isVisible(researchId);
        }

        private static string GetDisplayName(Func<string, string> getDisplayName, string researchId)
        {
            return getDisplayName == null ? researchId : getDisplayName(researchId);
        }
    }
}
