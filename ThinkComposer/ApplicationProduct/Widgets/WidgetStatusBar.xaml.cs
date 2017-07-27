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

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetStatusBar.xaml
    /// </summary>
    public partial class WidgetStatusBar : UserControl
    {
        public WidgetStatusBar()
        {
            InitializeComponent();

            this.StampBorder.SetVisible(false);
        }

        public void SetModeText(string Text = null, StampStyle StampKind = StampStyle.Red)
        {
            if (Text.IsAbsent())
            {
                this.StampBorder.SetVisible(false);
                return;
            }

            if (StampKind == StampStyle.Blue)
            {
                this.StampBorder.Background = Brushes.LightBlue;
                this.StampBorder.BorderBrush = Brushes.DodgerBlue;
                this.StampText.Foreground = Brushes.Blue;
            }
            else
                if (StampKind == StampStyle.Green)
                {
                    this.StampBorder.Background = Brushes.Aquamarine;
                    this.StampBorder.BorderBrush = Brushes.Teal;
                    this.StampText.Foreground = Brushes.Green;
                }
                else
                {
                    this.StampBorder.Background = Brushes.Yellow;
                    this.StampBorder.BorderBrush = Brushes.Gold;
                    this.StampText.Foreground = Brushes.Red;
                }

            this.StampBorder.SetVisible(true);
            this.StampText.Text = Text;
        }

        private void BtnScaleDecrease_Click(object sender, RoutedEventArgs e)
        {
            var NewValue = this.SldScaleLevel.Value -= this.SldScaleLevel.SmallChange;

            if (NewValue >= this.SldScaleLevel.Minimum)
                this.SldScaleLevel.Value = NewValue;
        }

        private void BtnScaleIncrease_Click(object sender, RoutedEventArgs e)
        {
            var NewValue = this.SldScaleLevel.Value += this.SldScaleLevel.SmallChange;

            if (NewValue <= this.SldScaleLevel.Maximum)
                this.SldScaleLevel.Value = NewValue;
        }

        private void CompanyLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProductDirector.ShowAbout();
        }

        public enum StampStyle
        {
            Green,
            Blue,
            Red
        }
    }
}
