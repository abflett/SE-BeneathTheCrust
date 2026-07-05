using System.Collections.Generic;
using Sandbox.Game;
using VRage;
using VRage.Game;
using VRage.Game.ObjectBuilders;

namespace WkKn
{
    internal static class ComponentInventory
    {
        internal static Dictionary<MyDefinitionId, MyFixedPoint> Snapshot(MyInventory inventory)
        {
            var snapshot = new Dictionary<MyDefinitionId, MyFixedPoint>(MyDefinitionId.Comparer);
            if (inventory == null)
                return snapshot;

            foreach (var item in inventory.GetItems())
            {
                if (item.Content == null || item.Content.TypeId != typeof(MyObjectBuilder_Component))
                    continue;

                var id = new MyDefinitionId(item.Content.TypeId, item.Content.SubtypeId);
                MyFixedPoint amount;
                if (snapshot.TryGetValue(id, out amount))
                    snapshot[id] = amount + item.Amount;
                else
                    snapshot.Add(id, item.Amount);
            }

            return snapshot;
        }

    }
}
