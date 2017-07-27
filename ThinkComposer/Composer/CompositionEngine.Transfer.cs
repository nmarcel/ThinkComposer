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
// File   : CompositionEngine.Clipboard.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.06.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Takes care of the edition of a particular Composition instance (Transfer operations partial-file).
    /// </summary>
    public partial class CompositionEngine : DocumentEngine
    {
        /// <summary>
        /// Clipboard format code for ThinkComposer Ideas.
        /// </summary>
        public const string FORMAT_CODE_IDEA = ProductDirector.APPLICATION_NAME + ".Idea";

        public const string PASTE_IN_SELECTEDIDEAS = "InSelectedIdeas";

        public const string PASTE_MULTI_TARGETS = "MultiTargets";

        public const string PASTE_AS_MAINCONTENT = "AsMainContent";
        public const string PASTE_AS_DETAIL = "AsDetail";

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool ClipboardHasPasteableContent()
        {
            try
            {
                var Result = (Clipboard.GetFileDropList().Count > 0);
                Result = (Result || Clipboard.GetDataObject() != null);
                return Result;
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot access Windows Clipboard!");
            }

            return false;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void ClipboardCut(View SourceView)
        {
            if (ClipboardCopy(SourceView))
                MarkSelectedObjectsForCut();
        }

        public static void MarkSelectedObjectsForCut()
        {
            if (ClipboardTransferSourceView == null)
                return; // This should not happen

            ApplyVanishingToSelectedObjects(true);
            IsClipboardTransferring = true;
        }

        public static void UnmarkSelectedObjectsForCut()
        {
            if (ClipboardTransferSourceView == null)
                return;

            ApplyVanishingToSelectedObjects(false);
            IsClipboardTransferring = false;
        }

        private static void ApplyVanishingToSelectedObjects(bool IsVanishing)
        {
            ClipboardTransferSourceView.EditEngine.StartCommandVariation("Apply Vanishing");

            foreach (var VisObject in ClipboardTransferSelectedObjects)
            {
                VisObject.IsVanished = IsVanishing;
                VisObject.RenderRepresentatedObject();
            }

            ClipboardTransferSourceView.EditEngine.CompleteCommandVariation();
        }

        public static List<VisualObject> ClipboardTransferSelectedObjects = new List<VisualObject>();

        public static bool IsClipboardTransferring { get; protected set; }

        public static View ClipboardTransferSourceView
        {
            get { return ClipboardTransferSourceView_; }
            protected set
            {
                ClipboardTransferSourceView_ = value;
                ClipboardTransferSelectedObjects.Clear();
                IsClipboardTransferring = false;
            }
        }
        private static View ClipboardTransferSourceView_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool ClipboardCopy(View SourceView)
        {
            if (!SourceView.SelectedObjects.Any())
                return false;

            ClipboardTransferSourceView = SourceView;   // must be before the unmarking

            UnmarkSelectedObjectsForCut();

            General.Execute(() => Clipboard.Clear(), "Cannot access Windows Clipboard!");
            ClipboardTransferSelectedObjects = SourceView.SelectedObjects.Where(vobj => vobj is VisualElement || vobj is VisualComplement).ToList();

            string Data = null;
            var DataContainer = new DataObject();

            // Copy Idea's visual representations Guids
            var VisRespsGuids = new StringBuilder();

            try
            {
                // Copy visual Representations and individual Complements Guids and Drawings
                var DrawingComponents = new Dictionary<Drawing, int>(); // Temporal storage for Drawing and z-order per object
                Rect TotalBounds = Rect.Empty;
                var SelectedRepresentators = ClipboardTransferSelectedObjects.CastAs<VisualElement, VisualObject>()
                                                .Select(velem => velem.OwnerRepresentation).Distinct();
                foreach (var Selection in SelectedRepresentators)
                {
                    VisRespsGuids.Append(Selection.GlobalId.ToString() + '\t'); 
                    
                    var NewDrawings = Selection.CreateIntegralGraphic();
                    foreach(var NewDraw in NewDrawings)
                    {
                        TotalBounds = Rect.Union(TotalBounds, NewDraw.Key.Bounds);
                        DrawingComponents.Add(NewDraw.Key, NewDraw.Value);
                    }
                }

                var SelectedComplements = ClipboardTransferSelectedObjects.CastAs<VisualComplement, VisualObject>()
                                                .Where(vcomp => vcomp.Target.IsGlobal);
                foreach (var Selection in SelectedComplements)
                {
                    // Consider to append individual Complements?
                    // VisRespsGuids.Append(Selection.GlobalId.ToString() + '\t'); 

                    var NewDraw = Selection.CreateDraw(false);
                    TotalBounds = Rect.Union(TotalBounds, NewDraw.Bounds);
                    DrawingComponents.Add(NewDraw, Selection.ZOrder);
                }

                Data = VisRespsGuids.ToString();
                DataContainer.SetData(IdeaTransferFormat.Name, Data);

                // Draw considering z-order
                var DrawnObjects = new DrawingGroup();
                foreach (var DrawComponent in DrawingComponents.OrderBy(cmp => cmp.Value))
                    DrawnObjects.Children.Add(DrawComponent.Key);

                // Set margin
                TotalBounds = new Rect(TotalBounds.X - View.SNAPSHOT_MARGIN, TotalBounds.Y - View.SNAPSHOT_MARGIN,
                                       TotalBounds.Width + View.SNAPSHOT_MARGIN * 2.0, TotalBounds.Height + View.SNAPSHOT_MARGIN * 2.0);

                // Adjust position
                DrawnObjects.Transform = new TranslateTransform(-TotalBounds.X, -TotalBounds.Y);

                DrawnObjects.Children.Insert(0, new GeometryDrawing(Brushes.White, null, new RectangleGeometry(TotalBounds)));

                // Render to bitmap
                var Result = DrawnObjects.RenderToDrawingVisual().RenderToBitmap((int)TotalBounds.Width, (int)TotalBounds.Height);

                DataContainer.SetImage(Result);

                // Copy also as text only when pressing [Alt] (which takes precedence over image when pasting in MS-Office)
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {

                    // Copy only Ideas synopsis (without duplicates from shortcuts or synonyms)
                    var Ideas = ClipboardTransferSelectedObjects.CastAs<VisualElement, VisualObject>().Select(velem => velem.OwnerRepresentation.RepresentedIdea).Distinct();
                    var IdeasSynopsis = new StringBuilder();

                    foreach (var Selection in Ideas)
                        IdeasSynopsis.AppendLine(Selection.GetTextSynopsis());

                    Data = IdeasSynopsis.ToString();
                    DataContainer.SetText(Data);
                }

                General.Execute(() => Clipboard.SetDataObject(DataContainer), "Cannot access Windows Clipboard!");
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot Copy content and place it into clipboard. Problem: " + Problem.Message);
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void ClipboardPaste(View TargetView, bool AsIdeaShortcut = false, Point TargetPosition = default(Point), bool IsDroppingFiles = false)
        {
            if (IsClipboardTransferring && TargetView == ClipboardTransferSourceView)
            {
                UnmarkSelectedObjectsForCut();
                return;
            }

            try
            {
                // Paste Intra-Composition content ............................................................
                if (General.Execute(() => Clipboard.ContainsData(IdeaTransferFormat.Name),
                    "Cannot access Windows Clipboard!").Result || AsIdeaShortcut)
                {
                    InjectCompositionContentToView(TargetView, AsIdeaShortcut);
                    return;
                }

                // Determine target types
                ExtensionPanel ExtraOptions = null;
                IEnumerable<string> FilePaths = null;
                if (General.Execute(() => Clipboard.ContainsFileDropList(),
                                    "Cannot access Windows Clipboard!").Result)
                    FilePaths = General.Execute(() => Clipboard.GetFileDropList().Cast<string>(),
                                                      "Cannot access Windows Clipboard!").Result;

                var TargetIdeas = new List<Idea>();
                //- TargetIdeas.Add(TargetView.VisualizedCompositeIdea);

                if (TargetPosition == default(Point) || TargetPosition == Display.NULL_POINT)
                    TargetPosition = TargetView.CurrentPresentationCenter;

                var TargetRep = TargetView.SelectedObjects.Where(vobj => vobj is VisualElement)
                                    .Select(vobj => ((VisualElement)vobj).OwnerRepresentation).FirstOrDefault();

                if (General.Execute(() => Clipboard.ContainsText(), "Cannot access Windows Clipboard!").Result
                    || General.Execute(() => Clipboard.ContainsImage(), "Cannot access Windows Clipboard!").Result
                    || FilePaths != null)
                    ExtraOptions = CreatePastingExtraOptions(ExtraOptions, TargetRep, FilePaths);

                // Determine target
                string Selection = DetermineTargetRepresentations(TargetView, TargetPosition, ExtraOptions, TargetIdeas, TargetRep, FilePaths);
                if (Selection == null)
                    return;

                if (Selection == VisualComplement.__ClassDefinitor.TechName)
                    ClipboardPasteCreatingComplements(TargetView, TargetPosition, FilePaths);
                else
                    ClipboardPasteIntoIdeas(TargetView, ExtraOptions, FilePaths, TargetIdeas);
            }
            catch (Exception Problem)
            {
                if (TargetView.EditEngine.IsVariating)
                {
                    TargetView.EditEngine.CompleteCommandVariation();
                    TargetView.EditEngine.Undo();
                }
                Console.WriteLine("Cannot Paste content. Problem: " + Problem.Message);
            }
        }

        private static void ClipboardPasteCreatingComplements(View TargetView, Point TargetPosition, IEnumerable<string> FilePaths)
        {
            TargetView.EditEngine.StartCommandVariation("Clipboard Paste (creating Complements)");

            if (FilePaths != null)
            {
                foreach (var FilePath in FilePaths)
                {
                    if (FilePath != null && File.Exists(FilePath))
                        PasteCreateComplement(TargetView, TargetPosition, General.BytesToAppropriateObject(General.FileToBytes(FilePath)));
                }
            }
            else
            {
                if (General.Execute(() => Clipboard.ContainsImage(), "Cannot access Windows Clipboard!").Result)
                    PasteCreateComplement(TargetView, TargetPosition, Display.ImageFromClipboardDib());

                if (General.Execute(() => Clipboard.ContainsText(), "Cannot access Windows Clipboard!").Result)
                    PasteCreateComplement(TargetView, TargetPosition, General.Execute(() => Clipboard.GetText().ToStringAlways(), "Cannot access Windows Clipboard!").Result);
            }

            TargetView.EditEngine.CompleteCommandVariation();
        }

        private void ClipboardPasteIntoIdeas(View TargetView, ExtensionPanel ExtraOptions, IEnumerable<string> FilePaths, List<Idea> TargetIdeas)
        {
            var PasteAsDetail = true;

            if (ExtraOptions != null)
                PasteAsDetail = (ExtraOptions.GetSelectableOption() == PASTE_AS_DETAIL);

            TargetView.EditEngine.StartCommandVariation("Clipboard Paste (into Ideas)");

            if (FilePaths != null)
            {
                if (ExtraOptions.GetSwitchOption(PASTE_MULTI_TARGETS).IsTrue())
                {
                    var Index = 0;
                    foreach (var Target in TargetIdeas)
                    {
                        var FilePath = FilePaths.ElementAt(Index);
                        if (FilePath != null && File.Exists(FilePath))
                            PasteFileIntoIdea(Target, FilePath, PasteAsDetail);

                        Index++;
                    }
                }
                else
                    foreach (var FilePath in FilePaths)
                        if (FilePath != null && File.Exists(FilePath))
                            PasteFileIntoIdea(TargetIdeas.First(), FilePath, PasteAsDetail);
            }
            else
                InjectPastingContentToIdea(TargetView, TargetIdeas.First(), PasteAsDetail);

            TargetView.EditEngine.CompleteCommandVariation();
        }

        private static void PasteCreateComplement(View TargetView, Point TargetPosition, object Content)
        {
            OperationResult<VisualComplement> Complement = null;
            SimplePresentationElement Kind = null;
            string Property = null;

            if (Content is BitmapSource)
            {
                Kind = Domain.ComplementDefImage;
                Property = VisualComplement.PROP_FIELD_IMAGE;
            }
            else
            {
                if (Content is byte[])
                    Content = ((byte[])Content).BytesToString();
                else
                    if (!(Content is string))
                        Content = Content.ToStringAlways();

                Kind = Domain.ComplementDefText;
                Property = VisualComplement.PROP_FIELD_TEXT;
            }

            Complement = ComplementCreationCommand.CreateComplement(TargetView.OwnerCompositeContainer, Kind, TargetView, null, TargetPosition);

            if (Complement != null && Complement.WasSuccessful)
            {
                // IMPORTANT: The whole Content MUST be assigned in order to be undoable/readoable.
                Complement.Result.SetPropertyField(Property, Content);
                Complement.Result.Render();
            }
        }

        private static string DetermineTargetRepresentations(View TargetView, Point TargetPosition, ExtensionPanel ExtraOptions, List<Idea> TargetIdeas,
                                                             VisualRepresentation TargetRep, IEnumerable<string>FilePaths)
        {
            string Selection = null;

            if (TargetRep != null)
            {
                TargetIdeas.Clear();
                TargetIdeas.Add(TargetRep.RepresentedIdea);

                if (ExtraOptions != null)
                {
                    Selection = Display.DialogMultiOption("Paste...", "", null, ExtraOptions, true, null,
                                                            new SimplePresentationElement("In selection", PASTE_IN_SELECTEDIDEAS, "Paste into the selected Idea(s).",
                                                                                        Display.GetLibImage("accept.png")));
                    if (Selection.IsAbsent())
                        return null;
                }
            }
            else
            {
                var ConceptDefs = TargetView.OwnerCompositeContainer.CompositeContentDomain.ConceptDefinitions.ToArray();

                Selection = Display.DialogMultiOption("Paste...", "Select Concept type to Create", null, ExtraOptions, true, null, ConceptDefs);
                if (Selection.IsAbsent())
                    return null;

                if (Selection != VisualComplement.__ClassDefinitor.TechName)
                {
                    TargetIdeas.Clear();
                    var ConceptDef = ConceptDefs.First(cd => cd.TechName == Selection);

                    if (FilePaths == null)
                    {
                        var NewConcept = CreateConceptForPaste(TargetView, TargetPosition, ConceptDef, true);
                        TargetIdeas.Add(NewConcept);
                    }
                    else
                        FilePaths.ForEachIndexing(
                            (filepath, index) =>
                            {
                                var NewConcept = CreateConceptForPaste(TargetView, TargetPosition, ConceptDef, index == 0);
                                NewConcept.Name = Path.GetFileNameWithoutExtension(filepath);
                                TargetIdeas.Add(NewConcept);
                            });
                }
            }

            return Selection;
        }

        private static Concept CreateConceptForPaste(View TargetView, Point TargetPosition, ConceptDefinition ConceptDef, bool StartEdit = false)
        {
            var Creation = ConceptCreationCommand.CreateConcept(TargetView.OwnerCompositeContainer, ConceptDef,
                                                                TargetView, TargetPosition, 0, 0, true);
            if (Creation == null || !Creation.WasSuccessful)
                return null;

            if (StartEdit)
                TargetView.Presenter.PostCall(pres => TargetView.EditInPlace(Creation.Result.MainSymbol));

            return Creation.Result;
        }

        private static ExtensionPanel CreatePastingExtraOptions(ExtensionPanel ExtraOptions, VisualRepresentation TargetRep, IEnumerable<string> FilePaths)
        {
            ExtraOptions = ExtensionPanel.Create()
                .AddGroupPanel("Pasting options...")
                    .AddSelectableOption(PASTE_AS_MAINCONTENT, "As main content", true,
                                            "The pasted object is intended as Summary (when text) or Pictogram (if image).")
                    .AddSelectableOption(PASTE_AS_DETAIL, "As detail", false,
                                            "The pasted object must be appended as detail.");

            if (TargetRep == null && FilePaths.CountsAtLeast(2))
                ExtraOptions.AddSwitchOption(PASTE_MULTI_TARGETS, "Create multiple targets", true, "Create multiple targets (Concepts or Complements), one per each source file.");

            var ButtonPasteAsComplement = new PaletteButton("As Text/Image Complement", Domain.ComplementDefImage.Pictogram);
            ButtonPasteAsComplement.MinHeight = 32;
            ButtonPasteAsComplement.Summary = "Paste as a Text/Image Complement in the background";
            ButtonPasteAsComplement.Click +=
                ((sender, args) =>
                {
                    var OwnerWindow = ButtonPasteAsComplement.GetNearestVisualDominantOfType<Window>();
                    if (OwnerWindow == null)
                        return;

                    Display.SelectedOptionText = VisualComplement.__ClassDefinitor.TechName;
                    OwnerWindow.Close();
                });
            ExtraOptions.Children.Add(ButtonPasteAsComplement);
            return ExtraOptions;
        }

        public void InjectCompositionContentToView(View TargetView, bool InjectAsShortcut)
        {
            /*- var Content = General.Execute(() => Clipboard.GetDataObject(),
                                          "Cannot access Windows Clipboard!").Result;
            if (Content == null)
                return;

            var Formats = General.Execute(() => Content.GetFormats(false),
                                          "Cannot access Windows Clipboard!").Result;
            if (Formats == null)
                return; */

            // NOTE: Do not ask with  "if (ClipboardCutTargetView != null) ..."
            // Because the clipboard can be externally written.

            bool IsLocalCopy = (ClipboardTransferSourceView.OwnerCompositeContainer.OwnerComposition
                                == TargetView.OwnerCompositeContainer.OwnerComposition);

            if (!IsLocalCopy)
            {
                Display.DialogMessage("Attention", "Copy, Cut and Paste not supported between Compositions yet.");
                return;
            }

            /* Deprecated
            // Representations are identified as Text-GUIDs separated by tabs.
            var Source = Clipboard.GetData(IdeaTransferFormat.Name) as string;
            if (Source != null)
            {
                var CopiedRepresentations = GetLocalCompositionVisualRepresentations(Source);
            } */

            var CopiedObjects = ClipboardTransferSelectedObjects;

            if (CopiedObjects != null && CopiedObjects.Any())
            {
                // Determine total area to be pasted
                Rect TotalTargetArea = Rect.Empty;
                foreach (var VisObj in CopiedObjects)
                    TotalTargetArea = Rect.Union(TotalTargetArea, VisObj.TotalArea);

                // Determine center for pasting
                var BasePosition = new Point(this.CurrentView.CurrentPresentationCenter.X - (TotalTargetArea.Width / 2.0)
                                                * ((double)this.CurrentView.PageDisplayScale / 100.0),
                                                this.CurrentView.CurrentPresentationCenter.Y - (TotalTargetArea.Height / 2.0)
                                                * ((double)this.CurrentView.PageDisplayScale / 100.0));
                var InitialObject = CopiedObjects.First();
                var DeltaX = BasePosition.X - InitialObject.BaseLeft * ((double)this.CurrentView.PageDisplayScale / 100.0);
                var DeltaY = BasePosition.Y - InitialObject.BaseTop * ((double)this.CurrentView.PageDisplayScale / 100.0);

                // Paste into target
                TargetView.EditEngine.StartCommandVariation("Clipboard Paste");

                PasteVisualRepresentationsInto(TargetView, CopiedObjects, IsLocalCopy, InjectAsShortcut, DeltaX, DeltaY);

                if (IsClipboardTransferring)
                {
                    this.DeleteObjects(ClipboardTransferSelectedObjects);

                    ClipboardTransferSourceView = null;
                }

                TargetView.EditEngine.CompleteCommandVariation();
            }

            //! PENDING: Support pasting CSV and TSV to tables
        }

        public void InjectPastingContentToIdea(View TargetView, Idea TargetIdea, bool InjectAsDetail)
        {
            /*- var Content = General.Execute(() => Clipboard.GetDataObject(),
                                          "Cannot access Windows Clipboard!").Result;
            if (Content == null)
                return;

            var Formats = General.Execute(() => Content.GetFormats(false),
                                          "Cannot access Windows Clipboard!").Result;
            if (Formats == null)
                return; */

            BitmapSource SourceImage = null;
            string SourceText = null;
            byte[] Data = null;
            string MimeType = "";
            string DetailFileName = ""; // File extension needed to call external editor when editing attachment

            // NOTE: Do not ask with  "if (ClipboardCutTargetView != null) ..."
            // Because the clipboard can be externally written.

            var ContainsCSV = false;
            if (General.Execute(() => (ContainsCSV = Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue)) || Clipboard.ContainsText(),
                                "Cannot access Windows Clipboard!").Result)
            {
                SourceText = General.Execute(() => (ContainsCSV ? Clipboard.GetText(TextDataFormat.CommaSeparatedValue) : Clipboard.GetText()),
                                                "Cannot access Windows Clipboard!").Result;

                if (SourceText != null)
                {
                    // Gets "text/plain": General.GetMimeTypeFromContent(SourceText.StringToBytes())
                    MimeType = (ContainsCSV ? "text/csv" : General.GetMimeTypeFromContent(SourceText.StringToBytes()));
                    DetailFileName = "Detail " + TargetIdea.Details.Count.ToString() + ".txt";

                    Data = SourceText.StringToBytes();

                    if (InjectAsDetail)
                        AppendPastedDetail(TargetIdea, DetailFileName, MimeType, Data);
                    else
                        TargetIdea.Summary = SourceText;
                }
            }

            if (General.Execute(() => Clipboard.ContainsImage(),
                                "Cannot access Windows Clipboard!").Result)
            {
                SourceImage = Display.ImageFromClipboardDib();

                if (SourceImage != null)
                {
                    MimeType = "image/bmp";
                    DetailFileName = "Detail " + TargetIdea.Details.Count.ToString() + ".bmp";

                    if (InjectAsDetail)
                    {
                        Data = SourceImage.ToBytes();
                        AppendPastedDetail(TargetIdea, DetailFileName, MimeType, Data);
                    }
                    else
                        TargetIdea.Pictogram = SourceImage;
                }
            }

            //! PENDING: Support pasting CSV and TSV to tables

            TargetIdea.UpdateVisualRepresentators();
        }

        // POSTPONED: Paste Ideas from other Compositions.
        // Currently, that would create a mix of Ideas with different Domains.
        public VisualRepresentation[] GetLocalCompositionVisualRepresentations(string Source)
        {
            if (Source.IsAbsent())
                return (new VisualRepresentation[0]);

            var TextGuids = Source.Split('\t');
            var Result = this.TargetComposition.DeclaredIdeas
                                .SelectMany(idea =>
                                            idea.VisualRepresentators.Where(visrep =>
                                                TextGuids.Any(tguid => tguid.Trim().ToLower() == visrep.GlobalId.ToString())))
                                                    .Distinct().ToArray();
            return Result;
        }

        public void PasteVisualRepresentationsInto(View TargetView, IEnumerable<VisualObject> OriginalObjects,
                                                   bool CopiedFromSameComposition, bool CreateAsShortcut, double DeltaX, double DeltaY)
        {
            var OriginalRepresentators = OriginalObjects.CastAs<VisualElement, VisualObject>()
                                            .Select(velem => velem.OwnerRepresentation).Distinct();

            if (CreateAsShortcut)
            {
                TargetView.UnselectAllObjects();   // For selecting the object being pasted

                var First = true;
                foreach (var VisRep in OriginalRepresentators)
                {
                    var VisRepShortcut = AppendShortcut(TargetView, VisRep, DeltaX, DeltaY);
                    if (VisRepShortcut == null)
                        continue;

                    TargetView.SelectObject(VisRepShortcut.MainSymbol, First);

                    VisRepShortcut.Render();
                    First = false;
                }

                return;
            }

            // Determine the Ideas of the copied sub-graph for later re-pointing.
            // Each capsule contains: Value0=Original Representation, Value1=Cloned Representation.
            var InterrelationsMap = new List<Capsule<VisualRepresentation, VisualRepresentation>>();

            // Append cloned individual Complements
            var OriginalComplements = OriginalObjects.CastAs<VisualComplement, VisualObject>()
                                            .Where(vcomp => vcomp.Target.IsGlobal);

            foreach (var VisComp in OriginalComplements)
                if (AppendClone(TargetView, VisComp, DeltaX, DeltaY) == null)
                    break;

            // Append cloned Representations
            foreach (var VisRep in OriginalRepresentators)
            {
                var VisRepClone = AppendClone(TargetView, VisRep, DeltaX, DeltaY);
                if (VisRepClone == null)
                    break;

                InterrelationsMap.Add(Capsule.Create(VisRep, VisRepClone));
            }

            // Recreate links
            var VisRelInterrelations = InterrelationsMap.Where(intrel => intrel.Value0 is RelationshipVisualRepresentation &&
                                                                         intrel.Value1 != null);
            foreach (var VisRelInterrel in VisRelInterrelations)
                ReappointReferences((RelationshipVisualRepresentation)VisRelInterrel.Value0,
                                    (RelationshipVisualRepresentation)VisRelInterrel.Value1,
                                    InterrelationsMap, DeltaX, DeltaY);

            // Select the just pasted objects
            TargetView.UnselectAllObjects();

            foreach (var InterRel in InterrelationsMap)
                if (InterRel.Value1 != null)
                    TargetView.Manipulator.ApplySelection(InterRel.Value1.MainSymbol, false, true);
        }

        private void ReappointReferences(RelationshipVisualRepresentation SourceRelVisRep,
                                         RelationshipVisualRepresentation ClonedRelVisRep,
                                         List<Capsule<VisualRepresentation, VisualRepresentation>> InterrelationsMap,
                                         double DeltaX, double DeltaY)
        {
            // NOTE: Each InterrelationsMap capsule contains: Value0=Original Representation, Value1=Cloned Representation.

            foreach (var SourceVisConn in SourceRelVisRep.VisualConnectors)
            {
                var InterRel = InterrelationsMap.FirstOrDefault(
                                    intrel => intrel.Value0.RepresentedIdea == SourceVisConn.RepresentedLink.AssociatedIdea);
                if (InterRel == null || InterRel.Value1 == null)
                    continue;

                var ClonedRoleLink = new RoleBasedLink(ClonedRelVisRep.RepresentedRelationship, InterRel.Value1.RepresentedIdea,
                                                       SourceVisConn.RepresentedLink.RoleDefinitor, SourceVisConn.RepresentedLink.RoleVariant);

                if (SourceVisConn.RepresentedLink.Descriptor != null)
                    ClonedRoleLink.Descriptor = (SimplePresentationElement)SourceVisConn.RepresentedLink.Descriptor.CreateClone();

                // Next, the origin and target symbols are pointed and eventually replaced by the cloned ones
                // (if they were within the pasted visual representations)
                VisualSymbol OriginSymbol = SourceVisConn.OriginSymbol;
                var OriginInterRel = InterrelationsMap.FirstOrDefault(intrel => intrel.Value0.MainSymbol == SourceVisConn.OriginSymbol);
                if (OriginInterRel != null)
                    OriginSymbol = OriginInterRel.Value1.MainSymbol;

                VisualSymbol TargetSymbol = SourceVisConn.TargetSymbol;
                var TargetInterRel = InterrelationsMap.FirstOrDefault(intrel => intrel.Value0.MainSymbol == SourceVisConn.TargetSymbol);
                if (TargetInterRel != null)
                    TargetSymbol = TargetInterRel.Value1.MainSymbol;

                var ClonedVisConn = new VisualConnector(ClonedRelVisRep, ClonedRoleLink, OriginSymbol, TargetSymbol,
                                                        new Point(SourceVisConn.OriginPosition.X + DeltaX, SourceVisConn.OriginPosition.Y + DeltaY),
                                                        new Point(SourceVisConn.TargetPosition.X + DeltaX, SourceVisConn.TargetPosition.Y + DeltaY));

                ClonedVisConn.IntermediatePosition = (SourceVisConn.IntermediatePosition == Display.NULL_POINT
                                                      ? Display.NULL_POINT
                                                      : new Point(SourceVisConn.IntermediatePosition.X + DeltaX, SourceVisConn.IntermediatePosition.Y + DeltaY));

                ClonedRelVisRep.RepresentedRelationship.AddLink(ClonedRoleLink);
                ClonedRelVisRep.AddVisualPart(ClonedVisConn);
            }

            ClonedRelVisRep.Render();
        }

        /* POSTPONED
        /// <summary>
        /// Creates a local composition set of copies from visual representations of an external composition.
        /// This manages the referencing of domain based content (idea and table-structure definitions, formats, markers, etc.).
        /// </summary>
        public IEnumerable<VisualRepresentation> CreateLocalCompositionCopies(IEnumerable<VisualRepresentation> ExternalCompositionRepresentations)
        {
            // Detect Idea definitions
            // Detect Table-Structure definitions


        }

        public void ExtendLocalDomainFromExternal(Domain ExternalDomain)
        {

        }

        public class DomainExtensionEquivalences
        {
            public List<ConceptDefinition, ConceptDefinition> ConceptDefinitions; // Local instance, external instance.
        } */

        /* POSTPONED: This is to obtain recursive composing interrelated content.
        // It could be later used for entire subgraph cloning, but
        // consider mapping visual-relationships as subgraph also may contain Views
        // and therefore interrelated visual-representations
        public void DetermineInterrelatedIdeas(Idea SourceIdea, IList<Capsule<Idea, Idea>> InterrelationsMap)
        {
            // Return if already considered
            if (InterrelationsMap.Any(cap => SourceIdea == cap.Value0))
                return;

            // Register source Idea
            InterrelationsMap.Add(Capsule.Create(SourceIdea, (Idea)null));

            // Evaluate associations
            foreach (var AssociatedIdea in SourceIdea.AssociatingLinks.Select(link => link.AssociatedIdea))
                DetermineInterrelatedIdeas(AssociatedIdea, InterrelationsMap);

            // Evaluate composites
            foreach (var CompositeIdea in SourceIdea.CompositeIdeas)
                DetermineInterrelatedIdeas(CompositeIdea, InterrelationsMap);
        } */

        public VisualRepresentation AppendShortcut(View TargetView, VisualRepresentation OriginalRepresentation, double DeltaX, double DeltaY)
        {
            var NewPosition = new Point(OriginalRepresentation.MainSymbol.BaseCenter.X + DeltaX,
                                        OriginalRepresentation.MainSymbol.BaseCenter.Y + DeltaY);

            return AppendShortcut(TargetView, OriginalRepresentation.RepresentedIdea, NewPosition);
        }

        public VisualRepresentation AppendShortcut(View TargetView, Idea OriginalIdea, Point Position)
        {
            VisualRepresentation Result = null;

            if (OriginalIdea is Concept)
            {
                var RefConcept = (Concept)OriginalIdea;
                Result = ConceptCreationCommand.CreateConceptVisualRepresentation(RefConcept, TargetView, Position, true, true);
            }
            else
            {
                var RefRelationship = (Relationship)OriginalIdea;
                if (RefRelationship.RelationshipDefinitor.Value.IsSimple && RefRelationship.RelationshipDefinitor.Value.HideCentralSymbolWhenSimple)
                    Console.WriteLine("Shortcut omitted for simple Relationship with its Symbol hidden.");
                else
                    Result = RelationshipCreationCommand.CreateRelationshipVisualRepresentation(RefRelationship, TargetView, Position, true);
            }

            return Result;
        }

        public VisualComplement AppendClone(View TargetView, VisualComplement OriginalComplement,
                                            double DeltaX, double DeltaY)
        {
            var NewPosition = new Point(OriginalComplement.BaseCenter.X + DeltaX,
                                        OriginalComplement.BaseCenter.Y + DeltaY);

            var Result = OriginalComplement.GenerateIndependentDuplicate(Ownership.Create<View, VisualSymbol>(TargetView));
            Result.BaseCenter = NewPosition;

            TargetView.PutComplement(Result);

            return Result;
        }

        // IMPORTANT: This method is only for creating the Visual-Symbol parts of the representations.
        // Later, the appropriate interconnections should be added.
        public VisualRepresentation AppendClone(View TargetView, VisualRepresentation OriginalRepresentation,
                                                double DeltaX, double DeltaY)
        {
            var NewPosition = new Point(OriginalRepresentation.MainSymbol.BaseCenter.X + DeltaX,
                                        OriginalRepresentation.MainSymbol.BaseCenter.Y + DeltaY);
            VisualRepresentation Result = null;

            if (OriginalRepresentation is ConceptVisualRepresentation)
            {
                var OriginalConceptRep = OriginalRepresentation as ConceptVisualRepresentation;
                var ConceptRepCreation = ConceptCreationCommand.CreateConceptAndRepresentationCopy(OriginalConceptRep, TargetView, NewPosition);
                if (ConceptRepCreation == null || !ConceptRepCreation.WasSuccessful)
                {
                    Console.WriteLine("Cannot append new Concept. Problem: {0}", ConceptRepCreation.Message);
                    return null;
                }

                Result = ConceptRepCreation.Result;
            }
            else
            {
                var OriginalRelationshipRep = OriginalRepresentation as RelationshipVisualRepresentation;
                var RelationshipRepCreation = RelationshipCreationCommand.CreateRelationshipAndRepresentationCopy(OriginalRelationshipRep, TargetView, NewPosition);
                if (RelationshipRepCreation == null || !RelationshipRepCreation.WasSuccessful)
                {
                    Console.WriteLine("Cannot append new Relationship. Problem: {0}", RelationshipRepCreation.Message);
                    return null;
                }

                Result = RelationshipRepCreation.Result;
            }

            Result.RepresentedIdea.AddToComposite(TargetView.OwnerCompositeContainer);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool AppendPastedDetail(Idea Target, string DetailName, string MimeType, byte[] DetailContent)
        {
            if (DetailName.IsAbsent() || MimeType.IsAbsent() || DetailContent == null || DetailContent.Length < 1)
                return false;

            ContainedDetail Detail = null;

            if (MimeType.StartsWith("text/"))
            {
                var Owner = Ownership.Create<IdeaDefinition,Idea>(Target);
                var Content = DetailContent.BytesToString();
                if (Content.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                {
                    var NewLink = new ResourceLink(Target, new Assignment<DetailDesignator>(
                                                                DomainServices.CreateLinkDesignation(
                                                                    Owner,
                                                                    DetailName),
                                                                true));
                    NewLink.TargetLocation = Content;
                    Detail = NewLink;
                }
                else
                    if (MimeType.EndsWith("/tsv") || MimeType.EndsWith("/csv"))
                    {
                        var Delimiter = (MimeType.EndsWith("/csv")
                                         ? General.GetCurrentTextListDelimiter()
                                         : "\t");
                        var TextRecords = General.LoadStreamDelimitedIntoStrings(Content.StringToStream(), Delimiter);

                        var TypingResult = DomainServices.GenerateTypedRecordsList(TextRecords.Item1);

                        var TableDef = DomainServices.CreateCompatibleTableDefinition(Target.OwnerComposition.CompositeContentDomain,
                                                                                      DetailName + " - TableDef",
                                                                                      TypingResult.Item2, TextRecords.Item2);
                        var Designator = new TableDetailDesignator(Owner, TableDef, true, DetailName, DetailName.TextToIdentifier());
                        var NewTable = new Table(Target, Designator.Assign<DetailDesignator>(true));
                        foreach(var DataRecord in TypingResult.Item1)
                            NewTable.Add(new TableRecord(NewTable, DataRecord));

                        Detail = NewTable;
                    }
            }

            if (Detail == null)
            {
                var Route = new Uri(DetailName, UriKind.RelativeOrAbsolute);
                Detail = this.CreateIdeaDetailAttachment(Ownership.Create<IdeaDefinition, Idea>(Target),
                                                         Target, DetailName, Route, DetailContent, MimeType);
                if (Detail == null)
                    return false;
            }

            Target.Details.AddNew(Detail);

            return true;
        }

        public void PasteFileIntoIdea(Idea TargetIdea, string FilePath, bool AsDetail)
        {
            var InitialName = Path.GetFileNameWithoutExtension(FilePath);

            if (AsDetail)
            {
                var Route = new Uri(FilePath, UriKind.Absolute);
                var Detail = this.CreateIdeaDetailAttachment(Ownership.Create<IdeaDefinition, Idea>(TargetIdea),
                                                             TargetIdea, InitialName, Route);
                if (Detail == null)
                    return;

                TargetIdea.Details.AddNew(Detail);
            }
            else
            {
                var Content = General.BytesToAppropriateObject(General.FileToBytes(FilePath));

                if (Content is ImageSource)
                    TargetIdea.Pictogram = (ImageSource)Content;
                else
                    TargetIdea.Summary = (Content is byte[] ? ((byte[])Content).BytesToString() : Content.ToStringAlways());

                // PENDING: IF FILE IS .CSV OR .TSV (DETECT COMMA AND TAB SEPARATED CONTENT), THEN CREATE TABLE.
                //          THIS ALSO WILL BE USEFUL FOR PASTING FROM EXCEL.
            }

            TargetIdea.UpdateVisualRepresentators();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}