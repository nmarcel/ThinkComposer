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
// File   : EAlterability.cs
// Object : Instrumind.ThinkComposer.MetaModel.EAlterability (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.03.26 Néstor Sánchez A.  Creation
//

using System;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Set of possible modification scopes for a target.
    /// </summary>
    public enum EAlterability
    {
        /// <summary>
        /// Target is alterable only internally.
        /// </summary>
        System = 0,

        /// <summary>
        /// Target is alterable at definition level.
        /// </summary>
        Definition = 10,

        /// <summary>
        /// Target is alterable at all levels.
        /// </summary>
        Any = 99
    }
}