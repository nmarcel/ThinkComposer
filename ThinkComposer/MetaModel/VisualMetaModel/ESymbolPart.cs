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
// File   : ESymbolPart.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.ESymbolPart (Enum)
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
    /// Conforming parts of an Symbol.
    /// </summary>
    [Serializable]
    public enum ESymbolPart : byte
    {
        /// <summary>
        /// The symbol itself with no accesories.
        /// </summary>
        Body = ((byte)'B'),

        /// <summary>
        /// Accesory for the symbol such as: Tag, Note, Stereotype, Cue, Callout, etc.
        /// </summary>
        Annex = ((byte)'A')
    }
}