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
// File   : InternalLinkType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.InternalLinkType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.09 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Represents the reference to an internal object, such as an Idea property.
    /// </summary>
    [Serializable]
    public class InternalLinkType : LinkDataType
    {
        /// <summary>
        /// Typifier of internal links.
        /// </summary>
        public static readonly InternalLinkType InternalTypeAny;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static InternalLinkType()
        {
            InternalTypeAny = new InternalLinkType("<INTERNAL>", "INT", "Internal object value.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected InternalLinkType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
                : base(Name, TechName, Summary, Pictogram)
        {
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(string); } }

        /// <summary>
        /// Predefined link options available for designation.
        /// </summary>
        public override IEnumerable<IRecognizableElement> LinkOptions { get { return Idea.__ClassDefinitor.Properties.Where(prop => prop.IsLinkeable); } }
    }
}