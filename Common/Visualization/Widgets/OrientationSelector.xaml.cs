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
    /// Interaction logic for OrientationSelector.xaml
    /// </summary>
    public partial class OrientationSelector : UserControl
    {
        public OrientationSelector()
        {
            InitializeComponent();
        }

        public Orientation SelectedOrientation
        {
            get { return this.SelectedOrientation_; }
            set
            {
                if (this.SelectedOrientation_ == value)
                    return;

                this.SelectedOrientation_ = value;

                var PreviousFill = this.OrientationSelectorVertical.Fill;
                this.OrientationSelectorVertical.Fill = this.OrientationSelectorHorizontal.Fill;
                this.OrientationSelectorHorizontal.Fill = PreviousFill;

                if (this.SelectionAction != null)
                    this.SelectionAction(this.SelectedOrientation);
            }
        }
        private Orientation SelectedOrientation_ = Orientation.Vertical;

        public Action<Orientation> SelectionAction = null;

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectedOrientation = (this.SelectedOrientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical);
        }
    }
}
