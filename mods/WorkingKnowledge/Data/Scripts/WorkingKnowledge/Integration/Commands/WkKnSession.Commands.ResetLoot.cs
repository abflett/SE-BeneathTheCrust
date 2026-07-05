using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const int RandomCargoResetMaxDisplayedContainerTypes = 8;

        private void HandleAdminResetLootCommand(string[] args)
        {
            if (args.Length != 3 && args.Length != 4)
            {
                ShowWkWarningMessage("Usage: /wk admin resetloot or /wk admin resetloot confirm");
                return;
            }

            var confirm = false;
            if (args.Length == 4)
            {
                confirm = args[3].Equals("confirm", StringComparison.OrdinalIgnoreCase) ||
                          args[3].Equals("apply", StringComparison.OrdinalIgnoreCase);

                if (!confirm)
                {
                    ShowWkWarningMessage("Use /wk admin resetloot confirm to refill empty random cargo containers.");
                    return;
                }
            }

            if (MyAPIGateway.Session != null && !MyAPIGateway.Session.IsServer)
            {
                ShowWkWarningMessage("Loot reset must be run by the host or server.");
                return;
            }

            var stats = ScanOrResetEmptyRandomCargoContainers(confirm);
            ShowWkChatSection(
                confirm ? "Loot Reset Complete" : "Loot Reset Scan",
                GetRandomCargoResetReportLines(stats, confirm));
        }

        private RandomCargoResetStats ScanOrResetEmptyRandomCargoContainers(bool refill)
        {
            var stats = new RandomCargoResetStats();
            if (MyAPIGateway.Entities == null)
                return stats;

            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            foreach (var entity in entities)
                ScanOrResetEmptyRandomCargoContainer(entity, refill, stats);

            return stats;
        }

        private void ScanOrResetEmptyRandomCargoContainer(IMyEntity entity, bool refill, RandomCargoResetStats stats)
        {
            if (entity == null || entity.Components == null)
                return;

            IMyRandomCargoEntityComponent randomCargo;
            if (!entity.Components.TryGet<IMyRandomCargoEntityComponent>(out randomCargo) || randomCargo == null)
                return;

            stats.RandomCargoEntities++;
            stats.AddContainerType(randomCargo.ContainerType);

            var inventory = entity.GetInventory();
            if (inventory == null)
            {
                stats.MissingInventory++;
                return;
            }

            if (!inventory.Empty())
            {
                stats.NonEmptySkipped++;
                return;
            }

            stats.EmptyEligible++;
            if (!refill)
                return;

            try
            {
                randomCargo.SpawnRandomCargo();
            }
            catch (Exception exception)
            {
                stats.Errors++;
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to reset random cargo container loot: " + exception);
                return;
            }

            if (inventory.Empty())
            {
                stats.EmptyAfterSpawn++;
                return;
            }

            stats.Refilled++;
        }

        private static IEnumerable<string> GetRandomCargoResetReportLines(RandomCargoResetStats stats, bool refill)
        {
            var lines = new List<string>();
            if (refill)
                lines.Add("Refilled empty random cargo containers only. Non-empty inventories were skipped.");
            else
                lines.Add("Dry run only. Run /wk admin resetloot confirm to refill empty random cargo containers.");

            lines.Add("Random cargo containers found: " + stats.RandomCargoEntities);
            lines.Add("Empty eligible: " + stats.EmptyEligible);
            if (refill)
            {
                lines.Add("Refilled: " + stats.Refilled);
                lines.Add("Still empty after spawn: " + stats.EmptyAfterSpawn);
            }

            lines.Add("Skipped non-empty: " + stats.NonEmptySkipped);
            lines.Add("Skipped without inventory: " + stats.MissingInventory);
            if (stats.Errors > 0)
                lines.Add("Errors: " + stats.Errors + " (see Space Engineers log)");

            AddRandomCargoResetContainerTypeLines(stats, lines);
            return lines;
        }

        private static void AddRandomCargoResetContainerTypeLines(RandomCargoResetStats stats, List<string> lines)
        {
            if (stats.ContainerTypes.Count == 0)
                return;

            var containerTypes = new List<KeyValuePair<string, int>>(stats.ContainerTypes);
            containerTypes.Sort(CompareRandomCargoResetContainerTypes);

            lines.Add("Container types:");
            var displayed = Math.Min(RandomCargoResetMaxDisplayedContainerTypes, containerTypes.Count);
            for (var i = 0; i < displayed; i++)
                lines.Add("- " + containerTypes[i].Key + ": " + containerTypes[i].Value);

            if (containerTypes.Count > displayed)
                lines.Add("- +" + (containerTypes.Count - displayed) + " more types");
        }

        private static int CompareRandomCargoResetContainerTypes(KeyValuePair<string, int> left, KeyValuePair<string, int> right)
        {
            var countComparison = right.Value.CompareTo(left.Value);
            if (countComparison != 0)
                return countComparison;

            return string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class RandomCargoResetStats
        {
            internal readonly Dictionary<string, int> ContainerTypes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            internal int RandomCargoEntities;
            internal int EmptyEligible;
            internal int NonEmptySkipped;
            internal int MissingInventory;
            internal int Refilled;
            internal int EmptyAfterSpawn;
            internal int Errors;

            internal void AddContainerType(string containerType)
            {
                if (string.IsNullOrWhiteSpace(containerType))
                    containerType = "(none)";

                int count;
                ContainerTypes.TryGetValue(containerType, out count);
                ContainerTypes[containerType] = count + 1;
            }
        }
    }
}
