using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        private readonly List<IMyTerminalControl> reclamationSpawnerControls =
            new List<IMyTerminalControl>();

        private readonly List<IMyTerminalAction> reclamationSpawnerActions =
            new List<IMyTerminalAction>();

        private readonly HashSet<long> reclamationInfoSubscribedBlocks =
            new HashSet<long>();

        private IMyTerminalControlListbox reclamationCatalogListControl;
        private IMyTerminalControlListbox reclamationSequenceListControl;
        private IMyTerminalControlListbox reclamationAppearanceListControl;

        private void RegisterReclamationSpawnerControls()
        {
            if (MyAPIGateway.TerminalControls == null || reclamationSpawnerControls.Count > 0)
                return;

            var title = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("WwReclamationTitle");
            title.Label = MyStringId.GetOrCompute("Block Spawner");
            title.Visible = IsReclamationSpawner;
            reclamationSpawnerControls.Add(title);

            var legacyWarning = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("WwBlockSpawnerLegacyWarning");
            legacyWarning.Label = MyStringId.GetOrCompute("Legacy test block: replace with the new Block Spawner.");
            legacyWarning.Visible = IsLegacyReclamationSpawner;
            reclamationSpawnerControls.Add(legacyWarning);

            var customName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTerminalBlock>("WwBlockSpawnerName");
            customName.Visible = IsReclamationSpawner;
            customName.Getter = block => new StringBuilder(block.CustomName ?? string.Empty);
            customName.Setter = (block, value) => block.CustomName = value != null ? value.ToString() : string.Empty;
            SetReclamationControlText(customName, "Custom Name", "Name this Block Spawner for terminal lists and story purposes.");
            reclamationSpawnerControls.Add(customName);

            var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("WwReclamationSeparator");
            separator.Visible = IsReclamationSpawner;
            reclamationSpawnerControls.Add(separator);

            var search = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTerminalBlock>("WwReclamationSearch");
            search.Visible = IsReclamationSpawner;
            search.Getter = GetReclamationSearch;
            search.Setter = SetReclamationSearch;
            SetReclamationControlText(search, "Search", "Filter loaded public blocks by name or Type/Subtype ID.");
            reclamationSpawnerControls.Add(search);

            reclamationCatalogListControl = CreateReclamationListBox(
                "WwReclamationCatalog",
                "Available Blocks",
                "Select a loaded public block definition.",
                9,
                PopulateReclamationCatalog,
                SelectReclamationCatalogEntry);

            reclamationSpawnerControls.Add(reclamationCatalogListControl);
            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationAdd",
                "Add Selected",
                "Append the selected block to the spawn sequence.",
                AddSelectedReclamationEntry));

            reclamationSequenceListControl = CreateReclamationListBox(
                "WwReclamationSequence",
                "Spawn Sequence",
                "Ordered sequence. Duplicate entries spawn more than once and weight Random mode.",
                9,
                PopulateReclamationSequence,
                SelectReclamationSequenceEntry);

            reclamationSpawnerControls.Add(reclamationSequenceListControl);
            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationRemove",
                "Remove Selected",
                "Remove only the selected occurrence from the sequence.",
                RemoveSelectedReclamationEntry));
            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationMoveUp",
                "Move Up",
                "Move the selected sequence entry one position earlier.",
                MoveSelectedReclamationEntryUp));
            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationMoveDown",
                "Move Down",
                "Move the selected sequence entry one position later.",
                MoveSelectedReclamationEntryDown));

            var mode = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyTerminalBlock>("WwReclamationMode");
            mode.Visible = IsReclamationSpawner;
            mode.ComboBoxContent = PopulateReclamationModes;
            mode.Getter = block => (long)ReadReclamationSpawnerConfig(block).Mode;
            mode.Setter = (block, value) => RequestReclamationOperation(block, "mode", index: (int)value);
            SetReclamationControlText(mode, "Sequence Mode", "Once stops at the end; Loop restarts; Random chooses a weighted entry.");
            reclamationSpawnerControls.Add(mode);

            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationSpawn",
                "Spawn Next",
                "Request one grid. The request waits until the full spawn volume is clear.",
                block => RequestReclamationOperation(block, "spawn")));

            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationReset",
                "Reset Sequence",
                "Return the sequence to its first entry and cancel any waiting spawn.",
                block => RequestReclamationOperation(block, "reset")));

            var interval = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("WwReclamationInterval");
            interval.Visible = IsReclamationSpawner;
            interval.SetLimits(MinimumAutomaticIntervalSeconds, MaximumAutomaticIntervalSeconds);
            interval.Getter = block => ReadReclamationSpawnerConfig(block).AutomaticIntervalSeconds;
            interval.Setter = (block, value) => RequestReclamationOperation(block, "interval", number: value);
            interval.Writer = (block, output) => output.Append(ReadReclamationSpawnerConfig(block).AutomaticIntervalSeconds.ToString("0.0", CultureInfo.InvariantCulture)).Append(" s");
            SetReclamationControlText(interval, "Automatic Interval", "Minimum delay after each successful automatic spawn.");
            reclamationSpawnerControls.Add(interval);

            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationStart",
                "Start Automatic",
                "Start spawning immediately, then continue using the automatic interval.",
                block => RequestReclamationOperation(block, "start")));

            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationStop",
                "Stop Automatic",
                "Stop automatic spawning and cancel a spawn that is waiting for room.",
                block => RequestReclamationOperation(block, "stop")));

            var velocity = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("WwReclamationVelocity");
            velocity.Visible = IsReclamationSpawner;
            velocity.SetLimits(0f, MaximumOutwardVelocity);
            velocity.Getter = block => ReadReclamationSpawnerConfig(block).OutwardVelocity;
            velocity.Setter = (block, value) => RequestReclamationOperation(block, "velocity", number: value);
            velocity.Writer = (block, output) => output.Append(ReadReclamationSpawnerConfig(block).OutwardVelocity.ToString("0.0", CultureInfo.InvariantCulture)).Append(" m/s");
            SetReclamationControlText(velocity, "Outward Velocity", "Velocity relative to the spawner grid. Vanilla world physics decides the final speed cap.");
            reclamationSpawnerControls.Add(velocity);

            var rotation = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("WwReclamationRotation");
            rotation.Visible = IsReclamationSpawner;
            rotation.SetLimits(0f, 100f);
            rotation.Getter = block => ReadReclamationSpawnerConfig(block).RotationVariance;
            rotation.Setter = (block, value) => RequestReclamationOperation(block, "rotation", number: value);
            rotation.Writer = (block, output) => output.Append(ReadReclamationSpawnerConfig(block).RotationVariance.ToString("0", CultureInfo.InvariantCulture)).Append("%");
            SetReclamationControlText(rotation, "Rotation Variance", "Zero stays aligned; 100 percent gives a completely random starting orientation without spin.");
            reclamationSpawnerControls.Add(rotation);

            var minimumIntegrity = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("WwReclamationMinimumIntegrity");
            minimumIntegrity.Visible = IsReclamationSpawner;
            minimumIntegrity.SetLimits(10f, 100f);
            minimumIntegrity.Getter = block => ReadReclamationSpawnerConfig(block).MinimumIntegrity;
            minimumIntegrity.Setter = (block, value) => RequestReclamationOperation(block, "minimum-integrity", number: value);
            minimumIntegrity.Writer = (block, output) => output.Append(ReadReclamationSpawnerConfig(block).MinimumIntegrity.ToString("0", CultureInfo.InvariantCulture)).Append("%");
            SetReclamationControlText(minimumIntegrity, "Minimum Integrity", "Lowest random integrity assigned to a spawned block.");
            reclamationSpawnerControls.Add(minimumIntegrity);

            var maximumIntegrity = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("WwReclamationMaximumIntegrity");
            maximumIntegrity.Visible = IsReclamationSpawner;
            maximumIntegrity.SetLimits(10f, 100f);
            maximumIntegrity.Getter = block => ReadReclamationSpawnerConfig(block).MaximumIntegrity;
            maximumIntegrity.Setter = (block, value) => RequestReclamationOperation(block, "maximum-integrity", number: value);
            maximumIntegrity.Writer = (block, output) => output.Append(ReadReclamationSpawnerConfig(block).MaximumIntegrity.ToString("0", CultureInfo.InvariantCulture)).Append("%");
            SetReclamationControlText(maximumIntegrity, "Maximum Integrity", "Highest random integrity assigned to a spawned block.");
            reclamationSpawnerControls.Add(maximumIntegrity);

            reclamationAppearanceListControl = CreateReclamationListBox(
                "WwReclamationAppearances",
                "Appearance Presets",
                "One paint color and skin is chosen randomly for every spawn. Duplicate presets add weight.",
                6,
                PopulateReclamationAppearances,
                SelectReclamationAppearance);
            reclamationSpawnerControls.Add(reclamationAppearanceListControl);

            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationAddAppearance",
                "Add Current Appearance",
                "Capture the Block Spawner's current paint color and skin as a random preset.",
                block => RequestReclamationOperation(block, "add-appearance")));

            reclamationSpawnerControls.Add(CreateReclamationButton(
                "WwReclamationRemoveAppearance",
                "Remove Appearance",
                "Remove only the selected appearance preset.",
                RemoveSelectedReclamationAppearance));

            RegisterReclamationAction(
                "WwReclamationSpawnNext",
                "Spawn Next",
                "Textures\\GUI\\Icons\\Actions\\Start.dds",
                block => RequestReclamationOperation(block, "spawn"),
                (block, output) => output.Append(pendingReclamationSpawns.ContainsKey(block.EntityId) ? "Waiting" : "Spawn"));

            RegisterReclamationAction(
                "WwReclamationResetSequence",
                "Reset Sequence",
                "Textures\\GUI\\Icons\\Actions\\Reset.dds",
                block => RequestReclamationOperation(block, "reset"),
                (block, output) => output.Append("Reset"));

            RegisterReclamationAction(
                "WwReclamationStartAutomatic",
                "Start Automatic",
                "Textures\\GUI\\Icons\\Actions\\Start.dds",
                block => RequestReclamationOperation(block, "start"),
                (block, output) => output.Append(runningReclamationSpawners.ContainsKey(block.EntityId) ? "Running" : "Start"));

            RegisterReclamationAction(
                "WwReclamationStopAutomatic",
                "Stop Automatic",
                "Textures\\GUI\\Icons\\Actions\\SwitchOff.dds",
                block => RequestReclamationOperation(block, "stop"),
                (block, output) => output.Append("Stop"));

            MyAPIGateway.TerminalControls.CustomControlGetter += AddReclamationSpawnerControlsToTerminal;
            MyAPIGateway.TerminalControls.CustomActionGetter += FilterReclamationSpawnerActions;
        }

        private void UnregisterReclamationSpawnerControls()
        {
            if (MyAPIGateway.TerminalControls != null)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter -= AddReclamationSpawnerControlsToTerminal;
                MyAPIGateway.TerminalControls.CustomActionGetter -= FilterReclamationSpawnerActions;

                for (var i = 0; i < reclamationSpawnerActions.Count; i++)
                    MyAPIGateway.TerminalControls.RemoveAction<IMyFunctionalBlock>(reclamationSpawnerActions[i]);
            }

            foreach (var entityId in reclamationInfoSubscribedBlocks)
            {
                var block = MyAPIGateway.Entities != null
                    ? MyAPIGateway.Entities.GetEntityById(entityId) as IMyTerminalBlock
                    : null;
                if (block != null)
                    block.AppendingCustomInfo -= AppendReclamationSpawnerInfo;
            }

            reclamationInfoSubscribedBlocks.Clear();
            reclamationSpawnerActions.Clear();
            reclamationSpawnerControls.Clear();
            reclamationCatalogListControl = null;
            reclamationSequenceListControl = null;
            reclamationAppearanceListControl = null;
        }

        private void AddReclamationSpawnerControlsToTerminal(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (!IsReclamationSpawner(block) || controls == null)
                return;

            if (reclamationInfoSubscribedBlocks.Add(block.EntityId))
                block.AppendingCustomInfo += AppendReclamationSpawnerInfo;

            controls.InsertRange(0, reclamationSpawnerControls);
        }

        private void FilterReclamationSpawnerActions(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (IsReclamationSpawner(block) || actions == null)
                return;

            for (var i = actions.Count - 1; i >= 0; i--)
            {
                if (reclamationSpawnerActions.Contains(actions[i]))
                    actions.RemoveAt(i);
            }
        }

        private IMyTerminalControlListbox CreateReclamationListBox(
            string id,
            string title,
            string tooltip,
            int rows,
            Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>, List<MyTerminalControlListBoxItem>> content,
            Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>> selected)
        {
            var list = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyTerminalBlock>(id);
            list.Visible = IsReclamationSpawner;
            list.VisibleRowsCount = rows;
            list.Multiselect = false;
            list.ListContent = content;
            list.ItemSelected = selected;
            SetReclamationControlText(list, title, tooltip);
            return list;
        }

        private IMyTerminalControlButton CreateReclamationButton(
            string id,
            string title,
            string tooltip,
            Action<IMyTerminalBlock> action)
        {
            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>(id);
            button.Visible = IsReclamationSpawner;
            button.Action = action;
            SetReclamationControlText(button, title, tooltip);
            return button;
        }

        private void RegisterReclamationAction(
            string id,
            string name,
            string icon,
            Action<IMyTerminalBlock> actionDelegate,
            Action<IMyTerminalBlock, StringBuilder> writer)
        {
            var action = MyAPIGateway.TerminalControls.CreateAction<IMyFunctionalBlock>(id);
            action.Name = new StringBuilder(name);
            action.Icon = icon;
            action.Action = actionDelegate;
            action.Enabled = IsReclamationSpawner;
            action.Writer = writer;
            action.ValidForGroups = true;
            reclamationSpawnerActions.Add(action);
            MyAPIGateway.TerminalControls.AddAction<IMyFunctionalBlock>(action);
        }

        private static void SetReclamationControlText(IMyTerminalControl control, string title, string tooltip)
        {
            var titled = control as IMyTerminalControlTitleTooltip;
            if (titled == null)
                return;

            titled.Title = MyStringId.GetOrCompute(title);
            titled.Tooltip = MyStringId.GetOrCompute(tooltip);
        }

        private StringBuilder GetReclamationSearch(IMyTerminalBlock block)
        {
            string search;
            reclamationSearchByBlock.TryGetValue(block.EntityId, out search);
            return new StringBuilder(search ?? string.Empty);
        }

        private void SetReclamationSearch(IMyTerminalBlock block, StringBuilder value)
        {
            reclamationSearchByBlock[block.EntityId] = value != null ? value.ToString().Trim() : string.Empty;
            if (reclamationCatalogListControl != null)
                reclamationCatalogListControl.UpdateVisual();
        }

        private void PopulateReclamationCatalog(
            IMyTerminalBlock block,
            List<MyTerminalControlListBoxItem> items,
            List<MyTerminalControlListBoxItem> selectedItems)
        {
            items.Clear();
            selectedItems.Clear();

            string search;
            reclamationSearchByBlock.TryGetValue(block.EntityId, out search);
            search = (search ?? string.Empty).Trim().ToLowerInvariant();

            string selectedKey;
            selectedCatalogEntryByBlock.TryGetValue(block.EntityId, out selectedKey);
            for (var i = 0; i < reclamationBlockCatalog.Count; i++)
            {
                var entry = reclamationBlockCatalog[i];
                if (search.Length > 0 && entry.SearchText.IndexOf(search, StringComparison.Ordinal) < 0)
                    continue;

                var item = new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute(entry.DisplayName),
                    MyStringId.GetOrCompute(entry.Key),
                    entry.Key);
                items.Add(item);
                if (entry.Key.Equals(selectedKey, StringComparison.OrdinalIgnoreCase))
                    selectedItems.Add(item);
            }
        }

        private void SelectReclamationCatalogEntry(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> selected)
        {
            if (selected == null || selected.Count == 0)
            {
                selectedCatalogEntryByBlock.Remove(block.EntityId);
                return;
            }

            selectedCatalogEntryByBlock[block.EntityId] = selected[0].UserData as string;
        }

        private void PopulateReclamationSequence(
            IMyTerminalBlock block,
            List<MyTerminalControlListBoxItem> items,
            List<MyTerminalControlListBoxItem> selectedItems)
        {
            items.Clear();
            selectedItems.Clear();

            var config = ReadReclamationSpawnerConfig(block);
            int selectedIndex;
            if (!selectedSequenceIndexByBlock.TryGetValue(block.EntityId, out selectedIndex))
                selectedIndex = -1;

            for (var i = 0; i < config.Entries.Count; i++)
            {
                ReclamationBlockCatalogEntry catalogEntry;
                var label = reclamationBlockCatalogByKey.TryGetValue(config.Entries[i], out catalogEntry)
                    ? catalogEntry.DisplayName
                    : config.Entries[i] + " [Missing]";

                var marker = config.Mode != ReclamationSequenceMode.Random && !config.Completed && config.Cursor == i
                    ? "> "
                    : "  ";
                var item = new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute(marker + (i + 1).ToString(CultureInfo.InvariantCulture) + ". " + label),
                    MyStringId.GetOrCompute(config.Entries[i]),
                    i);
                items.Add(item);
                if (i == selectedIndex)
                    selectedItems.Add(item);
            }
        }

        private void SelectReclamationSequenceEntry(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> selected)
        {
            if (selected == null || selected.Count == 0 || !(selected[0].UserData is int))
            {
                selectedSequenceIndexByBlock.Remove(block.EntityId);
                return;
            }

            selectedSequenceIndexByBlock[block.EntityId] = (int)selected[0].UserData;
        }

        private void AddSelectedReclamationEntry(IMyTerminalBlock block)
        {
            string key;
            if (selectedCatalogEntryByBlock.TryGetValue(block.EntityId, out key))
                RequestReclamationOperation(block, "add", key);
        }

        private void RemoveSelectedReclamationEntry(IMyTerminalBlock block)
        {
            int index;
            if (selectedSequenceIndexByBlock.TryGetValue(block.EntityId, out index))
                RequestReclamationOperation(block, "remove", index: index);
        }

        private void MoveSelectedReclamationEntryUp(IMyTerminalBlock block)
        {
            int index;
            if (!selectedSequenceIndexByBlock.TryGetValue(block.EntityId, out index) || index <= 0)
                return;

            selectedSequenceIndexByBlock[block.EntityId] = index - 1;
            RequestReclamationOperation(block, "move-up", index: index);
        }

        private void MoveSelectedReclamationEntryDown(IMyTerminalBlock block)
        {
            int index;
            if (!selectedSequenceIndexByBlock.TryGetValue(block.EntityId, out index))
                return;

            selectedSequenceIndexByBlock[block.EntityId] = index + 1;
            RequestReclamationOperation(block, "move-down", index: index);
        }

        private void PopulateReclamationAppearances(
            IMyTerminalBlock block,
            List<MyTerminalControlListBoxItem> items,
            List<MyTerminalControlListBoxItem> selectedItems)
        {
            items.Clear();
            selectedItems.Clear();

            var config = ReadReclamationSpawnerConfig(block);
            int selectedIndex;
            if (!selectedAppearanceIndexByBlock.TryGetValue(block.EntityId, out selectedIndex))
                selectedIndex = -1;

            for (var i = 0; i < config.AppearancePresets.Count; i++)
            {
                var preset = config.AppearancePresets[i];
                var color = ColorExtensions.HSVtoColor(MyColorPickerConstants.HSVOffsetToHSV(preset.ColorMaskHsv));
                var skin = string.IsNullOrWhiteSpace(preset.SkinSubtypeId) ? "Default" : preset.SkinSubtypeId;
                var label = (i + 1).ToString(CultureInfo.InvariantCulture) + ". #" +
                            color.R.ToString("X2", CultureInfo.InvariantCulture) +
                            color.G.ToString("X2", CultureInfo.InvariantCulture) +
                            color.B.ToString("X2", CultureInfo.InvariantCulture) +
                            " — " + skin;
                var item = new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute(label),
                    MyStringId.GetOrCompute("Captured paint color and skin"),
                    i);
                items.Add(item);
                if (i == selectedIndex)
                    selectedItems.Add(item);
            }

            if (config.AppearancePresets.Count == 0)
            {
                items.Add(new MyTerminalControlListBoxItem(
                    MyStringId.GetOrCompute("Current Block Spawner appearance"),
                    MyStringId.GetOrCompute("No presets: each spawn uses the spawner's current paint and skin."),
                    -1));
            }
        }

        private void SelectReclamationAppearance(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> selected)
        {
            if (selected == null || selected.Count == 0 || !(selected[0].UserData is int))
            {
                selectedAppearanceIndexByBlock.Remove(block.EntityId);
                return;
            }

            var index = (int)selected[0].UserData;
            if (index >= 0)
                selectedAppearanceIndexByBlock[block.EntityId] = index;
            else
                selectedAppearanceIndexByBlock.Remove(block.EntityId);
        }

        private void RemoveSelectedReclamationAppearance(IMyTerminalBlock block)
        {
            int index;
            if (selectedAppearanceIndexByBlock.TryGetValue(block.EntityId, out index))
                RequestReclamationOperation(block, "remove-appearance", index: index);
        }

        private static void PopulateReclamationModes(List<MyTerminalControlComboBoxItem> items)
        {
            items.Clear();
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ReclamationSequenceMode.Once, Value = MyStringId.GetOrCompute("Once") });
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ReclamationSequenceMode.Loop, Value = MyStringId.GetOrCompute("Loop") });
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ReclamationSequenceMode.Random, Value = MyStringId.GetOrCompute("Random") });
        }

        private void AppendReclamationSpawnerInfo(IMyTerminalBlock block, StringBuilder output)
        {
            if (!IsReclamationSpawner(block))
                return;

            var config = ReadReclamationSpawnerConfig(block);
            output.AppendLine("Worldwright Block Spawner");
            output.Append("Mode: ").AppendLine(config.Mode.ToString());
            output.Append("Velocity: ").Append(config.OutwardVelocity.ToString("0.0", CultureInfo.InvariantCulture)).AppendLine(" m/s");
            output.Append("Automatic interval: ").Append(config.AutomaticIntervalSeconds.ToString("0.0", CultureInfo.InvariantCulture)).AppendLine(" s");
            output.Append("Rotation variance: ").Append(config.RotationVariance.ToString("0", CultureInfo.InvariantCulture)).AppendLine("%");
            output.Append("Integrity: ").Append(config.MinimumIntegrity.ToString("0", CultureInfo.InvariantCulture)).Append("-").Append(config.MaximumIntegrity.ToString("0", CultureInfo.InvariantCulture)).AppendLine("%");
            output.Append("Appearances: ").AppendLine(config.AppearancePresets.Count.ToString(CultureInfo.InvariantCulture));
            output.Append("Entries: ").AppendLine(config.Entries.Count.ToString(CultureInfo.InvariantCulture));

            PendingReclamationSpawn pending;
            if (pendingReclamationSpawns.TryGetValue(block.EntityId, out pending))
                output.Append("Status: Waiting for room (" + pending.DefinitionKey + ")").AppendLine();
            else if (runningReclamationSpawners.ContainsKey(block.EntityId))
                output.AppendLine("Status: Running automatically");
            else if (config.Entries.Count == 0)
                output.AppendLine("Status: Empty sequence");
            else if (config.Mode == ReclamationSequenceMode.Once && config.Completed)
                output.AppendLine("Status: Once sequence complete");
            else
                output.AppendLine("Status: Ready");
        }

        private void RefreshReclamationSpawnerVisuals(IMyTerminalBlock block)
        {
            if (reclamationCatalogListControl != null)
                reclamationCatalogListControl.UpdateVisual();
            if (reclamationSequenceListControl != null)
                reclamationSequenceListControl.UpdateVisual();
            if (reclamationAppearanceListControl != null)
                reclamationAppearanceListControl.UpdateVisual();

            if (block != null)
            {
                block.SetDetailedInfoDirty();
                block.RefreshCustomInfo();
            }
        }
    }
}
