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
// File   : ViewConnectorManipulationAdorner.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ViewConnectorManipulationAdorner (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.05 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Presents visual cues for manipulating view connector elements.
    /// </summary>
    public class ViewConnectorManipulationAdorner : ViewManipulationAdorner
    {
        public const double INDICATOR_SIZE = 5;
        public const double ACTIONER_SIZE = 20;
        public const double MANIPULATING_CONNECTOR_WIDTH = 3.0; // 8.0 This should not be wider until resolve "alternate" pointing problem

        public VisualConnector ManipulatedConnector { get { return this.ManipulatedObject as VisualConnector; } }
        public Point ManipulationAlternatePosition;
        public Point ManipulConnDisplacingPos;
        public Point ManipulConnRelinkingPos;

        public EConnectorManipulationAction IntendedAction { get { return (EConnectorManipulationAction)this.IntendedAction_; } set { this.IntendedAction_ = (byte)value; } }
        public EConnectorManipulationAction TentativeAction { get { return (EConnectorManipulationAction)this.TentativeAction_; } set { this.TentativeAction_ = (byte)value; } }

        Pen FrmPencil = new Pen(Brushes.LightCyan, 0);
        Brush FrmStroke = Brushes.Yellow.Clone();
        Brush FrmStrokeEdit = Brushes.Goldenrod;
        Brush FrmStrokeUnpointed = Brushes.LightGray;

        Pen ActPencil = new Pen(Brushes.Blue, 1);
        Brush ActStroke = Brushes.Yellow.Clone();

        Pen IndPencil = new Pen(Brushes.Blue, 1);
        Brush IndStroke = Brushes.White;

        internal DrawingVisual RelinkActionTargetIndicator = null;
        internal DrawingVisual RelinkActionOriginIndicator = null;

        public DrawingVisual IndOriginPoint { get; protected set; }
        public DrawingVisual IndTargetPoint { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewConnectorManipulationAdorner(ViewManipulationManager OwnerManager, VisualConnector WorkingConnector,
                                                AdornerLayer WorkingLayer, Action<ViewManipulationAdorner, bool, bool, bool, bool, bool, bool, bool, bool> ManipulationOperation)
             : base(OwnerManager, WorkingConnector, WorkingLayer)
        {
            this.TentativeAction_ = (byte)EConnectorManipulationAction.Displace;
            this.DefaultAction_ = (byte)EConnectorManipulationAction.Displace;

            this.ManipulationOperation = ManipulationOperation;

            var OriginEdgePoint = WorkingConnector.OriginPosition.DetermineNearestIntersectingPoint(WorkingConnector.TargetPosition, OwnerManager.OwnerView.Presenter,
                                                                                                   WorkingConnector.OriginSymbol.Graphic, OwnerManager.OwnerView.VisualHitTestFilter);
            var TargetEdgePoint = WorkingConnector.TargetPosition.DetermineNearestIntersectingPoint(WorkingConnector.OriginPosition, OwnerManager.OwnerView.Presenter,
                                                                                                   WorkingConnector.TargetSymbol.Graphic, OwnerManager.OwnerView.VisualHitTestFilter);

            var ViewPosition = Mouse.GetPosition(this.OwnerManager.OwnerView.Presenter);
            var DistanceToOrigin = (ViewPosition - OriginEdgePoint).Length;
            var DistanceToTarget = (ViewPosition - TargetEdgePoint).Length;

            this.ManipulationAlternatePosition = (WorkingConnector.OriginSymbol == WorkingConnector.OwnerRelationshipRepresentation.MainSymbol
                                                  ? WorkingConnector.OriginPosition : WorkingConnector.TargetPosition);

            this.ManipulConnDisplacingPos = (this.ManipulatedConnector.IntermediatePosition == Display.NULL_POINT
                                                  ? (WorkingConnector.OriginSymbol == WorkingConnector.OwnerRelationshipRepresentation.MainSymbol
                                                     ? new Point((this.ManipulationAlternatePosition.X + TargetEdgePoint.X) / 2.0,
                                                                 (this.ManipulationAlternatePosition.Y + TargetEdgePoint.Y) / 2.0)
                                                     : new Point((OriginEdgePoint.X + this.ManipulationAlternatePosition.X) / 2.0,
                                                                 (OriginEdgePoint.Y + this.ManipulationAlternatePosition.Y) / 2.0))
                                                  : this.ManipulatedConnector.IntermediatePosition);
            //T Console.WriteLine("ManConnDisPos=" + this.ManipulConnDisplacingPos + "     at " + DateTime.Now.Ticks);

            if (DistanceToOrigin < DistanceToTarget)
            {
                this.WorkingOnOrigin = true;
                this.ManipulConnRelinkingPos = OriginEdgePoint;
            }
            else
            {
                this.WorkingOnOrigin = false;
                this.ManipulConnRelinkingPos = TargetEdgePoint;
            }

            this.ActStroke.Opacity = 0.1;
        }

        public bool WorkingOnOrigin { get; private set; }

        //------------------------------------------------------------------------------------------------------------------------
        public override void Visualize(bool Show = true, bool OnlyAdornAsSelected = false)
        {
            this.AlternateActions.Clear();
            this.ClearAllIndicators();
            //T Console.WriteLine("Visualizing 111 ..." + DateTime.Now.Ticks);

            // Validate that the Adorner still points something.
            // Else, maybe an "Undo" was performed, so the Represented-Idea may not exist anymore.
            if (this.ManipulatedConnector == null || this.ManipulatedConnector.OwnerRepresentation == null)
            {
                if (this.ManipulatedConnector != null)
                    this.OwnerManager.RemoveAdorners();

                this.OwnerManager.OwnerView.UnselectAllObjects();

                return;
            } 
            
            if (!Show)
                return;

            //T Console.WriteLine("Visualizing 222 ..." + DateTime.Now.Ticks);
            //T Console.WriteLine("Showing connector manipulation adorner.");

            var PaintBrush = FrmStroke;
            PaintBrush.Opacity = 0.5;

            var RelDef = this.ManipulatedConnector.OwnerRelationshipRepresentation.RepresentedRelationship.RelationshipDefinitor.Value;

            if (this.IntendedAction == EConnectorManipulationAction.ReLink)
            {
                var ConnFormat = RelDef.DefaultConnectorsFormat;

                Point PosTarget, PosOrigin;

                var ViewPosition = this.MousePositionCurrent = Mouse.GetPosition(this.OwnerManager.OwnerView.Presenter);

                if (!this.WorkingOnOrigin)
                {
                    PosTarget = this.ManipulConnRelinkingPos;
                    PosOrigin = this.ManipulatedConnector.FinalOriginPoint;
                }
                else
                {
                    PosTarget = this.ManipulatedConnector.FinalTargetPoint;
                    PosOrigin = this.ManipulConnRelinkingPos;
                }

                var RelinkingConnector = MasterDrawer.CreateDrawingConnector(Plugs.None, Plugs.SimpleArrow,
                                                                             ConnFormat.LineBrush, ConnFormat.LineThickness,
                                                                             ConnFormat.LineDash, ConnFormat.LineJoin,
                                                                             ConnFormat.LineCap, ConnFormat.PathStyle,
                                                                             ConnFormat.PathCorner, ConnFormat.MainBackground,
                                                                             ConnFormat.Opacity,
                                                                             PosTarget,
                                                                             PosOrigin).RenderToDrawingVisual();

                this.Indicators.Insert(0, RelinkingConnector);
                //T Console.WriteLine("Visualizing 333 ..." + DateTime.Now.Ticks);
                this.RefreshAdorner();
                return;
            }

            //T Console.WriteLine("Visualizing 444 ..." + DateTime.Now.Ticks);
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                this.CurrentManipulationAction = EConnectorManipulationAction.Remove;
            else
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                     && (this.ManipulatedConnector.IntermediatePosition != Display.NULL_POINT
                         || (RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple)))
                    this.CurrentManipulationAction = EConnectorManipulationAction.StraightenLine;
                else
                    if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                        /*? && this.ManipulatedConnector.RepresentedLink.RoleDefinitor.AllowedVariants.Count > 1 */)
                        this.CurrentManipulationAction = EConnectorManipulationAction.CycleThroughVariants;
                    else
                        this.CurrentManipulationAction = EConnectorManipulationAction.Displace;

            /* Remove: problematic in autoref? */
            var PosX = this.ManipulatedConnector.OriginPosition.X - INDICATOR_SIZE / 2.0;
            var PosY = this.ManipulatedConnector.OriginPosition.Y - INDICATOR_SIZE / 2.0;
            this.IndOriginPoint = (new GeometryDrawing(IndStroke, IndPencil, new RectangleGeometry(new Rect(PosX, PosY, INDICATOR_SIZE, INDICATOR_SIZE)))).RenderToDrawingVisual();
            Indicators.Add(this.IndOriginPoint);    // Origin must be first

            PosX = this.ManipulatedConnector.TargetPosition.X - INDICATOR_SIZE / 2.0;
            PosY = this.ManipulatedConnector.TargetPosition.Y - INDICATOR_SIZE / 2.0;
            this.IndTargetPoint = (new GeometryDrawing(IndStroke, IndPencil, new RectangleGeometry(new Rect(PosX, PosY, INDICATOR_SIZE, INDICATOR_SIZE)))).RenderToDrawingVisual();
            Indicators.Add(this.IndTargetPoint);    // Origin must be last

            // Determine whether exposition of adorner for a hidden relationship's Central/Main-Symbol (which may be reccently deleted) is needed
            if (this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol != null
                && this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol.IsHidden)
            {
                PosX = this.ManipulationAlternatePosition.X - ACTIONER_SIZE / 2.0;
                PosY = this.ManipulationAlternatePosition.Y - ACTIONER_SIZE / 2.0;
                var AltActionIndicator = CreateActioner(PosX, PosY, this.CurrentManipulationAction, false, Brushes.Red);
                this.AlternateActions.Add(AltActionIndicator);
                this.ManipulatedAlternateObject = this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol;

                this.Indicators.Add(AltActionIndicator);
                this.ExclusivePointingIndicators.Add(AltActionIndicator);
            }

            // Draw lines before other visuals
            var PencilYellow = new Pen(PaintBrush, MANIPULATING_CONNECTOR_WIDTH);
            PencilYellow.DashCap = PenLineCap.Round;
            PencilYellow.StartLineCap = PenLineCap.Round;
            PencilYellow.EndLineCap = PenLineCap.Round;

            // NOTE: Yellow and Red are always from origin to target, over the side where the mouse pointer is working on.
            var PencilRed = PencilYellow;   // new Pen(Brushes.Red, MANIPULATING_CONNECTOR_WIDTH);
            var PencilGreen = PencilYellow; // new Pen(Brushes.Green, MANIPULATING_CONNECTOR_WIDTH);
            var PencilBlue = PencilYellow;  // new Pen(Brushes.Blue, MANIPULATING_CONNECTOR_WIDTH);

            var StartPoint = (this.ManipulatedConnector.OriginSymbol == this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol
                              ? this.ManipulationAlternatePosition
                              : this.ManipulatedConnector.OriginPosition);

            var EndPoint = (this.ManipulatedConnector.OriginSymbol == this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol
                            ? this.ManipulatedConnector.TargetPosition
                            : this.ManipulationAlternatePosition);

            DrawingVisual IndConnectCurrentLine = null;

            var CommonPoint = this.ManipulConnDisplacingPos;

            // PENDING: Solve ungly misplaced indicator at the previous intermediate-position
            if (RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple
                && this.ManipulatedConnector.IntermediatePosition == Display.NULL_POINT
                && this.IsWorkingOnAlternateTarget)
                CommonPoint = new Point((StartPoint.X + EndPoint.X) / 2.0,
                                        (StartPoint.Y + EndPoint.Y) / 2.0);

            //T Console.WriteLine("Visualizing Compoint=" + CommonPoint.ToString() + ". NEW  At " + DateTime.Now.Ticks);

            this.DefaultActionIndicator = CreateActioner(CommonPoint.X - ACTIONER_SIZE / 2.0, CommonPoint.Y - ACTIONER_SIZE / 2.0,
                                                         this.CurrentManipulationAction, true, Brushes.Blue);
            this.Indicators.Add(this.DefaultActionIndicator);
            this.ExclusivePointingIndicators.Add(this.DefaultActionIndicator);

            /*T if (StartPoint == EndPoint)
                    Console.WriteLine("EqualPoints"); */

            IndConnectCurrentLine = (new GeometryDrawing(PaintBrush, PencilYellow, new LineGeometry(StartPoint, CommonPoint))).RenderToDrawingVisual();
            this.Indicators.Insert(0, IndConnectCurrentLine);

            IndConnectCurrentLine = (new GeometryDrawing(PaintBrush, PencilRed, new LineGeometry(CommonPoint, EndPoint))).RenderToDrawingVisual();
            this.Indicators.Insert(0, IndConnectCurrentLine);

            var Connectors = this.ManipulatedConnector.OwnerRelationshipRepresentation.VisualConnectors;

            // Indicators for re-linking
            if (this.CurrentManipulationAction != EConnectorManipulationAction.CycleThroughVariants
                && ((!(RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple)
                       && (RelDef.PreciseConnectByDefault || (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
                    || this.ManipulatedConnector.TargetSymbol != this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol))
            {
                this.RelinkActionTargetIndicator = CreateActioner(this.ManipulatedConnector.TargetEdgePosition.X - ACTIONER_SIZE / 2.0,
                                                                    this.ManipulatedConnector.TargetEdgePosition.Y - ACTIONER_SIZE / 2.0,
                                                                    EConnectorManipulationAction.ReLink,
                                                                    this.ManipulatedConnector.TargetSymbol == this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol,
                                                                    Brushes.Orange);
                this.Indicators.Add(this.RelinkActionTargetIndicator);
            }

            if (this.CurrentManipulationAction != EConnectorManipulationAction.CycleThroughVariants
                && ((!(RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple)
                       && (RelDef.PreciseConnectByDefault || (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
                    || this.ManipulatedConnector.OriginSymbol != this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol))
            {
                this.RelinkActionOriginIndicator = CreateActioner(this.ManipulatedConnector.OriginEdgePosition.X - ACTIONER_SIZE / 2.0,
                                                                    this.ManipulatedConnector.OriginEdgePosition.Y - ACTIONER_SIZE / 2.0,
                                                                    EConnectorManipulationAction.ReLink,
                                                                    this.ManipulatedConnector.OriginSymbol == this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol,
                                                                    Brushes.Green);
                this.Indicators.Add(this.RelinkActionOriginIndicator);
            }

            if (RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple
                && Connectors.Count() > 1)
            {
                var OppositeConnector = (this.ManipulatedConnector == Connectors.First() ? Connectors.Skip(1).First() : Connectors.First());

                StartPoint = (this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol == OppositeConnector.TargetSymbol
                                ? OppositeConnector.OriginIntermediateOrFinalPosition
                                : this.ManipulationAlternatePosition);

                EndPoint = (this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol == OppositeConnector.TargetSymbol
                            ? this.ManipulationAlternatePosition
                            : OppositeConnector.TargetIntermediateOrFinalPosition);

                var IndConnectOppositeLine = (new GeometryDrawing(PaintBrush, PencilGreen, new LineGeometry(StartPoint, EndPoint))).RenderToDrawingVisual();
                this.Indicators.Insert(0, IndConnectOppositeLine);
                this.AlternateActions.Add(IndConnectOppositeLine);

                Point OppositeCenter = OppositeConnector.IntermediatePosition;

                if (OppositeCenter == Display.NULL_POINT)
                {
                    StartPoint = (this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol == OppositeConnector.TargetSymbol
                                    ? OppositeConnector.OriginIntermediateOrFinalPosition
                                        .DetermineNearestIntersectingPoint(EndPoint, this.OwnerManager.OwnerView.Presenter,
                                                                           OppositeConnector.OriginSymbol.Graphic, this.OwnerManager.OwnerView.VisualHitTestFilter)
                                    : this.ManipulationAlternatePosition);

                    EndPoint = (this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol == OppositeConnector.TargetSymbol
                                ? this.ManipulationAlternatePosition
                                : OppositeConnector.TargetIntermediateOrFinalPosition
                                        .DetermineNearestIntersectingPoint(StartPoint, this.OwnerManager.OwnerView.Presenter,
                                                                           OppositeConnector.TargetSymbol.Graphic, this.OwnerManager.OwnerView.VisualHitTestFilter));

                    OppositeCenter = StartPoint.DetermineCenterRespect(EndPoint);
                }

                if (OppositeConnector.IntermediatePosition != Display.NULL_POINT)
                {
                    StartPoint = OppositeConnector.IntermediatePosition;
                    EndPoint = (this.ManipulatedConnector.OwnerRelationshipRepresentation.MainSymbol == OppositeConnector.TargetSymbol
                                ? OppositeConnector.OriginPosition : OppositeConnector.TargetPosition);

                    var IndConnectOppositeFarthestLine = (new GeometryDrawing(PaintBrush, PencilBlue, new LineGeometry(StartPoint, EndPoint))).RenderToDrawingVisual();
                    this.Indicators.Insert(0, IndConnectOppositeFarthestLine);
                    this.AlternateActions.Add(IndConnectOppositeFarthestLine);
                }

                OppositeCenter.X -= ACTIONER_SIZE / 2.0;
                OppositeCenter.Y -= ACTIONER_SIZE / 2.0;
                var OppositeActionIndicator = CreateActioner(OppositeCenter.X, OppositeCenter.Y, this.CurrentManipulationAction, true, Brushes.Violet);
                this.Indicators.Add(OppositeActionIndicator);
                this.ExclusivePointingIndicators.Add(OppositeActionIndicator);
            }

            //T Console.WriteLine("Visualizing 555 ..." + DateTime.Now.Ticks);

            // Needed in order to show this adorner's indicators on top of a potentially selected visual element
            this.RefreshAdorner();
        }

        public DrawingVisual CreateActioner(double PosX, double PosY, EConnectorManipulationAction Manipulation,
                                            bool ShowSimplified = false, Brush PenBrush = null)
        {
            ImageSource Source = null;

            if (Manipulation == EConnectorManipulationAction.ReLink)
                Source = Display.GetAppImage(ShowSimplified ? "actconn_repos.png" : "actconn_relink.png");
            else
                if (Manipulation == EConnectorManipulationAction.EditProperties)
                    Source = ImgSrcEditProperties ?? Display.GetAppImage("page_white_edit.png");
                else
                    if (Manipulation == EConnectorManipulationAction.Remove)
                        Source = Display.GetAppImage("actconn_delete.png");
                    else
                        if (Manipulation == EConnectorManipulationAction.StraightenLine)
                            Source = Display.GetAppImage("actconn_straighten.png");
                        else
                            if (Manipulation == EConnectorManipulationAction.CycleThroughVariants)
                                Source = Display.GetAppImage("actconn_cycle.png");
                            else
                                if (Manipulation == EConnectorManipulationAction.Displace)
                                    Source = Display.GetAppImage(ShowSimplified ? "actconn_displace_part.png" : "actconn_displace_main.png");

            if (Source == null)
                throw new InternalAnomaly("Actioner is not defined for manipulation-action.", Manipulation);

            var ContainerArea = new Rect(PosX, PosY, ACTIONER_SIZE, ACTIONER_SIZE);
            var ContentArea = new Rect(PosX + 2, PosY + 2, ACTIONER_SIZE - 4, ACTIONER_SIZE - 4);
            var Drawer = new DrawingGroup();

            /* T Drawer.Children.Add(new GeometryDrawing(ActStroke, (PenBrush == null ? ActPencil : new Pen(PenBrush, 2.0)),
                                                    new RectangleGeometry(ContainerArea, 2, 2))); */

            var Icon = new ImageDrawing(Source, ContentArea);
            var Pad = new GeometryDrawing(this.ActStroke, null,    // Helps to avoid selecting incorrect indicator
                                          new RectangleGeometry(Icon.Rect));
            Drawer.Children.Add(Pad);
            Drawer.Children.Add(Icon);
            Drawer.Opacity = 0.85;
            return Drawer.RenderToDrawingVisual();
        }

        public EConnectorManipulationAction CurrentManipulationAction { get; protected set; }

        //------------------------------------------------------------------------------------------------------------------------
        public override Visual DeterminePointedVisual(Point Position)
        {
            this.PreviousPosition = this.CurrentPosition;
            this.CurrentPosition = Position;

            if (this.CurrentPosition == this.PreviousPosition || this.IsManipulating)
                return this.CurrentPointedVisual;

            var NewPointed = GetPointedVisual(Position);

            if (NewPointed != this.CurrentPointedVisual)
            {
                this.CurrentPointedVisual = NewPointed;

                /* POSTPONED: Displace connecting points
                if (NewPointed.IsOneOf(IndOriginPoint, IndTargetPoint))
                {
                    this.TentativeAction = EConnectorManipulationAction.Displace;
                    this.Cursor = Cursors.Cross;
                } */

                if (NewPointed != null /* && !NewPointed.IsOneOf(IndOriginPoint, IndTargetPoint)*/ )
                {
                    if (NewPointed == this.DefaultActionIndicator || NewPointed.IsIn(AlternateActions))
                        this.TentativeAction = this.CurrentManipulationAction;
                    else
                        if (NewPointed == this.RelinkActionTargetIndicator || NewPointed == this.RelinkActionOriginIndicator)
                            this.TentativeAction = EConnectorManipulationAction.ReLink;

                    if (this.TentativeAction == EConnectorManipulationAction.Displace)
                        this.Cursor = Cursors.ScrollAll;
                    else
                        if (this.TentativeAction == EConnectorManipulationAction.ReLink)
                            this.Cursor = Cursors.Cross;
                        else
                            this.Cursor = Cursors.Hand;
                }

                var IndDescription = this.TentativeAction.GetDescription();

                if (this.TentativeAction == EConnectorManipulationAction.Displace)
                    IndDescription = IndDescription + " Double-click for Edit. " +
                    "Action icons: [Ctrl]=Straighten line, [Shift]=Delete connector, [Alt]=Cycle variant plugs.";

                ProductDirector.ShowAssistance(IndDescription);

                /* DANGER: This tooltip stops the adorner working
                var Tip = this.ToolTip as ToolTip;

                if (Tip == null || (Tip.Content as string).IsAbsent())
                {
                    Tip = (Tip == null ? new ToolTip() : Tip);
                    Tip.Content = IndDescription;
                    Tip.IsOpen = true;
                    Tip.StaysOpen = false;
                    this.ToolTip = Tip;
                } */
                //- }

                ProductDirector.ShowPointingTo(this.ManipulatedConnector);
            }

            return NewPointed;
        }

        //------------------------------------------------------------------------------------------------------------------------
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            this.OwnerManager.OwnerView.Engine.ShowContextMenu(this.OwnerManager.OwnerView.Presenter, this.ManipulatedConnector, this.OwnerManager.OwnerView);
        }

        //------------------------------------------------------------------------------------------------------------------------
    }
}