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
// File   : TableRecordLinkType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.TableRecordLinkType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.03.30 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows.Media;

using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Declares a field referencing a Table-Record.
    /// </summary>
    [Serializable]
    public class TableRecordLinkType : LinkDataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TableRecordLinkType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(Name, TechName, Summary, Pictogram)
        {
        }

        /// <summary>
        /// Type of the Value which will reference the target Table-Record.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(TableRecord); } }

        /// <summary>
        /// Predefined link options available for designation.
        /// </summary>
        public override IEnumerable<IRecognizableElement> LinkOptions { get { return null; } }
    }
}