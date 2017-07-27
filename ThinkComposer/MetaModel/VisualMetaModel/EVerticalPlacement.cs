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
// File   : EVerticalPlacement.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.EVerticalPlacement (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.02 Néstor Sánchez A.  Creation
//

using System;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Arrangement for visual objects on the Y-Axis.
    /// </summary>
    public enum EVerticalPlacement : byte
    {
        Upper = ((byte)'U'),
        Middle = ((byte)'M'),
        Lower = ((byte)'L')
    }
}