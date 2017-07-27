// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : PaletteDropColorizerButton.cs
// Object : Instrumind.Common.Visualization.Widgets.PaletteDropColorizerButton (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.08.23 Néstor Sánchez A.  Creation
//
using System;
using System.Collections.Generic;
using System.Collections;
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

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Standard button for tool palettes.
    /// </summary>
    public partial class PaletteDropColorizerButton : Button
    {
        public static readonly DependencyProperty CurrentBrushProperty;
        public static readonly DependencyProperty ButtonTextProperty;
        public static readonly DependencyProperty ButtonImageProperty;
        public static readonly DependencyProperty ButtonImageSizeProperty;
        public static readonly DependencyProperty ButtonOrientationProperty;
        public static readonly DependencyProperty ButtonShowTextProperty;

        static PaletteDropColorizerButton()
        {
            PaletteDropColorizerButton.CurrentBrushProperty = DependencyProperty.Register("CurrentBrush", typeof(Brush), typeof(PaletteDropColorizerButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCurrentBrushChanged)));

            /*- PaletteDropColorizerButton.ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(PaletteDropColorizerButton),
                new FrameworkPropertyMetadata("Button's Text", new PropertyChangedCallback(OnButtonTextChanged)));

            PaletteDropColorizerButton.ButtonImageProperty = DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(PaletteDropColorizerButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonImageChanged)));

            PaletteDropColorizerButton.ButtonImageSizeProperty = DependencyProperty.Register("ButtonImageSize", typeof(double), typeof(PaletteDropColorizerButton),
                new FrameworkPropertyMetadata(Display.ICOSIZE_LIT, new PropertyChangedCallback(OnButtonImageSizeChanged)));

            PaletteDropColorizerButton.ButtonOrientationProperty = DependencyProperty.Register("ButtonOrientation", typeof(Orientation), typeof(PaletteDropColorizerButton),
                new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnButtonOrientationChanged)));

            PaletteDropColorizerButton.ButtonShowTextProperty = DependencyProperty.Register("ButtonShowText", typeof(bool), typeof(PaletteDropColorizerButton),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnButtonShowTextChanged))); */
        }

        public PaletteDropColorizerButton()
        {
            InitializeComponent();
        }

        private void ThisControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentExpositor = this.GetNearestVisualDominantOfType<EntityPropertyExpositor>();

            this.Colorizer.SelectionAction =
                (brush) =>
                {
                    if (brush == null)  // If selection cancelled, then exit
                    {
                        this.PostCall(pdcb => pdcb.ShowColorizer(false));
                        return;
                    }

                    this.CurrentBrush = brush.Item1;

                    this.PostCall(pdcb => pdcb.ShowColorizer(false));
                };
        }

        public EntityPropertyExpositor ParentExpositor { get; protected set; }

        public Brush CurrentBrush
        {
            get { return (Brush)GetValue(PaletteDropColorizerButton.CurrentBrushProperty); }
            set { SetValue(PaletteDropColorizerButton.CurrentBrushProperty, value); }
        }

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public ImageSource ButtonImage
        {
            get { return (ImageSource)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }

        public double ButtonImageSize
        {
            get { return (double)GetValue(ButtonImageSizeProperty); }
            set { SetValue(ButtonImageSizeProperty, value); }
        }

        public Orientation ButtonOrientation
        {
            get { return (Orientation)GetValue(ButtonOrientationProperty); }
            set { SetValue(ButtonOrientationProperty, value); }
        }

        public bool ButtonShowText
        {
            get { return (bool)GetValue(ButtonShowTextProperty); }
            set { SetValue(ButtonShowTextProperty, value); }
        }

        /// <summary>
        /// Action to be called just after the brush has been selected.
        /// </summary>
        public Action<Brush> SelectionAction = null;

        private static void OnCurrentBrushChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (PaletteDropColorizerButton)depobj;
            var NewBrush = (Brush)evargs.NewValue;

            Target.ImageBackgroundBorder.Background = NewBrush;

            if (Target.SelectionAction != null)
                Target.SelectionAction(NewBrush);
        }

        /*-
        private static void OnButtonTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropColorizerButton;
            btn.BtnText.Text = evargs.NewValue as string;

            btn.BtnText.SetVisible(btn.BtnText.Text.IsAbsent());
        }

        private static void OnButtonImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropColorizerButton;
            btn.BtnImage.Source = evargs.NewValue as ImageSource;
        }

        private static void OnButtonImageSizeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropColorizerButton;
            btn.BtnImage.Width = (double)evargs.NewValue;
            btn.BtnImage.Height = (double)evargs.NewValue;
        }

        private static void OnButtonOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropColorizerButton;
            btn.BackPanel.Orientation = (Orientation)evargs.NewValue;
        }

        private static void OnButtonShowTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropColorizerButton;
            btn.BtnText.SetVisible((bool)evargs.NewValue);
        }*/

        /// <summary>
        /// Function to provide the command parameter.
        /// </summary>
        public Func<object> CommandParameterExtractor { get; protected set; }

        /// <summary>
        /// Function to provide the command target.
        /// </summary>
        public Func<IInputElement> CommandTargetExtractor { get; protected set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.ClickDropsColorizer)
            {
                this.BtnDropper_Click(this, null);
                return;
            }

            if (this.CommandParameterExtractor != null)
                this.CommandParameter = this.CommandParameterExtractor();

            if (this.CommandTargetExtractor != null)
                this.CommandTarget = this.CommandTargetExtractor();

            this.ShowColorizer(false);

            if (this.SelectionAction != null)
                this.SelectionAction(this.CurrentBrush);
        }

        /// <summary>
        /// Intended to be used in property-expositor/editing.
        /// Maybe should be a Dependency Property.
        /// </summary>
        public bool ClickDropsColorizer = false;

        private void BtnDropper_Click(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            this.PostCall(pdcb => pdcb.ShowColorizer(!pdcb.PopupArea.IsOpen));
        }

        private void PopupArea_Opened(object sender, EventArgs e)
        {
            this.ParentExpositor.CanProcessLostFocus = false;
        }

        private void PopupArea_Closed(object sender, EventArgs e)
        {
            this.ParentExpositor.CanProcessLostFocus = true;
        }

        private void PopupArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Avoids to click the containing button
            e.Handled = true;
        }

        public void ShowColorizer(bool DoShow)
        {
            if (DoShow)
                this.Colorizer.InitialBrush = this.CurrentBrush;

            this.PopupArea.IsOpen = DoShow;
        }

        private void ThisControl_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            this.PostCall(
                (pdcb) =>
                {
                    var Focuser = FocusManager.GetFocusedElement(FocusManager.GetFocusScope(pdcb.PopupArea)) as FrameworkElement;

                    if (Focuser != null)
                    {
                        var Focused = Focuser.GetNearestDominantOfType<Popup>();

                        if (Focused == null)
                            this.ShowColorizer(false);
                    }
                });
        }
    }
}
