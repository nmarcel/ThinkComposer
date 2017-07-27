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
// File   : IdeaLinkType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.IdeaLinkType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.09.14 Néstor Sánchez A.  Creation
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
    /// Represents the reference to an Idea.
    /// </summary>
    [Serializable]
    public class IdeaLinkType : LinkDataType
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public IdeaLinkType(string Name, string TechName, string Summary = "",
                            bool LinksConcepts = true, bool LinksRelationships = true, ImageSource Pictogram = null)
                : base(Name, TechName, Summary, Pictogram)
        {
            this.LinksConcepts = LinksConcepts;
            this.LinksRelationships = LinksRelationships;
        }

        /// <summary>
        /// Indicates that this type can link to Concept Ideas.
        /// </summary>
        public bool LinksConcepts { get; protected set; }

        /// <summary>
        /// Indicates that this type can link to Relationship Ideas.
        /// </summary>
        public bool LinksRelationships { get; protected set; }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public override Type ContainerType { get { return typeof(Idea); } }

        /// <summary>
        /// Predefined link options available for designation.
        /// </summary>
        public override IEnumerable<IRecognizableElement> LinkOptions { get { return null; } }
    }
}