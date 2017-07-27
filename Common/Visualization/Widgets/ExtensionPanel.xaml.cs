using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Visual panel for exposing/requesting multiple data items by the dynamic creation of children controls.
    /// </summary>
    public partial class ExtensionPanel : StackPanel
    {
        /// <summary>
        /// Creates and returns a new Extension panel.
        /// </summary>
        public static ExtensionPanel Create()
        {
            return (new ExtensionPanel());
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExtensionPanel()
        {
            InitializeComponent();
        }

        protected readonly Dictionary<string, CheckBox> SwitchOptionsExpositors = new Dictionary<string, CheckBox>();
        protected readonly Dictionary<string, List<RadioButton>> SelectableOptionsExpositors = new Dictionary<string, List<RadioButton>>();
        protected string LastReferencedGroup;

        /// <summary>
        /// Creates a new grouping panel with the supplied title, code (optional) and summary (optional) for the specified group code (or the last used if none is declared).
        /// Returns this extension panel.
        /// </summary>
        public ExtensionPanel AddGroupPanel(string Title, string GroupCode = General.UNSPECIFIED, string Summary = null, string TargetGroupCode = null)
        {
            TargetGroupCode = TargetGroupCode ?? this.LastReferencedGroup;
            if (TargetGroupCode != this.LastReferencedGroup)
                this.LastReferencedGroup = TargetGroupCode;

            var NewGroupBox = new GroupBox();
            NewGroupBox.Header = Title;
            NewGroupBox.ToolTip = Summary;
            var NewGroupPanel = new StackPanel();
            NewGroupPanel.Tag = GroupCode;
            NewGroupBox.Content = NewGroupPanel;

            Panel TargetPanel = GetGroupPanel(this, TargetGroupCode) ?? this;
            TargetPanel.Children.Add(NewGroupBox);

            LastReferencedGroup = GroupCode;
            return this;
        }

        /// <summary>
        /// Creates a new switch option with the supplied code, title, switch state and summary (optional) for the specified group code (or the last used if none is declared).
        /// Returns this extension panel.
        /// </summary>
        public ExtensionPanel AddSwitchOption(string OptionCode, string OptionTitle, bool IsSwitchedOn, string OptionSummary = null, string TargetGroupCode = null)
        {
            TargetGroupCode = TargetGroupCode ?? this.LastReferencedGroup;
            if (TargetGroupCode != this.LastReferencedGroup)
                this.LastReferencedGroup = TargetGroupCode;

            var NewOption = new CheckBox();
            NewOption.Content = OptionTitle;
            NewOption.IsChecked = IsSwitchedOn;
            //- NewOption.ToolTip = OptionSummary;

            Panel TargetPanel = GetGroupPanel(this, TargetGroupCode) ?? this; 
            TargetPanel.Children.Add(NewOption);

            if (!OptionSummary.IsAbsent())
                TargetPanel.Children.Add(CreateOptionText(OptionSummary));

            if (!this.SwitchOptionsExpositors.ContainsKey(OptionCode))
                this.SwitchOptionsExpositors.Add(OptionCode, NewOption);

            return this;
        }

        /// <summary>
        /// Creates a new selectable option with the supplied code, title, selection state and summary (optional) for the specified group code (or the last used if none is declared).
        /// Returns this extension panel.
        /// </summary>
        public ExtensionPanel AddSelectableOption(string OptionCode, string OptionTitle, bool IsSelected, string OptionSummary = null, string TargetGroupCode = null)
        {
            TargetGroupCode = TargetGroupCode ?? this.LastReferencedGroup;
            if (TargetGroupCode != this.LastReferencedGroup)
                this.LastReferencedGroup = TargetGroupCode;

            Panel TargetPanel = GetGroupPanel(this, TargetGroupCode) ?? this;
            var NewOption = new RadioButton();
            NewOption.Tag = OptionCode;
            NewOption.Content = OptionTitle;
            NewOption.IsChecked = IsSelected;
            //- NewOption.ToolTip = OptionSummary;
            TargetPanel.Children.Add(NewOption);

            if (!OptionSummary.IsAbsent())
                TargetPanel.Children.Add(CreateOptionText(OptionSummary));

            if (!this.SelectableOptionsExpositors.ContainsKey(TargetGroupCode))
                this.SelectableOptionsExpositors.Add(TargetGroupCode, new List<RadioButton>());

            this.SelectableOptionsExpositors[TargetGroupCode].Add(NewOption);

            return this;
        }

        protected TextBlock CreateOptionText(string Text)
        {
            TextBlock NewOptionText = new TextBlock();
            NewOptionText.Text = Text;
            NewOptionText.TextWrapping = TextWrapping.Wrap;
            NewOptionText.Foreground = Brushes.DimGray;

            return NewOptionText;
        }

        /// <summary>
        /// Gets the child grouping panel tagged with the supplied group code, or null if none matches.
        /// </summary>
        public Panel GetGroupPanel(Panel Target, string TargetGroupCode)
        {
            if (Target.Tag.ToStringOrDefault() == TargetGroupCode)
                return Target;

            foreach (var Child in Target.Children)
            {
                Panel ChildPanel = GetChildPanel(Child);

                if (ChildPanel != null)
                {
                    var Result = GetGroupPanel(ChildPanel, TargetGroupCode);

                    if (Result != null)
                        return Result;
                }
            }

            return null;
        }

        protected Panel GetChildPanel(object Source)
        {
            if (Source is Panel)
                return Source as Panel;

            if (!(Source is ContentControl))
                return null;

            ContentControl SourceContentControl = Source as ContentControl;
            return GetChildPanel(SourceContentControl.Content);
        }

        /// <summary>
        /// Gets the value of the first specified Option that matches the supplied code, or null if no option found.
        /// </summary>
        public bool? GetSwitchOption(string OptionCode)
        {
            if (SwitchOptionsExpositors.ContainsKey(OptionCode))
                return SwitchOptionsExpositors[OptionCode].IsChecked;

            return null;
        }

        /// <summary>
        /// Gets the first Option code within the specified Group that matches the supplied code (optional), or null if no selection or group found.
        /// </summary>
        public string GetSelectableOption(string GroupCode = null)
        {
            GroupCode = GroupCode ?? LastReferencedGroup;

            if (SelectableOptionsExpositors.ContainsKey(GroupCode))
                foreach(var Option in SelectableOptionsExpositors[GroupCode])
                    if (Option.IsChecked.IsTrue())
                        return (Option.Tag as string);

            return null;
        }
    }
}
