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
// File   : EPropertyKind.cs
// Object : Instrumind.Common.EntityDefinition.EPropertyKind (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.15 Néstor Sánchez A.  Creation
//

using System;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Types of properties based on intended usage.
    /// </summary>
    public enum EPropertyKind : byte
    {
        /// <summary>
        /// Common properties with no special treatment.
        /// </summary>
        Common = ((byte)'C'),

        /// <summary>
        /// Properties used for identification or access purposes with constrained format (system level).
        /// </summary>
        Key = ((byte)'K'),

        /// <summary>
        /// Properties used for naming objects with free format (user level).
        /// </summary>
        Name = ((byte)'N'),

        /// <summary>
        /// Properties used for describing objects with some detail.
        /// </summary>
        Description = ((byte)'D')
    }
}