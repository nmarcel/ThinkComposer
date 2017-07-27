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
// File   : AttachmentTypeDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.PictureType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.01.12 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a Picture data type.
    /// </summary>
    [Serializable]
    public class PictureType : DataType
    {
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public PictureType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
                         : base(Name, TechName, Summary, Pictogram)
        {
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(ImageAssignment); } }
    }
}