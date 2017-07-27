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
// File   : EPlacementOnBorderHorizontal.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.EPlacementOnBorderHorizontal (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.10.25 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Placement position of a visual object on a horizontal border.
    /// </summary>
    /* PENDING: Document, with examples...
    Center Middle
    Center Outward
    Center Inward
    Left Middle
    Left Outward
    Left Inward
    Right Middle
    Right Outward
    Right Inward
    Full Width
     */
    public enum EPlacementOnBorderHorizontal
    {
        [FieldName("Center Middle")]
        CenterMiddle = 0,

        [FieldName("Center Outward")]
        CenterOutward = 1,

        [FieldName("Center Inward")]
        CenterInward = 2,

        [FieldName("Left Middle")]
        LeftMiddle = 10,

        [FieldName("Left Outward")]
        LeftOutward = 20,

        [FieldName("Left Inward")]
        LeftInward = 30,

        [FieldName("Right Middle")]
        RightMiddle = 100,

        [FieldName("Right Outward")]
        RightOutward = 110,

        [FieldName("Right Inward")]
        RightInward = 120,

        [FieldName("Full Width")]
        SameWidth = 200
    }
}