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
    /// Interaction logic for BrushTypeSelector.xaml
    /// </summary>
    public partial class BrushTypeSelector : UserControl
    {
        public BrushTypeSelector()
        {
            InitializeComponent();
        }

        public int SelectedBrushType
        {
            get { return this.SelectedBrushType_; }
            set
            {
                if (this.SelectedBrushType_ == value)
                    return;

                this.SelectedBrushType_ = value;

                this.BrushTypeSolid.Fill = (this.SelectedBrushType == 1 ? Brushes.LightGray : Brushes.White);
                this.BrushTypeGradientDouble.Fill = (this.SelectedBrushType == 2 ? Brushes.LightGray : Brushes.White);
                this.BrushTypeGradientTriple.Fill = (this.SelectedBrushType == 3 ? Brushes.LightGray : Brushes.White);

                if (this.SelectionAction != null)
                    this.SelectionAction(this.SelectedBrushType);
            }
        }
        // NOTE: Start from 1 thinking in future use 0 for Drawing, Image or Hatch based brush.
        private int SelectedBrushType_ = 1;

        public Orientation Direction
        {
            get { return this.Direction_; }
            set
            {
                if (this.Direction_ == value)
                    return;

                this.Direction_ = value;

                if (this.Direction == Orientation.Vertical)
                    this.BackPanel.LayoutTransform = new RotateTransform(270.0);
                else
                    this.BackPanel.LayoutTransform = null;
            }
        }
        private Orientation Direction_ = Orientation.Horizontal;

        public Action<int> SelectionAction = null;

        private void SelectionPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.SelectedBrushType >= 3)
                this.SelectedBrushType = 1;
            else
                this.SelectedBrushType++;
        }
    }
}
