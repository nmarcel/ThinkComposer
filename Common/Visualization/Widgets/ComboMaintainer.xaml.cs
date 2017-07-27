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
    /// Interaction logic for ComboMaintainer.xaml
    /// </summary>
    public partial class ComboMaintainer : UserControl
    {
        public IList<object> ItemsSource { get; protected set; }

        public object CurrentItem
        {
            get { return this.CbxSelector.SelectedItem; }
            set { this.CbxSelector.SelectedItem = value; }
        }

        public object PreviousItem { get; private set; }

        public Action<object, object> ItemSelection { get; protected set; }

        public bool CanCreate { get; set; }

        public Func<object> ItemCreation { get; protected set; }

        public bool CanEdit { get; set; }

        public Action<object> ItemEdit { get; protected set; }

        public bool CanDelete { get; set; }
        
        public Func<object, bool> ItemDeletionConfirmation { get; protected set; }

        public ComboMaintainer()
        {
            InitializeComponent();

            this.CanCreate = true;
            this.CanEdit = true;
            this.CanDelete = true;
        }

        public void Initialize(IList<object> ItemsSource, int PreselectedItemIndex, Action<object, object> ItemSelection,
                               Func<object> ItemCreation, Action<object> ItemEdit, Func<object, bool> ItemDeletionConfirmation, string DisplayMemberPath = null)
        {
            this.ItemsSource = ItemsSource;

            this.ItemSelection = ItemSelection;
            this.ItemCreation = ItemCreation;
            this.ItemEdit = ItemEdit;
            this.ItemDeletionConfirmation = ItemDeletionConfirmation;

            this.CbxSelector.ItemsSource = this.ItemsSource;

            if (DisplayMemberPath != null)
                this.CbxSelector.DisplayMemberPath = DisplayMemberPath;

            if (PreselectedItemIndex >= 0 && PreselectedItemIndex < this.ItemsSource.Count)
                this.CbxSelector.SelectedItem = this.ItemsSource[PreselectedItemIndex];
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void CbxSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.PreviousItem = (e.RemovedItems == null || e.RemovedItems.Count < 1 ? null : e.RemovedItems[0]);

            if (this.ItemSelection == null)
                return;

            this.ItemSelection(this.CurrentItem, this.PreviousItem);
        }

        private void BtnEditItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ItemEdit == null)
                return;

            this.ItemEdit(this.CurrentItem);
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ItemCreation == null)
                return;

            var Item = this.ItemCreation();
            if (Item == null)
                return;

            this.ItemsSource.Add(Item);
            this.CbxSelector.SelectedItem = Item;
        }

        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentItem == null)
                return;

            if (this.ItemDeletionConfirmation != null)
                if (!this.ItemDeletionConfirmation(this.CurrentItem))
                    return;

            this.ItemsSource.Remove(this.CurrentItem);
        }
    }
}
