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
// File   : EShellVisualContentType.cs
// Object : Instrumind.Common.Visualization.EShellVisualContentType (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.22 Néstor Sánchez A.  Creation
//

using System;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{

    /// <summary>
    /// Usage based types of visual content to be hosted by a shell provider.
    /// </summary>
    public enum EShellVisualContentType : byte
    {
        /// <summary>
        /// Command selection areas, such as toolbars, palettes or other select-apply resources.
        /// </summary>
        PaletteContent = ((byte)'P'),

        /// <summary>
        /// Direct/fast access command selection area.
        /// </summary>
        QuickPaletteContent = ((byte)'Q'),

        /// <summary>
        /// Exploration lists, trees, pan or zoom tools.
        /// </summary>
        NavigationContent = ((byte)'N'),

        /// <summary>
        /// Main areas for work on a document.
        /// </summary>
        DocumentContent = ((byte)'D'),

        /// <summary>
        /// Property editors and also select-apply resources.
        /// </summary>
        EditingContent = ((byte)'E'),

        /// <summary>
        /// Announces and results of performed operations (e.g.: search, tracing, etc.)
        /// </summary>
        MessagingContent = ((Byte)'M'),

        /// <summary>
        /// General status information.
        /// </summary>
        StatusContent = ((byte)'S'),

        /// <summary>
        /// Floating context menus or toolbars/palettes.
        /// </summary>
        FloatingContextContent = ((byte)'F')
    }
}