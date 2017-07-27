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
// File   : BasicDataType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.BasicDataType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.07 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common.Visualization;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Definition of a basic type for table fields.
    /// </summary>
    [Serializable]
    public abstract class BasicDataType : DataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicDataType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
        }

        /// <summary>
        /// Gets the minimum number of characters to be used when displayed (zero for unspecified).
        /// </summary>
        public int DisplayMinLength
        {
            get { return this.DisplayMinLength_; }
            set { this.DisplayMinLength_ = value; }
        }
        public int DisplayMinLength_ = 0;

        /// <summary>
        /// Gets the visual alignment to be used when displayed as text.
        /// </summary>
        public TextAlignment DisplayAlignment
        {
            get { return this.DisplayAlignment_; }
            set { this.DisplayAlignment_ = value; }
        }
        public TextAlignment DisplayAlignment_ = TextAlignment.Left;

        /// <summary>
        /// Gets the visual format to be used when displayed as text.
        /// </summary>
        public string DisplayFormat
        {
            get { return this.DisplayFormat_; }
            set { this.DisplayFormat_ = value; }
        }
        public string DisplayFormat_ = "";
    }
}