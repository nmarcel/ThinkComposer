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
// File   : IDocumentView.cs
// Object : Instrumind.Common.Visualization.IDocumentView (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents a document view to be shown in a Document Visualizer.
    /// </summary>
    public interface IDocumentView : IUniqueElement, IModelEntity
    {
        /// <summary>
        /// Document owner of the View.
        /// </summary>
        ISphereModel ParentDocument { get; }

        /// <summary>
        /// Title for the view.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Visual presenter to be exposed.
        /// </summary>
        FrameworkElement PresenterControl { get; }

        /// <summary>
        /// Gets the adorner layer of the presenter control.
        /// </summary>
        AdornerLayer PresenterLayer { get; }

        /// <summary>
        /// Hosting Grid for the Presenter control.
        /// </summary>
        Grid PresenterHostingGrid { get; set; }

        /// <summary>
        /// Top Canvas of the Presenter control.
        /// </summary>
        Canvas TopCanvas { get; set; }

        /// <summary>
        /// Background brush to be shown.
        /// </summary>
        Brush BackgroundWorkingBrush { get; }

        /// <summary>
        /// Background image to be shown (on the background work brush, if transparent).
        /// </summary>
        ImageSource BackgroundWorkingImage { get; }

        /// <summary>
        /// Size of the View.
        /// </summary>
        Size ViewSize { get; }

        /// <summary>
        /// Central point of the View.
        /// </summary>
        Point ViewCenter { get; }

        /// <summary>
        /// Scaling percentage for the View page.
        /// </summary>
        int PageDisplayScale { get; set; }

        /// <summary>
        /// Indicates whether to display the assigned Background.
        /// </summary>
        bool ShowContextBackground { get; set; }

        /// <summary>
        /// Indicates whether to display the assigned Grid.
        /// </summary>
        bool ShowContextGrid { get; set; }

        /// <summary>
        /// References the control that contains the document view.
        /// </summary>
        ScrollViewer HostingScrollViewer { get; set; }

        /// <summary>
        /// Reacts to a mouse wheel action.
        /// </summary>
        void ReactToMouseWheel(Object Sender, MouseWheelEventArgs Args);

        /// <summary>
        /// Action to execute after this document view PageDisplayScale has changed.
        /// </summary>
        Action<int> PostChangeOfPageDisplalyScale { get; set; }

        /// <summary>
        /// Reacts to a view deactivation (as when focusing another one).
        /// </summary>
        void ReactToDeactivation();

        /// <summary>
        /// Signals the view close.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the background image of the view, if any.
        /// </summary>
        Brush GetBackgroundImageBrush();

        /// <summary>
        /// Gets or sets the last scroll offset.
        /// </summary>
        Point LastScrollOffset { get; set; }
    }
}