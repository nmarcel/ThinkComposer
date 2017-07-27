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
// File   : PaletteDropButton.cs
// Object : Instrumind.Common.Visualization.Widgets.PaletteDropButton (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.19 Néstor Sánchez A.  Creation
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
    public partial class PaletteDropButton : Button
    {
        public static readonly DependencyProperty ButtonTextProperty;
        public static readonly DependencyProperty ButtonImageProperty;
        public static readonly DependencyProperty ButtonImageSizeProperty;
        public static readonly DependencyProperty ButtonOrientationProperty;
        public static readonly DependencyProperty ButtonShowTextProperty;

        static PaletteDropButton()
        {
            PaletteDropButton.ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(PaletteDropButton),
                new FrameworkPropertyMetadata("Button's Text", new PropertyChangedCallback(OnButtonTextChanged)));

            PaletteDropButton.ButtonImageProperty = DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(PaletteDropButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonImageChanged)));

            PaletteDropButton.ButtonImageSizeProperty = DependencyProperty.Register("ButtonImageSize", typeof(double), typeof(PaletteDropButton),
                new FrameworkPropertyMetadata(Display.ICOSIZE_LIT, new PropertyChangedCallback(OnButtonImageSizeChanged)));

            PaletteDropButton.ButtonOrientationProperty = DependencyProperty.Register("ButtonOrientation", typeof(Orientation), typeof(PaletteDropButton),
                new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnButtonOrientationChanged)));

            PaletteDropButton.ButtonShowTextProperty = DependencyProperty.Register("ButtonShowText", typeof(bool), typeof(PaletteDropButton),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnButtonShowTextChanged)));
        }

        public PaletteDropButton()
        {
            InitializeComponent();

            ToolTipService.SetShowOnDisabled(this, true);
        }

        public PaletteDropButton(WorkCommandExpositor[] SelectableOptions, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
            : this((object[])SelectableOptions, ImageSize, Direction)
        {
        }

        public PaletteDropButton(Tuple<IRecognizableElement, Action<object>>[] SelectableOptions, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
            : this((object[])SelectableOptions, ImageSize, Direction)
        {
        }

        protected PaletteDropButton(object[] SelectableOptions, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
        {
            this.SetOptions(ImageSize, Direction, SelectableOptions);
        }

        public void SetOptions(WorkCommandExpositor[] SelectableOptions, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
        {
            this.SetOptions(ImageSize, Direction, SelectableOptions);
        }

        public void SetOptions(Tuple<IRecognizableElement, Action<object>>[] SelectableOptions, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
        {
            this.SetOptions(ImageSize, Direction, SelectableOptions);
        }

        // Notice the change in parameters order respect its overloaded siblings to avoid recursive/infinite-loop calls
        protected void SetOptions(double ImageSize, Orientation Direction, object[] SelectableOptions)
        {
            General.ContractRequires(SelectableOptions.Length > 0);

            this.OptionButtons.Clear();
            this.LstboxOptionButtons.Items.Clear();

            foreach (var Declaration in SelectableOptions)
            {
                // Notice that command is not attached to the generated option-button
                var NewButton = Display.GenerateButton(Declaration, this.BtnImage.Height, this.BackPanel.Orientation, false);
                NewButton.Click +=
                    ((sender, rtdevargs) =>
                    {
                        rtdevargs.Handled = true;
                        this.LstboxOptionButtons.SelectedItem = NewButton;
                    });

                this.OptionButtons.Add(NewButton, Declaration);
                this.LstboxOptionButtons.Items.Add(NewButton);
            }

            this.BackPanel.Orientation = Direction;
            this.BtnImage.Height = ImageSize;
            this.BtnImage.Width = ImageSize;

            SetSelectedButton(OptionButtons.First().Key);

            if (SelectableOptions.Length == 1)
                this.BtnDropper.SetVisible(false);
        }

        protected Dictionary<PaletteButton, object> OptionButtons = new Dictionary<PaletteButton, object>();
        protected int CurrentOptionButtonIndex = -1;

        /// <summary>
        /// Provide this function to pass your desired parameter to the Operation action.
        /// Else this Button is passed.
        /// </summary>
        public Func<object> OperationParameterExtractor;

        protected Action<object> Operation;

        protected void SetSelectedButton(PaletteButton SelectedButton)
        {
            var OptionButtonSource = OptionButtons[SelectedButton];

            var SourceExpositor = OptionButtonSource as WorkCommandExpositor;
            if (SourceExpositor != null)
            {
                this.BtnText.Text = SourceExpositor.Name;
                this.BtnImage.Source = SourceExpositor.Pictogram;
                this.Command = SourceExpositor.Command;
                this.CommandParameterExtractor = SourceExpositor.CommandParameterExtractor;
                this.CommandTargetExtractor = SourceExpositor.CommandTargetExtractor;
                this.ToolTip = SourceExpositor.Summary;
                // In this case the execution is by Command-Binding
                this.Operation = null;
            }

            var SourceDeclaration = OptionButtonSource as Tuple<IRecognizableElement,Action<object>>;
            if (SourceDeclaration != null)
            {
                this.BtnText.Text = SourceDeclaration.Item1.Name;
                this.BtnImage.Source = SourceDeclaration.Item1.Pictogram;
                this.Command = null;
                this.CommandParameterExtractor = null;
                this.CommandTargetExtractor = null;
                this.ToolTip = SourceDeclaration.Item1.Summary;
                this.Operation = SourceDeclaration.Item2;
            }
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

        private static void OnButtonTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropButton;
            btn.BtnText.Text = evargs.NewValue as string;

            btn.BtnText.SetVisible(btn.BtnText.Text.IsAbsent());
        }

        private static void OnButtonImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropButton;
            btn.BtnImage.Source = evargs.NewValue as ImageSource;
        }

        private static void OnButtonImageSizeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropButton;
            btn.BtnImage.Width = (double)evargs.NewValue;
            btn.BtnImage.Height = (double)evargs.NewValue;
        }

        private static void OnButtonOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropButton;
            btn.BackPanel.Orientation = (Orientation)evargs.NewValue;
        }

        private static void OnButtonShowTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var btn = depobj as PaletteDropButton;
            btn.BtnText.SetVisible((bool)evargs.NewValue);
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

            if (this.Operation != null)
                this.Operation(OperationParameterExtractor == null ? this : OperationParameterExtractor());

            this.PopupOptions.IsOpen = false;
        }

        private void BtnDropper_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.PopupOptions.IsOpen = true;
        }

        private bool IgnoreNextSelectionChange = false;
        private void LstboxOptionButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IgnoreNextSelectionChange)
            {
                this.IgnoreNextSelectionChange = false;
                return;
            }

            this.PopupOptions.IsOpen = false;

            this.SetSelectedButton((PaletteButton)e.AddedItems[0]);

            this.Button_Click(this, null);

            this.IgnoreNextSelectionChange = true;
            this.LstboxOptionButtons.SelectedItem = null;
        }
    }
}
