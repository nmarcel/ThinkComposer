using System;
using System.Collections;
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

using Instrumind.Common;
using Instrumind.Common.EntityDefinition;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for TabbedEditPanel.xaml
    /// </summary>
    public partial class TabbedEditPanel : UserControl
    {
        protected readonly Dictionary<string, TabItem> DeclaredTabs = new Dictionary<string, TabItem>();

        public IEntityView ContainerEntity { get; protected set; }

        public TabbedEditPanel()
        {
            InitializeComponent();

            // Clear the initial tabs (used for Design-time editing)
            this.DeclaredTabs.Clear();
            this.MainTabControl.Items.Clear();

            this.MainTabControl.SelectionChanged +=
                ((sender, args) =>
                {
                    var CurrWnd = Display.GetCurrentWindow();
                    CurrWnd.Cursor = Cursors.Wait;

                    var Tab = this.MainTabControl.SelectedItem as TabItem;
                    if (Tab == null)
                        return;

                    var Headerer = Tab.Header as TextBlock;
                    if (Headerer == null)
                        return;

                    Headerer.FontWeight = FontWeights.Bold;

                    if (this.PreviousHeaderer != null
                        && this.PreviousHeaderer != Headerer)
                        this.PreviousHeaderer.FontWeight = FontWeights.Normal;

                    this.PreviousHeaderer = Headerer;

                    this.PostCall(tabctl => CurrWnd.Cursor = Cursors.Arrow);
                });
        }

        private TextBlock PreviousHeaderer = null;

        public IEnumerable<KeyValuePair<string, TabItem>> Tabs
        {
            get
            {
                foreach (var dectab in this.DeclaredTabs)
                    yield return dectab;
            }
        }

        public static TabItem CreateTab(string Key, string Title, string Description, object Content)
        {
            var NewTab = new TabItem();
            NewTab.Tag = Key;

            NewTab.Header = Title;

            var HeaderText = new TextBlock();
            HeaderText.Text = Title;
            if (!Description.IsAbsent())
                HeaderText.ToolTip = Description;

            NewTab.Header = HeaderText;
            NewTab.Content = Content;

            return NewTab;
        }

        public TabItem AddTab(string Key, string Title, string Description, object Content)
        {
            return AddTab(CreateTab(Key, Title, Description, Content));
        }

        public TabItem AddTab(TabItem NewTab)
        {
            this.DeclaredTabs.Add(NewTab.Tag as String, NewTab);
            this.MainTabControl.Items.Add(NewTab);
            return NewTab;
        }

        public bool SelectTab(string Key)
        {
            if (!this.DeclaredTabs.ContainsKey(Key))
                return false;

            this.MainTabControl.SelectedItem = this.DeclaredTabs[Key];
            return true;
        }

        public TabItem GetTab(string Key)
        {
            return this.DeclaredTabs.First(kvp => kvp.Key == Key).Value;
        }

        public bool RemoveTab(string Key)
        {
            var Target = this.DeclaredTabs.First(kvp => kvp.Key == Key).Value;
            if (Target == null)
                return false;

            this.MainTabControl.Items.Remove(Target);
            return true;
        }

        public void RemoveAllTabs()
        {
            this.DeclaredTabs.Clear();
            this.MainTabControl.Items.Clear();
        }

        public bool MoveAfterTab(string FixedTabKey, string MovableTabKey)
        {
            if (!this.DeclaredTabs.ContainsKey(MovableTabKey))
                return false;

            var Movable = this.DeclaredTabs[MovableTabKey];
            this.MainTabControl.Items.Remove(Movable);

            var FixedPos = this.MainTabControl.Items.Cast<TabItem>().IndexOfMatch(tab => tab.Tag.ToStringAlways() == FixedTabKey);
            this.MainTabControl.Items.Insert(FixedPos + 1, Movable);

            return true;
        }

    }
}
