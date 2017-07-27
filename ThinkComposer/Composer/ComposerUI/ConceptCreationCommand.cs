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
// File   : ConceptCreationCommand.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ConceptCreationCommand (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Creates a Concept, with its visual representation, for a given composite Idea.
    /// </summary>
    public class ConceptCreationCommand : WorkCommandInteractive<MouseEventArgs>
    {
        public CompositionEngine ContextEngine { get; protected set; }
        public ConceptDefinition ConceptDef { get; protected set; }

        public Concept NewConcept { get; protected set; }

        public bool MouseButtonWasPressed { get; set; }

        public ConceptCreationCommand(CompositionEngine TargetEngine, ConceptDefinition ConceptDef)
            : base("Create Concept '" + ConceptDef.Name + "'.")
        {
            this.ContextEngine = TargetEngine;
            this.ConceptDef = ConceptDef;

            this.Initialize();

            this.Assistance = "Click to use a predefined size, or drag to give a custom size.";
        }

        VisualRepresentation TargetRepresentation = null;
        Point TargetLocation = Display.NULL_POINT;

        public override void Initialize()
        {
            base.Initialize();

            var Loader = Display.GetResource<TextBlock>("CursorLoaderArrowIdea");   // Trick for assign custom cursors.
            Display.GetCurrentWindow().Cursor = Loader.Cursor;

            PointingAssistant.Start(this.ContextEngine.CurrentView, this.ConceptDef.GetSampleDrawing(true));
        }

        public override void Execute(MouseEventArgs Parameter = null)
        {
            base.Execute(Parameter);
        }

        public override bool Continue(MouseEventArgs Parameter, bool IsDefinitive = true)
        {
            var Go = base.Continue(Parameter, IsDefinitive);
            if (!Go)
            {
                if (CurrentCreationPreviewOrigin != default(Point))
                {
                    this.ContextEngine.CurrentView.Presenter.ClearTransientVisual(VISKEY_CONCCREA_PREVW);
                    CurrentCreationPreviewOrigin = Display.NULL_POINT;
                }

                return false;
            }

            TargetLocation = Parameter.GetPosition(this.ContextEngine.CurrentView.PresenterControl);
            TargetRepresentation = this.ContextEngine.GetPointedRepresentation(TargetLocation, false);

            // IMPORTANT: Get the snapped position AFTER obtain the pointed representation
            if (this.ContextEngine.CurrentView.SnapToGrid)
                TargetLocation = this.ContextEngine.CurrentView.GetGridSnappedPosition(TargetLocation, true);

            var Width = 0.0;
            var Height = 0.0;

            if (IsDefinitive)
            {
                if (CurrentCreationPreviewOrigin != Display.NULL_POINT)
                {
                    Width = Math.Abs(TargetLocation.X - CurrentCreationPreviewOrigin.X).EnforceMinimum(1.0);
                    Height = Math.Abs(TargetLocation.Y - CurrentCreationPreviewOrigin.Y).EnforceMinimum(1.0);

                    var DeltaX = TargetLocation.X - CurrentCreationPreviewOrigin.X;
                    var DeltaY = TargetLocation.Y - CurrentCreationPreviewOrigin.Y;
                    TargetLocation = new Point((TargetLocation.X - DeltaX) + ((this.ConceptDef.DefaultSymbolFormat.HasFixedWidth
                                                                               ? this.ConceptDef.DefaultSymbolFormat.InitialWidth
                                                                               : DeltaX) / 2.0),
                                               (TargetLocation.Y - DeltaY) + ((this.ConceptDef.DefaultSymbolFormat.HasFixedHeight
                                                                               ? this.ConceptDef.DefaultSymbolFormat.InitialHeight
                                                                               : DeltaY) / 2.0));

                    this.ContextEngine.CurrentView.Presenter.ClearTransientVisual(VISKEY_CONCCREA_PREVW);
                    CurrentCreationPreviewOrigin = Display.NULL_POINT;

                    // If too small, force to create as with click, with predefined size
                    if ((this.ContextEngine.CurrentView.SnapToGrid
                         && (Width < this.ContextEngine.CurrentView.GridSize
                             || Height < this.ContextEngine.CurrentView.GridSize))
                        || (Width < VisualSymbolFormat.SYMBOL_MIN_INI_SIZE
                            || Height < VisualSymbolFormat.SYMBOL_MIN_INI_SIZE))
                    {
                        Width = 0.0;
                        Height = 0.0;
                    }
                }

                OperationResult<Concept> CreationResult = new OperationResult<Concept>();

                if (TargetRepresentation != null)
                    CreationResult = CreateAutoConceptFromRepresentation(TargetRepresentation, this.ConceptDef,
                                                                         !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));
                else
                    CreationResult = CreateConcept(this.ContextEngine.CurrentView.VisualizedCompositeIdea, this.ConceptDef,
                                                   this.ContextEngine.CurrentView, TargetLocation, Width, Height);

                if (CreationResult.WasSuccessful)
                    this.ContextEngine.CurrentView.Presenter
                        .PostCall(pres =>   // Post-called in order to be executed after the symbol heading-content area is established (on first draw).
                                  {
                                      var Symbol = CreationResult.Result.VisualRepresentators[0].MainSymbol;
                                      pres.OwnerView.Manipulator.ApplySelection(Symbol);

                                      if (Keyboard.IsKeyDown(Key.LeftCtrl))
                                          pres.OwnerView.EditPropertiesOfVisualRepresentation(Symbol.OwnerRepresentation);
                                      else
                                          pres.OwnerView.EditInPlace(Symbol);
                                  });
                else
                {
                    Console.WriteLine("Cannot create Concept: " + CreationResult.Message.AbsentDefault("?"));
                    this.Terminate();
                    return false;
                }

            }
            else
                if (TargetRepresentation == null && Mouse.LeftButton == MouseButtonState.Pressed
                    && this.MouseButtonWasPressed  // This forces mouse-down before start creation-by-drag
                    && !(this.ConceptDef.DefaultSymbolFormat.HasFixedWidth
                         && this.ConceptDef.DefaultSymbolFormat.HasFixedHeight))
                    if (CurrentCreationPreviewOrigin == Display.NULL_POINT)
                        CurrentCreationPreviewOrigin = TargetLocation;
                    else
                        if (CurrentCreationPreviewOrigin.DetermineDistanceTo(TargetLocation) > 1.0)
                        {
                            this.ContextEngine.CurrentView.Presenter.ClearTransientVisual(VISKEY_CONCCREA_PREVW);

                            Width = (this.ConceptDef.DefaultSymbolFormat.HasFixedWidth
                                     ? this.ConceptDef.DefaultSymbolFormat.InitialWidth
                                     : Math.Abs(TargetLocation.X - CurrentCreationPreviewOrigin.X).EnforceMinimum(1.0));

                            Height = (this.ConceptDef.DefaultSymbolFormat.HasFixedHeight
                                      ? this.ConceptDef.DefaultSymbolFormat.InitialHeight
                                      : Math.Abs(TargetLocation.Y - CurrentCreationPreviewOrigin.Y).EnforceMinimum(1.0));

                            var Sample = MasterDrawer.CreateDrawingSample(this.ConceptDef, false, false, Width, Height);
                            var Preview = Sample.RenderToDrawingVisual();
                            Preview.Transform = Display.CreateTransform(Math.Min(CurrentCreationPreviewOrigin.X, TargetLocation.X),
                                                                        Math.Min(CurrentCreationPreviewOrigin.Y, TargetLocation.Y));

                            /*T var CreationRectangle = new Rect(CurrentCreationPreviewerOrigin, TargetLocation);
                            var Drawer = new DrawingVisual();
                            using (DrawingContext Context = Drawer.RenderOpen())
                            {
                                Context.DrawRectangle(Brushes.Transparent, CreationPreviewerPen, CreationRectangle);
                            }
                            this.ContextEngine.CurrentView.Presenter.PutTransientVisual(VISKEY_CONCCREA_PREVW, Drawer); */

                            this.ContextEngine.CurrentView.Presenter.PutTransientVisual(VISKEY_CONCCREA_PREVW, Preview);

                            var BorderTresholdRect = new Rect(TargetLocation.X - View.BORDER_TRESHOLD, TargetLocation.Y - View.BORDER_TRESHOLD,
                                                              View.BORDER_TRESHOLD * 2.0, View.BORDER_TRESHOLD * 2.0);
                            this.ContextEngine.CurrentView.Presenter.BringIntoView(BorderTresholdRect);
                            this.ContextEngine.CurrentView.Presenter.UpdateLayout();

                            if (LastViewportOffset.X != this.ContextEngine.CurrentView.HostingScrollViewer.HorizontalOffset
                                || LastViewportOffset.Y != this.ContextEngine.CurrentView.HostingScrollViewer.VerticalOffset)
                                System.Threading.Thread.Sleep(View.INTERACTIVE_SLOW_DOWN);  // Slow down the scroller. Too fast for the user!

                            LastViewportOffset = new Point(this.ContextEngine.CurrentView.HostingScrollViewer.HorizontalOffset,
                                                           this.ContextEngine.CurrentView.HostingScrollViewer.VerticalOffset);
                        }
            
            /* Stop the command...
            this.Terminate(true, Parameter);
            return false;
            */

            // Continue the command...
            return true;
        }

        private static Point LastViewportOffset;

        public const string VISKEY_CONCCREA_PREVW = "ConceptCreationPreviewer";

        private static Point CurrentCreationPreviewOrigin = Display.NULL_POINT;

        private static Pen CreationPreviewerPen = new Pen(Brushes.Red, 2.0);

        public override void Terminate(bool IsNormalTermination = false, MouseEventArgs Parameter = null)
        {
            base.Terminate(IsNormalTermination, Parameter);
            PointingAssistant.Finish();

            Display.GetCurrentWindow().Cursor = Cursors.Arrow;

            ProductDirector.ConceptPaletteControl.ClearSelection();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static OperationResult<Concept> CreateConcept(Idea DestinationComposite, ConceptDefinition Definitor, View TargetView, Point Position,
                                                             double Width = 0, double Height = 0, bool ShowDetailsPoster = false, bool SendToFront = true)
        {
            General.ContractRequiresNotNull(DestinationComposite, Definitor, TargetView);

            var MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.IdeasCreationQuotas);

            if (!ProductDirector.ValidateEditionLimit(DestinationComposite.OwnerComposition.DeclaredIdeas.Count() + 1, MaxQuota, "create", "Ideas (Concepts + Relationships)"))
                return OperationResult.Failure<Concept>("This product edition cannot create more than " + MaxQuota + " Ideas.");

            if (DestinationComposite.IdeaDefinitor.CompositeContentDomain == null)
                return OperationResult.Failure<Concept>("Destination Container doest not accept Composite-Content.", DestinationComposite);

            if (DestinationComposite.CompositeContentDomain.GlobalId != Definitor.OwnerDomain.GlobalId)
                return OperationResult.Failure<Concept>("Cannot create Concept of the '" + Definitor.OwnerDomain.Name + "' Domain, " +
                                                 "because the Destination Container only accepts content of the '" + DestinationComposite.CompositeContentDomain.Name + "' Domain.");

            string NewName = Definitor.Name;

            var AppendNumToName = AppExec.GetConfiguration<bool>("IdeaEditing", "Concept.OnCreationAppendDefNameAndNumber", true);
            if (AppendNumToName)
                NewName = NewName + " (Idea " + (DestinationComposite.CompositeIdeas.Count + 1).ToString() + ")";

            DestinationComposite.EditEngine.StartCommandVariation("Create Concept");

            var NewConcept = new Concept(DestinationComposite.OwnerComposition, Definitor, NewName, NewName.TextToIdentifier());

            if (Definitor.IsVersionable)
                NewConcept.Version = new VersionCard();

            NewConcept.AddToComposite(DestinationComposite);

            if (Position != Display.NULL_POINT)
            {
                var Representation = CreateConceptVisualRepresentation(NewConcept, TargetView, Position, false, false, Width, Height);

                /* Cancelled... 
                if (Definitor.DefaultSymbolFormat.UsePictogramAsSymbol)
                 
                   EITHER... Edit for changing the Pictogram (unnecessary since it is editable in the Properties widget)
                    TargetView.Manipulator.EditProperties(Representation);

                    OR... Select image from disk
                    var Selection = Display.DialogGetImageFromFile();
                    if (Selection != null)
                        NewConcept.Pictogram = Selection; */
            }

            NewConcept.MainSymbol.AreDetailsShown = ShowDetailsPoster;

            if (SendToFront)
                 TargetView.SendUpwards(NewConcept.MainSymbol, true);

            DestinationComposite.UpdateVersion();
            DestinationComposite.EditEngine.CompleteCommandVariation();

            return OperationResult.Success(NewConcept);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a Concept Visual Representation of the target Concept, for the specified target-view, center-position,
        /// as-shortcut indication and exclude-attached-complements indication.
        /// </summary>
        public static ConceptVisualRepresentation CreateConceptVisualRepresentation(Concept Target, View TargetView, Point CenterPosition,
                                                                                    bool AsShortcut = false, bool ExcludeAttachedComplements = false,
                                                                                    double Width = 0, double Height = 0)
        {
            // Cancelled?: SHOW A LITTLE CIRCLE FOR SIMPLE MAIN-SYMBOL HIDING RELATIONSHIPS.

            var Representator = new ConceptVisualRepresentation(Target, TargetView);
            Representator.IsShortcut = AsShortcut;

            if (Width <= 0 || Target.ConceptDefinitor.Value.DefaultSymbolFormat.HasFixedWidth)
                Width = Target.ConceptDefinitor.Value.DefaultSymbolFormat.InitialWidth
                            .SubstituteFor(0, ProductDirector.DefaultConceptBodySymbolSize.Width);

            if (Height <= 0 || Target.ConceptDefinitor.Value.DefaultSymbolFormat.HasFixedHeight)
                Height = Target.ConceptDefinitor.Value.DefaultSymbolFormat.InitialHeight
                            .SubstituteFor(0, ProductDirector.DefaultConceptBodySymbolSize.Height);

            if (TargetView.SnapToGrid)
            {
                var SnappedArea = TargetView.GetGridSnappedArea(CenterPosition, Width, Height);

                if (!Target.ConceptDefinitor.Value.DefaultSymbolFormat.HasFixedWidth)
                    Width = SnappedArea.Width.EnforceMinimum(TargetView.GridSize);

                if (!Target.ConceptDefinitor.Value.DefaultSymbolFormat.HasFixedHeight)
                    Height = SnappedArea.Height.EnforceMinimum(TargetView.GridSize);

                CenterPosition = new Point(SnappedArea.X + SnappedArea.Width / 2.0, SnappedArea.Y + SnappedArea.Height / 2.0);
            }
            else
            {
                Width = Width.EnforceMinimum(VisualSymbolFormat.SYMBOL_MIN_INI_SIZE);
                Height = Height.EnforceMinimum(VisualSymbolFormat.SYMBOL_MIN_INI_SIZE);
            }

            var Body = new VisualShape(Representator, EVisualRepresentationPart.ConceptBodySymbol, CenterPosition,
                                       Width, Height);

            Representator.AddVisualPart(Body);
            Representator.Render();


            // Append possible Group Region/Line Complements
            if (!ExcludeAttachedComplements)
            {
                if (Target.IdeaDefinitor.HasGroupRegion)
                {
                    var Params = VisualComplement.GetGroupRegionInitialParams(Body);
                    ComplementCreationCommand.CreateComplement(TargetView.OwnerCompositeContainer, Domain.ComplementDefGroupRegion, TargetView, Body, Params.Item1, Params.Item2);
                }

                // Append possible Group Line Complement
                if (Target.IdeaDefinitor.HasGroupLine)
                    ComplementCreationCommand.CreateComplement(TargetView.OwnerCompositeContainer, Domain.ComplementDefGroupLine, TargetView, Body, VisualComplement.GetGroupLineInitialPosition(Body));
            }

            return Representator;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a copy of the supplied original Concept and its Visual Representation, for the specified target-view and position.
        /// </summary>
        public static OperationResult<ConceptVisualRepresentation> CreateConceptAndRepresentationCopy(ConceptVisualRepresentation OriginalConceptRep,
                                                                                                      View TargetView, Point Position)
        {
            var NewConcept = OriginalConceptRep.RepresentedConcept.GenerateIndependentConceptDuplicate();

            var NewRepresentator = CreateConceptVisualRepresentation(NewConcept, TargetView, Position, false, true);

            foreach (var Complement in OriginalConceptRep.MainSymbol.AttachedComplements)
            {
                var NewComplementTarget = (Complement.Target.IsGlobal
                                           ? Ownership.Create<View, VisualSymbol>(TargetView)
                                           : Ownership.Create<View, VisualSymbol>(NewRepresentator.MainSymbol));
                var NewComplement = Complement.GenerateIndependentDuplicate(NewComplementTarget);
                if (!NewComplement.Target.IsGlobal)
                    NewComplement.BaseCenter = new Point(NewComplement.BaseCenter.X + (NewComplementTarget.OwnerLocal.BaseCenter.X - OriginalConceptRep.MainSymbol.BaseCenter.X),
                                                         NewComplement.BaseCenter.Y + (NewComplementTarget.OwnerLocal.BaseCenter.Y - OriginalConceptRep.MainSymbol.BaseCenter.Y));

                NewRepresentator.MainSymbol.AddComplement(NewComplement);
                TargetView.PutComplement(NewComplement);
            }

            return OperationResult.Success(NewRepresentator);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static OperationResult<Concept> CreateAutoConceptFromRepresentation(VisualRepresentation TargetRepresentation,
                                                                                   ConceptDefinition NewConceptDef = null, bool FromOriginToTarget = true,
                                                                                   bool Forced = false)
        {
            /*-? if (TargetRepresentation.DisplayingView.InPlaceEditingTarget != null)
                TargetRepresentation.DisplayingView.StopEditInplace(); */

            if (TargetRepresentation == null
                || (!Forced && !(TargetRepresentation.RepresentedIdea.IdeaDefinitor.CanAutomaticallyCreateRelatedConcepts)))
                return OperationResult.Failure<Concept>("The pointed Concept is not defined to create automatic dependant Concepts.");

            if (TargetRepresentation is ConceptVisualRepresentation)
                return CreateAutoConceptFromConcept(TargetRepresentation as ConceptVisualRepresentation, NewConceptDef, FromOriginToTarget, Forced);

            return CreateAutoConceptFromRelationship(TargetRepresentation as RelationshipVisualRepresentation, NewConceptDef, FromOriginToTarget);
        }

        public static OperationResult<Concept> CreateAutoConceptAsSibling(VisualRepresentation TargetRepresentation, ConceptDefinition NewConceptDef = null,
                                                                          bool? FromOriginToTarget = null, bool Forced = false)
        {
            /*-? if (TargetRepresentation.DisplayingView.InPlaceEditingTarget != null)
                TargetRepresentation.DisplayingView.StopEditInplace(); */

            if (TargetRepresentation.MainSymbol.OriginConnections.Count > 0)
            {
                TargetRepresentation = TargetRepresentation.MainSymbol.OriginConnections.First().OriginSymbol.OwnerRepresentation;

                if (TargetRepresentation is RelationshipVisualRepresentation)
                {
                    var Targeter = TargetRepresentation.MainSymbol.OriginConnections.FirstOrDefault();
                    if (Targeter != null)
                        TargetRepresentation = Targeter.OriginSymbol.OwnerRepresentation;
                }

                return CreateAutoConceptFromRepresentation(TargetRepresentation, NewConceptDef,
                                                           (FromOriginToTarget == null
                                                            ? !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                                                            : FromOriginToTarget.Value), Forced);
            }

            return CreateAutoConceptFromRepresentation(TargetRepresentation, NewConceptDef, true, Forced);
        }

        public static OperationResult<Concept> CreateAutoConceptFromConcept(ConceptVisualRepresentation InitialRepresentator,
                                                                            ConceptDefinition NewConceptDef = null,
                                                                            bool FromOriginToTarget = true, bool Forced = false)
        {
            if (NewConceptDef == null)
                NewConceptDef = InitialRepresentator.RepresentedConcept.ConceptDefinitor.Value.AutomaticCreationConceptDef;

            if (NewConceptDef == null && Forced)
                NewConceptDef = InitialRepresentator.RepresentedConcept.ConceptDefinitor.Value;

            var BaseConceptDef = InitialRepresentator.RepresentedConcept.ConceptDefinitor.Value;
            var WorkingRelationshipDef = BaseConceptDef.AutomaticCreationRelationshipDef;
                
            if (WorkingRelationshipDef == null && Forced)
                WorkingRelationshipDef = BaseConceptDef.OwnerDomain.RelationshipDefinitions.
                                                FirstOrDefault(rd => rd.OriginOrParticipantLinkRoleDef.AssociableIdeaDefs.Count < 1
                                                                     && rd.TargetLinkRoleDef.AssociableIdeaDefs.Count < 1);

            if (NewConceptDef == null)
                return OperationResult.Failure<Concept>("Concept Definition (targeted) has no Concept Definition assigned for automatic creation.");

            if (WorkingRelationshipDef == null)
                return OperationResult.Failure<Concept>("Concept Definition (targeted) has no Relationship Definition assigned for automatic creation.");

            var NewConceptWidth = NewConceptDef.DefaultSymbolFormat.InitialWidth
                                        .SubstituteFor(0, ProductDirector.DefaultConceptBodySymbolSize.Width);

            var NewConceptHeight = NewConceptDef.DefaultSymbolFormat.InitialHeight
                                        .SubstituteFor(0, ProductDirector.DefaultConceptBodySymbolSize.Height);

            var NewRelationshipWidth = (WorkingRelationshipDef.IsSimple &&
                                        WorkingRelationshipDef.HideCentralSymbolWhenSimple
                                        ? /* ProductDirector.DefaultRelationshipCentralSymbolSize.Width * 2 */ VisualSymbol.HIDING_SIZE
                                        : WorkingRelationshipDef.DefaultSymbolFormat.InitialWidth
                                            .SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Width));

            var NewRelationshipHeight = (WorkingRelationshipDef.IsSimple &&
                                         WorkingRelationshipDef.HideCentralSymbolWhenSimple
                                         ? /* ProductDirector.DefaultRelationshipCentralSymbolSize.Height * 2  */ VisualSymbol.HIDING_SIZE
                                         : WorkingRelationshipDef.DefaultSymbolFormat.InitialHeight
                                             .SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Height));

            var AutoLocatingMode = DetermineBestAutoPositioningModeFrom(InitialRepresentator,
                                                                        InitialRepresentator.RepresentedConcept.ConceptDefinitor.Value.AutomaticCreationPositioningMode);

            var IsAlternating = InitialRepresentator.RepresentedConcept.ConceptDefinitor.Value.AutomaticCreationPositioningMode
                                        .IsOneOf(EAutoPositioningMode.VerticalAlternated, EAutoPositioningMode.HorizontalAlternated);

            NewConceptDef.EditEngine.StartCommandVariation("Automatic Concept creation from Concept");

            var Creation = CreateAutoConcept(InitialRepresentator, NewConceptDef, AutoLocatingMode, IsAlternating,
                                             InitialRepresentator.RepresentedConcept.ConceptDefinitor.Value.AutomaticCreationPositioningIsRadialized,
                                             NewConceptWidth, NewConceptHeight, NewRelationshipWidth, NewRelationshipHeight, FromOriginToTarget);

            if (Creation.WasSuccessful)
            {
                var ConceptVisRep = Creation.Result.VisualRepresentators.First();
                ConceptVisRep.MainSymbol.IsAutoPositionable = true;

                var RepOrigin = (FromOriginToTarget ? InitialRepresentator : Creation.Result.VisualRepresentators.First());
                var RepTarget = (!FromOriginToTarget ? InitialRepresentator : Creation.Result.VisualRepresentators.First());

                var RelationshipCreation = RelationshipCreationCommand
                                             .CreateRelationship(InitialRepresentator.RepresentedIdea.OwnerContainer,
                                                                 WorkingRelationshipDef,
                                                                 InitialRepresentator.DisplayingView,
                                                                 RepOrigin, RepOrigin.MainSymbol.BaseCenter,
                                                                 RepTarget, RepTarget.MainSymbol.BaseCenter);
                /* POSTPONED
                if (RelationshipCreation.WasSuccessful)
                {
                    var RelationVisRep = RelationshipCreation.Result.VisualRepresentators.First();


                    AdjustSiblingSubtrees(InitialRepresentator.DisplayingView,
                                          ConceptVisRep.MainSymbol.TotalContentArea,
                                          RelationVisRep.MainSymbol.TotalContentArea);
                }
                else */
                if (!RelationshipCreation.WasSuccessful)
                    Creation = OperationResult.Failure<Concept>("Cannot create associating Relationship for automatic Concept creation. Problem: "
                                                                + RelationshipCreation.Message);
            }

            NewConceptDef.EditEngine.CompleteCommandVariation();

            if (!Creation.WasSuccessful)
                NewConceptDef.EditEngine.Undo();

            return Creation;
        }

        public static OperationResult<Concept> CreateAutoConceptFromRelationship(RelationshipVisualRepresentation InitialRepresentator,
                                                                                 ConceptDefinition NewConceptDef = null, bool FromOriginToTarget = true)
        {
            if (NewConceptDef == null)
            {
                // By default try to create a Concept like the last sibling
                var BaseConceptRep = InitialRepresentator.MainSymbol.TargetConnections.Select(conn => conn.TargetSymbol.OwnerRepresentation)
                                        .CastAs<ConceptVisualRepresentation, VisualRepresentation>().LastOrDefault();

                if (BaseConceptRep != null)
                    NewConceptDef = BaseConceptRep.RepresentedConcept.ConceptDefinitor.Value;
                else    // If no Concept siblings found, then try to use the auto-concept defined by the last targeter
                {
                    BaseConceptRep = InitialRepresentator.MainSymbol.OriginConnections.Select(conn => conn.OwnerRepresentation)
                                        .CastAs<ConceptVisualRepresentation, VisualRepresentation>().LastOrDefault();

                    if (BaseConceptRep != null)
                        NewConceptDef = BaseConceptRep.RepresentedConcept.ConceptDefinitor.Value.AutomaticCreationConceptDef;
                }
            }

            if (NewConceptDef == null)
            {
                NewConceptDef = InitialRepresentator.RepresentedRelationship.RelationshipDefinitor.Value
                                   .TargetLinkRoleDef.AssociableIdeaDefs.FirstOrDefault(idef => idef is ConceptDefinition) as ConceptDefinition;

                if (NewConceptDef == null)
                    NewConceptDef = InitialRepresentator.RepresentedRelationship.OwnerContainer.CompositeContentDomain
                                        .ConceptDefinitions.FirstOrDefault();
            }

            var NewConceptWidth = NewConceptDef.DefaultSymbolFormat.InitialWidth
                                        .SubstituteFor(0, ProductDirector.DefaultConceptBodySymbolSize.Width);

            var NewConceptHeight = NewConceptDef.DefaultSymbolFormat.InitialHeight
                                        .SubstituteFor(0, ProductDirector.DefaultConceptBodySymbolSize.Height);

            var AutoLocatingMode = DetermineBestAutoPositioningModeFrom(InitialRepresentator, EAutoPositioningMode.VerticalAlternated);

            NewConceptDef.EditEngine.StartCommandVariation("Automatic Concept creation from Relationship");

            var ConceptCreation = CreateAutoConcept(InitialRepresentator, NewConceptDef, AutoLocatingMode, true, true,
                                                    NewConceptWidth, NewConceptHeight, 0, 0, FromOriginToTarget);

            if (ConceptCreation.WasSuccessful)
            {
                var ConceptVisRep = ConceptCreation.Result.VisualRepresentators.First();
                ConceptVisRep.MainSymbol.IsAutoPositionable = true;

                var RepOrigin = (FromOriginToTarget ? InitialRepresentator : ConceptCreation.Result.VisualRepresentators.First());
                var RepTarget = (!FromOriginToTarget ? InitialRepresentator : ConceptCreation.Result.VisualRepresentators.First());

                var RelationshipCreation = RelationshipCreationCommand
                                             .CreateRelationship(InitialRepresentator.RepresentedIdea.OwnerContainer,
                                                                 InitialRepresentator.RepresentedRelationship.RelationshipDefinitor.Value,
                                                                 InitialRepresentator.DisplayingView,
                                                                 RepOrigin, RepOrigin.MainSymbol.BaseCenter,
                                                                 RepTarget, RepTarget.MainSymbol.BaseCenter);

                /* POSTPONED
                if (RelationshipCreation.WasSuccessful)
                {
                    var RelationVisRep = RelationshipCreation.Result.VisualRepresentators.First();

                    AdjustSiblingSubtrees(InitialRepresentator.DisplayingView,
                                          ConceptVisRep.MainSymbol.TotalContentArea,
                                          RelationVisRep.MainSymbol.TotalContentArea);
                }
                else*/

                if (!RelationshipCreation.WasSuccessful)
                    ConceptCreation = OperationResult.Failure<Concept>("Cannot extend associating Relationship for automatic Concept creation. Problem: "
                                                                + RelationshipCreation.Message);
            }

            NewConceptDef.EditEngine.CompleteCommandVariation();

            if (!ConceptCreation.WasSuccessful)
                NewConceptDef.EditEngine.Undo();

            return ConceptCreation;
        }

        public static OperationResult<Concept> CreateAutoConcept(VisualRepresentation InitialRepresentator,
                                                                 ConceptDefinition NewConceptDef,
                                                                 EAutoPositioningMode AutomaticCreationPositioningMode,
                                                                 bool AutomaticCreationPositioningIsAlternating,
                                                                 bool AutomaticCreationPositioningIsRadialized,
                                                                 double NewConceptWidth, double NewConceptHeight,
                                                                 double NewRelationshipWidth, double NewRelationshipHeight,
                                                                 bool FromOriginToTarget)
        {
            var NewPosition = DetermineNewPosition(AutomaticCreationPositioningMode,
                                                   AutomaticCreationPositioningIsRadialized,
                                                   AutomaticCreationPositioningIsAlternating,
                                                   InitialRepresentator.MainSymbol,
                                                   NewConceptWidth, NewConceptHeight,
                                                   NewRelationshipWidth, NewRelationshipHeight,
                                                   FromOriginToTarget);

            var ConceptCreation = CreateConcept(InitialRepresentator.RepresentedIdea.OwnerContainer,
                                                NewConceptDef,
                                                InitialRepresentator.DisplayingView,
                                                NewPosition);

            return ConceptCreation;
        }

        /* POSTPONED
        public static void AdjustSiblingSubtrees(View DisplayingView, Rect NewConceptArea, Rect NewRelationshipArea)
        {
            // Detect preexistent symbols in the areas to be used
            var Preexistents = DisplayingView.VisualRepresentations
                                .Where(vrep => NewConceptArea.Contains(vrep.MainSymbol.TotalContentArea)
                                               || NewRelationshipArea.Contains(vrep.MainSymbol.TotalContentArea));

            // if there is free space to allocate new object, quit
            if (!Preexistents.Any())
                return;

            // Determine common dominant

            // Determine direction
            //- ? var TargetQuadrant = DetermineTargetQuadrant(NewConceptArea, new Point(NewRelationshipArea.Left + NewRelationshipArea.Width / 2.0,
            //                                                                           NewRelationshipArea.Top + NewRelationshipArea.Height / 2.0));

            // Determine Subtrees to displace, if any

            // Displace

        } */

        /*- ?
        public static DetermineNearestCommonSourceParent(VisualSymbol Current, IEnumerable<VisualSymbol> PotentialSiblings)
        {
            var CurrentHierarchy = Current.GetSourceSymbolsHierarchy();
        } */

        public static EAutoPositioningMode DetermineBestAutoPositioningModeFrom(VisualRepresentation Source, EAutoPositioningMode Preferred)
        {
            var Result = Preferred;
            var FirstConnIsOrigin = true;
            var FirstConnected = Source.MainSymbol.OriginConnections.FirstOrDefault();

            if (FirstConnected == null)
            {
                FirstConnIsOrigin = false;
                FirstConnected = Source.MainSymbol.TargetConnections.FirstOrDefault();
            }

            if (FirstConnected != null)
            {
                var AimedQuadrant = DetermineAimedQuadrant(Source.MainSymbol.TotalArea,
                                                           (FirstConnected.IntermediatePosition == Display.NULL_POINT
                                                            ? (FirstConnIsOrigin ? FirstConnected.OriginEdgePosition : FirstConnected.TargetEdgePosition)
                                                            : FirstConnected.IntermediatePosition));

                if (Preferred == EAutoPositioningMode.HorizontalAlternated)
                {
                    if (AimedQuadrant.IsOneOf(EVecinityQuadrant.Up, EVecinityQuadrant.LeftUp, EVecinityQuadrant.RightUp))
                        Result = EAutoPositioningMode.ToBottom;
                    else
                        if (AimedQuadrant.IsOneOf(EVecinityQuadrant.Down, EVecinityQuadrant.LeftDown, EVecinityQuadrant.RightDown))
                            Result = EAutoPositioningMode.ToUp;
                }
                else
                    if (Preferred == EAutoPositioningMode.VerticalAlternated)
                    {
                        if (AimedQuadrant.IsOneOf(EVecinityQuadrant.Left, EVecinityQuadrant.LeftUp, EVecinityQuadrant.LeftDown))
                            Result = EAutoPositioningMode.ToRight;
                        else
                            if (AimedQuadrant.IsOneOf(EVecinityQuadrant.Right, EVecinityQuadrant.RightUp, EVecinityQuadrant.RightDown))
                                Result = EAutoPositioningMode.ToLeft;
                    }
            }
            else
                if (!Preferred.IsOneOf(EAutoPositioningMode.HorizontalAlternated, EAutoPositioningMode.VerticalAlternated))
                {
                    Result = EAutoPositioningMode.ToBottom;
                    var AvailableQuadrants = ConnectionsFreeQuadrants(Source.MainSymbol);

                    if (AvailableQuadrants.Count > 0
                        && !AvailableQuadrants.Any(qdt => QuadrantToPositioningMode(qdt) == Preferred))
                        Result = QuadrantToPositioningMode(AvailableQuadrants.First());
                }

            return Result;
        }

        public static EAutoPositioningMode QuadrantToPositioningMode(EVecinityQuadrant Quadrant)
        {
            var Result = EAutoPositioningMode.ToBottom;

            if (Quadrant == EVecinityQuadrant.Up)
                Result = EAutoPositioningMode.ToUp;
            else
                if (Quadrant.IsOneOf(EVecinityQuadrant.Left, EVecinityQuadrant.LeftUp, EVecinityQuadrant.LeftDown))
                    Result = EAutoPositioningMode.ToLeft;
                else
                    if (Quadrant.IsOneOf(EVecinityQuadrant.Right, EVecinityQuadrant.RightUp, EVecinityQuadrant.RightDown))
                        Result = EAutoPositioningMode.ToRight;

            return Result;
        }

        public static List<EVecinityQuadrant> ConnectionsFreeQuadrants(VisualSymbol Source)
        {
            var UsedQuadrants = new List<EVecinityQuadrant>();

            var ConnectingPoints = Source.OriginConnections.Select(conn => (conn.IntermediatePosition == Display.NULL_POINT
                                                                               ? conn.OriginEdgePosition
                                                                               : conn.IntermediatePosition))
                                    .Concat(Source.TargetConnections.Select(conn => (conn.IntermediatePosition == Display.NULL_POINT
                                                                                         ? conn.TargetEdgePosition
                                                                                         : conn.IntermediatePosition)));

            foreach (var ConnectingPoint in ConnectingPoints)
                UsedQuadrants.AddNew(DetermineAimedQuadrant(Source.TotalArea, ConnectingPoint));

            var Result = Enum.GetValues(typeof(EVecinityQuadrant)).Cast<EVecinityQuadrant>().Except(UsedQuadrants).ToList();

            return Result;
        }

        public static EVecinityQuadrant DetermineAimedQuadrant(Rect SourceArea, Point AimPoint)
        {
            EVecinityQuadrant Result = EVecinityQuadrant.Center;

            if (AimPoint.X < SourceArea.Left)
            {
                if (AimPoint.Y < SourceArea.Top)
                    Result = EVecinityQuadrant.LeftUp;
                else
                    if (AimPoint.Y > SourceArea.Bottom)
                        Result = EVecinityQuadrant.LeftDown;
                    else
                        Result = EVecinityQuadrant.Left;
            }
            else
                if (AimPoint.X > SourceArea.Right)
                {
                    if (AimPoint.Y < SourceArea.Top)
                        Result = EVecinityQuadrant.RightUp;
                    else
                        if (AimPoint.Y > SourceArea.Bottom)
                            Result = EVecinityQuadrant.RightDown;
                        else
                            Result = EVecinityQuadrant.Right;
                }
                else
                {
                    if (AimPoint.Y > (SourceArea.Left + SourceArea.Width / 2.0))
                        Result = EVecinityQuadrant.Down;
                    else
                        Result = EVecinityQuadrant.Up;
                }

            return Result;
        }

        public static Point DetermineNewPosition(EAutoPositioningMode AutomaticCreationPositioningMode,
                                                 bool AutomaticCreationPositioningIsAlternating,
                                                 bool AutomaticCreationPositioningIsRadialized,
                                                 VisualSymbol BaseSymbol,
                                                 double NewConceptWidth, double NewConceptHeight,
                                                 double NewRelationshipWidth, double NewRelationshipHeight,
                                                 bool FromOriginToTarget)
        {
            var IsVerticalLocating = AutomaticCreationPositioningMode
                                        .IsOneOf(EAutoPositioningMode.VerticalAlternated, EAutoPositioningMode.ToLeft, EAutoPositioningMode.ToRight);

            var AlternatingDisposition = EVisualDispositionMonodimensional.Hidden;

            var ConnectionsCount = BaseSymbol.TargetConnections.Count + BaseSymbol.OriginConnections.Count;

            if (AutomaticCreationPositioningIsAlternating)
                AlternatingDisposition = ((ConnectionsCount % 2) != 0
                                          ? EVisualDispositionMonodimensional.After
                                          : EVisualDispositionMonodimensional.Before);

            var BaseToDependantDistance = (IsVerticalLocating
                                           ? (BaseSymbol.BaseWidth + NewRelationshipWidth + NewConceptWidth / 2.0)
                                           : (BaseSymbol.TotalHeight + NewRelationshipHeight + NewConceptHeight * 1.5));

            if (AutomaticCreationPositioningIsRadialized)
                BaseToDependantDistance = BaseToDependantDistance -
                                          (ConnectionsCount * 6.0).EnforceMaximum(BaseToDependantDistance * 0.75);

            var NextSymbolSize = (IsVerticalLocating ? NewConceptHeight : NewConceptWidth);

            var VariatingCoordinate = DetermineNewVariatingCoordinate(BaseSymbol, IsVerticalLocating,
                                                                      AlternatingDisposition, NextSymbolSize,
                                                                      FromOriginToTarget);

            if ((AutomaticCreationPositioningIsAlternating && AlternatingDisposition == EVisualDispositionMonodimensional.Before)
                || AutomaticCreationPositioningMode.IsOneOf(EAutoPositioningMode.ToLeft, EAutoPositioningMode.ToUp))
                BaseToDependantDistance = -BaseToDependantDistance;

            var NewPosition = new Point(IsVerticalLocating ? BaseSymbol.BaseCenter.X + BaseToDependantDistance
                                                           : VariatingCoordinate,
                                        IsVerticalLocating ? VariatingCoordinate
                                                           : BaseSymbol.BaseCenter.Y + BaseToDependantDistance);

            return NewPosition;
        }

        public static double DetermineNewVariatingCoordinate(VisualSymbol BaseSymbol, bool IsVerticalLocating,
                                                             EVisualDispositionMonodimensional AlternatingDisposition,
                                                             double NextSymbolSize, bool FromOriginToTarget)
        {
            // Notice that possibly there are a main Alternation
            // (e.g.: for vertical locating, two main columns, each at either side of the Central/Main-Symbol),
            // Then the standard sibling alignment on the same logical "Path" alternating from one end to the other.
            // (column or row depending on if vertical or horizontal locating).

            double Result = 0;

            var BaseCenter = (IsVerticalLocating ? BaseSymbol.BaseCenter.X : BaseSymbol.BaseCenter.Y);

            var DirectedConnsCount = BaseSymbol.TargetConnections.Count + BaseSymbol.OriginConnections.Count;
            var DirectedConnections = BaseSymbol.TargetConnections.Concat(BaseSymbol.OriginConnections);

            IEnumerable<VisualConnector> PathSiblings = null;

            if (AlternatingDisposition == EVisualDispositionMonodimensional.Hidden)
                PathSiblings = DirectedConnections;
            else
            {
                var AlternatedBefore = ((DirectedConnsCount % 2) == 0);

                PathSiblings = DirectedConnections.Where(conn =>
                    {
                        var DestinationSymbol = (conn.IsIn(BaseSymbol.TargetConnections)
                                                 ? conn.PrimaryRelatedTargetSymbol.NullDefault(conn.TargetSymbol)
                                                 : conn.PrimaryRelatedOriginSymbol.NullDefault(conn.OriginSymbol));

                        return (AlternatingDisposition == EVisualDispositionMonodimensional.Before
                                ? ((IsVerticalLocating ? DestinationSymbol.BaseCenter.X : DestinationSymbol.BaseCenter.Y)
                                   < (IsVerticalLocating ? BaseSymbol.BaseCenter.X : BaseSymbol.BaseCenter.Y))
                                : ((IsVerticalLocating ? DestinationSymbol.BaseCenter.X : DestinationSymbol.BaseCenter.Y)
                                   > (IsVerticalLocating ? BaseSymbol.BaseCenter.X : BaseSymbol.BaseCenter.Y)));
                    }).ToList();
            }

            if (DirectedConnsCount < (AlternatingDisposition == EVisualDispositionMonodimensional.Hidden ? 1 : 2))
                Result = (IsVerticalLocating ? BaseSymbol.BaseCenter.Y : BaseSymbol.BaseCenter.X);
            else
            {
                var AlignBefore = ((PathSiblings.Count() % 2) == 0);
                var PreviousSiblings = PathSiblings.Select(conn =>
                    {
                        var DestinationSymbol = (conn.IsIn(BaseSymbol.TargetConnections)
                                                 ? conn.PrimaryRelatedTargetSymbol.NullDefault(conn.TargetSymbol)
                                                 : conn.PrimaryRelatedOriginSymbol.NullDefault(conn.OriginSymbol));

                        return (IsVerticalLocating ? DestinationSymbol.BaseTop + (AlignBefore ? 0 : DestinationSymbol.TotalHeight)
                                                   : DestinationSymbol.BaseLeft + (AlignBefore ? 0 : DestinationSymbol.BaseWidth));
                    });

                var PreviousCoordinate = (PreviousSiblings.Any()
                                          ? (AlignBefore ? PreviousSiblings.Min() : PreviousSiblings.Max())
                                          : (IsVerticalLocating ? BaseSymbol.BaseCenter.Y : BaseSymbol.BaseCenter.X));

                Result = (AlignBefore ? PreviousCoordinate - NextSymbolSize * (IsVerticalLocating ? 1.5 : 0.75)
                                      : PreviousCoordinate + NextSymbolSize * (IsVerticalLocating ? 1.5 : 0.75));
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
