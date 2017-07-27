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

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Extends the CheckBox class for direct read/write to Entity properties.
    /// </summary>
    public partial class ExtendedEditCheckBox : CheckBox, IDynamicStoreDataGridEditor
    {
        public static readonly DependencyProperty StorageFieldNameProperty;
        public static readonly DependencyProperty ApplyDirectAccessProperty;

        static ExtendedEditCheckBox()
        {
            ExtendedEditCheckBox.StorageFieldNameProperty = DependencyProperty.Register("StorageFieldName", typeof(string), typeof(ExtendedEditCheckBox),
                                                      new FrameworkPropertyMetadata(null));

            ExtendedEditCheckBox.ApplyDirectAccessProperty = DependencyProperty.Register("ApplyDirectAccess", typeof(bool), typeof(ExtendedEditCheckBox),
                                                      new FrameworkPropertyMetadata(false));
        }

        public ExtendedEditCheckBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Action to be called just after the value has been edited.
        /// </summary>
        public Action<bool> EditingAction = null;

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ApplyDirectAccess && !this.StorageFieldName.IsAbsent())
                this.IsChecked = (bool)this.PerformDirectRead(this.StorageFieldName);
        }

        public string StorageFieldName
        {
            get { return (string)GetValue(ExtendedEditCheckBox.StorageFieldNameProperty); }
            set { SetValue(ExtendedEditCheckBox.StorageFieldNameProperty, value); }
        }

        public bool ApplyDirectAccess
        {
            get { return (bool)GetValue(ExtendedEditCheckBox.ApplyDirectAccessProperty); }
            set { SetValue(ExtendedEditCheckBox.ApplyDirectAccessProperty, value); }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.IsChecked = !this.IsChecked.IsTrue();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (this.StorageFieldName.IsAbsent())
                return;

            var StoredValue = this.PerformDirectRead(this.StorageFieldName);

            // IMPORTANT: This prevents infinite-loop (must use IsEqual() because of not casting Object)
            if (StoredValue.IsEqual(this.IsChecked.IsTrue()))
                return;

            if (this.EditingAction != null)
                this.EditingAction(this.IsChecked.IsTrue());

            if (this.ApplyDirectAccess)
                this.PerformDirectWrite(this.StorageFieldName, this.IsChecked.IsTrue());
        }
    }
}
