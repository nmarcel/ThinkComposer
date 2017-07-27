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

using Instrumind.Common;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for ExtendedEditComboBox.xaml
    /// </summary>
    public partial class ExtendedEditComboBox : UserControl, IDynamicStoreDataGridEditor
    {
        public static readonly DependencyProperty StorageFieldNameProperty;
        public static readonly DependencyProperty ApplyDirectAccessProperty;

        public static readonly DependencyProperty SelectedItemProperty;
        public static readonly DependencyProperty ItemsSourceProperty;
        public static readonly DependencyProperty SelectedValuePathProperty;
        public static readonly DependencyProperty DisplayMemberPathProperty;
        public static readonly DependencyProperty IsReadOnlyProperty;

        static ExtendedEditComboBox()
        {
            ExtendedEditComboBox.StorageFieldNameProperty = DependencyProperty.Register("StorageFieldName", typeof(string), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata("" /*T , new PropertyChangedCallback(OnStorageFieldNameChanged)*/ ));

            ExtendedEditComboBox.ApplyDirectAccessProperty = DependencyProperty.Register("ApplyDirectAccess", typeof(bool), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata(false));

            ExtendedEditComboBox.SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged)));

            ExtendedEditComboBox.ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<object>), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged)));

            ExtendedEditComboBox.SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnSelectedValuePathChanged)));

            ExtendedEditComboBox.DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnDisplayMemberPathChanged)));

            ExtendedEditComboBox.IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ExtendedEditComboBox),
                                                        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsReadOnlyChanged)));
        }

        public ExtendedEditComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Action to be called just after the value has been edited.
        /// </summary>
        public Action<object> EditingAction = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.ApplyDirectAccess)
                return;

            if (this.StorageFieldName.IsAbsent())
                throw new UsageAnomaly("The Storage-Field-Name must be populated.");

            var SelectionValue = this.PerformDirectRead(this.StorageFieldName);

            //T Console.WriteLine("LOAD: Field='{0}', SelectionValue=[{1}]", this.StorageFieldName, SelectionValue.ToStringAlways());

            if (this.SelectedValuePath.IsAbsent())
                this.Selector.SelectedItem = SelectionValue;
            else
            {
                var SelectionIndex = this.ItemsSource.GetMatchingIndex(SelectionValue, this.SelectedValuePath);
                if (SelectionIndex >= 0)
                    this.Selector.SelectedIndex = SelectionIndex;
            }
        }

        public string StorageFieldName
        {
            get { return (string)GetValue(ExtendedEditComboBox.StorageFieldNameProperty); }
            set { SetValue(ExtendedEditComboBox.StorageFieldNameProperty, value); }
        }

        public bool ApplyDirectAccess
        {
            get { return (bool)GetValue(ExtendedEditComboBox.ApplyDirectAccessProperty); }
            set { SetValue(ExtendedEditComboBox.ApplyDirectAccessProperty, value); }
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // IMPORTANT TO REMEMBER: If no data is shown selected, maybe is because absence of Value-Converter.
            // So, maybe the data is present, but cannot be exposed.

            // If no Storage-Field-Name is setted, then manual update is not required
            if (this.StorageFieldName.IsAbsent())
            {
                /*T var hc = this.GetHashCode();
                Console.WriteLine("ExtendedComboBox=" + hc.ToString()); */
                return;
            }

            object SelectionValue = null;

            if (!this.SelectedValuePath.IsAbsent())
                SelectionValue = this.Selector.SelectedValue;
            else
                SelectionValue = this.Selector.SelectedItem;

            //T Console.WriteLine("Field='{0}', SelectionValue=[{1}]", this.StorageFieldName, SelectionValue.ToStringAlways());

            var StoredValue = this.PerformDirectRead(this.StorageFieldName);

            // IMPORTANT: This prevents infinite-loop (must use IsEqual() because of not casting Object)
            if (StoredValue.IsEqual(SelectionValue))
                return;

            if (this.EditingAction != null)
                this.EditingAction(SelectionValue);

            if (this.ApplyDirectAccess)
                this.PerformDirectWrite(this.StorageFieldName, SelectionValue);
        }

        public object SelectedItem
        {
            get { return (object)GetValue(ExtendedEditComboBox.ItemsSourceProperty); }
            set { SetValue(ExtendedEditComboBox.ItemsSourceProperty, value); }
        }
        private static void OnSelectedItemChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditComboBox;

            Target.Selector.SelectedItem = evargs.NewValue;
        }

        public IEnumerable<object> ItemsSource
        {
            get { return (IEnumerable<object>)GetValue(ExtendedEditComboBox.ItemsSourceProperty); }
            set { SetValue(ExtendedEditComboBox.ItemsSourceProperty, value); }
        }
        private static void OnItemsSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditComboBox;
            var NewValue = (IEnumerable<object>)evargs.NewValue;

            Target.Selector.ItemsSource = NewValue;
        }

        public string SelectedValuePath
        {
            get { return (string)GetValue(ExtendedEditComboBox.SelectedValuePathProperty); }
            set { SetValue(ExtendedEditComboBox.SelectedValuePathProperty, value); }
        }
        private static void OnSelectedValuePathChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditComboBox;
            var NewValue = (string)evargs.NewValue;

            Target.Selector.SelectedValuePath = NewValue;
        }

        public string DisplayMemberPath
        {
            get { return (string)GetValue(ExtendedEditComboBox.DisplayMemberPathProperty); }
            set { SetValue(ExtendedEditComboBox.DisplayMemberPathProperty, value); }
        }
        private static void OnDisplayMemberPathChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditComboBox;
            var NewValue = (string)evargs.NewValue;

            Target.Selector.DisplayMemberPath = NewValue;
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(ExtendedEditComboBox.IsReadOnlyProperty); }
            set { SetValue(ExtendedEditComboBox.IsReadOnlyProperty, value); }
        }
        private static void OnIsReadOnlyChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditComboBox;
            var NewValue = (bool)evargs.NewValue;

            Target.Selector.IsReadOnly = NewValue;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            this.SelectNullItemActioner.SetVisible(true);
            this.Selector.Focus();

            this.Selector.IsDropDownOpen = true;
        }

        private void SelectNullItemActioner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Selector.SelectedIndex = -1;
        }
    }
}
