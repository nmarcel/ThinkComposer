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

using Instrumind.Common;
using Instrumind.Common.Visualization;

namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Interaction logic for AppOptions.xaml
    /// </summary>
    public partial class AppOptions : UserControl
    {
        public AppOptions()
        {
            InitializeComponent();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }
    }
}
