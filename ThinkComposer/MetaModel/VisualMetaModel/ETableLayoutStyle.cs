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
// File   : ETableLayoutStyle.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.ETableLayoutStyle (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.15 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Available layouts for table display.
    /// </summary>
    public enum ETableLayoutStyle : byte
    {
        /// <summary>
        /// Shows records on rows (one upon the other, in the vertical axis) and fields on columns (side by side, in the horizontal axis).
        /// </summary>
        [Description("Shows records on rows (one upon the other, in the vertical axis) and fields on columns (side by side, in the horizontal axis).")]
        Conventional = ((byte)'C'),

        /* POSTPONED /// <summary>
        /// Shows records on a hierarchical relation defined by Key-Fields and Dominant-Reference fields.
        /// </summary>
        [Description("Shows records on a nested (tree like) relation defined by Key-Fields and Dominant-Reference fields.")]
        Hierarchical = ((byte)'H'), */

        /// <summary>
        /// Shows records on rows (one upon the other, in the vertical axis) and just the assigned Label-Fields.
        /// </summary>
        [Description("Shows a simple list of records on rows (one upon the other, in the vertical axis) and just the assigned Label-Fields.")]
        Simple = ((byte)'S'),

        /// <summary>
        /// Shows fields on rows (one upon the other, in the vertical axis) and records on columns (side by side, in the horizontal axis).
        /// </summary>
        [Description("Shows fields on rows (one upon the other, in the vertical axis) and records on columns (side by side, in the horizontal axis).")]
        Transposed = ((byte)'T')
    }
}