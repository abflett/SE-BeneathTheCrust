using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;



namespace WkKn
{
    public partial class WkKnSession
    {
        private void RegisterResearchPedestalControls()
        {
            if (MyAPIGateway.TerminalControls == null || researchPedestalControls.Count > 0)
                return;

            var title = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("WkKnResearchTitle");
            title.Visible = IsResearchPedestal;
            title.Label = MyStringId.GetOrCompute("Research Archive");
            AddResearchPedestalControl(title);

            var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("WkKnResearchSeparator");
            separator.Visible = IsResearchPedestal;
            AddResearchPedestalControl(separator);

            AddResearchPedestalViewComboBox();

            playerResearchListControl = AddResearchPedestalListBox("WkKnPlayerResearchList", "Player Schematics", "Filtered personal schematic progress.", PopulatePlayerResearchList);

            AddResearchPedestalButton("WkKnResearchSync", "Sync", "Sync personal and faction schematic progress both ways.", SyncResearch);

            factionResearchListControl = AddResearchPedestalListBox("WkKnFactionResearchList", "Faction Schematics", "Filtered faction archive progress.", PopulateFactionResearchList);

            var footerSeparator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("WkKnResearchFooterSeparator");
            footerSeparator.Visible = IsResearchPedestal;
            AddResearchPedestalControl(footerSeparator);

            MyAPIGateway.TerminalControls.CustomControlGetter += AddResearchPedestalControlsToTerminal;
        }

        private void AddResearchPedestalControl(IMyTerminalControl control)
        {
            if (control != null)
                researchPedestalControls.Add(control);
        }

        private void AddResearchPedestalControlsToTerminal(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (!IsResearchPedestal(block) || controls == null)
                return;

            controls.InsertRange(0, researchPedestalControls);
        }

        private void AddResearchPedestalButton(string id, string title, string tooltip, Action<IMyTerminalBlock> action)
        {
            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>(id);
            button.Visible = IsResearchPedestal;
            button.Enabled = CanUseResearchPedestalControl;
            button.Action = action;
            SetTerminalControlText(button, title, tooltip);
            AddResearchPedestalControl(button);
        }

        private IMyTerminalControlListbox AddResearchPedestalListBox(string id, string title, string tooltip, Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>, List<MyTerminalControlListBoxItem>> listContent)
        {
            var listBox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyTerminalBlock>(id);
            listBox.Visible = IsResearchPedestal;
            listBox.VisibleRowsCount = 8;
            listBox.Multiselect = false;
            listBox.ListContent = listContent;
            SetTerminalControlText(listBox, title, tooltip);
            AddResearchPedestalControl(listBox);
            return listBox;
        }

        private void AddResearchPedestalViewComboBox()
        {
            var comboBox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyTerminalBlock>("WkKnResearchView");
            comboBox.Visible = IsResearchPedestal;
            comboBox.Enabled = CanUseResearchPedestalControl;
            comboBox.ComboBoxContent = PopulateResearchPedestalViewOptions;
            comboBox.Getter = GetResearchPedestalView;
            comboBox.Setter = SetResearchPedestalView;
            SetTerminalControlText(comboBox, string.Empty, "Choose which schematic progress bucket to show.");
            AddResearchPedestalControl(comboBox);
        }

        private void UnregisterResearchPedestalControls()
        {
            if (MyAPIGateway.TerminalControls == null)
                return;

            MyAPIGateway.TerminalControls.CustomControlGetter -= AddResearchPedestalControlsToTerminal;

            researchPedestalControls.Clear();
            playerResearchListControl = null;
            factionResearchListControl = null;
        }

        private static void SetTerminalControlText(IMyTerminalControl control, string title, string tooltip)
        {
            var titledControl = control as IMyTerminalControlTitleTooltip;
            if (titledControl == null)
                return;

            titledControl.Title = MyStringId.GetOrCompute(title);
            titledControl.Tooltip = MyStringId.GetOrCompute(tooltip);
        }

        private bool IsResearchPedestal(IMyTerminalBlock block)
        {
            return researchTerminalAdapter.IsResearchTerminal(block);
        }

        private bool CanUseResearchPedestalControl(IMyTerminalBlock block)
        {
            return IsResearchPedestal(block);
        }

        private static void PopulateResearchPedestalViewOptions(List<MyTerminalControlComboBoxItem> items)
        {
            if (items == null)
                return;

            items.Clear();
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ResearchPedestalView.Default, Value = MyStringId.GetOrCompute("Default") });
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ResearchPedestalView.Researching, Value = MyStringId.GetOrCompute("Researching") });
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ResearchPedestalView.Completed, Value = MyStringId.GetOrCompute("Completed") });
            items.Add(new MyTerminalControlComboBoxItem { Key = (long)ResearchPedestalView.All, Value = MyStringId.GetOrCompute("All") });
        }

        private long GetResearchPedestalView(IMyTerminalBlock block)
        {
            return (long)GetResearchPedestalViewForBlock(block);
        }

        private void SetResearchPedestalView(IMyTerminalBlock block, long value)
        {
            if (block == null)
                return;

            researchPedestalViewsByBlock[block.EntityId] = GetResearchPedestalView(value);
            RefreshResearchPedestalLists(block);
        }

        private ResearchPedestalView GetResearchPedestalViewForBlock(IMyTerminalBlock block)
        {
            if (block != null)
            {
                ResearchPedestalView view;
                if (researchPedestalViewsByBlock.TryGetValue(block.EntityId, out view))
                    return view;
            }

            return ResearchPedestalView.Default;
        }

        private static ResearchPedestalView GetResearchPedestalView(long value)
        {
            switch ((ResearchPedestalView)value)
            {
                case ResearchPedestalView.Researching:
                    return ResearchPedestalView.Researching;

                case ResearchPedestalView.Completed:
                    return ResearchPedestalView.Completed;

                case ResearchPedestalView.All:
                    return ResearchPedestalView.All;

                default:
                    return ResearchPedestalView.Default;
            }
        }

        private void RefreshResearchPedestalLists(IMyTerminalBlock block)
        {
            RefreshResearchPedestalList(playerResearchListControl);
            RefreshResearchPedestalList(factionResearchListControl);
            RefreshResearchPedestalBlock(block);
        }

        private static void RefreshResearchPedestalList(IMyTerminalControlListbox control)
        {
            if (control == null)
                return;

            control.UpdateVisual();
        }

        private static void RefreshResearchPedestalBlock(IMyTerminalBlock block)
        {
            if (block == null)
                return;

            block.SetDetailedInfoDirty();
            block.RefreshCustomInfo();
        }

        private void RequestResearchPedestalRefresh(IMyTerminalBlock block)
        {
            RefreshResearchPedestalLists(block);
        }

    }
}
