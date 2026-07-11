using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRageMath;

namespace Worldwright
{
    internal enum ReclamationSequenceMode
    {
        Once = 0,
        Loop = 1,
        Random = 2,
    }

    internal sealed class ReclamationSpawnerConfig
    {
        internal const float DefaultVelocity = 1.5f;
        internal const float DefaultAutomaticIntervalSeconds = 3f;

        internal readonly List<string> Entries = new List<string>();
        internal readonly List<ReclamationAppearancePreset> AppearancePresets =
            new List<ReclamationAppearancePreset>();

        internal ReclamationSequenceMode Mode = ReclamationSequenceMode.Once;
        internal float OutwardVelocity = DefaultVelocity;
        internal float AutomaticIntervalSeconds = DefaultAutomaticIntervalSeconds;
        internal float RotationVariance;
        internal float MinimumIntegrity = 100f;
        internal float MaximumIntegrity = 100f;
        internal int Cursor;
        internal bool Completed;

        internal void ResetSequence()
        {
            Cursor = 0;
            Completed = false;
        }

        internal void Normalize()
        {
            OutwardVelocity = Math.Max(0f, Math.Min(WorldwrightSession.MaximumOutwardVelocity, OutwardVelocity));
            AutomaticIntervalSeconds = Math.Max(
                WorldwrightSession.MinimumAutomaticIntervalSeconds,
                Math.Min(WorldwrightSession.MaximumAutomaticIntervalSeconds, AutomaticIntervalSeconds));
            RotationVariance = Math.Max(0f, Math.Min(100f, RotationVariance));
            MinimumIntegrity = Math.Max(10f, Math.Min(100f, MinimumIntegrity));
            MaximumIntegrity = Math.Max(10f, Math.Min(100f, MaximumIntegrity));
            if (MinimumIntegrity > MaximumIntegrity)
                MaximumIntegrity = MinimumIntegrity;

            if (Entries.Count == 0)
            {
                Cursor = 0;
                Completed = false;
                return;
            }

            if (Mode == ReclamationSequenceMode.Once)
            {
                Cursor = Math.Max(0, Math.Min(Entries.Count, Cursor));
                Completed = Completed || Cursor >= Entries.Count;
                return;
            }

            Cursor = Math.Max(0, Cursor) % Entries.Count;
            Completed = false;
        }
    }

    internal sealed class ReclamationAppearancePreset
    {
        internal Vector3 ColorMaskHsv;
        internal string SkinSubtypeId;
    }

    internal sealed class ReclamationBlockCatalogEntry
    {
        internal string Key;
        internal string DisplayName;
        internal MyCubeBlockDefinition Definition;

        internal string SearchText
        {
            get { return (DisplayName + " " + Key).ToLowerInvariant(); }
        }
    }

    internal sealed class PendingReclamationSpawn
    {
        internal long BlockEntityId;
        internal string DefinitionKey;
        internal int SequenceIndex;
        internal Vector3D Forward;
        internal Vector3D Up;
        internal ReclamationAppearancePreset Appearance;
        internal float IntegrityPercent;
    }

    internal sealed class RunningReclamationSpawner
    {
        internal long BlockEntityId;
        internal int NextSpawnFrame;
    }

    public sealed class ReclamationSpawnerNetworkMessage
    {
        public string Kind { get; set; }
        public string Operation { get; set; }
        public long BlockEntityId { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }
        public float Number { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
