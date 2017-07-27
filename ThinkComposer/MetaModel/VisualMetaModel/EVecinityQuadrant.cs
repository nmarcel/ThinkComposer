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
// File   : EVecinityQuadrant.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.EVecinityQuadrant (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.06.29 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Quadrants around a symbol.
    /// </summary>
    public enum EVecinityQuadrant
    {
        Center,
        LeftUp,
        Up,
        RightUp,
        Left,
        Right,
        LeftDown,
        Down,
        RightDown
    }
}