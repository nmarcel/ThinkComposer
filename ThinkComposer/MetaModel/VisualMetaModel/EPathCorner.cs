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
// File   : EPathCorner.cs
// Object : Instrumind.ThinkComposer.DomainModel.DomainVisualSchema.EPathCorner (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.31 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Types of multiline path corners.
    /// </summary>
    [Serializable]
    public enum EPathCorner : byte
    {
        /// <summary>
        /// Sharp corner style.
        /// </summary>
        Sharp = ((byte)'S'),

        /// <summary>
        /// Rounded corner style.
        /// </summary>
        Rounded = ((byte)'R')
    }
}