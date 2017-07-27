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
// File   : MainWindowHeader.cs
// Object : Instrumind.ThinkComposer.ApplicationShell.MainWindowHeader (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.20 Néstor Sánchez A.  Creation
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using Instrumind.ThinkComposer.ApplicationProduct;

/// Provides the user-interface top level frame of the application.
namespace Instrumind.ThinkComposer.ApplicationShell
{
    /// <summary>
    /// The header section of the main window of the application.
    /// Shows the working title, subtitle and image, plus controls for the window handling.
    /// </summary>
    public partial class MainWindowHeader : UserControl
    {
        public event Action<MouseButtonEventArgs> Dragging;
        public event Action Minimizing;
        public event Action RestoringOrMaximizing;
        public event Action Closing;

        static MainWindowHeader()
        {
        }

        public MainWindowHeader()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Make the Menu Toolbar start and remain open.
            this.PostCall(
                winhdr =>
                {
                    this.PaletteSupraContainer.Show();
                    this.PaletteSupraContainer.CanCollapse = false;
                });
        }

        private void MainWindowHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Handler = Dragging;

            if (Handler != null)
                Handler(e);
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            var Handler = Minimizing;

            if (Handler != null)
                Handler();
        }

        private void BtnRestoreOrMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            RestoringOrMaximizing();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            var Handler = Closing;

            if (Handler != null)
                Handler();
        }

        private void CompanyLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProductDirector.ShowAbout();
        }
    }
}
