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
// File   : SelectionButton.cs
// Object : Instrumind.Common.Visualization.Widgets.SelectionButton (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.28 Néstor Sánchez A.  Creation
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
    public partial class SelectionButton : Button
    {
        public static readonly DependencyProperty ButtonCodeProperty;
        public static readonly DependencyProperty ButtonTextProperty;
        public static readonly DependencyProperty ButtonSummaryProperty;
        public static readonly DependencyProperty ButtonImageProperty;
        public static readonly DependencyProperty ButtonImageSizeProperty;

        static SelectionButton()
        {
            SelectionButton.ButtonCodeProperty = DependencyProperty.Register("ButtonCode", typeof(string), typeof(SelectionButton),
                new FrameworkPropertyMetadata("Button's Code", new PropertyChangedCallback(OnButtonCodeChanged)));

            SelectionButton.ButtonTextProperty = DependencyProperty.Register("ButtonNameText", typeof(string), typeof(SelectionButton),
                new FrameworkPropertyMetadata("Button's Name Text", new PropertyChangedCallback(OnButtonTextChanged)));

            SelectionButton.ButtonSummaryProperty = DependencyProperty.Register("ButtonSummaryText", typeof(string), typeof(SelectionButton),
                new FrameworkPropertyMetadata("Button's Summary Text", new PropertyChangedCallback(OnButtonSummaryChanged)));

            SelectionButton.ButtonImageProperty = DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(SelectionButton),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnButtonImageChanged)));

            SelectionButton.ButtonImageSizeProperty = DependencyProperty.Register("ButtonImageSize", typeof(double), typeof(SelectionButton),
                new FrameworkPropertyMetadata(Display.ICOSIZE_LIT, new PropertyChangedCallback(OnButtonImageSizeChanged)));

            ToolBarStyle = (Style)Application.Current.FindResource(ToolBar.ButtonStyleKey);
        }
        static Style ToolBarStyle;

        public SelectionButton()
        {
            InitializeComponent();
        }

        public SelectionButton(IRecognizableElement SourceElement, Action<string> ButtonAction)
             : this(SourceElement.TechName, SourceElement.Name, SourceElement.Summary, SourceElement.Pictogram, ButtonAction)
        {
        }

        public SelectionButton(string Code, string Name, string Summary, ImageSource Pictogram, Action<string> ButtonAction)
             : this()
        {
            General.ContractRequiresNotAbsent(Code, Name);

            this.ButtonCode = Code;
            this.BtnText.Text = Name;
            this.BtnSummary.Text = Summary;
            this.BtnImage.Source = Pictogram;
            this.ButtonAction = ButtonAction;
        }

        // Notice that these static fields may impact any Selection-Button of other already opened dialog windown.
        private static Window PreviousParentWindow = null;
        private static SelectionButton PreviousDefaultButton = null;

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            var CurrentParentWindow = this.GetNearestVisualDominantOfType<Window>();
            if (PreviousParentWindow == null || PreviousParentWindow != CurrentParentWindow)
            {
                PreviousDefaultButton = null;
                PreviousParentWindow = CurrentParentWindow;
            }

            this.InitialRaisedStyle = this.Style;
            this.Style = ToolBarStyle;

            if (this.IsDefault)
                SetDefaultLook();
        }
        Style InitialRaisedStyle;
        public string ButtonCode
        {
            get { return (string)GetValue(ButtonCodeProperty); }
            set { SetValue(ButtonCodeProperty, value); }
        }

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public string ButtonSummary
        {
            get { return (string)GetValue(ButtonSummaryProperty); }
            set { SetValue(ButtonSummaryProperty, value); }
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

        private static void OnButtonCodeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            SelectionButton tb = depobj as SelectionButton;
            tb.Tag = evargs.NewValue as string;
        }

        private static void OnButtonTextChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            SelectionButton tb = depobj as SelectionButton;
            tb.BtnText.Text = evargs.NewValue as string;

            tb.BtnText.Visibility = (tb.BtnText.Text.IsAbsent() ? Visibility.Collapsed : Visibility.Visible);
        }

        private static void OnButtonSummaryChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            SelectionButton tb = depobj as SelectionButton;
            tb.BtnSummary.Text = evargs.NewValue as string;

            tb.BtnSummary.Visibility = (tb.BtnSummary.Text.IsAbsent() ? Visibility.Collapsed : Visibility.Visible);
        }

        private static void OnButtonImageChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            SelectionButton tb = depobj as SelectionButton;
            tb.BtnImage.Source = evargs.NewValue as ImageSource;
        }

        private static void OnButtonImageSizeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            SelectionButton tb = depobj as SelectionButton;
            tb.BtnImage.Width = (double)evargs.NewValue;
            tb.BtnImage.Height = (double)evargs.NewValue;
        }

        /// <summary>
        /// Action to call when the button is selected.
        /// </summary>
        public Action<string> ButtonAction { get; protected set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonAction != null)
                ButtonAction(this.ButtonCode);
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Focus();
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            SetDefaultLook();
        }

        private void SetDefaultLook()
        {
            if (PreviousDefaultButton != null)
            {
                PreviousDefaultButton.IsDefault = false;
                PreviousDefaultButton.Style = ToolBarStyle;
            }

            this.IsDefault = true;
            PreviousDefaultButton = this;
            this.Style = this.InitialRaisedStyle;
        }
    }
}
