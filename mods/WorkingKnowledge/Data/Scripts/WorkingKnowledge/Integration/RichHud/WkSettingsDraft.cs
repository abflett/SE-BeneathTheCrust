using System;
using System.Collections.Generic;

namespace WkKn
{
    // Window-local edits only. The existing command path remains authoritative on Apply.
    internal sealed class WkSettingsDraft
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action> actions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> order = new List<string>();

        internal int Count { get { return order.Count; } }

        internal string GetValue(string key, Func<string> current)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : current();
        }

        internal void Stage(string key, string value, Action apply)
        {
            if (!actions.ContainsKey(key))
                order.Add(key);
            values[key] = value;
            actions[key] = apply;
        }

        internal void Clear()
        {
            values.Clear();
            actions.Clear();
            order.Clear();
        }

        internal void Apply()
        {
            var pending = new List<Action>(order.Count);
            for (var i = 0; i < order.Count; i++)
                pending.Add(actions[order[i]]);
            Clear();
            for (var i = 0; i < pending.Count; i++)
                pending[i]();
        }
    }
}
