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

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetNavMap.xaml
    /// </summary>
    public partial class WidgetNavMap : UserControl
    {
        public ScrollViewer MapSource
        {
            get { return this.MapSource_; }
            internal set
            {
                this.MapSource_ = value;
                this.ViewboxMap.DataContext = this;
            }
        }
        protected ScrollViewer MapSource_ = null;

        public int Magnification { get; internal set; }

        public double ZoneWidth { get { return this.MapSource.ViewportWidth * this.Magnification / 100.0; } }

        public double ZoneHeight { get { return this.MapSource.ViewportHeight * this.Magnification / 100.0; } }

        public WidgetNavMap()
        {
            InitializeComponent();
        }
    }
}
