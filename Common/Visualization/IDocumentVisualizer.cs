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
// File   : IDocumentVisualizer.cs
// Object : Instrumind.Common.Visualization.IDocumentVisualizer (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows.Controls;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents a visualizer for a multi-view document.
    /// </summary>
    public interface IDocumentVisualizer
    {
        /// <summary>
        /// Get all the displayed views, or just these related to the parent document if specified.
        /// </summary>
        IEnumerable<IDocumentView> GetAllViews(ISphereModel ParentDocument = null);

        /// <summary>
        /// Adds or replaces a view, with the supplied content, considering wether its specified key already exists or not.
        /// Returns the controls which finally presents the document view.
        /// </summary>
        ScrollViewer PutView(IDocumentView DocView);

        /// <summary>
        /// Removes the view related to the specified key.
        /// </summary>
        void DiscardView(Guid Key);

        /// <summary>
        /// Removes all the displayed views, or just these related to the parent document if specified.
        /// </summary>
        void DiscardAllViews(ISphereModel ParentDocument = null);

        /// <summary>
        /// Gets or sets the active document View.
        /// </summary>
        IDocumentView ActiveView { get; set; }

        /// <summary>
        /// Action to execute after a view has been activated.
        /// </summary>
        Action<IDocumentView> PostViewActivation { get; set; }

        /// <summary>
        /// Function to execute when about to close a view, indicated by key. Returns indication to proceed or cancel the closing.
        /// </summary>
        Func<Guid, bool> CloseConfirmation { get; set; }

        /// <summary>
        /// Scrolls the view containter a segment to the specified direction and offset.
        /// </summary>
        void ScrollSegment(Orientation Direction, double Offset);
    }
}