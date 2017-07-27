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
// File   : BasicWindow.cs
// Object : Instrumind.Common.Visualization.BasicWindow (Class)
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
using System.Windows.Shapes;

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for BasicWindow.xaml
    /// </summary>
    public partial class BasicWindow : Window
    {
        public static new readonly DependencyProperty TitleProperty;

        static BasicWindow()
        {
            BasicWindow.TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BasicWindow),
                new FrameworkPropertyMetadata("BasicWindow's Title", new PropertyChangedCallback(OnTitleChanged)));
        }

        public BasicWindow()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;

            this.SizeChanged += AdjustSize;
            this.LocationChanged += AdjustSize;
        }

        public bool CanMinimize
        {
            get { return this.CanMinimize_; }
            set
            {
                this.CanMinimize_ = value;
                var Button = this.GetTemplateChild<Button>("BtnMinimize", true);
                Button.SetVisible(this.CanMinimize);
            }
        }
        public bool CanMinimize_ = true;

        public bool CanMaximize
        {
            get { return this.CanMaximize_; }
            set
            {
                this.CanMaximize_ = value;
                var Button = this.GetTemplateChild<Button>("BtnRestoreOrMaximize", true);
                Button.SetVisible(this.CanMaximize);
            }
        }
        public bool CanMaximize_ = true;

        private void AdjustSize(object sender, EventArgs args)
        {
            //-? if (this.Left + this.ActualWidth > SystemParameters.WorkArea.Width)
            this.MaxWidth = (this.ExplicitMaxWidth.IsNan() ? SystemParameters.WorkArea.Width - this.Left
                                                           : this.ExplicitMaxWidth).EnforceRange(SystemParameters.WorkArea.Width / 4.0, SystemParameters.WorkArea.Width);

            //-? if (this.Top + this.ActualHeight > SystemParameters.WorkArea.Height)
            this.MaxHeight = (this.ExplicitMaxHeight.IsNan() ? SystemParameters.WorkArea.Height - this.Top
                                                             : this.ExplicitMaxHeight).EnforceRange(SystemParameters.WorkArea.Height / 4.0, SystemParameters.WorkArea.Height);
        }

        public double ExplicitMaxWidth = double.NaN;

        public double ExplicitMaxHeight = double.NaN;

        private void BasicWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();

            if (e.ClickCount == 2)
                BtnRestoreOrMaximize_Click(this, null);
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (!this.CanMinimize)
                return;

            if (e != null)
                e.Handled = true;

            this.WindowState = WindowState.Minimized;
        }

        private Thickness AlternateBorderPadding = new Thickness(8);
        private void BtnRestoreOrMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (!this.CanMaximize)
                return;

            if (e != null)
                e.Handled = true;

            var BackPanel = Display.GetTemplateChild<Border>(this, "BackPanel");
            if (BackPanel == null)
                return;

            var SwapBorderPadding = BackPanel.Padding;
            BackPanel.Padding = AlternateBorderPadding;
            AlternateBorderPadding = SwapBorderPadding;

            if (this.WindowState == WindowState.Normal)
            {
                this.SizeToContent = System.Windows.SizeToContent.Manual;
                this.MaxWidth = SystemParameters.WorkArea.Width;    //- double.PositiveInfinity;
                this.MaxHeight = SystemParameters.WorkArea.Height;    //- double.PositiveInfinity;
                this.Width = double.NaN;
                this.Height = double.NaN;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
                this.WindowState = WindowState.Normal;
            }

        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            this.Close();
        }

        public new string Title
        {
            get { return (string)GetValue(BasicWindow.TitleProperty); }
            set { SetValue(BasicWindow.TitleProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var textblock = Display.GetTemplateChild<TextBlock>(depobj, "TitleTextBlock");
            if (textblock == null)
                return;

            textblock.Text = evargs.NewValue as string;
        }
    }
}
