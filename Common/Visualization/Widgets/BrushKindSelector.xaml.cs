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
    /// Interaction logic for BrushKindSelector.xaml
    /// </summary>
    public partial class BrushKindSelector : UserControl
    {
        public BrushKindSelector()
        {
            InitializeComponent();

            Selectors = General.CreateArray(this.BrushNullSelector, this.BrushSolidSelector,
                                            this.BrushGradientDoubleSelector, this.BrushGradientTripleSelector);
        }
        Grid[] Selectors = null;

        public int SelectedBrushKind
        {
            get { return this.SelectedBrushKind_; }
            set
            {
                if (this.SelectedBrushKind_ == value)
                    return;

                this.SelectedBrushKind_ = value;

                this.BrushNullSelector.Background = (this.SelectedBrushKind == 0 ? Brushes.LightGray : Brushes.White);
                this.BrushSolidSelector.Background = (this.SelectedBrushKind == 1 ? Brushes.LightGray : Brushes.White);
                this.BrushGradientDoubleSelector.Background = (this.SelectedBrushKind == 2 ? Brushes.LightGray : Brushes.White);
                this.BrushGradientTripleSelector.Background = (this.SelectedBrushKind == 3 ? Brushes.LightGray : Brushes.White);

                if (this.SelectionAction != null)
                    this.SelectionAction(this.SelectedBrushKind);
            }
        }
        // NOTE: Start from 1 thinking in future use 0 for Drawing, Image or Hatch based brush.
        private int SelectedBrushKind_ = 1;

        public Action<int> SelectionAction = null;

        private void SelectionPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var Selector = sender as Grid;
            this.SelectedBrushKind = Array.IndexOf(Selectors, Selector).EnforceRange(0, 3);
        }
    }
}
