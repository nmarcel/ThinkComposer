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
    /// Interaction logic for TextStyleSelector.xaml
    /// </summary>
    public partial class TextStyleSelector : UserControl
    {
        public TextStyleSelector()
        {
            InitializeComponent();
        }

        public bool SelectedBold
        {
            get { return this.SelectedBold_; }
            set
            {
                if (this.SelectedBold_ == value)
                    return;

                this.SelectedBold_ = value;
                this.SelectorBold.Background = (value ? Brushes.LightGray : Brushes.Transparent);
                this.SelectorBold.BorderBrush = (value ? Brushes.Black : Brushes.Transparent);
            }
        }
        bool SelectedBold_ = false;

        public bool SelectedItalic
        {
            get { return this.SelectedItalic_; }
            set
            {
                if (this.SelectedItalic_ == value)
                    return;

                this.SelectedItalic_ = value;
                this.SelectorItalic.Background = (value ? Brushes.LightGray : Brushes.Transparent);
                this.SelectorItalic.BorderBrush = (value ? Brushes.Black : Brushes.Transparent);
            }
        }
        bool SelectedItalic_ = false;

        public bool SelectedUnderline
        {
            get { return this.SelectedUnderline_; }
            set
            {
                if (this.SelectedUnderline_ == value)
                    return;

                this.SelectedUnderline_ = value;
                this.SelectorUnderline.Background = (value ? Brushes.LightGray : Brushes.Transparent);
                this.SelectorUnderline.BorderBrush = (value ? Brushes.Black : Brushes.Transparent);
            }
        }
        bool SelectedUnderline_ = false;

        public bool SelectedStrikethrough
        {
            get { return this.SelectedStrikethrough_; }
            set
            {
                if (this.SelectedStrikethrough_ == value)
                    return;

                this.SelectedStrikethrough_ = value;
                this.SelectorStrikethrough.Background = (value ? Brushes.LightGray : Brushes.Transparent);
                this.SelectorStrikethrough.BorderBrush = (value ? Brushes.Black : Brushes.Transparent);
            }
        }
        bool SelectedStrikethrough_ = false;

        public Action SelectionAction = null;

        private void Selector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var Target = sender as Border;
            if (Target == null || !(Target.Parent is Panel))
                return;

            if (Target == SelectorBold)
                this.SelectedBold = !this.SelectedBold;

            if (Target == SelectorItalic)
                this.SelectedItalic = !this.SelectedItalic;

            if (Target == SelectorUnderline)
                this.SelectedUnderline = !this.SelectedUnderline;

            if (Target == SelectorStrikethrough)
                this.SelectedStrikethrough = !this.SelectedStrikethrough;

            if (this.SelectionAction != null)
                this.SelectionAction();
        }
    }
}
