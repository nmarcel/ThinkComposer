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
// File   : VisualConnector.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualConnector (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Composer.ComposerUI;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Makes a visual connection between two elements.
    /// </summary>
    [Serializable]
    public class VisualConnector : VisualElement, IModelEntity, IModelClass<VisualConnector>
    {
        /// <summary>
        /// Size adjustment factor for plug and lines respect its system definitions.
        /// </summary>
        public const double VISUAL_MAGNITUDE_ADJUSTMENT = 0.65;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualConnector()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualConnector>("VisualConnector", VisualElement.__ClassDefinitor, "Visual Connector",
                                                                        "Makes a visual connection between two elements.");
            __ClassDefinitor.DeclareProperty(__OwnerRelationshipRepresentation);
            __ClassDefinitor.DeclareProperty(__RepresentedLink);
            __ClassDefinitor.DeclareProperty(__OriginPosition);
            __ClassDefinitor.DeclareProperty(__OriginEdgePosition);
            __ClassDefinitor.DeclareProperty(__OriginSymbol);
            __ClassDefinitor.DeclareProperty(__TargetPosition);
            __ClassDefinitor.DeclareProperty(__TargetEdgePosition);
            __ClassDefinitor.DeclareProperty(__TargetSymbol);
            __ClassDefinitor.DeclareProperty(__IntermediatePosition);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualConnector(RelationshipVisualRepresentation OwnerRelationshipRepresentation, RoleBasedLink RepresentedLink,
                               VisualSymbol OriginSymbol, VisualSymbol TargetSymbol, Point OriginPosition, Point TargetPosition)
             : base(EVisualRepresentationPart.RelationshipLinkConnector)
        {
            this.OwnerRelationshipRepresentation = OwnerRelationshipRepresentation;
            this.RepresentedLink = RepresentedLink;
            this.OriginSymbol = OriginSymbol;
            this.TargetSymbol = TargetSymbol;
            this.OriginPosition = OriginPosition;
            this.TargetPosition = TargetPosition;

            this.OriginSymbol.TargetConnections.Add(this);
            this.TargetSymbol.OriginConnections.Add(this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects the bounds created on construction with the Origin and Target symbols.
        /// </summary>
        public void Disconnect()
        {
            // Remove semantic Link

            // IMPORTANT: If the future, and if the link can be represented by more than one connector,
            //            then only remove the link when only one representative connector remains.
            this.OwnerRelationshipRepresentation.RepresentedRelationship.RemoveLink(this.RepresentedLink);

            this.OriginSymbol.TargetConnections.Remove(this);
            this.TargetSymbol.OriginConnections.Remove(this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a new draw implementing this visual connector for an optional presentation context.
        /// </summary>
        public override DrawingGroup CreateDraw(UIElement PresentationContext, bool ShowManipulationAdorners)
        {
            // Calculate the Edge Positions
            if (this.OriginSymbol.Graphic == null)
                this.OriginSymbol.GenerateGraphic(PresentationContext, ShowManipulationAdorners);

            if (this.TargetSymbol.Graphic == null)
                this.TargetSymbol.GenerateGraphic(PresentationContext, ShowManipulationAdorners);

            // IMPORTANT: This validation allows to calculate edge-positions ONLY when not present.
            //            The last calculated edge-positions are used for when presentation-context is not supplied,
            //            which is used for generating the graphics of a composite-content view inside a symbol's details poster.
            if (PresentationContext == null
                && (this.OriginEdgePosition == Display.NULL_POINT || this.TargetEdgePosition == Display.NULL_POINT))
                PresentationContext = this.OwnerRepresentation.DisplayingView.Presenter;

            DrawingGroup Result = null;

            if (this.IntermediatePosition == Display.NULL_POINT)
            {
                if (PresentationContext != null)
                {
                    /*T Console.WriteLine("OriSym={0}=>{1}, TarSym={2}=>{3}.",
                                      this.OriginSymbol, this.OriginSymbol.Graphic.GetHashCode(),
                                      this.TargetSymbol, this.TargetSymbol.Graphic.GetHashCode()); */

                    if (this.OriginSymbol.IsRelatedVisible)
                        this.OriginEdgePosition = (this.OriginSymbol.IsHidden ? this.OriginPosition :
                                                   this.OriginPosition.DetermineNearestIntersectingPoint(this.TargetPosition, PresentationContext,
                                                                                                         this.OriginSymbol.Graphic, this.OwnerRepresentation.DisplayingView.VisualHitTestFilter));

                    if (this.TargetSymbol.IsRelatedVisible)
                        this.TargetEdgePosition = (this.TargetSymbol.IsHidden ? this.TargetPosition :
                                                   this.TargetPosition.DetermineNearestIntersectingPoint(this.OriginPosition, PresentationContext,
                                                                                                         this.TargetSymbol.Graphic, this.OwnerRepresentation.DisplayingView.VisualHitTestFilter));

                    // Nasty trick to compensate miscalculation of border position (or after undoing a relationship-central-symbol move)
                    if (this.OriginEdgePosition == this.TargetSymbol.BaseCenter)
                    {
                        this.OriginPosition = this.OriginSymbol.BaseCenter.FindBoundary(this.OriginPosition, PresentationContext, this.OriginSymbol.Graphic, true)
                                                                                .SubstituteFor(default(Point), this.OriginSymbol.BaseCenter);
                        this.OriginEdgePosition = this.OriginPosition;
                    }

                    if (this.TargetEdgePosition == this.OriginSymbol.BaseCenter)
                    {
                        this.TargetPosition = this.TargetSymbol.BaseCenter.FindBoundary(this.TargetPosition, PresentationContext, this.TargetSymbol.Graphic, true)
                                                                                .SubstituteFor(default(Point), this.TargetSymbol.BaseCenter);
                        this.TargetEdgePosition = this.TargetPosition;
                    }
                }

                /*T Console.WriteLine("OriginEdgePoint X={0}, Y={1}. TargetEdgePoint X={2}, Y={3}",
                                  Math.Truncate(this.OriginEdgePosition.X), Math.Truncate(this.OriginEdgePosition.Y),
                                  Math.Truncate(this.TargetEdgePosition.X), Math.Truncate(this.TargetEdgePosition.Y)); */

                // Draw the Connector line
                Result = MasterDrawer.CreateDrawingConnector(this.OriginPlug, this.TargetPlug,
                                                             VisualConnectorsFormat.GetLineBrush(this),
                                                             VisualConnectorsFormat.GetLineThickness(this),
                                                             VisualConnectorsFormat.GetLineDash(this),
                                                             VisualConnectorsFormat.GetLineJoin(this),
                                                             VisualConnectorsFormat.GetLineCap(this),
                                                             VisualConnectorsFormat.GetPathStyle(this),
                                                             VisualConnectorsFormat.GetPathCorner(this),
                                                             VisualConnectorsFormat.GetMainBackground(this),
                                                             VisualConnectorsFormat.GetOpacity(this),
                                                             this.TargetEdgePosition, this.OriginEdgePosition,
                                                             null, VISUAL_MAGNITUDE_ADJUSTMENT);

                /*T visual cue for detecting miscalculations...
                Result.Children.Add(new GeometryDrawing(Brushes.Green, new Pen(Brushes.Red, 1.0),
                                    new EllipseGeometry(new Point(this.OriginEdgePosition.X + 100, this.OriginEdgePosition.Y), 3, 3)));
                Result.Children.Add(new GeometryDrawing(Brushes.Blue, new Pen(Brushes.Red, 1.0),
                                    new EllipseGeometry(new Point(this.TargetEdgePosition.X + 100, this.TargetEdgePosition.Y), 3, 3))); */
            }
            else
            {
                if (PresentationContext != null)
                {
                    var EdgePos = (this.OriginSymbol.IsHidden ? this.OriginPosition :
                                   this.OriginPosition.DetermineNearestIntersectingPoint(this.IntermediatePosition, PresentationContext,
                                                                                         this.OriginSymbol.Graphic, this.OwnerRepresentation.DisplayingView.VisualHitTestFilter));
                    if (EdgePos != this.IntermediatePosition)
                        this.OriginEdgePosition = EdgePos;

                    EdgePos = (this.TargetSymbol.IsHidden ? this.TargetPosition :
                               this.TargetPosition.DetermineNearestIntersectingPoint(this.IntermediatePosition, PresentationContext,
                                                                                     this.TargetSymbol.Graphic, this.OwnerRepresentation.DisplayingView.VisualHitTestFilter));
                    if (EdgePos != this.IntermediatePosition)
                        this.TargetEdgePosition = EdgePos;
                }

                //T Console.WriteLine("OriginEdgePoint X={0}, Y={1}. TargetEdgePoint X={2}, Y={3}. IP={4}", OriginEdgePoint.X, OriginEdgePoint.Y, TargetEdgePoint.X, TargetEdgePoint.Y, this.IntermediatePosition);

                // Draw the Connector origin line
                Result = MasterDrawer.CreateDrawingConnector(this.OriginPlug, Plugs.None,
                                                             VisualConnectorsFormat.GetLineBrush(this),
                                                             VisualConnectorsFormat.GetLineThickness(this),
                                                             VisualConnectorsFormat.GetLineDash(this),
                                                             VisualConnectorsFormat.GetLineJoin(this),
                                                             VisualConnectorsFormat.GetLineCap(this),
                                                             VisualConnectorsFormat.GetPathStyle(this),
                                                             VisualConnectorsFormat.GetPathCorner(this),
                                                             VisualConnectorsFormat.GetMainBackground(this),
                                                             VisualConnectorsFormat.GetOpacity(this),
                                                             this.IntermediatePosition, this.OriginEdgePosition,
                                                             null, VISUAL_MAGNITUDE_ADJUSTMENT);
                // Draw the Connector target line
                Result.Children.Add(
                         MasterDrawer.CreateDrawingConnector(Plugs.None, this.TargetPlug,
                                                             VisualConnectorsFormat.GetLineBrush(this),
                                                             VisualConnectorsFormat.GetLineThickness(this),
                                                             VisualConnectorsFormat.GetLineDash(this),
                                                             VisualConnectorsFormat.GetLineJoin(this),
                                                             VisualConnectorsFormat.GetLineCap(this),
                                                             VisualConnectorsFormat.GetPathStyle(this),
                                                             VisualConnectorsFormat.GetPathCorner(this),
                                                             VisualConnectorsFormat.GetMainBackground(this),
                                                             VisualConnectorsFormat.GetOpacity(this),
                                                             this.TargetEdgePosition, this.IntermediatePosition,
                                                             null, VISUAL_MAGNITUDE_ADJUSTMENT));
            }

            /*T Console.WriteLine("OriginEdgePosition X={0}, Y={1}. TargetEdgePosition X={2}, Y={3}. IP={4}",
                              OriginEdgePosition.X, OriginEdgePosition.Y, TargetEdgePosition.X, TargetEdgePosition.Y, this.IntermediatePosition); */

            // PENDING: Register periferic decorators for drawing (such as callouts, notes, etc. Not to be confused with Text decorations)

            // Show main-symbol name if required
            var RelDef = this.RepresentedLink.OwnerRelationship.RelationshipDefinitor.Value;

            /* ?
            if (RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple && RelDef.ShowNameIfHidingCentralSymbol)
            {
                var LabelingBrushes = this.OwnerRelationshipRepresentation.MainSymbol.PutNameOnTop(Result);
                this.OwnerRelationshipRepresentation.MainSymbol.PutDefinitionOnTop(Result, LabelingBrushes.Item2, LabelingBrushes.Item1, 4);
            }
            else
                if (this.OwnerRelationshipRepresentation.MainSymbol.IsHidden)
                {
                    var LabelingBrushes = this.OwnerRelationshipRepresentation.MainSymbol.GetDefaultLabelBrushes();
                    this.OwnerRelationshipRepresentation.MainSymbol.PutDefinitionOnTop(Result, LabelingBrushes.Item2, LabelingBrushes.Item1);
                } */

            this.LabelArea = null;

            // Show link-role name decorator if required
            if (RelDef.DefaultConnectorsFormat.LabelLinkVariant
                || RelDef.DefaultConnectorsFormat.LabelLinkDefinitor
                || RelDef.DefaultConnectorsFormat.LabelLinkDescriptor
                || this.OwnerRepresentation.DisplayingView.ShowLinkRoleVariantLabels
                || this.OwnerRepresentation.DisplayingView.ShowLinkRoleDefNameLabels
                || this.OwnerRepresentation.DisplayingView.ShowLinkRoleDescNameLabels)
                using (var Context = Result.Append())
                {
                    var DecoratorCenter = this.IntermediatePosition;

                    if (DecoratorCenter == Display.NULL_POINT)
                        DecoratorCenter = new Point((this.OriginEdgePosition.X + this.TargetEdgePosition.X) / 2.0,
                                            (this.OriginEdgePosition.Y + this.TargetEdgePosition.Y) / 2.0);

                    /*T else
                            Console.WriteLine("DecoratorCenter = IntermediatePoint"); */

                    //T Console.WriteLine("DecoratorCenter: X={0}, Y={1}", DecoratorCenter.X, DecoratorCenter.Y);

                    var LinkDescriptorLabel = (((RelDef.DefaultConnectorsFormat.LabelLinkDescriptor
                                               || this.OwnerRepresentation.DisplayingView.ShowLinkRoleDescNameLabels)
                                               && this.RepresentedLink.Descriptor != null)
                                               ? this.RepresentedLink.Descriptor.Name : null);

                    var LinkDefinitorLabel = (RelDef.DefaultConnectorsFormat.LabelLinkDefinitor
                                              || this.OwnerRepresentation.DisplayingView.ShowLinkRoleDefNameLabels
                                              ? this.RepresentedLink.RoleDefinitor.Name : null);

                    var LinkRoleVariantLabel = (RelDef.DefaultConnectorsFormat.LabelLinkVariant
                                                || this.OwnerRepresentation.DisplayingView.ShowLinkRoleVariantLabels
                                                ? this.RepresentedLink.RoleVariant.ToString() : null);

                    this.LabelArea = MasterDrawer.PutConnectorLabeling(Context, RelDef, DecoratorCenter,
                                                                       VisualSymbolFormat.GetTextFormat(this.OwnerRelationshipRepresentation.MainSymbol,
                                                                                                        ETextPurpose.Extra),
                                                                       VisualConnectorsFormat.GetMainBackground(this),
                                                                       VisualConnectorsFormat.GetLineBrush(this),
                                                                       LinkDescriptorLabel, LinkDefinitorLabel, LinkRoleVariantLabel);
                }

            // Register Selection Indicators for drawing
            // NOTE: (selection indicators at the symbol's center interfere with in-place editing)
            if (ShowManipulationAdorners)
                if (this.OwnerRepresentation.IsSelected)
                {
                    var SizeFactor = (this.GetDisplayingView().SelectedObjects.Contains(this)
                                      ? 1.5 : 0.5);
                    this.OwnerRepresentation.DisplayingView.AttachAdorner(this, GenerateSelectionIndicators(INDICATOR_SIZE * SizeFactor,
                                                                                                            SelectionIndicatorBackground,
                                                                                                            SelectionIndicatorForeground).Select(tup => tup.Item1));
                }
                else
                    this.OwnerRepresentation.DisplayingView.DetachAdorner(this);

            if (this.OwnerRepresentation.IsVanished)
                Result.Opacity = VisualRepresentation.SELECTION_VANISHING_OPACITY;

            return Result;
        }

        /// <summary>
        /// Contains area of a possible label.
        /// </summary>
        [NonSerialized]
        public Rect? LabelArea = null;

        /// <summary>
        /// Recreates and returns the Graphic of this visual connector.
        /// </summary>
        public override ContainerVisual GenerateGraphic(UIElement PresentationContext, bool ShowManipulationAdorners)
        {
            this.Graphic = this.CreateDraw(PresentationContext, ShowManipulationAdorners).RenderToDrawingVisual();
            return this.Graphic;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the connector intermediate point to the specified position.
        /// </summary>
        public void UpdateIntermediatePoint(Point NewPosition)
        {
            this.IntermediatePosition = NewPosition;
            this.RenderElement();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the connector intermediate point.
        /// </summary>
        public void DoStraighten()
        {
            this.EditEngine.StartCommandVariation("Straighten Connector");

            this.UpdateIntermediatePoint(Display.NULL_POINT);
            this.GetDisplayingView().UpdateVersion();

            this.EditEngine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cycle through the available link-role variants.
        /// </summary>
        public void DoCycleThroughVariants()
        {
            /* Old validation, which not allowed modification (derivated from Link-Role Variants definitions change)
            if (this.RepresentedLink.RoleDefinitor.AllowedVariants.Count <= 1)
            {
                Console.WriteLine("Cannot cycle through variants because only one is allowed for that link-role type.");
                // return;
            } */

            var OriginalIndex = this.RepresentedLink.RoleDefinitor.AllowedVariants.IndexOf(this.RepresentedLink.RoleVariant);
            var VariantIndex = (OriginalIndex < this.RepresentedLink.RoleDefinitor.AllowedVariants.Count - 1
                                ? OriginalIndex + 1 : 0);

            if (VariantIndex == OriginalIndex)
            {
                Console.WriteLine("Cannot cycle through variants because only one is allowed for that link-role type.");
                return;
            }

            this.EditEngine.StartCommandVariation("Cycle Through Variants of Connector");

            this.RepresentedLink.RoleVariant = this.RepresentedLink.RoleDefinitor.AllowedVariants[VariantIndex];

            this.RepresentedLink.UpdateVersion();
            this.RenderElement();

            this.EditEngine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Edit the represented Link descriptor.
        /// </summary>
        public void DoEditDescriptor()
        {
            this.RepresentedLink.DoEditDescriptor(lnk => this.RenderElement());
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the owning visual representator.
        /// </summary>
        public override VisualRepresentation OwnerRepresentation { get { return this.OwnerRelationshipRepresentation; } set { this.OwnerRelationshipRepresentation = (RelationshipVisualRepresentation)value; } }

        /// <summary>
        /// References the owning relationship visual representator.
        /// </summary>
        public RelationshipVisualRepresentation OwnerRelationshipRepresentation { get { return __OwnerRelationshipRepresentation.Get(this); } set { __OwnerRelationshipRepresentation.Set(this, value); } }
        protected RelationshipVisualRepresentation OwnerRelationshipRepresentation_;
        public static readonly ModelPropertyDefinitor<VisualConnector, RelationshipVisualRepresentation> __OwnerRelationshipRepresentation =
                   new ModelPropertyDefinitor<VisualConnector, RelationshipVisualRepresentation>("OwnerRelationshipRepresentation", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerRelationshipRepresentation_, (ins, val) => ins.OwnerRelationshipRepresentation_ = val, false, false,
                                                                                                 "Owner Relationship Representation", "References the owning relationship visual representator.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the represented Role Based Link.
        /// </summary>
        public RoleBasedLink RepresentedLink { get { return __RepresentedLink.Get(this); } internal set { __RepresentedLink.Set(this, value); } }
        protected RoleBasedLink RepresentedLink_ = null;
        public static readonly ModelPropertyDefinitor<VisualConnector, RoleBasedLink> __RepresentedLink =
                   new ModelPropertyDefinitor<VisualConnector, RoleBasedLink>("RepresentedLink", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.RepresentedLink_, (ins, val) => ins.RepresentedLink_ = val, false, false,
                                                                              "Represented Link", "References the represented Role Based Link.");

        /// <summary>
        /// Source position of the connector.
        /// </summary>
        // IMPORTANT: Notice the return of symbol.BaseCenter when not populated.
        public Point OriginPosition { get { return __OriginPosition.Get(this)/*?.SubstituteFor(default(Point), (this.OriginSymbol == null ? Display.NULL_POINT : this.OriginSymbol.BaseCenter))*/; }
                             internal set { __OriginPosition.Set(this, value); } }
        protected Point OriginPosition_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualConnector, Point> __OriginPosition =
                   new ModelPropertyDefinitor<VisualConnector, Point>("OriginPosition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.OriginPosition_, (ins, val) => ins.OriginPosition_ = val, false, false,
                                                                      "Origin Position", "Source position of the connector.");

        /// <summary>
        /// Source edge-position of the connector respect the source symbol.
        /// </summary>
        public Point OriginEdgePosition { get { return __OriginEdgePosition.Get(this); } internal set { __OriginEdgePosition.Set(this, value); } }
        protected Point OriginEdgePosition_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualConnector, Point> __OriginEdgePosition =
                   new ModelPropertyDefinitor<VisualConnector, Point>("OriginEdgePosition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.OriginEdgePosition_, (ins, val) => ins.OriginEdgePosition_ = val, false, false,
                                                                      "Origin Edge-Position", "Source edge-position of the connector respect the source symbol.");

        /// <summary>
        /// Destination position of the connector.
        /// </summary>
        // IMPORTANT: Notice the return of symbol.BaseCenter when not populated.
        public Point TargetPosition { get { return __TargetPosition.Get(this)/*?.SubstituteFor(default(Point), (this.TargetSymbol == null ? Display.NULL_POINT : this.TargetSymbol.BaseCenter))*/; }
                             internal set { __TargetPosition.Set(this, value); } }
        protected Point TargetPosition_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualConnector, Point> __TargetPosition =
                   new ModelPropertyDefinitor<VisualConnector, Point>("TargetPosition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.TargetPosition_, (ins, val) => ins.TargetPosition_ = val, false, false,
                                                                      "Target Position", "Destination position of the connector.");

        /// <summary>
        /// Destination edge-position of the connector respect the target symbol.
        /// </summary>
        public Point TargetEdgePosition { get { return __TargetEdgePosition.Get(this); } internal set { __TargetEdgePosition.Set(this, value); } }
        protected Point TargetEdgePosition_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualConnector, Point> __TargetEdgePosition =
                   new ModelPropertyDefinitor<VisualConnector, Point>("TargetEdgePosition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.TargetEdgePosition_, (ins, val) => ins.TargetEdgePosition_ = val, false, false,
                                                                      "Target Edge-Position", "Destination edge-position of the connector respect the target symbol.");

        /// <summary>
        /// Symbol pointed by this Connector.
        /// </summary>
        public VisualSymbol TargetSymbol { get { return __TargetSymbol.Get(this); } internal set { __TargetSymbol.Set(this, value); } }
        protected VisualSymbol TargetSymbol_ = null;
        public static readonly ModelPropertyDefinitor<VisualConnector, VisualSymbol> __TargetSymbol =
                   new ModelPropertyDefinitor<VisualConnector, VisualSymbol>("TargetSymbol", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.TargetSymbol_, (ins, val) => ins.TargetSymbol_ = val, false, false,
                                                                             "Target Symbol", "Symbol pointed by this Connector.");

        /// <summary>
        /// Symbol where this Connector originates.
        /// </summary>
        public VisualSymbol OriginSymbol { get { return __OriginSymbol.Get(this); } internal set { __OriginSymbol.Set(this, value); } }
        protected VisualSymbol OriginSymbol_ = null;
        public static readonly ModelPropertyDefinitor<VisualConnector, VisualSymbol> __OriginSymbol =
                   new ModelPropertyDefinitor<VisualConnector, VisualSymbol>("OriginSymbol", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.OriginSymbol_, (ins, val) => ins.OriginSymbol_ = val, false, false,
                                                                             "Origin Symbol", "Symbol where this Connector originates.");

        /// <summary>
        /// Gets the connector intermediate point (if not empty/null) or the final origin position inside symbol.
        /// </summary>
        public Point OriginIntermediateOrFinalPosition
        { get{ return (this.IntermediatePosition != Display.NULL_POINT ? this.IntermediatePosition : this.OriginPosition); } }

        /// <summary>
        /// Gets the connector intermediate point (if not empty/null) or the final target position inside symbol.
        /// </summary>
        public Point TargetIntermediateOrFinalPosition
        { get{ return (this.IntermediatePosition != Display.NULL_POINT ? this.IntermediatePosition : this.TargetPosition); } }

        /// <summary>
        /// Gets the connector Origin point, considering if Relationship is "Simple" and hidden, plus intermediate points of both connectors in that case.
        /// </summary>
        public Point FinalOriginPoint
        {
            get
            {
                var RelDef = this.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value;

                if (RelDef.IsSimple)
                    if (this.IntermediatePosition != Display.NULL_POINT)
                        return this.IntermediatePosition;
                    else
                        if (!this.OriginSymbol.IsAutoPositionable)
                            return this.OriginPosition;
                        else
                            if (this.OwnerRelationshipRepresentation.VisualConnectorsCount > 1)
                            {
                                var OppositeConnector = (this.OwnerRelationshipRepresentation.VisualConnectors.First() == this
                                                         ? this.OwnerRelationshipRepresentation.VisualConnectors.Skip(1).First()
                                                         : this.OwnerRelationshipRepresentation.VisualConnectors.First());
                                if (OppositeConnector.IntermediatePosition != Display.NULL_POINT)
                                    return OppositeConnector.IntermediatePosition;
                                else
                                    return OppositeConnector.OriginPosition;
                            }

                return this.OriginIntermediateOrFinalPosition;
            }
        }

        /// <summary>
        /// Gets the connector Target point, considering if Relationship is "Simple" and hidden, plus intermediate points of both connectors in that case.
        /// </summary>
        public Point FinalTargetPoint
        {
            get
            {
                var RelDef = this.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value;

                if (RelDef.IsSimple)
                    if (this.IntermediatePosition != Display.NULL_POINT)
                        return this.IntermediatePosition;
                    else
                        if (!this.TargetSymbol.IsAutoPositionable)
                            return this.TargetPosition;
                        else
                            if (this.OwnerRelationshipRepresentation.VisualConnectorsCount > 1)
                            {
                                var OppositeConnector = (this.OwnerRelationshipRepresentation.VisualConnectors.First() == this
                                                         ? this.OwnerRelationshipRepresentation.VisualConnectors.Skip(1).First()
                                                         : this.OwnerRelationshipRepresentation.VisualConnectors.First());
                                if (OppositeConnector.IntermediatePosition != Display.NULL_POINT)
                                    return OppositeConnector.IntermediatePosition;
                                else
                                    return OppositeConnector.TargetPosition;
                            }

                return this.TargetIntermediateOrFinalPosition;
            }
        }

        /// <summary>
        /// Gets the plug type code for the origin side.
        /// </summary>
        [Description("Gets the plug type code for the origin side.")]
        public string OriginPlug
        {
            get
            {
                var Result = Plugs.None;

                if (this.OriginSymbol == this.OwnerRelationshipRepresentation.MainSymbol)
                    return Result;

                var PlugsSource = this.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value.DefaultConnectorsFormat.TailPlugs;
                Result = PlugsSource.GetValueOrFirst(this.RepresentedLink.RoleVariant);

                return Result;
            }
        }

        /// <summary>
        /// Gets the plug type code for the target side.
        /// </summary>
        [Description("Gets the plug type code for the target side.")]
        public string TargetPlug
        {
            get
            {
                var Result = Plugs.None;

                if (this.TargetSymbol == this.OwnerRelationshipRepresentation.MainSymbol)
                    return Result;

                if (this.RepresentedLink.RoleDefinitor.OwnerRelationshipDef.IsDirectional)
                {
                    var PlugsSource = this.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value.DefaultConnectorsFormat.HeadPlugs;
                    Result = PlugsSource.GetValueOrFirst(this.RepresentedLink.RoleVariant);
                }
                else
                {
                    var PlugsSource = this.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value.DefaultConnectorsFormat.TailPlugs;
                    Result = PlugsSource.GetValueOrFirst(this.RepresentedLink.RoleVariant);
                }

                return Result;
            }
        }

        /// <summary>
        /// When targeting to a Relationship Main Symbol, returns the first target Symbol from that Relationship or null if none is targeted,
        /// else, when targeting to a Concept Symbol, returns that symbol. Returns null if there is no targeted symbol.
        /// </summary>
        public VisualSymbol PrimaryRelatedTargetSymbol
        {
            get
            {
                if (this.TargetSymbol == null)
                    return null;

                if (this.TargetSymbol.OwnerRepresentation is ConceptVisualRepresentation)
                    return this.TargetSymbol;

                var FirstConnection = this.TargetSymbol.TargetConnections.FirstOrDefault();
                if (FirstConnection == null)
                    return null;

                return FirstConnection.TargetSymbol;
            }
        }

        /// <summary>
        /// When originated from a Relationship Main Symbol, returns the first Origin Symbol from that Relationship or null if none is originated,
        /// else, when originating from a Concept Symbol, returns that Origin symbol. Returns null if there is no originated symbol.
        /// </summary>
        public VisualSymbol PrimaryRelatedOriginSymbol
        {
            get
            {
                if (this.OriginSymbol == null)
                    return null;

                if (this.OriginSymbol.OwnerRepresentation is ConceptVisualRepresentation)
                    return this.OriginSymbol;

                var FirstConnection = this.OriginSymbol.OriginConnections.FirstOrDefault();
                if (FirstConnection == null)
                    return null;

                return FirstConnection.OriginSymbol;
            }
        }

        /// <summary>
        /// Draws and returns a set of indicator adorners (drawing, is-main and manipulation-direction), based on supplied Indicator Size, Stroke, Pencil and optional Geometry-Creator, for mark the selection of this visual element.
        /// </summary>
        public override List<Tuple<Drawing, bool, EManipulationDirection>> GenerateSelectionIndicators(double IndicatorSize, Brush IndStroke, Pen IndPencil, Func<Rect, Geometry> GeometryCreator = null)
        {
            /*T if (GeometryCreator == null)
            {
                IndicatorSize = IndicatorSize * 3;
                IndStroke = Brushes.Transparent;
            } */

            GeometryCreator = GeometryCreator.NullDefault((rect) => new EllipseGeometry(rect));

            var StandardIndicators = new List<Tuple<Drawing, bool, EManipulationDirection>>();
            double PosX, PosY;
            Drawing IndOrigin = null, IndTarget = null, IndIntermediate = null;

            PosX = this.OriginEdgePosition.X - (IndicatorSize / 2.0);
            PosY = this.OriginEdgePosition.Y - (IndicatorSize / 2.0);
            IndOrigin = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
            StandardIndicators.Add(Tuple.Create(IndOrigin, true, EManipulationDirection.TopLeft)); // Note: The Tuple Items 1 and 2 (is-main and manipulation-direction) are not currently used in the Connectors context

            PosX = this.TargetEdgePosition.X - (IndicatorSize / 2.0);
            PosY = this.TargetEdgePosition.Y - (IndicatorSize / 2.0);
            IndTarget = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
            StandardIndicators.Add(Tuple.Create(IndTarget, true, EManipulationDirection.BottomRight)); // Note: The Tuple Items 1 and 2 (is-main and manipulation-direction) are not currently used in the Connectors context

            if (this.IntermediatePosition != Display.NULL_POINT)
            {
                PosX = this.IntermediatePosition.X - (IndicatorSize / 2.0);
                PosY = this.IntermediatePosition.Y - (IndicatorSize / 2.0);
                IndIntermediate = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndIntermediate, false, EManipulationDirection.Top)); // Note: The Tuple Items 1 and 2 (is-main and manipulation-direction) are not currently used in the Connectors context
            }

            return StandardIndicators;
        }

        /// <summary>
        /// Intermediate optional position of the connector.
        /// </summary>
        public Point IntermediatePosition { get { return __IntermediatePosition.Get(this); } internal set { __IntermediatePosition.Set(this, value); } }
        protected Point IntermediatePosition_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualConnector, Point> __IntermediatePosition =
                   new ModelPropertyDefinitor<VisualConnector, Point>("IntermediatePosition", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.IntermediatePosition_, (ins, val) => ins.IntermediatePosition_ = val, false, false,
                                                                      "Intermediate Position", "Intermediate optional position of the connector.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current content area, considering the origin, target and intermediate points.
        /// </summary>
        public override Rect TotalArea
        {
            get
            {
                double LeftLimit = this.OriginPosition.X;
                double RightLimit = this.OriginPosition.X;
                double TopLimit = this.OriginPosition.Y;
                double BottomLimit = this.OriginPosition.Y;
                
                LeftLimit = Math.Min(LeftLimit, this.TargetPosition.X);
                RightLimit = Math.Max(RightLimit, this.TargetPosition.X);
                TopLimit = Math.Min(TopLimit, this.TargetPosition.Y);
                BottomLimit = Math.Max(BottomLimit, this.TargetPosition.Y);

                if (this.IntermediatePosition != Display.NULL_POINT)
                {
                    LeftLimit = Math.Min(LeftLimit, this.IntermediatePosition.X);
                    RightLimit = Math.Max(RightLimit, this.IntermediatePosition.X);
                    TopLimit = Math.Min(TopLimit, this.IntermediatePosition.Y);
                    BottomLimit = Math.Max(BottomLimit, this.IntermediatePosition.Y);
                }

                var Result = new Rect(LeftLimit, TopLimit, (RightLimit - LeftLimit) + 1.0, (BottomLimit - TopLimit) + 1.0);
                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this object can be moved.
        /// </summary>
        public override bool CanMove { get { return false; } }  // Move is not provided as by rectangle basis.

        /// <summary>
        /// Indicates whether this object can be resized.
        /// </summary>
        public override bool CanResize { get { return false; } }    // Resize is not provided as by rectangle basis.

        /// Center point of the object.
        /// </summary>
        public override Point BaseCenter { get { return default(Point); } set { } }

        /// <summary>
        /// Top X-coordinate of the object.
        /// </summary>
        public override double BaseTop { get { return 0; } set { } }

        /// <summary>
        /// Left Y-coordinate of the object.
        /// </summary>
        public override double BaseLeft { get { return 0; } set { } }

        /// <summary>
        /// Width of the object.
        /// </summary>
        public override double BaseWidth { get { return 0; } set { } }

        /// <summary>
        /// Height of the object.
        /// </summary>
        public override double BaseHeight { get { return 0; } set { } }

        /// <summary>
        /// Area of the figure.
        /// </summary>
        public override Rect BaseArea { get { return this.TotalArea; } }

        /// <summary>
        /// Gets the movable pieces which this visual-object considers as visually united, plus indication of being contained within a region.
        /// </summary>
        // MUST RETURN NOTHING FOR CONNECTORS, BECAUSE THEY ARE INDIRECTLY MOVED.
        public override IEnumerable<Tuple<VisualObject,bool>> GetMovableMembers(bool IncludeRelatedOrigins, bool IncludeRelatedTargets, bool IsForVisualization)
        {
            return Enumerable.Empty<Tuple<VisualObject,bool>>();
        }

        /// <summary>
        /// Moves the object to the specified coordinates.
        /// </summary>
        // MUST DO NOTHING, BECAUSE CONNECTORS ARE INDIRECTLY MOVED.
        public override void MoveTo(double PosX, double PosY, bool LockNewPosition = false, bool IsResizing = false) { }

        /// <summary>
        /// Resizes the object to the specified dimensions.
        /// Returns indication of valid resizing respect the minimum allowed.
        /// </summary>
        public override bool ResizeTo(double Width, double Height) { return false; }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualConnector> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualConnector> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualConnector> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualConnector CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualConnector)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualConnector PopulateFrom(VisualConnector SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        public override string ToString()
        {
            var Origin = (this.OriginSymbol == null ? "<Empty>" : this.OriginSymbol.OwnerRepresentation.RepresentedIdea.ToString());
            var Target = (this.TargetSymbol == null ? "<Empty>" : this.TargetSymbol.OwnerRepresentation.RepresentedIdea.ToString());
            var OwnerRep = (this.OwnerRepresentation == null ? "<Empty>" : this.OwnerRepresentation.RepresentedIdea.Name);

            return "Connector of '" + OwnerRep + "', from [" + Origin + "] to [" + Target + "]"; //T ", Id=" + this.GlobalId.ToString() + ".";
        }

    }
}