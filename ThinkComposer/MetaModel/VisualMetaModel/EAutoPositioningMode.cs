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
// File   : EAutoLocatingMode.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.EAutoPositioningMode (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.06.21 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// Ways to accommodate the symbols of automatically created Concepts.
    public enum EAutoPositioningMode : byte
    {
        /* POSTPONED:
        /// <summary>
        /// Tree with circularly positioned nodes respect a center root, like a flower.
        /// </summary>
        [Description("Tree with circularly positioned nodes respect a center root, like a flower.")]
        Radial = ((byte)'X'), */

        /// <summary>
        /// Horizontal tree, with nodes alternating at two rows on top and down sides.
        /// </summary>
        [FieldName("Horizontal Alternated"), Description("Horizontal tree, with nodes alternating at two rows on top and down sides.")]
        HorizontalAlternated = ((byte)'H'),

        /// <summary>
        /// Vertical tree, with nodes alternating at two columns on left and right sides.
        /// </summary>
        [FieldName("Vertical Alternated"), Description("Vertical tree, with nodes alternating at two columns on left and right sides.")]
        VerticalAlternated = ((byte)'V'),

        /// <summary>
        /// Tree with nodes to the Bottom direction.
        /// </summary>
        [FieldName("To Bottom"), Description("Tree with nodes to the Bottom direction.")]
        ToBottom = ((byte)'B'),

        /// <summary>
        /// Tree with nodes to the Right direction.
        /// </summary>
        [FieldName("To Right"), Description("Tree with nodes to the Right direction.")]
        ToRight = ((byte)'R'),

        /// <summary>
        /// Tree with nodes to the Up direction.
        /// </summary>
        [FieldName("To Up"), Description("Tree with nodes to the Up direction.")]
        ToUp = ((byte)'U'),

        /// <summary>
        /// Tree with nodes to the Left direction.
        /// </summary>
        [FieldName("To Left"), Description("Tree with nodes to the Left direction.")]
        ToLeft = ((byte)'L')
    }
}