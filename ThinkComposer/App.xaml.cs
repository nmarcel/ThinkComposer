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
// File   : App.xaml.cs
// Object : Instrumind.ThinkComposer.App (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;

/// Instrumind ThinkComposer product.
/// The Visual Thinking tool for analyze problems, design solutions and express knowledge.
/// - Create Concept Maps, Mind Maps and general purpose Diagrams with detailed content.
/// - Define your own visual language to conquer complexity on your specialized Domain.
/// - Leverage coherent results across your organization by using a solid semantic basis.
/// - Increase productivity and understanding with this easy-to-use and flexible software.
namespace Instrumind.ThinkComposer
{
    /// <summary>
    /// Application base: Entry point for Instrumind ThinkComposer.
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            // Assigns configurations
            AppExec.LogRegistrationPolicy = AppExec.GetConfiguration("Application", "LoggingPolicy", AppExec.LogRegistrationPolicy);

            // Set databinding string formatting to current-culture
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

            // Initializes Instrumind services
            AppExec.Initialize(ApplicationProduct.ProductDirector.APPLICATION_NAME,
                               ApplicationProduct.ProductDirector.APPLICATION_VERSION,
                               ApplicationProduct.ProductDirector.APPLICATION_DEFINITIONS_NAME,
                               ApplicationProduct.ProductDirector.USER_DOCUMENTS_NAME);
        }

        #region TextBox Selection
        protected override void OnStartup(StartupEventArgs e)
        {
            // Select the text in a TextBox when it recieves focus. 
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText));

            base.OnStartup(e);
        }

        void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox 
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and 
                    // stop further processing of this click event. 
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            var TextControl = e.OriginalSource as TextBox;
            if (TextControl != null && !TextControl.IsReadOnly)
                TextControl.SelectAll();
        }
        #endregion
    }
}
