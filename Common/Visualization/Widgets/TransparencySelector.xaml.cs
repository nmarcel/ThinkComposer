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
    /// Interaction logic for TransparencySelector.xaml
    /// </summary>
    public partial class TransparencySelector : UserControl
    {
        public readonly List<double> OpacityFactors = new List<double>();

        public TransparencySelector()
        {
            InitializeComponent();

            foreach (var Selector in this.SelectorsPanel.Children)
                OpacityFactors.Add(((Shape)Selector).Opacity);
        }

        public double SelectedOpacity
        {
            get { return this.SelectedOpacity_; }
            set
            {
                if (SelectedOpacity == value)
                    return;

                this.SelectedOpacity_ = value;

                var OpacityIndex = OpacityFactors.IndexOf(this.SelectedOpacity);
                if (OpacityIndex >= 0)
                {
                    this.SelectionIndicator.SetVisible(true);
                    Canvas.SetLeft(this.SelectionIndicator, this.SelectionIndicator.Width * OpacityIndex);
                }
                else
                    this.SelectionIndicator.SetVisible(false);

                if (this.SelectionAction != null)
                    this.SelectionAction(this.SelectedOpacity);
            }
        }
        private double SelectedOpacity_ = 1.0;

        public Action<double> SelectionAction = null;

        private void Selector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var Selector = sender as Shape;
            if (Selector == null)
                return;

            var Container = Selector.Parent as Panel;
            if (Container == null)
                return;

            var Index = Container.Children.IndexOf(sender as UIElement);
            if (Index < 0)
                return;

            this.SelectedOpacity = this.OpacityFactors[Index];
        }
    }
}
