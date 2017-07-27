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
// File   : PaletteToggleButton.cs
// Object : Instrumind.Common.Visualization.Widgets.PaletteToggleButton (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.19 Néstor Sánchez A.  Creation
//
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

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Standard button for tool palettes.
    /// </summary>
    public partial class PaletteToggleButton : ToggleButton
    {
        public static readonly DependencyProperty ButtonTextProperty;
        public static readonly DependencyProperty ButtonShowTextProperty;
        public static readonly DependencyProperty ButtonImageProperty;
        public static readonly DependencyProperty ButtonImageSizeProperty;
        public static readonly DependencyProperty ButtonOrientationProperty;

        static PaletteToggleButton()
        {
            PaletteToggleButton.ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(PaletteToggleButton),
                new FrameworkPropertyMetadata("Button's Text", new PropertyChangedCallback(OnButtonTextChanged)));

            PaletteToggleButton.ButtonShowTextProperty = DependencyProperty.Register("ButtonShowText", typeof(bool), typeof(PaletteToggleButton),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnButtonShowTextChanged)));

            PaletteToggleButton.ButtonImageProperty = DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(PaletteToggleButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonImageChanged)));

            PaletteToggleButton.ButtonImageSizeProperty = DependencyProperty.Register("ButtonImageSize", typeof(double), typeof(PaletteToggleButton),
                new FrameworkPropertyMetadata(Display.ICOSIZE_LIT, new PropertyChangedCallback(OnButtonImageSizeChanged)));

            PaletteToggleButton.ButtonOrientationProperty = DependencyProperty.Register("ButtonOrientation", typeof(Orientation), typeof(PaletteToggleButton),
                new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnButtonOrientationChanged)));
        }

        public PaletteToggleButton()
        {
            InitializeComponent();

            ToolTipService.SetShowOnDisabled(this, true);
        }

        public PaletteToggleButton(WorkCommandExpositor CommandExpositor, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
             : this(CommandExpositor.Name, CommandExpositor.Pictogram, ImageSize, Direction,
                    CommandExpositor.Command, CommandExpositor.CommandParameterExtractor, CommandExpositor.CommandTargetExtractor, CommandExpositor.Summary)
        {
        }

        public PaletteToggleButton(string Text, string ImageLocation, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal,
                             ICommand Command = null, Func<object> CommandParameterExtractor = null, Func<IInputElement> CommandTargetExtractor = null, string Summary = null)
             : this(Text, (ImageLocation.IsAbsent() ? null : Display.GetLibImage(ImageLocation)), ImageSize, Direction,
                    Command, CommandParameterExtractor, CommandTargetExtractor)
        {
        }

        public PaletteToggleButton(string Text, ImageSource Pictogram, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal,
                             ICommand Command = null, Func<object> CommandParameterExtractor = null, Func<IInputElement> CommandTargetExtractor = null, string Summary = null)
             : this()
        {
            this.ButtonText = Text;
            this.BackPanel.Orientation = Direction;
            this.BtnImage.Height = ImageSize;
            this.BtnImage.Width = ImageSize;
            this.BtnImage.Source = Pictogram;
            this.Command = Command;
            this.CommandParameterExtractor = CommandParameterExtractor;
            this.CommandTargetExtractor = CommandTargetExtractor;
            this.Summary = Summary;
            this.SetToolTip();
        }

        public string Summary { get; protected set; }

        public void SetToolTip(string Text = null)
        {
            if (!Text.IsAbsent())
                this.ToolTip = Text;
            else
                if (this.ToolTip == null || (this.ToolTip is string && this.ToolTip.ToString().IsAbsent()))
                    this.ToolTip = (!this.ButtonShowText && !this.ButtonText.IsAbsent() ? this.ButtonText + ": " : "") + this.Summary;
        }

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public bool ButtonShowText
        {
            get { return (bool)GetValue(ButtonShowTextProperty); }
            set { SetValue(ButtonShowTextProperty, value); }
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

        private static void OnButtonTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteToggleButton tb = depobj as PaletteToggleButton;
            tb.BtnText.Text = evargs.NewValue as string;

            tb.BtnText.SetVisible(!tb.BtnText.Text.IsAbsent() && tb.ButtonShowText);
            tb.SetToolTip();
        }

        private static void OnButtonShowTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteToggleButton tb = depobj as PaletteToggleButton;
            tb.BtnText.SetVisible((bool)evargs.NewValue);
            tb.SetToolTip();
        }

        private static void OnButtonImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteToggleButton tb = depobj as PaletteToggleButton;
            tb.BtnImage.Source = evargs.NewValue as ImageSource;
        }

        private static void OnButtonImageSizeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteToggleButton tb = depobj as PaletteToggleButton;
            tb.BtnImage.Width = (double)evargs.NewValue;
            tb.BtnImage.Height = (double)evargs.NewValue;
        }

        private static void OnButtonOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteToggleButton tb = depobj as PaletteToggleButton;
            tb.BackPanel.Orientation = (Orientation)evargs.NewValue;
        }

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
            if (this.CommandParameterExtractor != null)
                this.CommandParameter = this.CommandParameterExtractor();

            if (this.CommandTargetExtractor != null)
                this.CommandTarget = this.CommandTargetExtractor();
        }
    }
}
