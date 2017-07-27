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
// File   : ResourceLinkType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.ResourceLinkType (Class)
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
    /// Represents the reference to an external resource object, such as a file, folder or web address.
    /// </summary>
    public class ResourceLinkType : LinkDataType
    {
        // Predefined file types
        public static readonly ResourceLinkType ResourceTypeAny = new ResourceLinkType("<LINK>", "LNK", "Arbitrary resource address.",
                                                                                       Display.GetAppImage("link.png"));

        public static readonly ResourceLinkType ResourceTypeFile = new ResourceLinkType("File", "FIL", "External file path.",
                                                                                        Display.GetAppImage("page_white_link.png"));

        public static readonly ResourceLinkType ResourceTypeFolder = new ResourceLinkType("Folder", "DIR", "External folder/directory path.",
                                                                                          Display.GetAppImage("folder_link.png"));

        public static readonly ResourceLinkType ResourceTypeWeb = new ResourceLinkType("Web", "URL", "Web URL adress.",
                                                                                       Display.GetAppImage("world_link.png"));

        /// <summary>
        /// Collection of predefined resource-link types.
        /// </summary>
        public static readonly ResourceLinkType[] PredefinedResourceTypes = 
        {
            ResourceTypeAny,
            ResourceTypeFile,
            ResourceTypeFolder,
            ResourceTypeWeb
        };

        /// <summary>
        /// Determines and returns the Resource Link Type associated to the supplied Resource.
        /// </summary>
        public static ResourceLinkType GetResourceType(string Source)
        {
            if (Source.ToLower().StartsWith("http://"))
                return ResourceTypeWeb;

            var NotAllowedPathChars = System.IO.Path.GetInvalidPathChars();

            if (Source.EndsWith("/") || Source.EndsWith("\\"))
                if (!Source.Any(chr => chr.IsOneOf(NotAllowedPathChars)))
                    return ResourceTypeFolder;
                else
                    return ResourceTypeAny;

            var NotAllowedFileChars = System.IO.Path.GetInvalidFileNameChars();

            if (!Source.Any(chr => chr.IsOneOf(NotAllowedFileChars)))
                return ResourceTypeFile;

            return ResourceTypeAny;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ResourceLinkType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
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
        public override IEnumerable<IRecognizableElement> LinkOptions { get { return PredefinedResourceTypes; } }
    }
}