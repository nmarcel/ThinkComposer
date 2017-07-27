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
// File   : IShellProvider.cs
// Object : Instrumind.Common.Visualization.IShellProvider (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents a provider of shell services such as visual user interface.
    /// </summary>
    public interface IShellProvider
    {
        /// <summary>
        /// Selector for the current document or subject being worked on.
        /// </summary>
        Selector MainSelector { get; set; }

        /// <summary>
        /// Refreshes the indication of selected document (optionally specified) and optionally of all the exposed documents.
        /// </summary>
        void RefreshSelection(object SelectedDocument = null, bool ReExposeDocuments = false);

        /// <summary>
        /// Get the currently hosted visual contents.
        /// </summary>
        /// <returns>Enumerable list of existing contents</returns>
        IEnumerable<KeyValuePair<string, IShellVisualContent>> GetAllVisualContents();

        /// <summary>
        /// Get the visual content related to the specificied key or null if none is reltaed to it.
        /// </summary>
        /// <param name="Key">Key of the required content</param>
        /// <returns>Reference to the matching content or null if none</returns>
        IShellVisualContent GetVisualContent(string Key);

        /// <summary>
        /// Adds or replaces a supplied content  into the optional specified group,
        /// considering whether its specified key already exists or not.
        /// The ShellProvider will locate the content in accordance with its EShellVisualContentType.
        /// </summary>
        /// <param name="Content">Content to be added or replaced</param>
        void PutVisualContent(IShellVisualContent Content, int Group = 0);

        /// <summary>
        /// Adds or replaces a supplied content into the optional specified group.
        /// The ShellProvider will locate the content in accordance with its EShellVisualContentType.
        /// </summary>
        /// <param name="Content">Content to be added or replaced</param>
        void PutVisualContent(EShellVisualContentType Kind, object Content, int Group = 0);

        /// <summary>
        /// Removes the content host related to the specified key.
        /// </summary>
        /// <param name="Key">Key of the content to be removed</param>
        void DiscardVisualContent(string Key);

        /// <summary>
        /// Clears the current contents hosting object.
        /// </summary>
        void DiscardAllVisualContents();

        /// <summary>
        /// Function to execute when about to close the shell. Returns indication to proceed or cancel the closing.
        /// </summary>
        Func<bool> CloseConfirmation { get; set; }

        /// <summary>
        /// Announces a keyboard action.
        /// </summary>
        event KeyEventHandler KeyActioned;
    }
}