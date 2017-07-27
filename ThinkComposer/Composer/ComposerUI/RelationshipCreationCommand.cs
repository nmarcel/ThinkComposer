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
// File   : RelationshipCreationCommand.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.RelationshipCreationCommand (Class)
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
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.Composer.ComposerUI;
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
    /// Creates a Relationship, with its visual representation, for a given composite Idea.
    /// </summary>
    public class RelationshipCreationCommand : WorkCommandInteractive<MouseEventArgs>
    {
        public CompositionEngine ContextEngine { get; protected set; }
        public RelationshipDefinition RelationshipDef { get; protected set; }
        public Relationship NewRelationship { get; protected set; }

        public Brush DefaultPointingConnectorBrush = Brushes.Gray;

        VisualRepresentation OriginRepresentation = null;
        Point OriginLocation = Display.NULL_POINT;
        VisualRepresentation TargetRepresentation = null;
        Point TargetLocation = Display.NULL_POINT;
        VisualConnectorsFormat PointingConnectorFormat = null;

        protected bool WasKeyShiftPressed = false;
        protected bool WasKeyCtrlPressed = false;

        public bool IsConnecting { get; protected set; }
        public View WorkingView { get; protected set; }

        public static VisualConnectorsFormat DefaultPointingConnectorsFormat { get; private set; }

        public RelationshipCreationCommand(CompositionEngine TargetEngine, RelationshipDefinition RelationshipDef)
             : base("Create Relationship '" + RelationshipDef.Name + "'.")
        {
            this.ContextEngine = TargetEngine;
            this.RelationshipDef = RelationshipDef;
            this.PointingConnectorFormat = RelationshipDef.DefaultConnectorsFormat;

            this.Initialize();

            this.Assistance = "Press [Left-ALT] to link the pointed Idea as origin.";
        }

        public override void Initialize()
        {
            OriginRepresentation = null;
            OriginLocation = Display.NULL_POINT;
            TargetRepresentation = null;
            TargetLocation = Display.NULL_POINT;
            WasKeyShiftPressed = false;
            WasKeyCtrlPressed = false;
            IsConnecting = false;   // Very important to clear prior to restart
            PointingConnectorFormat = null;

            base.Initialize();

            var Loader = Display.GetResource<TextBlock>("CursorLoaderArrowIdea");   // Trick for assign custom cursors.
            Display.GetCurrentWindow().Cursor = Loader.Cursor;

            PointingAssistant.Start(this.ContextEngine.CurrentView, this.RelationshipDef.GetSampleDrawing(true), true, Cursors.Cross);
        }

        public override void Execute(MouseEventArgs Parameter = null)
        {
            base.Execute(Parameter);
        }

        public override bool Continue(MouseEventArgs Parameter, bool IsDefinitive = true)
        {
            var Go = base.Continue(Parameter, IsDefinitive);
            if (!Go)
                return false;

            if (this.WorkingView == null)
                this.WorkingView = this.ContextEngine.CurrentView;

            if (this.WorkingView != this.ContextEngine.CurrentView)
            {
                this.Terminate();
                return false;
            }

            this.TargetLocation = Parameter.GetPosition(this.WorkingView.PresenterControl);
            this.TargetRepresentation = this.ContextEngine.GetPointedRepresentation(this.TargetLocation, true);
            var TargetObject = this.ContextEngine.GetPointedVisualObject(this.TargetLocation, 2.0);

            // IMPORTANT: Get the snapped position AFTER obtain the pointed representation
            if (this.WorkingView.SnapToGrid)
                this.TargetLocation = this.WorkingView.GetGridSnappedPosition(TargetLocation, false);

            this.WasKeyShiftPressed = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
            this.WasKeyCtrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));

            if (this.TargetRepresentation != null
                && !(this.TargetRepresentation.RepresentedIdea.IdeaDefinitor.PreciseConnectByDefault
                     || this.WasKeyCtrlPressed))
                this.TargetLocation = this.TargetRepresentation.MainSymbol.BaseCenter;

            if (IsDefinitive)   // If selecting a target representation...
            {
                //T Console.WriteLine("Pointed VisObj={0}", TargetObject.ToStringAlways("<NULL>"));

                PointingAssistant.ClearConnector();

                if (!this.IsConnecting)
                {
                    /* Better at definitive target (?)
                    this.WasKeyCtrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl)  || Keyboard.IsKeyDown(Key.RightCtrl));
                    this.WasKeyShiftPressed = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)); */

                    this.OriginLocation = Parameter.GetPosition(this.WorkingView.PresenterControl);
                    this.OriginRepresentation = this.ContextEngine.GetPointedRepresentation(this.OriginLocation, true);

                    // IMPORTANT: Get the snapped position AFTER obtain the pointed representation
                    if (this.WorkingView.SnapToGrid)
                        this.OriginLocation = this.WorkingView.GetGridSnappedPosition(this.OriginLocation, false);

                    if (this.OriginRepresentation == null)   // Exit if nothing pointed at the beginning
                        return true;

                    if (!this.OriginRepresentation.RepresentedIdea.IdeaDefinitor.PreciseConnectByDefault
                        && !this.WasKeyCtrlPressed)
                        this.OriginLocation = this.OriginRepresentation.MainSymbol.BaseCenter;

                    this.IsConnecting = true;
                    return true;
                }

                if (this.OriginRepresentation == this.TargetRepresentation)
                    return true;

                // Needed to discard cancellation from a previous mouse-right-button click.
                this.RestartAfterTermination = true;

                if (this.TargetRepresentation == null && TargetObject != null && TargetObject is VisualComplement)
                {
                    var Complement = (VisualComplement)TargetObject;

                    // NOTE: This don't work if pointing to a complement with null background (it is not seen).
                    if (Complement.CanGroup &&
                        (!Complement.Target.IsGlobal &&
                         Complement.Target.OwnerLocal.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.CanAutomaticallyCreateGroupedConcepts &&
                         Complement.Target.OwnerLocal.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.AutomaticGroupedConceptDef != null))
                    {
                        // PENDING: Create grouped Concept and use it as target-representation.
                        var ConceptDef = Complement.Target.OwnerLocal.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.AutomaticGroupedConceptDef;

                        var Position = this.TargetLocation;

                        if (Complement.IsComplementGroupLine)
                        {
                            var Direction = Complement.GetPropertyField<Orientation>(VisualComplement.PROP_FIELD_ORIENTATION);

                            if (Direction == Orientation.Vertical)
                                Position = new Point(Complement.BaseCenter.X,
                                                     this.TargetLocation.Y + ((ConceptDef.DefaultSymbolFormat.InitialHeight / 2.0) *
                                                                              (this.TargetLocation.Y >= Complement.Target.OwnerLocal.BaseCenter.Y
                                                                               ? 1.0 : -1.0)));
                            else
                                Position = new Point(this.TargetLocation.X + ((ConceptDef.DefaultSymbolFormat.InitialWidth / 2.0) *
                                                                              (this.TargetLocation.X >= Complement.Target.OwnerLocal.BaseCenter.X
                                                                               ? 1.0 : -1.0)),
                                                     Complement.BaseCenter.Y);
                        }

                        var Creation = ConceptCreationCommand.CreateConcept(this.WorkingView.VisualizedCompositeIdea, ConceptDef,
                                                                            this.WorkingView, Position);
                        if (Creation.WasSuccessful)
                            this.TargetRepresentation = Creation.Result.VisualRepresentators.First();
                    }
                }

                var OrigRep = this.OriginRepresentation;
                var TargRep = this.TargetRepresentation;
                var OrigLoc = this.OriginLocation;
                var TargLoc = this.TargetLocation;

                // Allows extend Relationship from alternate origin.
                // Notice that the target (later used as origin) must be not-null.
                if ((Keyboard.IsKeyDown(Key.LeftAlt))
                    && !this.WasKeyShiftPressed && TargRep != null)
                {
                    var TmpRep = OrigRep;
                    OrigRep = TargRep;
                    TargRep = TmpRep;

                    var TmpLoc = OrigLoc;
                    OrigLoc = TargLoc;
                    TargLoc = TmpLoc;
                }

                // NOTE: TargetRepresentation can be null, so the intermediate MainSymbol can be created with no targets yet.

                var CreationResult = CreateRelationship(this.WorkingView.VisualizedCompositeIdea,
                                                        this.RelationshipDef,
                                                        this.WorkingView,
                                                        OrigRep, OrigLoc,
                                                        TargRep, TargLoc,
                                                        !this.WasKeyShiftPressed,   // Extend from origin.
                                                        !(Keyboard.IsKeyDown(Key.RightAlt)    // IMPORTANT: This crashes if applied to a Simple-and-hiding-central-symbol Relationship
                                                          && !(this.RelationshipDef.IsSimple
                                                               && this.RelationshipDef.HideCentralSymbolWhenSimple)));   // Extend Relationship, else create Rel. from Rel.

                this.WasKeyShiftPressed = false;
                this.WasKeyCtrlPressed = false;

                if (CreationResult.WasSuccessful)
                {
                    bool CanSelect = !this.RelationshipDef.HideCentralSymbolWhenSimple;

                    // If a True boolean was passed, indicating to edit-in-place and can select...
                    bool CanEditInPlace = (CreationResult.Parameters.InterpretItem<bool>() && CanSelect);

                    this.OriginRepresentation = CreationResult.Result.VisualRepresentators[0];

                    // Re-start linking from Relationship Central/Main-Symbol center.
                    this.OriginLocation = this.OriginRepresentation.MainSymbol.BaseCenter;

                    this.WorkingView.Presenter
                        .PostCall(pres =>   // Post-called in order to be executed after the symbol heading-content area is established (on first draw).
                        {
                            var Symbol = CreationResult.Result.VisualRepresentators[0].MainSymbol;
                            if (CanSelect)
                                pres.OwnerView.Manipulator.ApplySelection(Symbol);

                            if (CanEditInPlace)
                                pres.OwnerView.EditInPlace(Symbol);
                        });

                    // PENDING: Also quit if role is non multi-connectable
                    if (this.RelationshipDef.IsSimple)
                    {
                        var RelationshipRep = CreationResult.Context as RelationshipVisualRepresentation;

                        if (RelationshipRep != null)
                        {
                            if (RelationshipRep.VisualConnectorsCount > 1)
                                return false;
                            else
                                if (!RelationshipDef.HideCentralSymbolWhenSimple)
                                    RelationshipRep.MainSymbol.IsAutoPositionable = false;
                        }
                        else
                            return false;
                    }
                }
                else
                {
                    this.IsConnecting = false;
                    Console.WriteLine("Cannot create/change Relationship: " + CreationResult.Message.AbsentDefault("?"));
                    this.Terminate();

                    return false;
                }
            }
            else    // Else, if just pointing...
            {
                if (this.IsConnecting && this.OriginRepresentation != null)
                {
                    if (this.PointingConnectorFormat == null)
                        this.PointingConnectorFormat = new VisualConnectorsFormat(this.RelationshipDef.OwnerDomain.LinkRoleVariants.First(), Plugs.None,
                                                                                  this.RelationshipDef.OwnerDomain.LinkRoleVariants.First(), Plugs.SimpleArrow,
                                                                                  DefaultPointingConnectorBrush);

                    var OrigPlug = Plugs.None;
                    var TargPlug = Plugs.SimpleArrow;

                    if ((Keyboard.IsKeyDown(Key.LeftAlt))
                        && !this.WasKeyShiftPressed)
                    {
                        var TmpPlug = OrigPlug;
                        OrigPlug = TargPlug;
                        TargPlug = TmpPlug;
                    }

                    var GraphicPointer = MasterDrawer.CreateDrawingConnector(OrigPlug, TargPlug,
                                                                             this.PointingConnectorFormat.LineBrush, this.PointingConnectorFormat.LineThickness,
                                                                             this.PointingConnectorFormat.LineDash, this.PointingConnectorFormat.LineJoin,
                                                                             this.PointingConnectorFormat.LineCap, this.PointingConnectorFormat.PathStyle,
                                                                             this.PointingConnectorFormat.PathCorner, this.PointingConnectorFormat.MainBackground,
                                                                             this.PointingConnectorFormat.Opacity,
                                                                             this.TargetLocation, this.OriginLocation);
                    PointingAssistant.PutConnector(GraphicPointer);

                    this.RestartAfterTermination = true;
                }
            }

            // Continue the command...
            return true;
        }

        public override void Terminate(bool IsNormalTermination = false, MouseEventArgs Parameter = null)
        {
            var WorkingRelationshipRep = this.OriginRepresentation as RelationshipVisualRepresentation;

            if (this.IsConnecting && this.RelationshipDef.IsSimple && this.RelationshipDef.HideCentralSymbolWhenSimple
                && WorkingRelationshipRep != null && WorkingRelationshipRep.VisualConnectorsCount <= 1)
                this.ContextEngine.DeleteObjects(this.OriginRepresentation.MainSymbol.IntoEnumerable());

            base.Terminate(IsNormalTermination, Parameter);
            PointingAssistant.Finish();

            Display.GetCurrentWindow().Cursor = Cursors.Arrow;

            ProductDirector.RelationshipPaletteControl.ClearSelection();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static OperationResult<Relationship> CreateRelationship(Idea DestinationComposite, RelationshipDefinition Definitor, View TargetView,
                                                                       VisualRepresentation OriginRepresentation, Point OriginPosition,
                                                                       VisualRepresentation TargetRepresentation, Point TargetPosition,
                                                                       bool ExtendFromOriginRelationship = true, // (Shift not pressed) Applicable only when origin and target are both relationships. Else (Shift pressed) create Rel. from Target to Origin.
                                                                       bool ExtendExistingRelationship = true)   // (Right-Alt not pressed) Applicable only when origin is a not (simple and hiding-central-symbol) relationship. Else (Right-Alt pressed) created Rel. from Rel.
        {
            bool EditInPlace = false;

            General.ContractRequiresNotNull(DestinationComposite, Definitor, TargetView, OriginRepresentation);

            var MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.IdeasCreationQuotas);

            if (!ProductDirector.ValidateEditionLimit(DestinationComposite.OwnerComposition.DeclaredIdeas.Count() + 1, MaxQuota, "create", "Ideas (Concepts + Relationships)"))
                return OperationResult.Failure<Relationship>("This product edition cannot create more than " + MaxQuota + " Ideas.");

            if (!ExtendFromOriginRelationship && (TargetRepresentation == null || !(TargetRepresentation.RepresentedIdea is Relationship)))
                ExtendFromOriginRelationship = true;

            var PotentialOriginRelationship = OriginRepresentation.RepresentedIdea as Relationship;
            var PotentialTargetRelationship = (TargetRepresentation == null ? (Relationship)null : TargetRepresentation.RepresentedIdea as Relationship);
            bool CreatingNewRelationship = (/*?*/!ExtendExistingRelationship ||/*?*/ !((PotentialOriginRelationship != null && PotentialOriginRelationship.RelationshipDefinitor.Value == Definitor)
                                                                             || (PotentialTargetRelationship != null && PotentialTargetRelationship.RelationshipDefinitor.Value == Definitor)));
            bool? ExtensionFromOriginRelationship = (CreatingNewRelationship ? (bool?)null : (PotentialOriginRelationship != null && ExtendFromOriginRelationship));

            if (!CreatingNewRelationship && TargetRepresentation == null)
                if (OriginRepresentation is RelationshipVisualRepresentation)
                    return OperationResult.Success(((RelationshipVisualRepresentation)OriginRepresentation).RepresentedRelationship, "Creating new Relationship.");
                else
                    return OperationResult.Failure<Relationship>("There is no targeted Idea to relate.");

            if (DestinationComposite.IdeaDefinitor.CompositeContentDomain == null)
                return OperationResult.Failure<Relationship>("Destination Container doest not accept Composite-Content.", DestinationComposite);

            if (DestinationComposite.CompositeContentDomain.GlobalId != Definitor.OwnerDomain.GlobalId)
                return OperationResult.Failure<Relationship>("The destination container only accepts content of the '" + DestinationComposite.CompositeContentDomain.Name + "' Domain.");

            Relationship WorkingRelationship = null;
            LinkRoleDefinition OriginLinkRoleDef = null;
            RoleBasedLink OriginLink = null;

            DestinationComposite.EditEngine.StartCommandVariation("Create Relationship");

            if (CreatingNewRelationship)
            {
                if (TargetRepresentation != null)
                {
                    var CanLink = Definitor.CanLink(OriginRepresentation.RepresentedIdea.IdeaDefinitor, TargetRepresentation.RepresentedIdea.IdeaDefinitor);

                    if (!CanLink.Result)
                    {
                        DestinationComposite.EditEngine.DiscardCommandVariation();
                        return OperationResult.Failure<Relationship>(CanLink.Message);
                    }
                }

                string NewName = Definitor.Name;

                var AppendNumToName = AppExec.GetConfiguration<bool>("IdeaEditing", "Relationship.OnCreationAppendDefNameAndNumber", true);
                if (AppendNumToName)
                    NewName = NewName +" (Idea " + (DestinationComposite.CompositeIdeas.Count + 1).ToString() + ")";

                WorkingRelationship = new Relationship(DestinationComposite.OwnerComposition, Definitor, NewName, NewName.TextToIdentifier());

                if (Definitor.IsVersionable)
                    WorkingRelationship.Version = new VersionCard();

                WorkingRelationship.AddToComposite(DestinationComposite);

                OriginLinkRoleDef = Definitor.GetLinkForRole(ERoleType.Origin);
                OriginLink = new RoleBasedLink(WorkingRelationship, OriginRepresentation.RepresentedIdea, OriginLinkRoleDef, OriginLinkRoleDef.AllowedVariants[0]);
                WorkingRelationship.AddLink(OriginLink);

                PotentialOriginRelationship = WorkingRelationship;
                EditInPlace = true;
            }
            else
            {
                OperationResult<bool> CanLink = OperationResult.Success(true);

                if (ExtensionFromOriginRelationship.IsTrue())
                    WorkingRelationship = PotentialOriginRelationship;
                else
                    WorkingRelationship = (PotentialTargetRelationship != null ? PotentialTargetRelationship : PotentialOriginRelationship);

                if (TargetRepresentation != null)
                {
                    CanLink = EvaluateLinkability(Definitor, OriginRepresentation, TargetRepresentation,
                                                  PotentialOriginRelationship, PotentialTargetRelationship, WorkingRelationship);

                    if (!CanLink.Result)
                    {
                        DestinationComposite.EditEngine.DiscardCommandVariation();
                        return OperationResult.Failure<Relationship>("Cannot create/extend Relationship. Cause: " + CanLink.Message);
                    }
                }
            }

            LinkRoleDefinition WorkingLinkRoleDef = null;
            RoleBasedLink WorkingLink = null;

            if (TargetRepresentation != null && (CreatingNewRelationship || (WorkingRelationship == PotentialOriginRelationship
                                                                             && ExtensionFromOriginRelationship.IsTrue())))
            {
                WorkingLinkRoleDef = Definitor.GetLinkForRole(ERoleType.Target);
                if (WorkingRelationship.Links.Where(link => link.RoleDefinitor == WorkingLinkRoleDef
                    && link.AssociatedIdea == TargetRepresentation.RepresentedIdea).Any())
                {
                    DestinationComposite.EditEngine.DiscardCommandVariation();
                    return OperationResult.Failure<Relationship>("Cannot create Target Link. Cause: Already exists one of the same role-type associating the same Idea.");
                }

                WorkingLink = new RoleBasedLink(WorkingRelationship, TargetRepresentation.RepresentedIdea, WorkingLinkRoleDef, WorkingLinkRoleDef.AllowedVariants[0]);
                WorkingRelationship.AddLink(WorkingLink);
            }

            if (OriginRepresentation != null && !CreatingNewRelationship
                && WorkingRelationship == PotentialTargetRelationship  && !ExtensionFromOriginRelationship.IsTrue())
            {
                WorkingLinkRoleDef = Definitor.GetLinkForRole(ERoleType.Origin);
                if (WorkingRelationship.Links.Where(link => link.RoleDefinitor == WorkingLinkRoleDef
                    && link.AssociatedIdea == OriginRepresentation.RepresentedIdea).Any())
                {
                    DestinationComposite.EditEngine.DiscardCommandVariation();
                    return OperationResult.Failure<Relationship>("Cannot create Origin Link. Cause: Already exists one of the same role-type associating the same Idea.");
                }

                WorkingLink = new RoleBasedLink(WorkingRelationship, OriginRepresentation.RepresentedIdea, WorkingLinkRoleDef, WorkingLinkRoleDef.AllowedVariants[0]);
                WorkingRelationship.AddLink(WorkingLink);
            }

            RelationshipVisualRepresentation Representator = null;
            VisualSymbol CentralSymbol = null;

            if (TargetPosition != Display.NULL_POINT
                && OriginPosition != Display.NULL_POINT)
            {
                VisualConnector OriginConnector = null;

                var InitialPosition = OriginPosition;
                var EdgeOriginPosition = OriginPosition;
                var EdgeTargetPosition = TargetPosition;

                if (CreatingNewRelationship)
                {
                    // Force connect from Symbols' centers.
                    if (OriginRepresentation != null)
                        EdgeOriginPosition = OriginPosition.DetermineNearestIntersectingPoint(TargetPosition, TargetView.Presenter,
                                                                                              OriginRepresentation.MainSymbol.Graphic,
                                                                                              TargetView.VisualHitTestFilter);

                    if (TargetRepresentation == null)
                        InitialPosition = TargetPosition;
                    else
                    {
                        EdgeTargetPosition = TargetPosition.DetermineNearestIntersectingPoint(OriginPosition, TargetView.Presenter,
                                                                                              TargetRepresentation.MainSymbol.Graphic,
                                                                                              TargetView.VisualHitTestFilter);
                        InitialPosition = EdgeOriginPosition.DetermineCenterRespect(EdgeTargetPosition);
                    }

                    // Create representation
                    Representator = CreateRelationshipVisualRepresentation(WorkingRelationship, TargetView, InitialPosition);
                    CentralSymbol = Representator.MainSymbol;

                    // Notice that here the Origin connector is being drawn, so the Target plug is empty/none (because is connected to the Relationship Central/Main Symbol).
                    OriginConnector = new VisualConnector(Representator, OriginLink, OriginRepresentation.MainSymbol, CentralSymbol, OriginPosition, CentralSymbol.BaseCenter);
                }
                else
                {
                    if (WorkingRelationship == PotentialOriginRelationship)
                        Representator = (RelationshipVisualRepresentation)OriginRepresentation;
                    else
                        Representator = (RelationshipVisualRepresentation)TargetRepresentation;

                    CentralSymbol = OriginRepresentation.MainSymbol;
                    InitialPosition = CentralSymbol.BaseCenter;
                }

                VisualConnector TargetConnector = null;
                VisualConnector OriginAutoRefConnector = null;

                if (TargetRepresentation != null)
                {
                    TargetConnector = new VisualConnector(Representator, WorkingLink, CentralSymbol, TargetRepresentation.MainSymbol, InitialPosition, TargetPosition);

                    if (WorkingRelationship == PotentialOriginRelationship)
                        OriginAutoRefConnector = CentralSymbol.OriginConnections.FirstOrDefault(conn => conn.OriginSymbol == TargetRepresentation.MainSymbol);
                }

                if (CreatingNewRelationship)
                    Representator.AddVisualPart(OriginConnector);

                if (TargetConnector != null)
                    Representator.AddVisualPart(TargetConnector);

                if (OriginAutoRefConnector != null)
                    Representator.BendAutoRefConnectors(OriginAutoRefConnector, TargetConnector);

                Representator.Render();
            }

            DestinationComposite.UpdateVersion();
            DestinationComposite.EditEngine.CompleteCommandVariation();

            var InformedRelationship = WorkingRelationship;
            if (WorkingRelationship == PotentialTargetRelationship
                && PotentialOriginRelationship != null)
                InformedRelationship = PotentialOriginRelationship;

            return OperationResult.Success(InformedRelationship, null, null, CentralSymbol.OwnerRepresentation, EditInPlace);
        }

        /// <summary>
        /// Determines whether a working relationship being created/extended can support the creation of a new link.
        /// </summary>
        private static OperationResult<bool> EvaluateLinkability(RelationshipDefinition Definitor, VisualRepresentation OriginRepresentation, VisualRepresentation TargetRepresentation,
                                                          Relationship PotentialOriginRelationship, Relationship PotentialTargetRelationship, Relationship WorkingRelationship,
                                                          bool IsForRelink = false)
        {
            OperationResult<bool> CanLink = OperationResult.Success(true);

            if (WorkingRelationship == PotentialOriginRelationship)
            {
                if (!IsForRelink
                    && WorkingRelationship.RelationshipDefinitor.Value.IsSimple
                    && WorkingRelationship.Links.Count > 1)
                    CanLink = OperationResult.Failure<bool>("Simple source working relationship already has origin and target connected.");

                /*!? CANCELLED: Inhibits connect a Relationship (thinking that this will extend that Rel links)
                if (!IsForRelink
                    && CanLink.Result && PotentialTargetRelationship != null
                    && PotentialTargetRelationship.RelationshipDefinitor.Value.IsSimple
                    && PotentialTargetRelationship.Links.Count > 1)
                    CanLink = OperationResult.Failure<bool>("Simple destination working relationship already has origin and target connected."); */

                if (CanLink.Result && TargetRepresentation != null)
                    foreach (var SourceLink in PotentialOriginRelationship.Links.Where(link => link.RoleDefinitor.RoleType != ERoleType.Target))
                    {
                        CanLink = Definitor.CanLink(SourceLink.AssociatedIdea.IdeaDefinitor, TargetRepresentation.RepresentedIdea.IdeaDefinitor);
                        if (CanLink.Result)
                            break;
                    }
            }
            else
            {
                if (!IsForRelink
                    && WorkingRelationship.RelationshipDefinitor.Value.IsSimple
                    && WorkingRelationship.Links.Count > 1)
                    CanLink = OperationResult.Failure<bool>("Simple destination relationship already has origin and target connected.");

                /*!? CANCELLED: Inhibits connect a Relationship (thinking that this will extend that Rel links)
                if (!IsForRelink
                    && CanLink.Result && PotentialOriginRelationship != null
                    && PotentialOriginRelationship.RelationshipDefinitor.Value.IsSimple
                    && PotentialOriginRelationship.Links.Count > 1)
                    CanLink = OperationResult.Failure<bool>("Simple source relationship already has origin and target connected."); */

                if (CanLink.Result && OriginRepresentation != null)
                    foreach (var DestinationLink in PotentialTargetRelationship.Links.Where(link => link.RoleDefinitor.RoleType == ERoleType.Target))
                    {
                        CanLink = Definitor.CanLink(OriginRepresentation.RepresentedIdea.IdeaDefinitor, DestinationLink.AssociatedIdea.IdeaDefinitor);
                        if (CanLink.Result)
                            break;
                    }
            }

            return CanLink;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a Relationship Visual Representation shortcut (as Main Central/Main-Symbol)
        /// to the target Relationship, for the specified target-view, center-position, symbol and connectors format, plus as-shortcut indication.
        /// </summary>
        public static RelationshipVisualRepresentation CreateRelationshipVisualRepresentation(Relationship Target, View TargetView, Point CenterPosition,
                                                                                              bool AsShortcut = false)
        {
            var Representator = new RelationshipVisualRepresentation(Target, TargetView);

            Representator.IsShortcut = AsShortcut;

            var Width = Target.RelationshipDefinitor.Value.DefaultSymbolFormat.InitialWidth.SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Width);
            var Height = Target.RelationshipDefinitor.Value.DefaultSymbolFormat.InitialHeight.SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Height);

            // Notice that "hidden" symbols are not adjusted/snapped to grid
            if (TargetView.SnapToGrid && !(Target.RelationshipDefinitor.Value.IsSimple && Target.RelationshipDefinitor.Value.HideCentralSymbolWhenSimple))
            {
                var SnappedArea = TargetView.GetGridSnappedArea(CenterPosition, Width, Height);

                if (!Target.RelationshipDefinitor.Value.DefaultSymbolFormat.HasFixedWidth)
                    Width = SnappedArea.Width;

                if (!Target.RelationshipDefinitor.Value.DefaultSymbolFormat.HasFixedWidth)
                    Height = SnappedArea.Height;

                CenterPosition = new Point(SnappedArea.X + SnappedArea.Width / 2.0, SnappedArea.Y + SnappedArea.Height / 2.0);
            }

            var Body = new VisualShape(Representator, EVisualRepresentationPart.RelationshipCentralSymbol, CenterPosition,
                                       Width, Height);

            Body.IsAutoPositionable = true; // Very important (for users) to work easy

            Representator.AddVisualPart(Body);

            // Append possible Group Region Complement
            if (Target.IdeaDefinitor.HasGroupRegion)
            {
                var Params = VisualComplement.GetGroupRegionInitialParams(Body);
                ComplementCreationCommand.CreateComplement(TargetView.OwnerCompositeContainer, Domain.ComplementDefGroupRegion, TargetView, Body, Params.Item1, Params.Item2);
            }

            // Append possible Group Line Complement
            if (Target.IdeaDefinitor.HasGroupLine)
                ComplementCreationCommand.CreateComplement(TargetView.OwnerCompositeContainer, Domain.ComplementDefGroupLine, TargetView, Body, VisualComplement.GetGroupLineInitialPosition(Body));

            // NOTE: This does not render because the connectors are expected to be added later.

            return Representator;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a copy of the supplied original Relationship and its Visual Representation, for the specified target-view and position.
        /// </summary>
        public static OperationResult<RelationshipVisualRepresentation> CreateRelationshipAndRepresentationCopy(RelationshipVisualRepresentation OriginalRelationshipRep,
                                                                                                         View TargetView, Point Position)
        {
            // PENDING: SHOW A LITTLE CIRCLE FOR SIMPLE MAIN-SYMBOL HIDING RELATIONSHIPS.

            var NewRelationship = OriginalRelationshipRep.RepresentedRelationship.GenerateIndependentRelationshipDuplicate();

            var NewRepresentator = CreateRelationshipVisualRepresentation(NewRelationship, TargetView, Position);

            NewRepresentator.MainSymbol.IsAutoPositionable = OriginalRelationshipRep.MainSymbol.IsAutoPositionable;
            NewRepresentator.MainSymbol.AreDetailsShown = OriginalRelationshipRep.MainSymbol.AreDetailsShown;
            NewRepresentator.MainSymbol.ResizeTo(OriginalRelationshipRep.MainSymbol.BaseWidth,
                                                 OriginalRelationshipRep.MainSymbol.BaseHeight);
            NewRepresentator.MainSymbol.DetailsPosterHeight = OriginalRelationshipRep.MainSymbol.DetailsPosterHeight;

            foreach (var Complement in OriginalRelationshipRep.MainSymbol.AttachedComplements)
            {
                var NewComplementTarget = (Complement.Target.IsGlobal
                                           ? Ownership.Create<View, VisualSymbol>(TargetView)
                                           : Ownership.Create<View, VisualSymbol>(NewRepresentator.MainSymbol));
                var NewComplement = Complement.GenerateIndependentDuplicate(NewComplementTarget);
                if (!NewComplement.Target.IsGlobal)
                    NewComplement.BaseCenter = new Point(NewComplement.BaseCenter.X + (NewComplementTarget.OwnerLocal.BaseCenter.X - OriginalRelationshipRep.MainSymbol.BaseCenter.X),
                                                         NewComplement.BaseCenter.Y + (NewComplementTarget.OwnerLocal.BaseCenter.Y - OriginalRelationshipRep.MainSymbol.BaseCenter.Y));

                NewRepresentator.MainSymbol.AddComplement(NewComplement);
                TargetView.PutComplement(NewComplement);
            }

            return OperationResult.Success(NewRepresentator);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Reassign a Link, represented by the supplied Connector, to the specified new-target.
        /// </summary>
        public static void RelinkRelationship(VisualConnector Connector, VisualSymbol NewConnectedSymbol,
                                              Point RelinkingPosition, bool IsConnectingTarget)
        {
            // If ReLinking, not only changing connection position, from/to a non relationship central/main symbol...
            if ((IsConnectingTarget && Connector.TargetSymbol != NewConnectedSymbol
                 && Connector.OwnerRelationshipRepresentation.MainSymbol != Connector.TargetSymbol)
                || (!IsConnectingTarget && Connector.OriginSymbol != NewConnectedSymbol
                    && Connector.OwnerRelationshipRepresentation.MainSymbol != Connector.OriginSymbol))
            {
                var CanLink = EvaluateLinkability(
                        Connector.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value,
                        (IsConnectingTarget ? Connector.OriginSymbol.OwnerRepresentation : NewConnectedSymbol.OwnerRepresentation /*- Connector.TargetSymbol.OwnerRepresentation*/ ),
                        (IsConnectingTarget ? NewConnectedSymbol.OwnerRepresentation /*- Connector.TargetSymbol.OwnerRepresentation*/ : Connector.OriginSymbol.OwnerRepresentation),
                        (IsConnectingTarget ? Connector.OwnerRelationshipRepresentation.RepresentedRelationship : NewConnectedSymbol.OwnerRepresentation.RepresentedIdea as Relationship),
                        (IsConnectingTarget ? NewConnectedSymbol.OwnerRepresentation.RepresentedIdea as Relationship : Connector.OwnerRelationshipRepresentation.RepresentedRelationship),
                        Connector.OwnerRelationshipRepresentation.RepresentedRelationship,
                        true);

                if (!CanLink.Result)
                {
                    Console.WriteLine("Cannot change link. Cause: " + CanLink.Message);
                    return;
                }

                if (IsConnectingTarget)
                    Connector.TargetSymbol.OriginConnections.Remove(Connector);
                else
                    Connector.OriginSymbol.TargetConnections.Remove(Connector);

                Connector.RepresentedLink.AssociatedIdea.AssociatingLinks.Remove(Connector.RepresentedLink);

                Connector.RepresentedLink.AssociatedIdea = NewConnectedSymbol.OwnerRepresentation.RepresentedIdea;
                Connector.RepresentedLink.AssociatedIdea.AssociatingLinks.Add(Connector.RepresentedLink);

                if (IsConnectingTarget)
                {
                    Connector.TargetSymbol = NewConnectedSymbol;
                    Connector.TargetSymbol.OriginConnections.Add(Connector);
                }
                else
                {
                    Connector.OriginSymbol = NewConnectedSymbol;
                    Connector.OriginSymbol.TargetConnections.Add(Connector);
                }
            }
            else    // else, if only changing position, then ensure that the target and origin symbol is the same
                if (NewConnectedSymbol != (IsConnectingTarget ? Connector.TargetSymbol : Connector.OriginSymbol))
                    return;

            if (IsConnectingTarget)
                Connector.TargetPosition = RelinkingPosition;
            else
                Connector.OriginPosition = RelinkingPosition;

            var NewEdgePos = RelinkingPosition.DetermineNearestIntersectingPoint(
                                        (IsConnectingTarget ? Connector.FinalOriginPoint
                                                            : Connector.FinalTargetPoint),
                                        NewConnectedSymbol.OwnerRepresentation.DisplayingView.Presenter,
                                        NewConnectedSymbol.Graphic,
                                        NewConnectedSymbol.OwnerRepresentation.DisplayingView.VisualHitTestFilter);

            if (IsConnectingTarget)
                Connector.TargetEdgePosition = NewEdgePos;
            else
                Connector.OriginEdgePosition = NewEdgePos;

            NewConnectedSymbol.UpdateConnectorMiddleSymbolOfSimpleRel(Connector, !IsConnectingTarget, Enumerable.Empty<VisualSymbol>());

            Connector.RenderElement();

            Connector.OwnerRelationshipRepresentation.RepresentedRelationship.NotifyPropertyChange("DescriptiveCaption");
        }


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
