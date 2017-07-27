using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Instrumind.Common;
using Instrumind.Common.Visualization;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class BrushSelector : UserControl
    {
        public BrushSelector()
        {
            InitializeComponent();
        }

        public BrushSelector(Brush InitialBrush)
             : this()
        {
            var InitialGradient = InitialBrush as LinearGradientBrush;
            if (InitialGradient != null)
            {
                GradientOffsetInitial = InitialGradient.GradientStops[0].Offset;
                if (InitialGradient.GradientStops.Count == 2)
                    GradientOffsetFinal = InitialGradient.GradientStops[1].Offset;
                else
                {
                    GradientOffsetCentral = InitialGradient.GradientStops[1].Offset;
                    GradientOffsetFinal = InitialGradient.GradientStops[2].Offset;
                }
            }

            this.InitialBrush = InitialBrush;
        }

        double GradientOffsetInitial = Display.GRADIENT_OFFSET_INITIAL;
        double GradientOffsetCentral = Display.GRADIENT_OFFSET_CENTRAL;
        double GradientOffsetFinal = Display.GRADIENT_OFFSET_FINAL;

        bool IsOverPopup = false;

        DialogOptionsWindow ParentWindow = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Required because the poup loads the control even when not yet shown.
            if (!this.IsVisible)
                return;

            this.IsOverPopup = this.Parent is Popup;

            if (this.IsOverPopup)
            {
                this.Background = Brushes.White;
                this.BorderThickness = new Thickness(0.5);

                this.BtnOK.IsDefault = false;
                this.BtnCancel.IsCancel = false;

                /*- this.HelpText.Text = "Hold the Ctrl key to extend selection.";
                this.BtnOK.SetVisible(false);
                this.BtnCancel.SetVisible(false); */
            }

            this.ParentWindow = this.GetNearestVisualDominantOfType<DialogOptionsWindow>();

            if (this.ParentWindow != null)
            {
                this.ParentWindow.MinWidth = 200;
                this.ParentWindow.ResizeMode = ResizeMode.NoResize;
            }

            this.SelectedBrush = (this.InitialBrush == null ? null : this.InitialBrush.Clone());

            this.BrushKindPicker.SelectionAction =
                (TypeIndex =>
                    {
                        this.PrimaryBrushColorsPalette.IsSelected = true;

                        GenerateSelectedBrush();
                    });

            this.OrientationPicker.SelectionAction =
                (direction =>
                    {
                        if (direction == Orientation.Vertical)
                            this.BrushColorsTab.TabStripPlacement = Dock.Left;
                        else
                            this.BrushColorsTab.TabStripPlacement = Dock.Top;

                        GenerateSelectedBrush();
                    });

            this.PrimaryColorPicker.SelectionAction = ((color, doubleclicked) => GenerateSelectedBrush(true, doubleclicked));
            this.SecondaryColorPicker.SelectionAction = ((color, doubleclicked) => GenerateSelectedBrush(true, doubleclicked));
            this.TertiaryColorPicker.SelectionAction = ((color, doubleclicked) => GenerateSelectedBrush(true, doubleclicked));

            this.TransparencyPicker.SelectionAction = (opacity => GenerateSelectedBrush());

            this.IsReadyForSelection = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.IsReadyForSelection = false;
        }

        public bool IsReadyForSelection { get; protected set; }

        public Brush InitialBrush { get; internal set; }

        public Brush SelectedBrush
        {
            get { return this.SelectedBrush_; }
            protected set
            {
                this.SelectedBrush_ = value;

                var Direction = this.SelectedBrush_.GetGradientOrientation();
                var Multiplicity = this.SelectedBrush_.GetGradientMultiplicity();

                this.BrushKindPicker.SelectedBrushKind = Multiplicity;

                this.OrientationPicker.SelectedOrientation = Direction;
                this.OrientationPicker.Visibility = (Multiplicity > 1 ? Visibility.Visible : Visibility.Hidden);

                this.TransparencyPicker.SelectedOpacity = (this.SelectedBrush == null ? 1.0 : this.SelectedBrush.Opacity);

                this.SelectedBrushExpo.Fill = this.SelectedBrush;

                this.PrimaryColorPicker.SelectedColor = (Multiplicity == 0 ? Colors.White : (Multiplicity == 1 ? ((SolidColorBrush)this.SelectedBrush).Color : ((LinearGradientBrush)this.SelectedBrush).GradientStops[0].Color));
                this.PrimaryPaletteHeader.Fill = this.PrimaryColorPicker.SelectedColor.GetStandardBrush();

                this.SecondaryBrushColorsPalette.SetVisible(Multiplicity >= 2);
                this.SecondaryColorPicker.SelectedColor = (Multiplicity >= 2 ? ((LinearGradientBrush)this.SelectedBrush).GradientStops[1].Color : Colors.White);
                this.SecondaryPaletteHeader.Fill = this.SecondaryColorPicker.SelectedColor.GetStandardBrush();

                this.TertiaryColorPicker.SelectedColor = (Multiplicity >= 3 ? ((LinearGradientBrush)this.SelectedBrush).GradientStops[2].Color : Colors.White);
                this.TertiaryBrushColorsPalette.SetVisible(Multiplicity >= 3);
                this.TertiaryPaletteHeader.Fill = this.TertiaryColorPicker.SelectedColor.GetStandardBrush();
            }
        }
        private Brush SelectedBrush_ = null;

        public void GenerateSelectedBrush(bool ColorJustSelected = false, bool ApplyAndExit = false)
        {
            Brush Result = null;

            if (ColorJustSelected && this.BrushKindPicker.SelectedBrushKind == 0)
            {
                this.BrushKindPicker.SelectedBrushKind = 1; // this will re-call this method
                return;
            }

            if (this.BrushKindPicker.SelectedBrushKind == 0)
                Result = null;
            else
            {
                if (this.BrushKindPicker.SelectedBrushKind == 1)
                    Result = new SolidColorBrush(this.PrimaryColorPicker.SelectedColor);
                else
                {
                    var EndPoint = (this.OrientationPicker.SelectedOrientation == Orientation.Vertical ? Display.DIR_ENDPOINT_VERTICAL : Display.DIR_ENDPOINT_HORIZONTAL);
                    var Stops = new GradientStopCollection(this.BrushKindPicker.SelectedBrushKind);
                    Stops.Add(new GradientStop(this.PrimaryColorPicker.SelectedColor, GradientOffsetInitial));
                    if (this.BrushKindPicker.SelectedBrushKind == 2)
                        Stops.Add(new GradientStop(this.SecondaryColorPicker.SelectedColor, GradientOffsetFinal));
                    else
                    {
                        Stops.Add(new GradientStop(this.SecondaryColorPicker.SelectedColor, GradientOffsetCentral));
                        Stops.Add(new GradientStop(this.TertiaryColorPicker.SelectedColor, GradientOffsetFinal));
                    }

                    Result = new LinearGradientBrush(Stops, Display.ZERO_POINT, EndPoint);
                }

                Result.Opacity = this.TransparencyPicker.SelectedOpacity;
                Result.Freeze();    // Important to allow use in other threads, such as for file/code-generation.
            }

            this.SelectedBrush = Result;

            /*- if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                && this.IsOverPopup)
                ApplyAndExit = true; */

            if (ApplyAndExit && this.IsReadyForSelection)
                BtnOK_Click(this, null);
        }

        // Selection action: If null then cancelled, else if the Item1 is null then represents an empty brush.
        public Action<Tuple<Brush>> SelectionAction = null;

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsOverPopup && e != null)
                e.Handled = true;

            if (this.SelectionAction == null)
                return;

            if (!this.IsOverPopup && this.ParentWindow != null)
                this.ParentWindow.DialogResult = true;

            SelectionAction(Tuple.Create(this.SelectedBrush));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsOverPopup && e != null)
                e.Handled = true;

            if (this.SelectionAction == null)
                return;

            if (!this.IsOverPopup && this.ParentWindow != null)
                this.ParentWindow.DialogResult = false;

            SelectionAction(null);
        }
    }
}
