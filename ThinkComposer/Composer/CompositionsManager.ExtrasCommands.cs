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
// File   : CompositionsManager.ExtrasCommands.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionsManager (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.06 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Manages the edition of user-defined Compositions, working as an intermediary for external consumers.
    /// Extras Commands part.
    /// </summary>
    public partial class CompositionsManager : WorkSphere
    {
        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandAbout_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.Handled = true;
        }

        public void CommandAbout_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            ProductDirector.ShowAbout();
        }

        // -------------------------------------------------------------------------------------------------------------------------
   }
}