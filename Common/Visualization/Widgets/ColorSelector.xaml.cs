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
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class ColorSelector : UserControl
    {
        public readonly List<Rectangle> ColorBoxes = new List<Rectangle>();

        public ColorSelector()
        {
            InitializeComponent();

            foreach (var ColumnPanel in this.ColorsPanel.Children)
                foreach (var ColorBox in (ColumnPanel as StackPanel).Children)
                    this.ColorBoxes.Add(ColorBox as Rectangle);
        }

        public Color SelectedColor
        {
            get { return this.SelectedColor_; }
            set
            {
                if (value == this.SelectedColor_)
                    return;

                this.SelectedColor_ = value;

                SetRgbTextBoxesFromColor(value);

                var TargetColorBox = this.ColorBoxes.FirstOrDefault(colorbox => (colorbox.Fill as SolidColorBrush).Color == this.SelectedColor_);
                if (TargetColorBox != null)
                    SelectColorBox(TargetColorBox);
                else
                    if (CurrentSelectedColorBox != null)
                        UnselectColorBox(CurrentSelectedColorBox);

                this.UpdateFineSelectors();
            }
        }
        private Color SelectedColor_;

        private void Selector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var TargetColorBox = sender as Rectangle;
            if (TargetColorBox == null)
                return;

            this.SelectedColor = (TargetColorBox.Fill as SolidColorBrush).Color;

            this.UpdateFineSelectors();

            if (this.SelectionAction != null)
                this.SelectionAction(this.SelectedColor, e.ClickCount >= 2);
        }

        private void FineSelectionBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectedColor = Display.GetPixelColor(PointToScreen(Mouse.GetPosition(this)));

            if (this.SelectionAction != null)
                this.SelectionAction(this.SelectedColor, e.ClickCount >= 2);
        }

        private void UpdateFineSelectors()
        {
            this.FineSelectionBorder.Background = Display.GetGradientBrush(Colors.White, this.SelectedColor, Colors.Black, true, 0.0, 0.5, 1.0);
            this.FineSelectionRedBorder.Background = Display.GetGradientBrush(this.SelectedColor, Colors.Red, true, 0.0, 1.0);
            this.FineSelectionGreenBorder.Background = Display.GetGradientBrush(this.SelectedColor, Colors.Lime, true, 0.0, 1.0);
            this.FineSelectionBlueBorder.Background = Display.GetGradientBrush(this.SelectedColor, Colors.Blue, true, 0.0, 1.0);
        }

        /// <summary>
        /// Passes color and indication of double-click
        /// </summary>
        public Action<Color, bool> SelectionAction = null;

        private void SelectColorBox(Rectangle Target)
        {
            if (CurrentSelectedColorBox != null)
                UnselectColorBox(CurrentSelectedColorBox);

            // Target.Margin = new Thickness(0);
            Target.Stroke = Brushes.Black;
            /* Target.StrokeThickness = 3;
            Target.Width = 18.0;
            Target.Height = 16.0; */

            CurrentSelectedColorBox = Target;
        }

        private void UnselectColorBox(Rectangle Target)
        {
            // Target.Margin = new Thickness(2);
            Target.Stroke = Brushes.Transparent;
            /* Target.StrokeThickness = 1.0;
            Target.Width = 14.0;
            Target.Height = 12.0; */
        }
        private Rectangle CurrentSelectedColorBox = null;

        public void SetRgbTextBoxesFromColor(Color Source)
        {
            this.TxtRgbRed.Text = Source.R.ToString();
            this.TxtRgbGreen.Text = Source.G.ToString();
            this.TxtRgbBlue.Text = Source.B.ToString();

            // this.TxtRgbHex.Text = "#" + (Source.R*256*256 + Source.G*256 + Source.B).ToString("X");
            this.TxtRgbHex.Text = Source.R.ToString("X2") +
                                  Source.G.ToString("X2") +
                                  Source.B.ToString("X2");
        }

        public void SetColorFromRgbTextBoxes(string Hex, string Red, string Green, string Blue)
        {
            byte R = 0, G = 0, B = 0;

            if (!Hex.IsAbsent())
            {
                Hex = ("000000" + Hex).GetRight(6);

                if (!Byte.TryParse(Hex.GetMid(0, 2), System.Globalization.NumberStyles.HexNumber,
                                   System.Globalization.CultureInfo.CurrentCulture.NumberFormat, out R)
                    || !Byte.TryParse(Hex.GetMid(2, 2), System.Globalization.NumberStyles.HexNumber,
                                      System.Globalization.CultureInfo.CurrentCulture.NumberFormat, out G)
                    || !Byte.TryParse(Hex.GetMid(4, 2), System.Globalization.NumberStyles.HexNumber,
                                      System.Globalization.CultureInfo.CurrentCulture.NumberFormat, out B))
                    return;
            }
            else
            {
                Red = Red.AbsentDefault("0");
                Green = Green.AbsentDefault("0");
                Blue = Blue.AbsentDefault("0");

                if (!Byte.TryParse(Red, out R)
                    || !Byte.TryParse(Green, out G)
                    || !Byte.TryParse(Blue, out B))
                    return;
            }

            this.SelectedColor = Color.FromRgb(R, G, B);

            this.UpdateFineSelectors();

            if (this.SelectionAction != null)
                this.SelectionAction(this.SelectedColor, false);
        }

        private void TxtRgbRed_LostFocus(object sender, RoutedEventArgs e)
        {
            SetColorFromRgbTextBoxes(null, TxtRgbRed.Text, TxtRgbGreen.Text, TxtRgbBlue.Text);
        }

        private void TxtRgbGreen_LostFocus(object sender, RoutedEventArgs e)
        {
            SetColorFromRgbTextBoxes(null, TxtRgbRed.Text, TxtRgbGreen.Text, TxtRgbBlue.Text);
        }

        private void TxtRgbBlue_LostFocus(object sender, RoutedEventArgs e)
        {
            SetColorFromRgbTextBoxes(null, TxtRgbRed.Text, TxtRgbGreen.Text, TxtRgbBlue.Text);
        }

        private void TxtRgbHex_LostFocus(object sender, RoutedEventArgs e)
        {
            TxtRgbHex.Text = TxtRgbHex.Text.Trim().ToUpper().Replace("0X", "").Replace("#", "");
            SetColorFromRgbTextBoxes(TxtRgbHex.Text, null, null, null);
        }

    }
}
