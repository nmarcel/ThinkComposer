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
// File   : CompositionEngine.Interaction.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
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
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Takes care of the edition of a particular Composition instance (Interaction partial-file).
    /// </summary>
    public partial class CompositionEngine : DocumentEngine
    {
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void ReactToViewChanged(IDocumentView NewView)
        {
            ObserveView(NewView as View);
        }

        protected KeyEventHandler KeyActionedAction = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Prints the current active view
        /// </summary>
        public void PrintCurrentView()
        {
            if (this.CurrentView == null)
                return;

            /*
            var PrintSettings = new PrintDialog();
            var DoPrint = PrintSettings.ShowDialog();
            if (!DoPrint.IsTrue())
                return;

            var Margin = (ReportConfiguration.DEFAULT_PAGE_MARGIN_CMS * 2.56) * Display.WPF_DPI;
            var SourceDocument = this.CurrentView.ToDocument(PrintSettings.PrintableAreaWidth,
                                                             PrintSettings.PrintableAreaHeight,
                                                             null, // Old (now can use Info-Card): this.CurrentView.OwnerCompositeContainer.OwnerComposition.Name,
                                                             null, // Old (now can use Info-Card): this.CurrentView.Name,
                                                             PrintSettings.PrintTicket.PageOrientation.IsOneOf(PageOrientation.Landscape, PageOrientation.ReverseLandscape),
                                                             false,   // Borders are not necessary
                                                             Margin, Margin, Margin, Margin); */

            var DialogResult = Display.DialogPrintSetup();

            if (DialogResult == null)
                return;

            // PENDING (?): APPLY THE SPECIFIED SETTINGS.
            var SourceDocument = this.CurrentView.ToDocument(DialogResult.Item1.Landscape ? DialogResult.Item1.PrintableArea.Height : DialogResult.Item1.PrintableArea.Width,
                                                             DialogResult.Item1.Landscape ? DialogResult.Item1.PrintableArea.Width : DialogResult.Item1.PrintableArea.Height,
                                                             null, // Old (now can use Info-Card): this.CurrentView.OwnerCompositeContainer.OwnerComposition.Name,
                                                             null, // Old (now can use Info-Card): this.CurrentView.Name,
                                                             false,   // Borders are not necessary
                                                             DialogResult.Item1.Margins.Left,
                                                             DialogResult.Item1.Margins.Top,
                                                             DialogResult.Item1.Margins.Right,
                                                             DialogResult.Item1.Margins.Bottom);

            var DocTitle = this.CurrentView.OwnerCompositeContainer.OwnerComposition.Name + " - " + this.CurrentView.Name;
            PrintPreviewControl = new PrintPreviewer(SourceDocument, null, DocTitle);
            Display.OpenContentDialogWindow(ref WinPrintPreview, "Print Preview of: " + DocTitle, PrintPreviewControl);
        }

        private static PrintPreviewer PrintPreviewControl = null;
        private static DialogOptionsWindow WinPrintPreview = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the currently exposed Palettes for add Concepts of this View.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IRecognizableElement> GetExposedConceptsPalettes() { return UnclusteredConceptsPalette.IntoEnumerable().Concat(this.TargetComposition.CompositeContentDomain.ConceptDefClusters); }
        private static FormalPresentationElement UnclusteredConceptsPalette = new FormalPresentationElement(String.Empty /* To not generate visible Header */, ConceptDefinition.CONDEF_CLUSTER_EMPTY_ID);

        /// <summary>
        /// Returns the currently exposed Palettes for add Relationships of this View.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IRecognizableElement> GetExposedRelationshipsPalettes() { return UnclusteredRelationshipsPalette.IntoEnumerable().Concat(this.TargetComposition.CompositeContentDomain.RelationshipDefClusters); }
        private static FormalPresentationElement UnclusteredRelationshipsPalette = new FormalPresentationElement(String.Empty /* To not generate visible Header */, RelationshipDefinition.RELDEF_CLUSTER_EMPTY_ID);

        /// <summary>
        /// Returns the currently exposed Palettes for add Markers of this View.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IRecognizableElement> GetExposedMarkersPalettes() { return this.TargetComposition.CompositeContentDomain.MarkerClusters; }

        /// <summary>
        /// Returns the currently exposed Palettes for add Complements of this View.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IRecognizableElement> GetExposedComplementsPalettes() { return UniqueComplementsPalette.IntoEnumerable(); }
        private static SimplePresentationElement UniqueComplementsPalette = new SimplePresentationElement("Complements", "Complements");

        /// <summary>
        /// Returns the currently exposed Items of the specified Concept Palette of this View.
        /// </summary>
        public override IEnumerable<IRecognizableElement> GetExposedItemsOfConceptPalette(IRecognizableElement Palette)
        {
            // IMPORTANT: Do not refer here to "this.TargetComposition.IdeaDefinitor.Definitions"
            return this.TargetComposition.CompositeContentDomain.ConceptDefinitions.Where(cdef => cdef.Cluster.TechName == Palette.TechName).ToList();
        }

        /// <summary>
        /// Returns the currently exposed Items of the specified Relationship Palette of this View.
        /// </summary>
        public override IEnumerable<IRecognizableElement> GetExposedItemsOfRelationshipPalette(IRecognizableElement Palette)
        {
            // IMPORTANT: Do not refer here to "this.TargetComposition.IdeaDefinitor.Definitions"
            return this.TargetComposition.CompositeContentDomain.RelationshipDefinitions.Where(rdef => rdef.Cluster.TechName == Palette.TechName).ToList();
        }

        /// <summary>
        /// Returns the currently exposed Items of the specified Marker Palette of this View.
        /// </summary>
        public override IEnumerable<IRecognizableElement> GetExposedItemsOfMarkerPalette(IRecognizableElement Palette)
        {
            // NOTE: Here is safe to use Tech-Name to locate the Palette because Marker clusters are not be editable (cannot be renamed)
            return this.TargetComposition.CompositeContentDomain.MarkerDefinitions
                .Where(mrk => mrk.ClusterKey == Palette.TechName).ToList();
        }

        /// <summary>
        /// Returns the currently exposed Items of the specified Complement Palette of this View.
        /// </summary>
        public override IEnumerable<IRecognizableElement> GetExposedItemsOfComplementPalette(IRecognizableElement Palette)
        {
            return Domain.StandardComplementDefinitions;
        }

        /// <summary>
        /// Executes the appropriate action for creating a new item for the specified Palette.
        /// </summary>
        public override void ApplyPaletteItemCreation(IRecognizableElement Palette)
        {
            if (Palette.IsIn(this.TargetComposition.Engine.GetExposedConceptsPalettes()))
            {
                CurrentView.Engine.StartCommandVariation("Create Concept Definition");

                var Result = DomainMaintainer.ConceptDefinitionCreate(CurrentView.Engine.TargetComposition.CompositeContentDomain,
                                                                      CurrentView.Engine.TargetComposition.CompositeContentDomain.ConceptDefinitions);
                if (Result != null)
                {
                    CurrentView.Engine.TargetComposition.CompositeContentDomain.ConceptDefinitions.Add(Result);
                    DomainServices.UpdateDomainDependants(CurrentView.Engine.TargetComposition.CompositeContentDomain);
                    CurrentView.Engine.CompleteCommandVariation();
                }
                else
                    CurrentView.Engine.DiscardCommandVariation();
            }
            else
                if (Palette.IsIn(this.TargetComposition.Engine.GetExposedRelationshipsPalettes()))
                {
                    CurrentView.Engine.StartCommandVariation("Create Relationship Definition");

                    var Result = DomainMaintainer.RelationshipDefinitionCreate(CurrentView.Engine.TargetComposition.CompositeContentDomain,
                                                                               CurrentView.Engine.TargetComposition.CompositeContentDomain.RelationshipDefinitions);
                    if (Result != null)
                    {
                        CurrentView.Engine.TargetComposition.CompositeContentDomain.RelationshipDefinitions.Add(Result);
                        DomainServices.UpdateDomainDependants(CurrentView.Engine.TargetComposition.CompositeContentDomain);
                        CurrentView.Engine.CompleteCommandVariation();
                    }
                    else
                        CurrentView.Engine.DiscardCommandVariation();
                }
                else
                    if (Palette.IsIn(this.TargetComposition.CompositeContentDomain.MarkerClusters))
                    {
                        CurrentView.Engine.StartCommandVariation("Create Marker Definition");

                        var Result = DomainMaintainer.MarkerDefinitionCreate(CurrentView.Engine.TargetComposition.CompositeContentDomain,
                                                                             CurrentView.Engine.TargetComposition.CompositeContentDomain.MarkerDefinitions);
                        if (Result != null)
                        {
                            CurrentView.Engine.TargetComposition.CompositeContentDomain.MarkerDefinitions.Add(Result);
                            DomainServices.UpdateDomainDependants(CurrentView.Engine.TargetComposition.CompositeContentDomain);
                            CurrentView.Engine.CompleteCommandVariation();
                        }
                        else
                            CurrentView.Engine.DiscardCommandVariation();
                    }
        }

        /// <summary>
        /// Executes the appropriate action for when the supplied Item, of the specified Palette, was selected or activated and intended for immediate action.
        /// </summary>
        public override void ApplyPaletteItemSelection(IRecognizableElement Item, IRecognizableElement Palette, bool IsForImmediateAction)
        {
            var IsMarker = Palette.IsIn(this.TargetComposition.CompositeContentDomain.MarkerClusters);
            var IsComplement = (Palette == UniqueComplementsPalette);

            if (!(Item is ConceptDefinition || Item is RelationshipDefinition)
                && !IsMarker && !IsComplement)
                return;

            if (this.RunningMouseCommand != null)
                this.DoCancelOperation(false, null);

            CurrentView.Manipulator.Abort();

            if (IsMarker)
            {
                if (IsForImmediateAction)
                {
                    CurrentView.Engine.StartCommandVariation("Modify Marker Definition");

                    if (DomainMaintainer.MarkerDefinitionEdit(CurrentView.Engine.TargetComposition.CompositeContentDomain,
                                                              CurrentView.Engine.TargetComposition.CompositeContentDomain.MarkerDefinitions,
                                                              (MarkerDefinition)Item))
                    {
                        DomainServices.UpdateDomainDependants(CurrentView.Engine.TargetComposition.CompositeContentDomain);
                        CurrentView.Engine.CompleteCommandVariation();
                    }
                    else
                        CurrentView.Engine.DiscardCommandVariation();

                    //ALT: Assign marker to currently selected representation
                    // MarkerAssignmentCommand.CreateMarker(this, (MarkerDefinition)Item, this.CurrentView, CurrentView.SelectedRepresentations.FirstOrDefault());
                }
                else
                    this.RunningMouseCommand = new MarkerAssignmentCommand(this, (MarkerDefinition)Item);
            }
            else
                if (IsComplement)
                    this.RunningMouseCommand = new ComplementCreationCommand(this, (SimplePresentationElement)Item);
                else
                    if (Item is ConceptDefinition)
                    {
                        if (IsForImmediateAction)
                        {
                            CurrentView.Engine.StartCommandVariation("Modify Concept Definition");

                            if (DomainMaintainer.ConceptDefinitionEdit(CurrentView.Engine.TargetComposition.CompositeContentDomain,
                                                                       CurrentView.Engine.TargetComposition.CompositeContentDomain.ConceptDefinitions,
                                                                       (ConceptDefinition)Item))
                            {
                                DomainServices.UpdateDomainDependants(CurrentView.Engine.TargetComposition.CompositeContentDomain);
                                ProductDirector.ContentTreeControl.Refresh();
                                CurrentView.Engine.CompleteCommandVariation();
                            }
                            else
                                CurrentView.Engine.DiscardCommandVariation();
                        }
                        else
                            this.RunningMouseCommand = new ConceptCreationCommand(this, (ConceptDefinition)Item);
                    }
                    else
                        if (Item is RelationshipDefinition)
                        {
                            if (IsForImmediateAction)
                            {
                                CurrentView.Engine.StartCommandVariation("Modify Relationship Definition");

                                if (DomainMaintainer.RelationshipDefinitionEdit(CurrentView.Engine.TargetComposition.CompositeContentDomain,
                                                                                CurrentView.Engine.TargetComposition.CompositeContentDomain.RelationshipDefinitions,
                                                                                (RelationshipDefinition)Item))
                                {
                                    DomainServices.UpdateDomainDependants(CurrentView.Engine.TargetComposition.CompositeContentDomain);
                                    ProductDirector.ContentTreeControl.Refresh();
                                    CurrentView.Engine.CompleteCommandVariation();
                                }
                                else
                                    CurrentView.Engine.DiscardCommandVariation();
                            }
                            else
                                this.RunningMouseCommand = new RelationshipCreationCommand(this, (RelationshipDefinition)Item);
                        }
                        else
                            throw new UsageAnomaly("Unknown palette item type.", Item);

            if (this.RunningMouseCommand != null)
                this.RunningMouseCommand.Execute();
        }

        /// <summary>
        /// Currently executing mouse based command.
        /// </summary>
        public WorkCommandInteractive<MouseEventArgs> RunningMouseCommand { get; set; }

        /// <summary>
        /// Tells to the Document Engine to cancel the currently running operation, if any.
        /// </summary>
        public override void DoCancelOperation()
        {
            this.DoCancelOperation(false, null, false);

            if (IsClipboardTransferring
                && ClipboardTransferSourceView.OwnerCompositeContainer.OwnerComposition == this.TargetComposition)
                UnmarkSelectedObjectsForCut();
        }

        /// <summary>
        /// Tells to the Document Engine to cancel the currently running operation, if any.
        /// Plus, indications of normal termination, mouse-event parameters and restart.
        /// </summary>
        public void DoCancelOperation(bool IsNormalTermination, MouseEventArgs MouseEventParams = null, bool Restart = true)
        {
            if (this.RunningMouseCommand != null)
            {
                // Use the Command variable because Terminate can set this.RunningMouseCommand to null.
                var LocalCommand = this.RunningMouseCommand;

                LocalCommand.Terminate(IsNormalTermination, MouseEventParams);
                
                if (Restart && LocalCommand.RestartAfterTermination)
                {
                    LocalCommand.Initialize();
                    LocalCommand.Execute();
                }
                else
                    this.RunningMouseCommand = null;
            }
        }

        /// <summary>
        /// Tells to the Document Engine to delete the currently selected object(s), if any.
        /// </summary>
        public override void DoDeleteSelection()
        {
            if (this.RunningMouseCommand != null || this.CurrentView == null)
                return;

            this.DeleteObjects(this.CurrentView.SelectedObjects);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the Visual Object, having a non-null brush, under the supplied mouse-event-args position
        /// with an optional detection-radius, if any (inerts are ignored).
        /// </summary>
        public VisualObject GetPointedVisualObject(Point Position, double DetectionRadius = Display.POINT_NEAR_TOLERANCE)
        {
            if (CurrentView == null)
                return null;

            // Old: var HitResult = VisualTreeHelper.HitTest(CurrentView.PresenterControl, Position);   // Faster, but noticeable imprecise
            var HitResult = Display.HitTestNear(CurrentView.PresenterControl, Position,
                                                CurrentView.BackgroundSheet.Graphic, DetectionRadius);   // More precise, but a bit slower
            if (HitResult == null)
                return null;

            /*T
            Console.WriteLine("Now={0}, Aiming to: Type=[{1}], HC=[{2}], Str=[{3}].", DateTime.Now,
                              HitResult.VisualHit == null ? "<NULL>" : HitResult.VisualHit.GetType().ToString(),
                              HitResult.VisualHit == null ? "<NULL>" : HitResult.VisualHit.GetHashCode().ToString(),
                              HitResult.VisualHit.ToStringAlways()); */

            var PointedVisual = HitResult.VisualHit as DrawingVisual;
            if (PointedVisual == null)
                return null;

            var Result = CurrentView.Presenter.GetVisualObjectFor(PointedVisual);
            if (Result is VisualInert)
                return null;

            return Result;
        }

        /// <summary>
        /// Returns the Idea Visual Representator under the supplied mouse-event-args position, if any.
        /// Also requires indication of ensuring that the position is effectively inside the main-symbol,
        /// else the position could be pointing to an indication just very near of the symbol but not directly over it.
        /// </summary>
        public VisualRepresentation GetPointedRepresentation(Point Position, bool EnsurePositionInsideMainSymbol)
        {
            var VisObject = GetPointedVisualObject(Position);

            if (VisObject is VisualElement)
            {
                var Result = ((VisualElement)VisObject).OwnerRepresentation;

                // VERY IMPORTANT: Ensure object is pointed directly inside the symbol
                if (EnsurePositionInsideMainSymbol &&
                    !this.CurrentView.Presenter.ContainsObjectWithPoint(Result.MainSymbol.Graphic,
                                                                        Position))
                    return null;

                return Result;
            }

            return null;
        }

        /// <summary>
        /// Returns the Idea under the supplied mouse-event-args position, if any.
        /// </summary>
        public Idea GetPointedIdea(Point Position)
        {
            var Representator = GetPointedRepresentation(Position, false);

            if (Representator == null)
                return null;

            return Representator.RepresentedIdea;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void ShowView(View TargetView)
        {
            if (this.Visualizer.ActiveView == TargetView)
                return;

            this.StartCommandVariation("Show View");

            TargetView.Initialize();
            this.Visualizer.PutView(TargetView);
            TargetView.ShowAll();
            TargetView.Presenter.PostCall(pres => pres.OwnerView.IsEditingActive = true);

            this.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void ObserveView(View TargetView)
        {
            if (TargetView == this.CurrentView)
                return;

            if (TargetView.PresenterControl == null)
                throw new UsageAnomaly("Cannot watch mouse events on view without presenter", TargetView);

            if (this.CurrentView != null)
                UnObserveView();

            TargetView.PresenterControl.MouseLeftButtonDown += PresenterControl_MouseLeftButtonDown;
            TargetView.PresenterControl.MouseLeftButtonUp += PresenterControl_MouseLeftButtonUp;
            TargetView.PresenterControl.MouseRightButtonDown += PresenterControl_MouseRightButtonDown;
            TargetView.PresenterControl.MouseRightButtonUp += PresenterControl_MouseRightButtonUp;
            TargetView.PresenterControl.MouseMove += PresenterControl_MouseMove;
            TargetView.PresenterControl.MouseEnter += PresenterControl_MouseEnter;
            TargetView.PresenterControl.MouseLeave += PresenterControl_MouseLeave;

            TargetView.MouseWheel += PresenterControl_MouseWheel;

            this.CurrentView = TargetView;

            ProductDirector.EditorInterrelationsControl.SetTarget(null);
        }

        public bool ReactToKeyDown(Key PressedKey)
        {
            if (this.CurrentView == null || !this.CurrentView.SelectedObjects.Any()
                || !PressedKey.IsOneOf(Key.Up, Key.Down, Key.Left, Key.Right))
                return false;

            this.CurrentView.Engine.StartCommandVariation("Move by keyboard");

            var DeltaX = 0.0;
            var DeltaY = 0.0;

            if (PressedKey == Key.Right) DeltaX = 1.0;
            if (PressedKey == Key.Left) DeltaX = -1.0;
            if (PressedKey == Key.Down) DeltaY = 1.0;
            if (PressedKey == Key.Up) DeltaY = -1.0;

            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                DeltaX *= this.CurrentView.GridSize;
                DeltaY *= this.CurrentView.GridSize;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                DeltaX *= 4.0;
                DeltaY *= 4.0;
            }

            foreach (var Selection in this.CurrentView.SelectedObjects)
                Selection.MoveTo(Selection.BaseCenter.X + DeltaX, Selection.BaseCenter.Y + DeltaY, true);

            this.CurrentView.Engine.CompleteCommandVariation();

            // e.Handled = true;
            return true;
        }

        public void UnObserveView()
        {
            if (this.CurrentView == null)
                return;

            if (this.CurrentView.PresenterControl == null)
                throw new UsageAnomaly("Cannot watch mouse events on view without presenter", CurrentView);

            this.CurrentView.PresenterControl.MouseLeftButtonDown -= PresenterControl_MouseLeftButtonDown;
            this.CurrentView.PresenterControl.MouseLeftButtonUp -= PresenterControl_MouseLeftButtonUp;
            this.CurrentView.PresenterControl.MouseRightButtonDown -= PresenterControl_MouseRightButtonDown;
            this.CurrentView.PresenterControl.MouseRightButtonUp -= PresenterControl_MouseRightButtonUp;
            this.CurrentView.PresenterControl.MouseMove -= PresenterControl_MouseMove;
            this.CurrentView.PresenterControl.MouseEnter -= PresenterControl_MouseEnter;
            this.CurrentView.PresenterControl.MouseLeave -= PresenterControl_MouseLeave;
            this.CurrentView.MouseWheel -= PresenterControl_MouseWheel;

            this.CurrentView = null;
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        #region Mouse-Related-Events

        public static bool IsSelectingByRectangle { get; set; }

        public static Point CurrentMousePosition { get; private set; }
        private static Point PreviousMousePosition = Display.NULL_POINT;

        private static VisualObject PointedObject = null;

        void PresenterControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMousePosition = e.GetPosition(CurrentView.PresenterControl);

            if (this.CurrentView.InPlaceEditingTarget != null)
                this.CurrentView.StopEditInplace();

            var Representation = GetPointedRepresentation(CurrentMousePosition, false);

            // PENDING...
            if (RunningMouseCommand != null)
            {
                var ConCreCommand = RunningMouseCommand as ConceptCreationCommand;
                if (ConCreCommand != null)
                    ConCreCommand.MouseButtonWasPressed = true;

                var RelCreCommand = RunningMouseCommand as RelationshipCreationCommand;
                if (RelCreCommand != null && !RelCreCommand.IsConnecting)
                    PresenterControl_MouseLeftButtonUp(this, e);

                PreviousMousePosition = CurrentMousePosition;
                return;
            }

            // When over EMPTY-SPACE

            // Begin [Rectangle-Select]
            if (Representation == null)
                IsSelectingByRectangle = true;

            PreviousMousePosition = CurrentMousePosition;
        }

        void PresenterControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CurrentMousePosition = e.GetPosition(this.CurrentView.PresenterControl);

            if (this.RunningMouseCommand == null)
            {
                if (this.CurrentView.Manipulator.IsManipulating)
                    this.CurrentView.Manipulator.Finish();
            }
            else
            {
                if (!this.RunningMouseCommand.Continue(e))
                    this.DoCancelOperation(true);
                    // this.RunningMouseCommand = null;

                PreviousMousePosition = CurrentMousePosition;
                return;
            }

            // PENDING...
            var Representation = GetPointedRepresentation(CurrentMousePosition, false);

            // When over EMPTY-SPACE
            if (Representation == null)
            {
                // When [Dragging]
                    // then drop

                // When [Rectangle-Select]
                if (IsSelectingByRectangle)
                {
                    // PENDING: Select whatever is under the selection-rectangle

                    IsSelectingByRectangle = false;
                }

                // Clear current selection (if not ctrl or shift are pressed)
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)
                    && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    this.CurrentView.UnselectAllObjects();
            }

            PreviousMousePosition = CurrentMousePosition;
        }

        void PresenterControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.CurrentView.PreviousPanPosition = default(Point);
            CurrentMousePosition = e.GetPosition(CurrentView.PresenterControl);

            if (this.CurrentView.InPlaceEditingTarget != null)
                this.CurrentView.StopEditInplace();

            if (this.RunningMouseCommand != null)
                return;

            PreviousMousePosition = CurrentMousePosition;
        }

        void PresenterControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CurrentMousePosition = e.GetPosition(CurrentView.PresenterControl);

            if (this.RunningMouseCommand != null)
            {
                this.DoCancelOperation(false, e, !(this.RunningMouseCommand is RelationshipCreationCommand
                                                   && ((RelationshipCreationCommand)this.RunningMouseCommand).RelationshipDef.IsSimple));

                if (this.RunningMouseCommand != null)
                    this.RunningMouseCommand.RestartAfterTermination = false;

                PreviousMousePosition = CurrentMousePosition;
                return;
            }

            if (CurrentView == null)
                return;

            var TargetedObject = this.GetPointedVisualObject(CurrentMousePosition);
            ShowContextMenu(CurrentView.Presenter,
                                  (TargetedObject != null && TargetedObject.GetType().IsOneOf(typeof(VisualComplement), typeof(VisualElement))
                                   ? (UniqueElement)TargetedObject
                                   : CurrentView),
                                  CurrentView);

            PreviousMousePosition = CurrentMousePosition;
        }

        public const int SCALING_INTERVAL = 10;

        void PresenterControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // When moving and Key-Control: [Zoom]
            if (Keyboard.IsKeyDown(Key.LeftCtrl)  || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                this.CurrentView.PageDisplayScale += (SCALING_INTERVAL * (e.Delta > 0 ? 1 : -1));
                return;
            }

            // When moving and Key-Shift: [Pan-Horizontal]
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                this.Visualizer.ScrollSegment(Orientation.Horizontal, (e.Delta * -1.0));
                return;
            }

            // When moving: [Pan-Vertical]
            this.Visualizer.ScrollSegment(Orientation.Vertical, (e.Delta * -1.0));
        }

        public void PresenterControl_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentMousePosition = e.GetPosition(CurrentView.PresenterControl);
            //T Console.WriteLine("At: " + e.GetPosition(null));

            var VisObject = GetPointedVisualObject(CurrentMousePosition);

            if ((Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton != MouseButtonState.Pressed)
                && this.RunningMouseCommand == this.PanCommand && this.PanCommand != null)
                this.DoCancelOperation(true, null);

            if (this.RunningMouseCommand == null)
            {
                var Manipulator = CurrentView.Manipulator;

                if (this.MouseLeftButtonDownOutsideControl != null && this.MouseLeftButtonDownOutsideControl.Value)
                {
                    if (Manipulator.WorkingAdorner != null)
                        Manipulator.WorkingAdorner.CancelledByMouseLeftButtonDownOutsideControl = true;

                    Manipulator.IsManipulating = false;
                    Manipulator.UnpointObject();
                    Manipulator.Finish();
                    this.MouseLeftButtonDownOutsideControl = null;
                    return;
                }
                else
                    if (Manipulator.IsManipulating)
                        Manipulator.Continue();
                    else
                        if (!Manipulator.PointObject(VisObject))
                        {
                            ProductDirector.ShowPointingTo();
                            ProductDirector.ShowAssistance();
                            // Cancelled: Too much to explain... ProductDirector.ShowAssistance("To Select: Move pressing [Mouse-Button]. To Pan: Move pressing [Mouse-Alternate-Button]. To Zoom in/out: Roll [Mouse-Wheel]+[Ctrl].");
                        }
            }
            else
            {
                if (!this.RunningMouseCommand.Continue(e, false))
                    this.RunningMouseCommand = null;

                PreviousMousePosition = CurrentMousePosition;
                return;
            }

            if (!VisObject.IsEqual(PointedObject))
            {
                PointedObject = VisObject;
                /*T Console.WriteLine("Pointing Representator of Idea: [{0}] at [{1}]. Element is at [{2}].",
                                  (Element == null ? "<NONE>" : Element.OwnerRepresentation.RepresentedIdea.Name),
                                  CurrentMousePosition,
                                  (Element == null ? Rect.Empty : Element.Graphic.ContentBounds )); */
            }

            // When [Dragging] and Selecting-by-Rectangle
            if (IsSelectingByRectangle && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (this.SelectByRectangleCommand == null)
                    this.SelectByRectangleCommand = new WorkCommandInteractive<MouseEventArgs>("SelectByRectangle",
                                                            (mevargs, definitive) =>
                                                            {
                                                                var MousePos = mevargs.GetPosition(this.CurrentView.Presenter);

                                                                if (mevargs.LeftButton == MouseButtonState.Pressed
                                                                    && !this.CurrentView.Manipulator.IsManipulating)
                                                                {
                                                                    //T Console.WriteLine("FinishedSelByRect ONE. Breaked.");
                                                                    this.CurrentView.SelectByRectangle(MousePos);
                                                                    return true;
                                                                }

                                                                /*T Console.WriteLine("FinishedSelByRect ONE. IsMan={0}, MALB={1}",
                                                                                  this.CurrentView.Manipulator.IsManipulating, mevargs.LeftButton); */
                                                                if (!this.CurrentView.Manipulator.IsManipulating)
                                                                    this.CurrentView.FinishSelectByRectangle(true, MousePos);

                                                                return false;
                                                            },
                                                            (normal, mevargs) =>
                                                            {
                                                                //T Console.WriteLine("FinishedSelByRect TWO. ");
                                                                this.CurrentView.FinishSelectByRectangle(false);
                                                            });

                this.RunningMouseCommand = this.SelectByRectangleCommand;  // To avoid Pointing of elements
            }
            else
                if (this.RunningMouseCommand == this.SelectByRectangleCommand && this.SelectByRectangleCommand != null)
                    this.DoCancelOperation(true, null);

            // When [Dragging]
            // then Update cursor depending on pointed target

            // When ([Dragging] and [Going out of view scope]) or [Mouse Right-Button down]
            // then [Pan]
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                if (this.RunningMouseCommand == null)
                {
                    if (this.PanCommand == null)
                        this.PanCommand = new WorkCommandInteractive<MouseEventArgs>("Pan",
                                                    (mevargs, definitive) => { this.CurrentView.Pan(); return true; },
                                                    (normal, mevargs) => { this.CurrentView.PreviousPanPosition = default(Point); });

                    this.RunningMouseCommand = this.PanCommand;  // To avoid Pointing of elements
                }
            }
            else
                if (this.RunningMouseCommand == this.PanCommand && this.PanCommand != null)
                    this.DoCancelOperation(true, null);

            PreviousMousePosition = CurrentMousePosition;
        }

        private WorkCommandInteractive<MouseEventArgs> SelectByRectangleCommand = null;

        private WorkCommandInteractive<MouseEventArgs> PanCommand = null;

        bool? MouseLeftButtonDownOutsideControl = null;
        void PresenterControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.MouseLeftButtonDownOutsideControl != null)
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    this.MouseLeftButtonDownOutsideControl = true;
                else
                    this.MouseLeftButtonDownOutsideControl = null;
        }

        void PresenterControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.MouseLeftButtonDownOutsideControl = false;
            else
                this.MouseLeftButtonDownOutsideControl = null;
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}