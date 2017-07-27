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
// File   : RelationshipVisualRepresentation.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.RelationshipVisualRepresentation (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.09.25 Néstor Sánchez A.  Creation
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

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Visually represents a Relationship.
    /// </summary>
    [Serializable]
    public class RelationshipVisualRepresentation : VisualRepresentation, IModelEntity, IModelClass<RelationshipVisualRepresentation>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static RelationshipVisualRepresentation()
        {
            __ClassDefinitor = new ModelClassDefinitor<RelationshipVisualRepresentation>("RelationshipVisualRepresentation", VisualRepresentation.__ClassDefinitor, "Relationship Visual Representation",
                                                                                         "Visually represents a Relationship.");
            __ClassDefinitor.DeclareProperty(__RepresentedRelationship);
            //- __ClassDefinitor.DeclareProperty(__ConnectorsFormat);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RelationshipVisualRepresentation(Relationship RepresentedRelationship, View DisplayingView)
             : base(DisplayingView)
        {
            this.RepresentedRelationship = RepresentedRelationship;

            this.RepresentedRelationship.VisualRepresentators.Add(this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Represented Idea by this visual element.
        /// </summary>
        public override Idea RepresentedIdea { get { return this.RepresentedRelationship; } }

        /// <summary>
        /// Represented Relationship by this visual element.
        /// </summary>
        public Relationship RepresentedRelationship { get { return __RepresentedRelationship.Get(this); } set { __RepresentedRelationship.Set(this, value); } }
        protected Relationship RepresentedRelationship_;
        public static readonly ModelPropertyDefinitor<RelationshipVisualRepresentation, Relationship> __RepresentedRelationship =
                   new ModelPropertyDefinitor<RelationshipVisualRepresentation, Relationship>("RepresentedRelationship", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.RepresentedRelationship_, (ins, val) => ins.RepresentedRelationship_ = val, false, false,
                                                                                              "Represented Relationship", "Represented Relationship by this visual element.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the graphic parts (drawing and z-order) of the visual depiction of this representation, plus any others needed to make it comprehensive.
        /// For example, a Relationship only is meaningful if, appart of showing its main-symbol and connectors, it also includes the connected symbols.
        /// </summary>
        public override IDictionary<Drawing, int> CreateIntegralGraphic()
        {
            var Result = new Dictionary<Drawing, int>();

            // Generate the connected symbols
            var Connectors = this.VisualParts.CastAs<VisualConnector, VisualElement>();
            var ConnectedSymbols = Connectors.Where(conn => conn.OriginSymbol != conn.TargetSymbol)
                                        .Select(conn => (conn.OriginSymbol == this.MainSymbol
                                                         ? conn.TargetSymbol : conn.OriginSymbol));

            foreach (var AssociatedSymbol in ConnectedSymbols)
                Result.Add(AssociatedSymbol.CreateDraw(null, false), AssociatedSymbol.ZOrder);

            // Generate the relationship parts
            foreach (var Part in this.VisualParts.OrderBy(part => !(part is VisualSymbol)))
                Result.Add(Part.CreateDraw(null, false), Part.ZOrder);

            // Generate complements
            foreach (var Complement in this.MainSymbol.AttachedComplements)
                Result.Add(Complement.CreateDraw(false), Complement.ZOrder);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the count of visual connectors.
        /// </summary>
        [Description("Gets the count of visual connectors.")]
        public int VisualConnectorsCount
        { get{ return this.VisualParts.Count(part => part.RepresentationPartType == EVisualRepresentationPart.RelationshipLinkConnector); } }

        /// <summary>
        /// Gets the visual connectors.
        /// </summary>
        [Description("Gets the visual connectors.")]
        public IEnumerable<VisualConnector> VisualConnectors
        { get{ return this.VisualParts.Where(part => part.RepresentationPartType == EVisualRepresentationPart.RelationshipLinkConnector).Cast<VisualConnector>(); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Straightens the connecting lines by eliminating intermediate visual points.
        /// </summary>
        public void DoStraighten()
        {
            this.EditEngine.StartCommandVariation("Straighten Relationship Visual Representation");

            this.MainSymbol.IsAutoPositionable = true;
            this.MainSymbol.RenderElement();

            foreach(var Connector in this.VisualConnectors)
                Connector.UpdateIntermediatePoint(Display.NULL_POINT);

            if (this.OriginRepresentations.First() != null)
                this.OriginRepresentations.First().MainSymbol.UpdateDependents();

            this.DisplayingView.UpdateVersion();
            this.EditEngine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies a parallel move, respect the specified source and deltas, to the whole visual representation, including main-symbol and connectors.
        /// </summary>
        public void DoParallelMove(VisualSymbol Source, double DeltaX, double DeltaY, IList<VisualObject> RegionContainedObjects = null)
        {
            this.EditEngine.StartCommandVariation("Move Relationship Visual Representation");

            if (RegionContainedObjects == null || !RegionContainedObjects.Contains(this.MainSymbol))
            {
                this.MainSymbol.MoveTo(this.MainSymbol.BaseCenter.X + DeltaX,
                                       this.MainSymbol.BaseCenter.Y + DeltaY,
                                       false, false);

                // IMPORTANT: The Origin and Target Positions are moved by the Symbol's MoveTo method

                foreach (var VisConn in this.VisualConnectors)
                    if (VisConn.IntermediatePosition != Display.NULL_POINT)
                        VisConn.IntermediatePosition = new Point(VisConn.IntermediatePosition.X + DeltaX, VisConn.IntermediatePosition.Y + DeltaY);
            }

            this.MainSymbol.UpdateDependents();
            this.Render();

            this.DisplayingView.UpdateVersion();
            this.EditEngine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Bends in mutual opposite directions the specified visual connectors which represents an auto-reference.
        /// </summary>
        public void BendAutoRefConnectors(VisualConnector OriginConnector, VisualConnector TargetConnector)
        {
            double Toleration = 20.0;
            double Expansion = Toleration * 2.0;
            double PosX, PosY;
            bool TooCloseX, TooCloseY;
            var PrimaryCenter = this.MainSymbol.BaseCenter;
            var SecondaryCenter = (OriginConnector.OriginSymbol == this.MainSymbol
                                   ? OriginConnector.TargetSymbol.BaseCenter
                                   : OriginConnector.OriginSymbol.BaseCenter);

            if (OriginConnector.IntermediatePosition == Display.NULL_POINT)
            {
                PosX = PrimaryCenter.X;
                PosY = SecondaryCenter.Y;
                TooCloseX = PosX.IsCloseTo(SecondaryCenter.X, Toleration);
                TooCloseY = PosY.IsCloseTo(PrimaryCenter.Y, Toleration);

                if (TooCloseX)
                {
                    PosX = PrimaryCenter.X - Expansion;
                    PosY = (PrimaryCenter.Y + SecondaryCenter.Y) / 2.0;
                }

                if (TooCloseY)
                {
                    PosY = SecondaryCenter.Y - Expansion;
                    PosX = (PrimaryCenter.X + SecondaryCenter.X) / 2.0;
                }

                OriginConnector.IntermediatePosition = new Point(PosX, PosY);
            }

            if (TargetConnector.IntermediatePosition == Display.NULL_POINT)
            {
                PosX = SecondaryCenter.X;
                PosY = PrimaryCenter.Y;
                TooCloseX = PosX.IsCloseTo(PrimaryCenter.X, Toleration);
                TooCloseY = PosY.IsCloseTo(SecondaryCenter.Y, Toleration);

                if (TooCloseX)
                {
                    PosX = SecondaryCenter.X + Expansion;
                    PosY = (PrimaryCenter.Y + SecondaryCenter.Y) / 2.0;
                }

                if (TooCloseY)
                {
                    PosY = PrimaryCenter.Y + Expansion;
                    PosX = (PrimaryCenter.X + SecondaryCenter.X) / 2.0;
                }

                TargetConnector.IntermediatePosition = new Point(PosX, PosY);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<RelationshipVisualRepresentation> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<RelationshipVisualRepresentation> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<RelationshipVisualRepresentation> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new RelationshipVisualRepresentation CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((RelationshipVisualRepresentation)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public RelationshipVisualRepresentation PopulateFrom(RelationshipVisualRepresentation SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        public override string ToString()
        {
            var RepIdea = (this.RepresentedIdea == null ? "<Empty>" : this.RepresentedIdea.ToString());
            var DispView = (this.DisplayingView == null ? "<Empty>" : this.DisplayingView.ToString());
            return "Visual Representation of Relationship '" + RepIdea + "' on View '" + DispView + "'";
        }
    }
}