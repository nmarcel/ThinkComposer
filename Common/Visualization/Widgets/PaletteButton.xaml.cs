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
// File   : PaletteButton.cs
// Object : Instrumind.Common.Visualization.Widgets.PaletteButton (Class)
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
    public partial class PaletteButton : Button
    {
        public static readonly DependencyProperty ButtonTextProperty;
        public static readonly DependencyProperty ButtonActionFieldNameProperty;
        public static readonly DependencyProperty ButtonActionFieldSourceProperty;
        public static readonly DependencyProperty ButtonClickActionProperty;
        public static readonly DependencyProperty ButtonShowTextProperty;
        public static readonly DependencyProperty ButtonShowImageProperty;
        public static readonly DependencyProperty ButtonShowShadowProperty;
        public static readonly DependencyProperty ButtonImageProperty;
        public static readonly DependencyProperty ButtonImageSizeProperty;
        public static readonly DependencyProperty ButtonOrientationProperty;

        static PaletteButton()
        {
            // NOTE: Don't change the "<Empty>" text, because is useful as default value for edit-window laucher buttons.
            PaletteButton.ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(PaletteButton),
                new FrameworkPropertyMetadata("<Empty>", new PropertyChangedCallback(OnButtonTextChanged)));

            PaletteButton.ButtonActionFieldNameProperty = DependencyProperty.Register("ButtonActionFieldName", typeof(string), typeof(PaletteButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonActionFieldNameChanged)));

            PaletteButton.ButtonActionFieldSourceProperty = DependencyProperty.Register("ButtonActionFieldSource", typeof(object), typeof(PaletteButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonActionFieldSourceChanged)));

            // Action arguments: Field-Name and Field-Source
            PaletteButton.ButtonClickActionProperty = DependencyProperty.Register("ButtonClickAction", typeof(Action<string, object>), typeof(PaletteButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonClickActionChanged)));

            PaletteButton.ButtonShowTextProperty = DependencyProperty.Register("ButtonShowText", typeof(bool), typeof(PaletteButton),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnButtonShowTextChanged)));

            PaletteButton.ButtonShowImageProperty = DependencyProperty.Register("ButtonShowImage", typeof(bool), typeof(PaletteButton),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnButtonShowImageChanged)));

            PaletteButton.ButtonShowShadowProperty = DependencyProperty.Register("ButtonShowShadow", typeof(bool), typeof(PaletteButton),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnButtonShowShadowChanged)));

            PaletteButton.ButtonImageProperty = DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(PaletteButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonImageChanged)));

            PaletteButton.ButtonImageSizeProperty = DependencyProperty.Register("ButtonImageSize", typeof(double), typeof(PaletteButton),
                new FrameworkPropertyMetadata(Display.ICOSIZE_LIT, new PropertyChangedCallback(OnButtonImageSizeChanged)));

            PaletteButton.ButtonOrientationProperty = DependencyProperty.Register("ButtonOrientation", typeof(Orientation), typeof(PaletteButton),
                new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnButtonOrientationChanged)));
        }

        public PaletteButton()
        {
            InitializeComponent();

            ToolTipService.SetShowOnDisabled(this, true);
        }

        public PaletteButton(WorkCommandExpositor CommandExpositor, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
             : this(CommandExpositor.Name, CommandExpositor.Pictogram, ImageSize, Direction,
                    CommandExpositor.Command, CommandExpositor.CommandParameterExtractor, CommandExpositor.CommandTargetExtractor, CommandExpositor.Summary)
        {
        }

        public PaletteButton(string Text, string ImageLocation, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal,
                             ICommand Command = null, Func<object> CommandParameterExtractor = null, Func<IInputElement> CommandTargetExtractor = null, string Summary = null)
             : this(Text, (ImageLocation.IsAbsent() ? null : Display.GetLibImage(ImageLocation)), ImageSize, Direction,
                    Command, CommandParameterExtractor, CommandTargetExtractor, Summary)
        {
        }

        public PaletteButton(string Text, ImageSource Pictogram, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal,
                             ICommand Command = null, Func<object> CommandParameterExtractor = null, Func<IInputElement> CommandTargetExtractor = null, string Summary = null)
             : this()
        {
            this.ButtonText = Text;
            this.ButtonActionFieldName = null;
            this.ButtonActionFieldSource = null;
            this.ButtonClickAction = null;
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

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            // Setted here where the template is available.
            this.ButtonShowShadow = true;
        }

        public string Summary { get; set; }

        public void SetToolTip(string Text = null)
        {
            if (!Text.IsAbsent())
                this.ToolTip = Text;
            else
                if (this.ToolTip == null || (this.ToolTip is string && this.ToolTip.ToString().IsAbsent()))
                {
                    var TipText = ((!this.ButtonShowText && !this.ButtonText.IsAbsent() ? this.ButtonText + ": " : "") + this.Summary).Trim();

                    if (!TipText.IsAbsent())
                        this.ToolTip = TipText;
                    else
                        this.ToolTip = null;
                }
        }

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public string ButtonActionFieldName
        {
            get { return (string)GetValue(ButtonActionFieldNameProperty); }
            set { SetValue(ButtonActionFieldNameProperty, value); }
        }

        public object ButtonActionFieldSource
        {
            get { return GetValue(ButtonActionFieldSourceProperty); }
            set { SetValue(ButtonActionFieldSourceProperty, value); }
        }

        // Action arguments: Field-Name and Field-Source
        public Action<string, object> ButtonClickAction
        {
            get { return (Action<string, object>)GetValue(ButtonClickActionProperty); }
            set { SetValue(ButtonClickActionProperty, value); }
        }

        public bool ButtonShowText
        {
            get { return (bool)GetValue(ButtonShowTextProperty); }
            set { SetValue(ButtonShowTextProperty, value); }
        }

        public bool ButtonShowImage
        {
            get { return (bool)GetValue(ButtonShowImageProperty); }
            set { SetValue(ButtonShowImageProperty, value); }
        }

        public bool ButtonShowShadow
        {
            get { return (bool)GetValue(ButtonShowShadowProperty); }
            set { SetValue(ButtonShowShadowProperty, value); }
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
            PaletteButton tb = depobj as PaletteButton;
            tb.BtnText.Text = evargs.NewValue as string;

            tb.BtnText.SetVisible(!tb.BtnText.Text.IsAbsent() && tb.ButtonShowText);
            tb.SetToolTip();
        }

        private static void OnButtonActionFieldNameChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.ButtonActionFieldName = evargs.NewValue as string;
        }

        private static void OnButtonActionFieldSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.ButtonActionFieldSource = evargs.NewValue;
        }

        private static void OnButtonClickActionChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.ButtonClickAction = (Action<string, object>)evargs.NewValue;
        }

        private static void OnButtonShowTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.BtnText.SetVisible((bool)evargs.NewValue);
            tb.SetToolTip();
        }

        private static void OnButtonShowImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.BtnImage.SetVisible((bool)evargs.NewValue);
        }

        private static void OnButtonShowShadowChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            var ButtonBorder = tb.GetTemplateChild<Border>("BackBorder");
            if (ButtonBorder == null)
                return;

            /* WPF still has minor problems with fonts over shadow
            if ((bool)evargs.NewValue)
            {
                var Shadow = new System.Windows.Media.Effects.DropShadowEffect();
                Shadow.Color = Colors.Gray;
                Shadow.Opacity = 0.2;
                ButtonBorder.Effect = Shadow;
            }
            else
                ButtonBorder.Effect = null; */
        }

        private static void OnButtonImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.BtnImage.Source = evargs.NewValue as ImageSource;
        }

        private static void OnButtonImageSizeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
            tb.BtnImage.Width = (double)evargs.NewValue;
            tb.BtnImage.Height = (double)evargs.NewValue;
        }

        private static void OnButtonOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            PaletteButton tb = depobj as PaletteButton;
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

            // Later, the appropriate Command of this button is executed (if attached with CommandBinding)

            if (this.ButtonClickAction != null)
                this.ButtonClickAction(this.ButtonActionFieldName, this.ButtonActionFieldSource);
        }
    }
}
