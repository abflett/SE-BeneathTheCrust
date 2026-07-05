using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchStore
    {
        private ResearchSaveData data = new ResearchSaveData();

        internal ResearchSaveData Data
        {
            get { return data; }
        }

        internal List<ResearchScopeRecord> PlayerScopes
        {
            get { return data.Players; }
        }

        internal List<ResearchScopeRecord> FactionScopes
        {
            get { return data.Factions; }
        }

        internal bool IsDirty { get; private set; }

        internal void Reset()
        {
            data = new ResearchSaveData();
            IsDirty = false;
        }

        internal void SetData(ResearchSaveData loaded)
        {
            data = loaded ?? new ResearchSaveData();
            IsDirty = false;
        }

        internal void MarkDirty()
        {
            IsDirty = true;
        }

        internal void MarkClean()
        {
            IsDirty = false;
        }

        internal void Normalize(double requiredResearchProgress)
        {
            if (data == null)
                data = new ResearchSaveData();

            if (data.Players == null)
                data.Players = new List<ResearchScopeRecord>();

            if (data.Factions == null)
                data.Factions = new List<ResearchScopeRecord>();

            NormalizeScopes(data.Players, requiredResearchProgress);
            NormalizeScopes(data.Factions, requiredResearchProgress);
        }

        internal ResearchScopeRecord GetOrCreateScope(List<ResearchScopeRecord> scopes, string scopeId)
        {
            var scope = FindScope(scopes, scopeId);
            if (scope != null)
                return scope;

            scope = new ResearchScopeRecord
            {
                Id = scopeId,
                Schematics = new List<ResearchSchematicRecord>(),
            };
            scopes.Add(scope);
            return scope;
        }

        internal ResearchScopeRecord FindScope(List<ResearchScopeRecord> scopes, string scopeId)
        {
            if (scopes == null)
                return null;

            foreach (var scope in scopes)
            {
                if (string.Equals(scope.Id, scopeId, StringComparison.OrdinalIgnoreCase))
                    return scope;
            }

            return null;
        }

        internal ResearchSchematicRecord GetOrCreateSchematic(ResearchScopeRecord scope, string researchId)
        {
            foreach (var schematic in scope.Schematics)
            {
                if (string.Equals(schematic.ResearchId, researchId, StringComparison.OrdinalIgnoreCase))
                    return schematic;
            }

            var newSchematic = new ResearchSchematicRecord
            {
                ResearchId = researchId,
                UnlockerSubtype = researchId,
                Progress = 0.0,
                ActiveProgress = 0.0,
                Unlocked = false,
                Ledger = new List<ResearchLedgerEntry>(),
            };
            scope.Schematics.Add(newSchematic);
            return newSchematic;
        }

        private static void NormalizeScopes(List<ResearchScopeRecord> scopes, double requiredResearchProgress)
        {
            foreach (var scope in scopes)
            {
                if (scope.Schematics == null)
                    scope.Schematics = new List<ResearchSchematicRecord>();

                foreach (var schematic in scope.Schematics)
                {
                    if (schematic.Ledger == null)
                        schematic.Ledger = new List<ResearchLedgerEntry>();

                    schematic.Progress = Clamp01(schematic.Progress);
                    schematic.ActiveProgress = Clamp(schematic.ActiveProgress, 0.0, schematic.Progress);

                    if (schematic.Progress >= requiredResearchProgress)
                        schematic.Unlocked = true;

                    if (schematic.Unlocked)
                    {
                        schematic.Progress = requiredResearchProgress;
                        schematic.ActiveToken = null;
                        schematic.ActiveProgress = 0.0;
                        schematic.Ledger.Clear();
                    }
                }
            }
        }

        private static double Clamp01(double value)
        {
            return Clamp(value, 0.0, 1.0);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}
