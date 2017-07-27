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
// File   : EVisualDispositionBidimensional.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.EVisualDispositionBidimensional (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.09.16 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Simple indication of visibility and order of an element respect another in two dimensions.
    /// </summary>
    public enum EVisualDispositionBidimensional
    {
        Hidden = -1,
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }
}