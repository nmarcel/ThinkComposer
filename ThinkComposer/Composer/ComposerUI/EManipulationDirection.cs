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
// 20yy.mm.dd Néstor Sánchez A.  Creation
//

using System;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Enumerates the possible directions for manipulating visual elements (such as resize).
    /// </summary>
    public enum EManipulationDirection
    {
        Top = 2,
        Bottom = 8,
        Left = 4,
        Right = 6,
        TopLeft = 1,
        TopRight = 3,
        BottomLeft = 7,
        BottomRight = 9
    }
}