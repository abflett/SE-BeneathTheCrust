using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ProficiencyStore
    {
        private ProficiencySaveData data = new ProficiencySaveData();

        internal ProficiencySaveData Data
        {
            get { return data; }
        }

        internal List<ProficiencyScopeRecord> PlayerScopes
        {
            get { return data.Players; }
        }

        internal bool IsDirty { get; private set; }

        internal void Reset()
        {
            data = new ProficiencySaveData();
            IsDirty = false;
        }

        internal void SetData(ProficiencySaveData loaded)
        {
            data = loaded ?? new ProficiencySaveData();
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

        internal void Normalize()
        {
            if (data == null)
                data = new ProficiencySaveData();

            if (data.Players == null)
                data.Players = new List<ProficiencyScopeRecord>();

            foreach (var scope in data.Players)
                NormalizeScope(scope);
        }

        internal ProficiencyScopeRecord GetOrCreateScope(string playerId)
        {
            foreach (var scope in data.Players)
            {
                if (string.Equals(scope.Id, playerId, StringComparison.OrdinalIgnoreCase))
                    return scope;
            }

            var newScope = new ProficiencyScopeRecord
            {
                Id = playerId,
                Proficiencies = new List<ProficiencyRecord>(),
            };
            data.Players.Add(newScope);
            return newScope;
        }

        internal ProficiencyScopeRecord FindScope(string playerId)
        {
            if (data == null || data.Players == null)
                return null;

            foreach (var scope in data.Players)
            {
                if (string.Equals(scope.Id, playerId, StringComparison.OrdinalIgnoreCase))
                    return scope;
            }

            return null;
        }

        internal ProficiencyRecord GetOrCreateProficiency(ProficiencyScopeRecord scope, string proficiencyId, long currentTick)
        {
            foreach (var proficiency in scope.Proficiencies)
            {
                if (string.Equals(proficiency.ProficiencyId, proficiencyId, StringComparison.OrdinalIgnoreCase))
                    return proficiency;
            }

            var newProficiency = new ProficiencyRecord
            {
                ProficiencyId = proficiencyId,
                Progress = 0.0,
                LastTouchedTick = currentTick,
            };
            scope.Proficiencies.Add(newProficiency);
            return newProficiency;
        }

        internal void NormalizeScope(ProficiencyScopeRecord scope)
        {
            if (scope == null)
                return;

            if (scope.Proficiencies == null)
                scope.Proficiencies = new List<ProficiencyRecord>();

            foreach (var proficiency in scope.Proficiencies)
            {
                if (proficiency != null)
                    proficiency.Progress = Clamp01(proficiency.Progress);
            }
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
