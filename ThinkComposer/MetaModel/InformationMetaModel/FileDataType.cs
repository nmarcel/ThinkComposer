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
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.AttachmentTypeDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.28 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.MetaModel;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    // SEE: http://stackoverflow.com/questions/691212/how-do-i-determine-a-files-content-type-in-net

    /// <summary>
    /// Defines a file data type.
    /// Plus, at a static level, has the collection of attachable file types.
    /// </summary>
    [Serializable]
    public class FileDataType : DataType
    {
        // Predefined file types
        public static readonly FileDataType FileTypeAny = new FileDataType("<ATTACHMENT>", "ATT", "", "*", "Any attachment type",
                                                                           Display.GetAppImage("page_attach.png" /* "page_white.png" */ ));

        public static readonly FileDataType FileTypeImage = new FileDataType("Image", "IMG",
                                                                             Display.IMAGE_FILE_READ_EXTENSIONS.GetLeft(Display.IMAGE_FILE_READ_EXTENSIONS.IndexOf(';')),
                                                                             Display.IMAGE_FILE_READ_EXTENSIONS.Substring(Display.IMAGE_FILE_READ_EXTENSIONS.IndexOf(';')),
                                                                             "Picture, portrait, or any external image",
                                                                             Display.GetAppImage("picture.png"));

        public static readonly FileDataType FileTypeText = new FileDataType("Text", "TXT", "txt", "text;dat", "Text (plain) document",
                                                                            Display.GetAppImage("page_white_text.png"));

        public static readonly FileDataType FileTypeWord = new FileDataType("Word", "WRD", "docx", "doc", "Microsoft Word text document",
                                                                            Display.GetAppImage("page_word.png"));

        public static readonly FileDataType FileTypeExcel = new FileDataType("Excel", "XLS", "xlsx", "xls", "Microsoft Excel spreadsheet document",
                                                                             Display.GetAppImage("page_excel.png"));

        public static readonly FileDataType FileTypePDF = new FileDataType("PDF", "PDF", "pdf", "", "Adobe PDF document",
                                                                           Display.GetAppImage("page_white_acrobat.png"));

        public static readonly FileDataType FileTypeComposition = new FileDataType("Composition", Domain.FILE_EXTENSION_DOMAIN.ToUpper(), Composition.FILE_EXTENSION_COMPOSITION,
                                                                                   "", "Instrumind ThinkComposer Composition document",
                                                                                   Display.GetAppImage("book.png"));

        public static readonly FileDataType FileTypeDomain = new FileDataType("Domain", Domain.FILE_EXTENSION_DOMAIN.ToUpper(), Domain.FILE_EXTENSION_DOMAIN,
                                                                              "", "Domain for defining Instrumind ThinkComposer Composition documents",
                                                                              Display.GetAppImage("book.png"));

        public static readonly FileDataType FileTypeAudio = new FileDataType("Audio", "AUD", "mp3", "wav;midi", "Sound sequence",
                                                                             Display.GetAppImage("music.png"));

        public static readonly FileDataType FileTypeVideo = new FileDataType("Video", "VID", "mpg", "mpeg;mpe;mp4", "Motion picture sequence",
                                                                             Display.GetAppImage("film.png"));

        /// <summary>
        /// Declared associations between file extensions and Mime-Types.
        /// </summary>
        public static readonly Dictionary<string, string> ExtensionToMimeTypeAssociations = new Dictionary<string, string>()
        {
            { Composition.FILE_EXTENSION_COMPOSITION, AppExec.ApplicationContentTypeCode + "-composition" },
            { Domain.FILE_EXTENSION_DOMAIN, AppExec.ApplicationContentTypeCode + "-domain" }
        };

        /// <summary>
        /// Collection of predefined file types.
        /// </summary>
        public static readonly FileDataType[] PredefinedFileTypes = 
        {
            FileTypeAny,
            FileTypeImage,
            FileTypeText,
            FileTypeWord,
            FileTypeExcel,
            FileTypePDF,
            FileTypeComposition,
            FileTypeDomain,
            FileTypeAudio,
            FileTypeVideo
        };

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static FileDataType()
        {
            StoreBox.RegisterStorableType<FileDataType>(filetypecode => PredefinedFileTypes.FirstOrDefault(predef => predef.TechName == filetypecode.BytesToString()).NullDefault(FileTypeAny),
                                                        filetype => filetype.TechName.AbsentDefault(FileTypeAny.TechName).StringToBytes());

            FileTypeImage.FilterForSave = Display.IMAGE_FILE_WRITE_EXTENSIONS.Split(';').GetConcatenation(ext => ext.ToUpper() + " format.|*." + ext, "|");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileDataType(string Name, string TechName, string Extension, string AssociatedExtensions, string Summary = "", ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            General.ContractRequiresNotNull(Extension, AssociatedExtensions);

            this.Extension = Extension;
            this.AssociatedExtensions = AssociatedExtensions;
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        // Maybe is better (more specific) to use byte[]
        public override Type ContainerType { get { return typeof(object); } }

        /// <summary>
        /// Standard extension of this file type.
        /// </summary>
        public string Extension { get; protected set; }

        /// <summary>
        /// List of -semicolon separated- file extensions related to this file type. Later used to conform file selection filters.
        /// </summary>
        public string AssociatedExtensions { get; protected set; }

        /// <summary>
        /// Gets the associated Mime-Type based on the filetype extension.
        /// </summary>
        public string MimeType
        {
            get
            {
                var Result = ExtensionToMimeTypeAssociations.GetValueOrDefault(this.Extension);

                if (Result.IsAbsent())
                    Result = General.GetMimeTypeFromFileExtension(this.Extension);

                return Result;
            }
        }

        /// <summary>
        /// Returns an open-file-dialog compatible filter based on this file type.
        /// </summary>
        public string FilterForOpen
        {
            get
            {
                if (this.FilterForOpen_ != null)
                    return this.FilterForOpen_;

                var FilterExtensions = "*." + ((this.Extension.Trim() + ";" + this.AssociatedExtensions).Replace(";", ";*."));
                var FilterLabel = this.Name.Replace("|", "/").Trim() + " (" + FilterExtensions + ")";
                var Result = (FilterLabel + "|" + FilterExtensions);

                return Result;
            }
            protected set { this.FilterForOpen_ = value; }
        }
        private string FilterForOpen_ = null;

        /// <summary>
        /// Returns an save-file-dialog compatible filter based on this file type.
        /// </summary>
        public string FilterForSave
        {
            get { return this.FilterForSave_.NullDefault(this.FilterForOpen); }
            protected set { this.FilterForSave_ = value;  }
        }
        private string FilterForSave_ = null;
    }
}