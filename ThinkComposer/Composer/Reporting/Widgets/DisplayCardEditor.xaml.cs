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
    /// Interaction logic for DisplayCardEditor.xaml
    /// </summary>
    public partial class DisplayCardEditor : UserControl
    {
        public static readonly DependencyProperty SourceValueProperty;
        public static readonly DependencyProperty SourceNameProperty;
        public static readonly DependencyProperty TitleWidthProperty;
        public static readonly DependencyProperty IncludeDefinitorProperty;

        static DisplayCardEditor()
        {
            DisplayCardEditor.SourceValueProperty = DependencyProperty.Register("SourceValue", typeof(DisplayCard), typeof(DisplayCardEditor),
                new FrameworkPropertyMetadata(new DisplayCard(), new PropertyChangedCallback(OnSourceValueChanged)));
            
            DisplayCardEditor.SourceNameProperty = DependencyProperty.Register("SourceName", typeof(string), typeof(DisplayCardEditor),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnSourceNameChanged)));

            DisplayCardEditor.TitleWidthProperty = DependencyProperty.Register("TitleWidth", typeof(double), typeof(DisplayCardEditor),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnTitleWidthChanged)));

            DisplayCardEditor.IncludeDefinitorProperty = DependencyProperty.Register("IncludeDefinitor", typeof(bool), typeof(DisplayCardEditor),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIncludeDefinitorChanged)));
        }

        public DisplayCardEditor()
        {
            InitializeComponent();
        }

        public DisplayCard SourceValue
        {
            get { return (DisplayCard)GetValue(DisplayCardEditor.SourceValueProperty); }
            set { SetValue(DisplayCardEditor.SourceValueProperty, value); }
        }
        private static void OnSourceValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayCardEditor)depobj;
            Target.DataPanel.DataContext = (DisplayCard)evargs.NewValue;
        }

        public string SourceName
        {
            get { return (string)GetValue(DisplayCardEditor.SourceNameProperty); }
            set { SetValue(DisplayCardEditor.SourceNameProperty, value); }
        }
        private static void OnSourceNameChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayCardEditor)depobj;
            Target.CbxTitle.Content = (string)evargs.NewValue;
        }

        public double TitleWidth
        {
            get { return (double)GetValue(DisplayCardEditor.TitleWidthProperty); }
            set { SetValue(DisplayCardEditor.TitleWidthProperty, value); }
        }
        private static void OnTitleWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayCardEditor)depobj;
            Target.CbxTitle.Width = ((double)evargs.NewValue + 12.0);   // 12.0 is the (estimated) size of the check-box square.
        }

        public bool IncludeDefinitor
        {
            get { return (bool)GetValue(DisplayCardEditor.IncludeDefinitorProperty); }
            set { SetValue(DisplayCardEditor.IncludeDefinitorProperty, value); }
        }
        private static void OnIncludeDefinitorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (DisplayCardEditor)depobj;
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
