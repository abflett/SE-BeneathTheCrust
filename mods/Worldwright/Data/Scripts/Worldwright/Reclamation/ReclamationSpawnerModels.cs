using System;
using System.Collections.Generic;
using Sandbox.Definitions;

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

        internal readonly List<string> Entries = new List<string>();
        internal ReclamationSequenceMode Mode = ReclamationSequenceMode.Once;
        internal float OutwardVelocity = DefaultVelocity;
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
