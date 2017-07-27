using System;
using System.Collections.Generic;
using System.Globalization;
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

using Instrumind.Common.Visualization;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for TreeItemSelector.xaml
    /// </summary>
    public partial class TreeItemSelector : UserControl, IDynamicStoreDataGridEditor
    {
        public static readonly DependencyProperty StorageFieldNameProperty;
        public static readonly DependencyProperty ApplyDirectAccessProperty;

        public static readonly DependencyProperty ValueProperty;

        public static readonly DependencyProperty CompositesSourceProperty;

        static TreeItemSelector()
        {
            TreeItemSelector.StorageFieldNameProperty = DependencyProperty.Register("StorageFieldName", typeof(string), typeof(TreeItemSelector),
                                                            new FrameworkPropertyMetadata(null));

            TreeItemSelector.ApplyDirectAccessProperty = DependencyProperty.Register("ApplyDirectAccess", typeof(bool), typeof(TreeItemSelector),
                                                            new FrameworkPropertyMetadata(false));

            TreeItemSelector.ValueProperty = DependencyProperty.Register("Value", typeof(IRecognizableComposite), typeof(TreeItemSelector),
                                              new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

            TreeItemSelector.CompositesSourceProperty = DependencyProperty.Register("CompositesSource", typeof(IEnumerable<IRecognizableComposite>), typeof(TreeItemSelector),
                                              new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCompositesSourceChanged)));
        }

        public Window ParentWindow { get; protected set; }

        public DataGridRow ParentRow { get; protected set; }

        public TreeItemSelector()
        {
            InitializeComponent();

            this.TreeActioner.SetVisible(false);
            this.SelectNullItemActioner.SetVisible(false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // When exposed inside a pop-up, maybe there is no parent Window, therefore use the main.
            this.ParentWindow = this.GetNearestVisualDominantOfType<Window>().NullDefault(Application.Current.MainWindow);
            this.ParentRow = this.GetNearestVisualDominantOfType<DataGridRow>();

            if (this.ParentWindow != null)
                this.ParentWindow.KeyDown += WhenKeyPressed;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ParentWindow != null)
                    this.ParentWindow.KeyDown -= WhenKeyPressed;
            }
            catch (Exception Problem)
            {
                // this happens when the Loaded event was never fired, hence no event handler was attached nor parent-window was populated.
            }
        }

        private void WhenKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.ResolveTreeActionerHide(true);
                this.PopupSelectorTree.IsOpen = false;
            }
        }

        /// <summary>
        /// Action to be called just after the value has been edited.
        /// </summary>
        public Action<IRecognizableComposite> EditingAction = null;

        public IRecognizableComposite Value
        {
            get { return (IRecognizableComposite)GetValue(TreeItemSelector.ValueProperty); }
            set { SetCurrentValue(TreeItemSelector.ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as TreeItemSelector;

            var NewValue = (IRecognizableComposite)evargs.NewValue;
            var OldValue = (Target.ApplyDirectAccess
                            ? (IRecognizableComposite)Target.PerformDirectRead(Target.StorageFieldName, Target.ParentRow)
                            : Target.Value);

            if (Target.EditingAction != null)
                Target.EditingAction(NewValue);

            if (OldValue == NewValue)
                return;

            //T Console.WriteLine("Old-Value=[{0}], New-Value=[{1}]", OldValue, NewValue);

            if (Target.ApplyDirectAccess && NewValue != Target.PerformDirectRead(Target.StorageFieldName, Target.ParentRow))
                Target.PerformDirectWrite(Target.StorageFieldName, NewValue, Target.ParentRow);

            if (NewValue == null)
                return;

            var TvItem = (TreeViewItem)Target.TrvSelectorTree.ItemContainerGenerator.ContainerFromItem(NewValue);
            if (TvItem != null && !TvItem.IsSelected)
                TvItem.IsSelected = true;
        }

        public IEnumerable<IRecognizableComposite> CompositesSource
        {
            get { return (IEnumerable<IRecognizableComposite>)GetValue(TreeItemSelector.CompositesSourceProperty); }
            set { SetCurrentValue(TreeItemSelector.CompositesSourceProperty, value); }
        }
        private static void OnCompositesSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as TreeItemSelector;
            var NewValue = (IEnumerable<IRecognizableComposite>)evargs.NewValue;

            Target.CompositesSource = NewValue;
        }

        public string StorageFieldName
        {
            get { return (string)GetValue(TreeItemSelector.StorageFieldNameProperty); }
            set { SetCurrentValue(TreeItemSelector.StorageFieldNameProperty, value); }
        }

        public bool ApplyDirectAccess
        {
            get { return (bool)GetValue(TreeItemSelector.ApplyDirectAccessProperty); }
            set { SetCurrentValue(TreeItemSelector.ApplyDirectAccessProperty, value); }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ResolveTreeActionerShow();
        }

        public bool HideSelectNullItemActioner { get; set; }

        private void ResolveTreeActionerShow()
        {
            if (this.CompositesSource == null || !this.CompositesSource.Any() || this.TreeActioner.IsVisible)
                return;

            this.TreeActioner.SetVisible(true);

            if (!this.HideSelectNullItemActioner)
                this.SelectNullItemActioner.SetVisible(true);
        }

        private void ResolveTreeActionerHide(bool ForceClose = false)
        {
            if (!ForceClose && (this.TrvSelectorTree.IsMouseOver || this.IsMouseOver))
                return;

            this.TreeActioner.SetVisible(false);
            this.SelectNullItemActioner.SetVisible(false);
        }

        private void TreeActioner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResolveTreeActionerShow();

            if (this.Value == null && !this.PopupSelectorTree.IsOpen)
                this.PopupSelectorTree.IsOpen = true;
            else
                if (this.TreeActioner.IsVisible)    // Detect if is focused (IsFocused property not working)
                    this.PopupSelectorTree.IsOpen = !this.PopupSelectorTree.IsOpen;
        }

        private void TrvSelectorTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.IsInitialized || !this.TrvSelectorTree.IsLoaded || this.TrvSelectorTree.SelectedItem == null
                || this.CompositesSource == null || !this.CompositesSource.Any() || this.IgnoreSelection)
                return;

            //T Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff") + " > Selected: " + this.TrvSelectorTree.SelectedItem.ToStringAlways());

            this.Value = (IRecognizableComposite)this.TrvSelectorTree.SelectedItem;

            // Must be post-called in order to have the popup open at the gotfocus handler execution time.
            this.PostCall(tvs => tvs.PopupSelectorTree.IsOpen = false, true);
        }

        private bool IgnoreSelection = false;

        private void TrvSelectorTree_Loaded(object sender, RoutedEventArgs e)
        {
            /* this causes a sudden disappearing of the popup (apparently due to treeview scrolling) */
            this.IgnoreSelection = true;

            var TvItem = (TreeViewItem)this.TrvSelectorTree.ItemContainerGenerator.ContainerFromItem(this.Value);
            if (TvItem != null)
            {
                if (!TvItem.IsSelected)
                    TvItem.IsSelected = true;

                TvItem.BringIntoView();
            }

            this.IgnoreSelection = false;
        }

        private void SelectNullItemActioner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Value = null;
            this.PopupSelectorTree.IsOpen = false;
        }

        private void SelectNullItemActioner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Value = null;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.PopupSelectorTree.IsOpen)
                return;

            this.ResolveTreeActionerShow();
            this.PopupSelectorTree.IsOpen = true;
        }
    }
}
