// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : EVisualLevel.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.EVisualLevel (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.08.08 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Levels of visual presentation.
    /// </summary>
    public enum EVisualLevel
    {
        /// <summary>
        /// Level for objects at the background (like diagram sheet labels and legends).
        /// </summary>
        Background = 0,

        /// <summary>
        /// Level for objects representing regions for group other objects (like Venn diagrams shapes).
        /// </summary>
        Regions = 32,

        /// <summary>
        /// Level for the main visual objects (like diagramming symbols and connectors).
        /// </summary>
        Elements = 64,

        /// <summary>
        /// Level for floating visual objects (like sticky notes).
        /// </summary>
        Floatings = 128,

        /// <summary>
        /// Level for transient visual objects (like selection or signaling cues).
        /// </summary>
        Transients = 256
    }
}
