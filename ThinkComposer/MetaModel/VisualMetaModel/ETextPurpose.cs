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
// File   : ETextPurpose.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.ETextPurpose (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.24 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Classifies the different purposes a text can be used for.
    /// </summary>
    public enum ETextPurpose : byte
    {
        /// <summary>
        /// Word/Phrase text for the main name or title.
        /// </summary>
        [FieldName("Title"), Description("Word/Phrase text for the main name or title.")]
        Title = ((byte)'T'),

        /// <summary>
        /// Word/Phrase text for secondary naming (such as an alias, tech-name or other relevant data).
        /// </summary>
        [FieldName("Subtitle"), Description("Word/Phrase text for secondary naming (such as an alias, tech-name or other relevant data).")]
        Subtitle = ((byte)'S'),

        /// <summary>
        /// Word text for use as decorator (such as classification data).
        /// </summary>
        [FieldName("Extra"), Description("Word text for use as decorator (such as classification data).")]
        Extra = ((byte)'E'),

        /// <summary>
        /// Abundant text for description or summary.
        /// </summary>
        [FieldName("Paragraph"), Description("Abundant text for description or summary.")]
        Paragraph = ((byte)'P'),

        /// <summary>
        /// Detail level Heading, such as the name of a table.
        /// </summary>
        [FieldName("Detail Heading"), Description("Detail level Heading, such as the name of a table.")]
        DetailHeading = ((byte)'H'),

        /// <summary>
        /// Detail level Caption, such as the name of a table field.
        /// </summary>
        [FieldName("Detail Caption"), Description("Detail level Caption, such as the name of a table field.")]
        DetailCaption = ((byte)'K'),

        /// <summary>
        /// Detail level Content, such as data values of a table.
        /// </summary>
        [FieldName("Detail Content"), Description("Detail level Content, such as data values of a table.")]
        DetailContent = ((byte)'C')
    }
}