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

using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common;

namespace Instrumind.ThinkComposer.Composer.Reporting.Widgets
{
    /// <summary>
    /// Interaction logic for DisplayListEditor.xaml
    /// </summary>
    public partial class DisplayListEditor : UserControl
    {
        public static readonly DependencyProperty SourceValueProperty;
        public static readonly DependencyProperty SourceNameProperty;
        public static readonly DependencyProperty TitleWidthProperty;
        public static readonly DependencyProperty IncludeDefinitorProperty;

        static DisplayListEditor()
        {
            DisplayListEditor.SourceValueProperty = DependencyProperty.Register("SourceValue", typeof(DisplayList), typeof(DisplayListEditor),
                new FrameworkPropertyMetadata(new DisplayList(), new PropertyChangedCallback(OnSourceValueChanged)));
            
            DisplayListEditor.SourceNameProperty = DependencyProperty.Register("SourceName", typeof(string), typeof(DisplayListEditor),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnSourceNameChanged)));

            DisplayListEditor.TitleWidthProperty = DependencyProperty.Register("TitleWidth", typeof(double), typeof(DisplayListEditor),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnTitleWidthChanged)));

            DisplayListEditor.IncludeDefinitorProperty = DependencyProperty.Register("IncludeDefinitor", typeof(bool), typeof(DisplayListEditor),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIncludeDefinitorChanged)));
        }

        public DisplayListEditor()
        {
            InitializeComponent();
        }

        public DisplayList SourceValue
        {
            get { return (DisplayList)GetValue(DisplayListEditor.SourceValueProperty); }
            set { SetValue(DisplayListEditor.SourceValueProperty, value); }
        }
        private static void OnSourceValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayListEditor)depobj;
            Target.DataPanel.DataContext = (DisplayList)evargs.NewValue;
        }

        public string SourceName
        {
            get { return (string)GetValue(DisplayListEditor.SourceNameProperty); }
            set { SetValue(DisplayListEditor.SourceNameProperty, value); }
        }
        private static void OnSourceNameChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayListEditor)depobj;
            Target.CbxTitle.Content = (string)evargs.NewValue;
        }

        public double TitleWidth
        {
            get { return (double)GetValue(DisplayListEditor.TitleWidthProperty); }
            set { SetValue(DisplayListEditor.TitleWidthProperty, value); }
        }
        private static void OnTitleWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayListEditor)depobj;
            Target.CbxTitle.Width = ((double)evargs.NewValue + 12.0);   // 12.0 is the (estimated) size of the check-box square.
        }

        public bool IncludeDefinitor
        {
            get { return (bool)GetValue(DisplayListEditor.IncludeDefinitorProperty); }
            set { SetValue(DisplayListEditor.IncludeDefinitorProperty, value); }
        }
        private static void OnIncludeDefinitorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayListEditor)depobj;
            var Include = (bool)evargs.NewValue;
            Target.CbxDefinitor.SetVisible(Include);
            if (!Include)
                Target.CbxDefinitor.IsChecked = false;
        }

        private void BtnExpander_Click(object sender, RoutedEventArgs e)
        {
            this.BrdDetailedConfig.SetVisible(this.BrdDetailedConfig.IsVisible);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Make sure that at least one (the first) checkbox is checked.
            var ExistChecked = false;

            foreach (var Child in this.WpnChecks.Children)
                if (Child is CheckBox && ((CheckBox)Child).IsChecked.IsTrue())
                {
                    ExistChecked = true;
                    break;
                }

            if (!ExistChecked && this.WpnChecks.Children.Count > 0
                && this.WpnChecks.Children[0] is CheckBox)
                ((CheckBox)this.WpnChecks.Children[0]).IsChecked = true;
        }
    }
}
