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
// File   : LinkDataType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.LinkDataType (Class)
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
using Instrumind.Common.Visualization;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// For a consuming related Idea definition, declares the reference to an arbitrary object.
    /// </summary>
    [Serializable]
    public class LinkDataType : DataType
    {
        private const byte LINKTYPECODE_GENERIC = ((byte)'G');
        private const byte LINKTYPECODE_INTERNAL = ((byte)'I');
        private const byte LINKTYPECODE_RESOURCE = ((byte)'R');

        public static readonly LinkDataType GenericLink = new LinkDataType("<LINK>", "LNK", "Arbitrary reference to an object", Display.GetAppImage("link.png"));

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static LinkDataType()
        {
            StoreBox.RegisterStorableType<LinkDataType>(block =>
                                                        {
                                                            if (block == null || block.Length < 1)
                                                                return null;

                                                            if (block[0] == LINKTYPECODE_GENERIC)
                                                                return GenericLink;

                                                            if (block[0] == LINKTYPECODE_INTERNAL)
                                                                return InternalLinkType.InternalTypeAny;

                                                            return ResourceLinkType.PredefinedResourceTypes
                                                                        .FirstOrDefault(predef => predef.TechName == block.ExtractSegment(1).BytesToString())
                                                                                .NullDefault(ResourceLinkType.ResourceTypeAny);
                                                        },
                                                        linkt =>
                                                        {
                                                            if (linkt == null)
                                                                return null;

                                                            if (linkt is InternalLinkType)
                                                                return LINKTYPECODE_INTERNAL.IntoArray();

                                                            if (linkt.GetType() == typeof(LinkDataType))
                                                                return LINKTYPECODE_GENERIC.IntoArray();

                                                            return BytesHandling.FusionateByteArrays(LINKTYPECODE_RESOURCE.IntoArray(),
                                                                        ((ResourceLinkType)linkt).TechName.AbsentDefault(ResourceLinkType.ResourceTypeAny.TechName).StringToBytes());
                                                        });
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LinkDataType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
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
        public virtual IEnumerable<IRecognizableElement> LinkOptions { get { yield return GenericLink; } }
    }
}