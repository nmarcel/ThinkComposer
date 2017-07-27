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
// File   : DocumentEngine.cs
// Object : Instrumind.Common.EntityBase.DocumentEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.17 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using System.Windows.Controls;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Provides the general mechanism and services for the interactive creation of documents.
    /// </summary>
    public abstract partial class DocumentEngine : EntityEditEngine, IStorageLocation, INotifyPropertyChanged
    {
        /// <summary>
        /// File extension for content stored in native (.NET binary) serialized format.
        /// </summary>
        public static string NativeFormatExtension = ".bin";

        /// <summary>
        /// Default document package part name.
        /// </summary>
        public static readonly Uri PART_DEFAULT = new Uri("/Document", UriKind.Relative);

        /// <summary>
        /// Part name for the package pictogram.
        /// </summary>
        public static readonly Uri PART_PICTOGRAM = new Uri("/Pictogram.png", UriKind.Relative);

        /// <summary>
        /// Part name for the package snapshot.
        /// </summary>
        public static readonly Uri PART_SNAPSHOT = new Uri("/Snapshot.jpg", UriKind.Relative);

        public const double PART_PICTOGRAM_SIZE = 32;
        public const double PART_SNAPSHOT_WIDTH = 256;
        public const double PART_SNAPSHOT_HEIGHT = 128;

        public const string FILEEXT_SAV_NEW = ".new";
        public const string FILEEXT_SAV_OLD = ".old";

        /// <summary>
        /// Max quantity of referenced recent documents.
        /// </summary>
        public const int MAX_RECENT_DOCS = 256;

        /// <summary>
        /// Recently used (stored or loaded) documents.
        /// </summary>
        public static readonly List<string> RecentDocuments = new List<string>();

        /// <summary>
        /// Document managed by this engine.
        /// </summary>
        public abstract override ISphereModel TargetDocument { get; }

        /// <summary>
        /// Indicates wether this (targeted) Document is selected for later applying a command.
        /// </summary>
        public bool IsSelected
        {
            get { return (this.TargetDocument == null ? false : this.TargetDocument.IsSelected); }
            set { if (this.TargetDocument != null) this.TargetDocument.IsSelected = value; }
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DocumentEngine()
        {
            LoadRecentDocuments();
        }

        /// <summary>
        /// Saves a content-object, of the specified kind and content-type, in an specified location and with optional Part-URI.
        /// Returns an error message if problems were detected.
        /// </summary>
        public static string StoreToLocation<TContent>(TContent Content, string Kind, string ContentType, Uri Location, Uri PartUri = null,
                                                       bool RegisterAsRecentDoc = true, bool SilentSave = false,
                                                       IFormalizedRecognizableElement Descriptor = null, Visual Snapshot = null, bool SafeSaving = true)
        {
            if (Location == null)
                throw new UsageAnomaly("Cannot store document without a destination location.", Location);

            if (!SilentSave)
                Console.WriteLine("Writing file: " + Location.LocalPath);

            SafeSaving = (SafeSaving && File.Exists(Location.LocalPath));
            var WorkFilePath = (SafeSaving
                               ? Path.Combine(Path.GetDirectoryName(Location.LocalPath),
                                              Path.GetFileName(Location.LocalPath) + "." + DateTime.Now.ToString("yyyyMMdd-hhmmss") + FILEEXT_SAV_NEW)
                               : Location.LocalPath);

            try
            {
                var Pack = Package.Open(WorkFilePath, FileMode.Create);
                PackagePart Part = null;

                if (PartUri == null)
                    PartUri = PART_DEFAULT;

                // Main Content
                if (Pack.PartExists(PartUri))
                    Part = Pack.GetPart(PartUri);
                else
                    Part = Pack.CreatePart(PartUri, ContentType, CompressionOption.Maximum);

                /*T var TestFile = new FileStream(Location.LocalPath + ".BIN", FileMode.Create);
                var NewSer = new StandardBinarySerializer(TestFile);
                NewSer.Serialize(Content);
                TestFile.Close();
                Display.DialogMessage("TEST", "Saved using new binary format."); */

                BytesHandling.Serialize<TContent>(Content, Part.GetStream());

                // Descriptor
                if (Descriptor != null)
                {
                    Pack.PackageProperties.Title = Descriptor.Name;
                    Pack.PackageProperties.Subject = Descriptor.TechName;
                    Pack.PackageProperties.Identifier = Descriptor.GlobalId.ToString();
                    Pack.PackageProperties.Description = Descriptor.Summary;

                    if (Descriptor.Version != null)
                    {
                        Pack.PackageProperties.Version = Descriptor.Version.VersionNumber;
                        Pack.PackageProperties.Revision = Descriptor.Version.VersionSequence.ToStringAlways();
                        Pack.PackageProperties.Creator = Descriptor.Version.Creator;
                        Pack.PackageProperties.Created = Descriptor.Version.Creation;
                        Pack.PackageProperties.LastModifiedBy = Descriptor.Version.LastModifier;
                        Pack.PackageProperties.Modified = Descriptor.Version.LastModification;
                    }

                    if (Descriptor.Pictogram != null)
                    {
                        if (Pack.PartExists(PART_PICTOGRAM))
                            Part = Pack.GetPart(PART_PICTOGRAM);
                        else
                            Part = Pack.CreatePart(PART_PICTOGRAM, "image/png", CompressionOption.Normal);

                        var Encoder = new PngBitmapEncoder();
                        using (var Torrent = Part.GetStream())
                            Display.ExportImageTo(Encoder, Torrent, Descriptor.Pictogram.ToVisual(PART_PICTOGRAM_SIZE, PART_PICTOGRAM_SIZE),
                                                    (int)PART_PICTOGRAM_SIZE, (int)PART_PICTOGRAM_SIZE);
                    }
                }

                // Snapshot
                if (Snapshot != null)
                {
                    if (Pack.PartExists(PART_SNAPSHOT))
                        Part = Pack.GetPart(PART_SNAPSHOT);
                    else
                        Part = Pack.CreatePart(PART_SNAPSHOT, "image/jpg", CompressionOption.Normal);

                    var Encoder = new JpegBitmapEncoder();
                    Encoder.QualityLevel = Display.DEF_JPEG_QUALITY;
                    using (var Torrent = Part.GetStream())
                        Display.ExportImageTo(Encoder, Torrent, Snapshot,
                                                (int)PART_SNAPSHOT_WIDTH, (int)PART_SNAPSHOT_HEIGHT);
                }
                else
                    if (Pack.PartExists(PART_SNAPSHOT))
                        Pack.DeletePart(PART_SNAPSHOT);

                Pack.Close();
            }
            catch (Exception Problem)
            {
                AppExec.LogException(Problem);
                return "Cannot save content into file.\n" +
                       "Anomaly: " + Problem.Message;
            }

            var Identified = Content as IIdentifiableElement;

            if (!SilentSave)
                Console.WriteLine("Document (" + Kind + ") successfully stored: "
                                    + (Identified == null ? Location.ToString() : Identified.Name));

            if (RegisterAsRecentDoc)
                RegisterRecentDocument(Location.LocalPath);

            if (SafeSaving)
            {
                // Create a temporal "old" name for the original file (meaningful, so it could be recovered)
                var OrigFilePath = Location.LocalPath;
                var DeleteFilePath = Path.Combine(Path.GetDirectoryName(OrigFilePath),
                                                  Path.GetFileName(OrigFilePath) + "." + DateTime.Now.ToString("yyyyMMdd-hhmmss") + FILEEXT_SAV_OLD);
                try
                {
                    // Rename the original file to the temporal "old" name
                    File.Move(OrigFilePath, DeleteFilePath);

                    // Rename the working "new" file to that of the original
                    File.Move(WorkFilePath, OrigFilePath);

                    // Delete the temporal "old" file
                    File.Delete(DeleteFilePath);
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Unsuccessful file saving of: " + OrigFilePath);
                    Console.WriteLine("Backup Old-file: " + DeleteFilePath);
                    Console.WriteLine("Temporal New-file: " + WorkFilePath);

                    return "Cannot complete the file saving.\n" +
                           "Anomaly: " + Problem.Message;
                }
            }

            return "";
        }

        /// <summary>
        /// Generates and returns a document-file-info from the specified location or null if no found.
        /// </summary>
        public static DocumentFileInfo GetDocumentFileInfoFrom(string Location)
        {
            var Result = new DocumentFileInfo();
            Result.FileLocation = Location;

            try
            {
                Result.SortKey = "_";

                var Pack = Package.Open(Location, FileMode.Open, FileAccess.Read, FileShare.Read);

                if (Pack.PackageProperties != null)
                {
                    Result.Name = Pack.PackageProperties.Title;
                    Result.TechName = Pack.PackageProperties.Subject;
                    Result.Id = new Guid(Pack.PackageProperties.Identifier);
                    Result.Summary = Pack.PackageProperties.Description;
                    Result.VersionNumRev = Pack.PackageProperties.Version +
                                           " [" + Pack.PackageProperties.Revision + "]";
                    Result.VersionCreation = Pack.PackageProperties.Created.AsExistent().ToShortDateString() +
                                             " by " + Pack.PackageProperties.Creator;
                    Result.VersionLastModif = Pack.PackageProperties.Modified.AsExistent().ToShortDateString() +
                                              " by " + Pack.PackageProperties.LastModifiedBy;

                    // For sorting...
                    Result.SortKey = (Pack.PackageProperties.Modified.AsExistent().ToString("yyyyMMdd")
                                      + Pack.PackageProperties.LastModifiedBy).AsInverseSortable();
                }

                PackagePart Part = null;

                if (Pack.PartExists(PART_PICTOGRAM))
                {
                    Part = Pack.GetPart(PART_PICTOGRAM);
                    Result.Pictogram = Display.ConvertByteArrayToImageSource(Part.GetStream().ToBytes());
                }

                if (Pack.PartExists(PART_SNAPSHOT))
                {
                    Part = Pack.GetPart(PART_SNAPSHOT);
                    Result.Snapshot = Display.ConvertByteArrayToImageSource(Part.GetStream().ToBytes());
                }

                Pack.Close();
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot interpret file [" + Location + "]. Problem: " + Problem.Message);
            }

            return Result;
        }

        /// <summary>
        /// Loads and returns a content-object, of the specified kind, from the supplied location and optional Part-URI.
        /// </summary>
        protected static Tuple<TContent, string> LoadFromLocation<TContent>(string Kind, Uri Location, Uri PartUri = null,
                                                                            bool RegisterAsRecentDoc = true)
        {
            TContent Content = default(TContent);

            if (Location == null)
                throw new UsageAnomaly("Cannot load document (" + Kind + ") without a source location.");

            if (!File.Exists(Location.LocalPath))
                throw new ExternalAnomaly("File not found: " + Location.LocalPath);

            try
            {
                Console.WriteLine("Reading file: " + Location.LocalPath);

                if (PartUri == null)
                    PartUri = PART_DEFAULT;

                var Pack = Package.Open(Location.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (Pack == null)
                    throw new ExternalAnomaly("Zip package cannot be open.");

                var Parts = Pack.GetParts();
                var Part = Parts.Where(part => part.Uri.IsEqual(PartUri)).FirstOrDefault().NullDefault(Parts.First());
                Content = BytesHandling.Deserialize<TContent>(Part.GetStream());
                Pack.Close();

                var Identified = Content as IIdentifiableElement;

                Console.WriteLine("Document (" + Kind + ") successfully loaded: "
                                      + (Identified == null ? Location.ToString() : Identified.Name) + ". Part=" + PartUri.ToString());

                if (RegisterAsRecentDoc)
                    RegisterRecentDocument(Location.LocalPath);
            }
            catch (Exception Problem)
            {
                throw new ExternalAnomaly("Cannot load document (" + Kind + ") from: " + Location.LocalPath, Problem);
            }

            return Tuple.Create(Content, "");
        }

        /// <summary>
        /// Register the specified document as recently used.
        /// </summary>
        public static void RegisterRecentDocument(string DocumentLocation)
        {
            if (DocumentLocation.Trim().IsAbsent())
                return;

            RecentDocuments.Remove(DocumentLocation);
            RecentDocuments.Insert(0, DocumentLocation);

            var MaxRegisters = AppExec.GetConfiguration<int>("Application", "MaxRecentDocuments", MAX_RECENT_DOCS);

            if (RecentDocuments.Count > MaxRegisters)
                RecentDocuments.RemoveRange(MaxRegisters, RecentDocuments.Count - MaxRegisters);

            try
            {
                var Strings = RecentDocuments.GetConcatenation(null, Environment.NewLine);
                General.StringToFile(AppExec.RecentDocumentsFilePath, Strings);
                /*- StreamWriter Writer = new StreamWriter(new FileStream(AppExec.RecentDocumentsFilePath,
                                                                      FileMode.OpenOrCreate,
                                                                      FileAccess.Write,
                                                                      FileShare.Read));
                foreach (var DocLocation in RecentDocuments)
                    Writer.WriteLine(DocLocation);

                Writer.Flush();
                Writer.Close(); */
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot write Recent-Documents list to '{0}'. Cause: {1}", AppExec.RecentDocumentsFilePath, Problem.Message);
            }
        }

        /// <summary>
        /// Loads the current list of recently used documents.
        /// </summary>
        public static void LoadRecentDocuments()
        {
            RecentDocuments.Clear();

            if (File.Exists(AppExec.RecentDocumentsFilePath))
                try
                {
                    RecentDocuments.AddRange(File.ReadAllLines(AppExec.RecentDocumentsFilePath).Where(line => !line.Trim().IsAbsent()));
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot read Recent-Documents list from '{0}'. Cause: {1}", AppExec.RecentDocumentsFilePath, Problem.Message);
                }
        }

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Physical location of the working document
        /// </summary>
        public virtual Uri Location { get; set; }

        #region IStorageLocation Members

        public Uri FullLocation
        {
            get { return this.Location; }
        }

        public string SimplifiedLocation
        {
            get
            {
                if (this.Location == null)
                    return "[Unsaved Document (No Folder)]";

                string Route = this.Location.LocalPath;
                return Path.GetFileName(Route) + " (" + Path.GetDirectoryName(Route) + ")";
            }
        }

        #endregion

        /// <summary>
        /// Visualization provider for document views.
        /// </summary>
        public IDocumentVisualizer Visualizer { get; protected set; }

        /// <summary>
        /// Views opened previously to a document switch or close.
        /// </summary>
        public IList<IDocumentView> LastOpenedViews { get; set; }

        /// <summary>
        /// Shows the underlying active document.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Returns the currently exposed Palettes for adding Concepts of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedConceptsPalettes();

        /// <summary>
        /// Returns the currently exposed Palettes for adding Relationships of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedRelationshipsPalettes();

        /// <summary>
        /// Returns the currently exposed Palettes for adding Marks of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedMarkersPalettes();

        /// <summary>
        /// Returns the currently exposed Palettes for adding Complements of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedComplementsPalettes();

        /// <summary>
        /// Returns the currently exposed Items of the specified Concept Palette of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedItemsOfConceptPalette(IRecognizableElement Palette);

        /// <summary>
        /// Returns the currently exposed Items of the specified Relationship Palette of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedItemsOfRelationshipPalette(IRecognizableElement Palette);

        /// <summary>
        /// Returns the currently exposed Items of the specified Marker Palette of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedItemsOfMarkerPalette(IRecognizableElement Palette);

        /// <summary>
        /// Returns the currently exposed Items of the specified Complement Palette of this Document Engine.
        /// </summary>
        public abstract IEnumerable<IRecognizableElement> GetExposedItemsOfComplementPalette(IRecognizableElement Palette);

        /// <summary>
        /// Executes the appropriate action for creating a new item for the specified Palette.
        /// </summary>
        public abstract void ApplyPaletteItemCreation(IRecognizableElement Palette);

        /// <summary>
        /// Executes the appropriate action for when the supplied Item, of the specified Palette, was selected or activated and intended for immediate use.
        /// </summary>
        public abstract void ApplyPaletteItemSelection(IRecognizableElement Item, IRecognizableElement Palette, bool ImmediateUse);

        /// <summary>
        /// Tells to the Document Engine to cancel the currently running operation, if any.
        /// </summary>
        public abstract void DoCancelOperation();

        /// <summary>
        /// Tells to the Document Engine to delete the currently selected object(s), if any.
        /// </summary>
        public abstract void DoDeleteSelection();

        /// <summary>
        /// Reacts to a document view changed.
        /// </summary>
        public abstract void ReactToViewChanged(IDocumentView NewView);

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}