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
// File   : EConceptType.cs
// Object : Instrumind.ThinkComposer.Model.GraphModel.EConceptType (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.GraphModel
{
    /// <summary>
    /// Types of Concepts
    /// </summary>
    public enum EConceptType : byte
    {
        /// <summary>
        /// Structure composed of various parts (such as table, timeline, tree, etc.)
        /// </summary>
        PolyStructure = ((byte)'P'),

        /// <summary>
        /// Individual Concept.
        /// </summary>
        Individual = ((byte)'I')
    }
}