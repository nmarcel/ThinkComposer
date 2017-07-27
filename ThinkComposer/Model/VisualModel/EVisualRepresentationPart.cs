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
// File   : EVisualRepresentationPart.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.EVisualRepresentationPart (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.15 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Part types that can exist in a Visual Representation.
    /// </summary>
    public enum EVisualRepresentationPart : byte
    {
        /// <summary>
        /// The part is a Concept whole body itself (so no more parts related).
        /// </summary>
        ConceptBodySymbol = ((byte)'C'),

        /// <summary>
        /// The part is the main-symbol of a Relationship.
        /// </summary>
        RelationshipCentralSymbol = ((byte)'R'),

        /// <summary>
        /// The part is the connector representing a Relationship Link (which also implements a Role).
        /// </summary>
        RelationshipLinkConnector = ((byte)'L')
    }
}