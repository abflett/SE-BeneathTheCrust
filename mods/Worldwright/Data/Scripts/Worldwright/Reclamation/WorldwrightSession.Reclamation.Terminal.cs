using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

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

        private void RegisterReclamationSpawnerControls()
        {
            if (MyAPIGateway.TerminalControls == null || reclamationSpawnerControls.Count > 0)
                return;

            var title = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("WwReclamationTitle");
            title.Label = MyStringId.GetOrCompute("Reclamation Spawner");
            title.Visible = IsReclamationSpawner;
            reclamationSpawnerControls.Add(title);

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

            var velocity = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("WwReclamationVelocity");
            velocity.Visible = IsReclamationSpawner;
            velocity.SetLimits(0f, MaximumOutwardVelocity);
            velocity.Getter = block => ReadReclamationSpawnerConfig(block).OutwardVelocity;
            velocity.Setter = (block, value) => RequestReclamationOperation(block, "velocity", number: value);
            velocity.Writer = (block, output) => output.Append(ReadReclamationSpawnerConfig(block).OutwardVelocity.ToString("0.0", CultureInfo.InvariantCulture)).Append(" m/s");
            SetReclamationControlText(velocity, "Outward Velocity", "Velocity relative to the spawner grid. Vanilla world physics decides the final speed cap.");
            reclamationSpawnerControls.Add(velocity);

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
                    MyAPIGateway.TerminalControls.RemoveAction<IMyTerminalBlock>(reclamationSpawnerActions[i]);
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
            var action = MyAPIGateway.TerminalControls.CreateAction<IMyTerminalBlock>(id);
            action.Name = new StringBuilder(name);
            action.Icon = icon;
            action.Action = actionDelegate;
            action.Enabled = IsReclamationSpawner;
            action.Writer = writer;
            action.ValidForGroups = true;
            reclamationSpawnerActions.Add(action);
            MyAPIGateway.TerminalControls.AddAction<IMyTerminalBlock>(action);
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
            output.AppendLine("Worldwright Reclamation Spawner");
            output.Append("Mode: ").AppendLine(config.Mode.ToString());
            output.Append("Velocity: ").Append(config.OutwardVelocity.ToString("0.0", CultureInfo.InvariantCulture)).AppendLine(" m/s");
            output.Append("Entries: ").AppendLine(config.Entries.Count.ToString(CultureInfo.InvariantCulture));

            PendingReclamationSpawn pending;
            if (pendingReclamationSpawns.TryGetValue(block.EntityId, out pending))
                output.Append("Status: Waiting for room (" + pending.DefinitionKey + ")").AppendLine();
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

            if (block != null)
            {
                block.SetDetailedInfoDirty();
                block.RefreshCustomInfo();
            }
        }
    }
}
