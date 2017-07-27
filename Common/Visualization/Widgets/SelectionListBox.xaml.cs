using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
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
    /// Interaction logic for SelectionListBox.xaml
    /// </summary>
    public partial class SelectionListBox : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedObjectsProperty;

        static SelectionListBox()
        {
            SelectionListBox.SelectedObjectsProperty = DependencyProperty.Register("SelectedObjectsProperty", typeof(IList), typeof(SelectionListBox),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedObjectsChanged)));
        }

        public SelectionListBox()
        {
            InitializeComponent();

            this.WorkItemsControl.ItemsSource = this.ExposedContainers;
        }

        public IList SelectedObjects
        {
            get { return (IList)GetValue(SelectedObjectsProperty); }
            set { SetValue(SelectedObjectsProperty, value); }
        }

        public bool CanUnselectAll { get; set; }

        public bool IsEmptySelection
        {
            get { return (this.SelectedObjects.Count < 1); }
            set
            {
                if (!value)
                    return;

                this.ResetSelection(this.SelectedObjects);

                var Handler = PropertyChanged;
                if (Handler != null)
                    Handler(this, new PropertyChangedEventArgs("IsEmptySelection"));
            }
        }

        public string EmptySelectionTitle { get; set; }

        private static void OnSelectedObjectsChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            SelectionListBox SelListBox = depobj as SelectionListBox;

            SelListBox.ResetSelection(evargs.NewValue as IList);
        }

        public void ApplySelect(SelectableContainer Container, bool Select)
        {
            Container.IsSelected = Select;

            if (Container.IsSelected)
            {
                if (!this.SelectedObjects.Contains(Container.Content))
                {
                    this.SelectedObjects.Add(Container.Content);

                    var Handler = PropertyChanged;
                    if (Handler != null)
                        Handler(this, new PropertyChangedEventArgs("IsEmptySelection"));
                }
            }
            else
                if (this.SelectedObjects.Contains(Container.Content))
                    this.SelectedObjects.Remove(Container.Content);
        }

        public IEnumerable AvailableObjects
        {
            get { return this.AvailableObjects_; }
            set
            {
                this.AvailableObjects_ = value;

                if (this.AvailableObjects_ == null)
                    return;

                this.ExposedContainers.Clear();
                foreach (var Item in this.AvailableObjects_)
                {
                    var NewContainer = new SelectableContainer(this, Item);

                    if (this.ExtraDataItems != null)
                        NewContainer.ExtraData = this.ExtraDataItems.GetValueOrDefault(Item);

                    this.ExposedContainers.Add(NewContainer);
                }

                this.ResetSelection(this.SelectedObjects);
            }
        }
        private IEnumerable AvailableObjects_;

        public void ResetSelection(IList NewSelectedObjects)
        {
            this.ExposedContainers.ForEach(container => container.IsSelected = false);

            if (NewSelectedObjects == null)
                return;

            foreach (var Item in this.ExposedContainers)
                Item.IsSelected = NewSelectedObjects.Contains(Item.Content);

            /*T foreach (var Selection in NewSelectedObjects)
                    if (Selection.IsEqual(Item.Content))
                        Item.IsSelected = true;  */
        }

        public readonly List<SelectableContainer> ExposedContainers = new List<SelectableContainer>();

        private void ApplySelectToAll(bool Select)
        {
            foreach (var Item in this.ExposedContainers)
                ApplySelect(Item, Select);
        }

        /// <summary>
        /// Extra data available values.
        /// </summary>
        public IEnumerable ExtraDataAvailableValues { get; set; }

        /// <summary>
        /// Extra data items (values) associated to the regular items (keys).
        /// </summary>
        public IDictionary<object, object> ExtraDataItems { get; set; }

        /// <summary>
        /// Operation for updating a modified Extra data item.
        /// Its arguments are: Standard item being edited and the extra-data object updated.
        /// </summary>
        public Action<object, object> ExtraDataUpdater { get; set; }

        public bool CanSelectExtraData { get { return this.ExtraDataAvailableValues != null; } }

        private void ItemPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var VisualContainer = (sender as UIElement).GetNearestVisualDominantOfType<ContentPresenter>();
            var Container = this.WorkItemsControl.ItemContainerGenerator.ItemFromContainer(VisualContainer) as SelectableContainer;
            if (Container == null)
                return;
                
            DoInteractiveSelection(Container, !Container.IsSelected);
        }

        private void SelectionCheck_Changed(object sender, RoutedEventArgs e)
        {
            var Selector = sender as CheckBox;
            if (Selector == null)
                return;

            var VisualContainer = Selector.GetNearestVisualDominantOfType<ContentPresenter>();
            var Container = this.WorkItemsControl.ItemContainerGenerator.ItemFromContainer(VisualContainer) as SelectableContainer;
            if (Container == null)
                return;

            DoInteractiveSelection(Container, Selector.IsChecked.IsTrue());
        }

        public void DoInteractiveSelection(SelectableContainer Container, bool Selection)
        {
            ApplySelect(Container, Selection);

            if (this.PreviousContainer != null && this.PreviousContainer != Container
                && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                var CurrContainerIndex = this.ExposedContainers.IndexOf(Container);
                var PrevContainerIndex = this.ExposedContainers.IndexOf(this.PreviousContainer);

                if (PrevContainerIndex > CurrContainerIndex)
                    for (int Index = CurrContainerIndex + 1; Index < PrevContainerIndex; Index++)
                        ApplySelect(this.ExposedContainers[Index], Selection);
                else
                    for (int Index = PrevContainerIndex + 1; Index < CurrContainerIndex; Index++)
                        ApplySelect(this.ExposedContainers[Index], Selection);

                this.PreviousContainer = null;
            }
            else
                this.PreviousContainer = Container;
        }
        private SelectableContainer PreviousContainer = null;

        public class SelectableContainer : INotifyPropertyChanged
        {
            public SelectionListBox Parent { get; protected set; }
            public object Content { get; set; }
            public string DisplayText { get { return Content.ToStringAlways(); } }
            public object ExtraData { get; set; }

            public SelectableContainer(SelectionListBox Parent, object Content)
            {
                this.Parent = Parent;
                this.Content = Content;
            }

            public bool IsSelected
            {
                get { return this.IsSelected_; }
                set
                {
                    this.IsSelected_ = value;

                    var Handler = this.PropertyChanged;
                    if (Handler != null)
                    {
                        Handler(this, new PropertyChangedEventArgs("IsSelected"));
                        Handler(this, new PropertyChangedEventArgs("ShowExtraData"));
                    }
                }
            }
            public bool IsSelected_ = false;

            public bool ShowExtraData { get { return (this.Parent.CanSelectExtraData && this.IsSelected);  } }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void ExtraDataEditCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Selector = sender as ComboBox;
            if (Selector == null)
                return;

            var VisualContainer = Selector.GetNearestVisualDominantOfType<ContentPresenter>();
            var Container = this.WorkItemsControl.ItemContainerGenerator.ItemFromContainer(VisualContainer) as SelectableContainer;
            if (Container == null)
                return;

            Container.ExtraData = Selector.SelectedItem;

            if (this.ExtraDataUpdater != null)
                this.ExtraDataUpdater(Container.Content, Container.ExtraData);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
