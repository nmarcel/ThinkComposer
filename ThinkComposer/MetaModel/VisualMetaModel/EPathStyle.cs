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
// File   : EPathStyle.cs
// Object : Instrumind.ThinkComposer.DomainModel.DomainVisualSchema.EPathStyle (Enum)
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
    /// Types of connector path styles considering number of lines and angulation.
    /// </summary>
    [Serializable]
    public enum EPathStyle : byte
    {
        /// <summary>
        /// Path has only one line which is straight.
        /// </summary>
        SinglelineStraight = ((byte)'S'),

        /// <summary>
        /// Path has only one line which is curved.
        /// </summary>
        SinglelineCurved = ((byte)'C'),

        /// <summary>
        /// Path has multiple lines in a right-angle disposition.
        /// </summary>
        MultilineRightAngled = ((byte)'R'),

        /// <summary>
        /// Path has multiple lines in a free-angle disposition.
        /// </summary>
        MultilineFreeAngled = ((byte)'F')
    }
}