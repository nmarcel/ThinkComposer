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
// File   : ViewComplementManipulationAdorner.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ViewComplementManipulationAdorner (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.08.18 Néstor Sánchez A.  Creation
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
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Presents visual cues for manipulating view complements.
    /// </summary>
    public class ViewComplementManipulationAdorner : ViewManipulationAdorner
    {
        public const double ACTIONER_SIZE = 20;

        public const double EDITAREA_MIN_WIDTH = 32.0;
        public const double EDITAREA_MIN_HEIGHT = 16.0;

        public const double FRAME_BORDER_SIZE = 10.0;

        public VisualComplement ManipulatedComplement { get { return this.ManipulatedObject as VisualComplement; } }

        internal Rect ManipulatingHeadingRectangle;

        public EComplementManipulationAction IntendedAction { get { return (EComplementManipulationAction)this.IntendedAction_; } protected set { this.IntendedAction_ = (byte)value; } }
        public EComplementManipulationAction TentativeAction { get { return (EComplementManipulationAction)this.TentativeAction_; } protected set { this.TentativeAction_ = (byte)value; } }

        DrawingVisual FrmPointingHeadingPanel = null;
        DrawingVisual FrmEditZone = null;
        Dictionary<Visual, DetailDesignator> FrmDetailTitleDesignateZones = new Dictionary<Visual,DetailDesignator>();
        Dictionary<Visual, DetailDesignator> FrmDetailTitleExpandZones = new Dictionary<Visual, DetailDesignator>();
        Dictionary<Visual, DetailDesignator> FrmDetailContentAssignZones = new Dictionary<Visual, DetailDesignator>();
        Dictionary<Visual, DetailDesignator> FrmDetailContentEditZones = new Dictionary<Visual, DetailDesignator>();

        Pen FrmPencil = new Pen(Brushes.LightCyan, 0);
        Brush FrmStroke = Brushes.Yellow;
        Brush FrmStrokeEdit = Brushes.Goldenrod;

        double FrmOpacity = 0.2;
        double FrmOpacityEdit = 0.2;
        
        Pen ActPencil = new Pen(Brushes.Gold, 1);
        Brush ActStroke = Brushes.PaleGoldenrod;

        Pen IndPencil = new Pen(Brushes.Blue, 1);
        Brush IndStroke = Brushes.White;

        DrawingVisual IndHeadingTop = null;
        DrawingVisual IndHeadingBottom = null;
        DrawingVisual IndHeadingLeft = null;
        DrawingVisual IndHeadingRight = null;

        DrawingVisual IndHeadingTopLeft = null;
        DrawingVisual IndHeadingTopRight = null;
        DrawingVisual IndHeadingBottomLeft = null;
        DrawingVisual IndHeadingBottomRight = null;

        public EManipulationDirection ResizingDirection { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewComplementManipulationAdorner(ViewManipulationManager OwnerManager, VisualComplement TargetComplement, AdornerLayer TargetLayer,
                                                 Action<ViewManipulationAdorner, bool, bool, bool, bool, bool, bool, bool, bool> ManipulationOperation)
             : base(OwnerManager, TargetComplement, TargetLayer)
        {
            this.DefaultAction_ = (byte)EComplementManipulationAction.Edit;

            this.ManipulatingHeadingRectangle = TargetComplement.BaseArea;
            this.ManipulationOperation = ManipulationOperation;
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public override void Visualize(bool Show = true, bool OnlyAdornAsSelected = false)
        {
            // Validate that the Adorner still points something.
            // Else, maybe an "Undo" was performed, so the Represented-Idea may not exist anymore.
            if (this.ManipulatedComplement == null)
            {
                this.OwnerManager.RemoveAdorners();
                this.OwnerManager.OwnerView.UnselectAllObjects();

                return;
            } 

            // Reset previous drawn indicators
            this.ClearAllIndicators();

            if (!Show)
                return;

            // Determine pointing areas
            var PointingHeadingArea = new Rect(this.ManipulatingHeadingRectangle.X - VisualSymbol.INDICATOR_SIZE / 2.0, this.ManipulatingHeadingRectangle.Y - VisualSymbol.INDICATOR_SIZE / 2.0,
                                               this.ManipulatingHeadingRectangle.Width + VisualSymbol.INDICATOR_SIZE, this.ManipulatingHeadingRectangle.Height + VisualSymbol.INDICATOR_SIZE);

            // Start drawings creation
            var DrwFrmPointingHeadingPanel = new DrawingGroup();
            DrwFrmPointingHeadingPanel.Opacity = this.FrmOpacity;

            var FrameGeom = (this.ManipulatedComplement.Kind.TechName.IsOneOf(Domain.ComplementDefImage.TechName,
                                                                              Domain.ComplementDefGroupRegion.TechName,
                                                                              Domain.ComplementDefGroupLine.TechName)
                             && PointingHeadingArea.Width > (FRAME_BORDER_SIZE * 2) && PointingHeadingArea.Height > (FRAME_BORDER_SIZE * 2)
                             ? new CombinedGeometry(GeometryCombineMode.Exclude,
                                                    new RectangleGeometry(PointingHeadingArea),
                                                    new RectangleGeometry(new Rect(PointingHeadingArea.X + FRAME_BORDER_SIZE, PointingHeadingArea.Y + FRAME_BORDER_SIZE,
                                                                                   PointingHeadingArea.Width - (FRAME_BORDER_SIZE * 2),
                                                                                   PointingHeadingArea.Height - (FRAME_BORDER_SIZE * 2)))) as Geometry
                             : new RectangleGeometry(PointingHeadingArea) as Geometry);
            DrwFrmPointingHeadingPanel.Children.Add(new GeometryDrawing(this.FrmStroke, this.FrmPencil, FrameGeom));
            this.FrmPointingHeadingPanel = DrwFrmPointingHeadingPanel.RenderToDrawingVisual();
            this.Indicators.Add(this.FrmPointingHeadingPanel);

            if (!OnlyAdornAsSelected && this.ManipulatingHeadingRectangle.Width >= EDITAREA_MIN_WIDTH
                                     && this.ManipulatingHeadingRectangle.Height >= EDITAREA_MIN_HEIGHT)
                if (this.ManipulatedComplement.Kind.TechName.IsOneOf(Domain.ComplementDefImage.TechName,
                                                                     Domain.ComplementDefGroupRegion.TechName,
                                                                     Domain.ComplementDefGroupLine.TechName))
                    this.FrmEditZone = null;
                else
                {
                    var DrwFrmEditPanel = new DrawingGroup();
                    DrwFrmEditPanel.Opacity = this.FrmOpacityEdit;

                    var EditingArea = new Rect(this.ManipulatingHeadingRectangle.X + this.ManipulatingHeadingRectangle.Width * 0.25,
                                               this.ManipulatingHeadingRectangle.Y + this.ManipulatingHeadingRectangle.Height * 0.25,
                                               this.ManipulatingHeadingRectangle.Width * 0.5,
                                               this.ManipulatingHeadingRectangle.Height * 0.5);

                    DrwFrmEditPanel.Children.Add(new GeometryDrawing(FrmStrokeEdit, FrmPencil, new RectangleGeometry(EditingArea)));
                    this.FrmEditZone = DrwFrmEditPanel.RenderToDrawingVisual();
                    this.Indicators.Add(this.FrmEditZone);
                    this.ExclusivePointingIndicators.Add(this.FrmEditZone);
                }

            var SelectionIndicators = this.ManipulatedComplement.GenerateSelectionIndicators(VisualSymbol.INDICATOR_SIZE,
                                                                                             (this.ManipulatedComplement.IsSelected
                                                                                             ? VisualElement.SelectionIndicatorBackground
                                                                                             : IndStroke),
                                                                                             IndPencil,
                                                                                             (this.ManipulatedComplement.IsSelected
                                                                                             ? VisualElement.SelectionIndicatorGeometryCreator
                                                                                             : null),
                                                                                             (this.ManipulatedComplement.IsComplementGroupLine
                                                                                              ? new Nullable<Orientation>(this.ManipulatedComplement.GetPropertyField<Orientation>(VisualComplement.PROP_FIELD_ORIENTATION))
                                                                                              : null));

            foreach (var SelInd in SelectionIndicators)
            {
                var IndVis = SelInd.Item1.RenderToDrawingVisual();
                this.Indicators.Add(IndVis);

                if (SelInd.Item2 == EManipulationDirection.Top) IndHeadingTop = IndVis;
                if (SelInd.Item2 == EManipulationDirection.Bottom) IndHeadingBottom = IndVis;
                if (SelInd.Item2 == EManipulationDirection.Left) IndHeadingLeft = IndVis;
                if (SelInd.Item2 == EManipulationDirection.Right) IndHeadingRight = IndVis;

                if (SelInd.Item2 == EManipulationDirection.TopLeft) IndHeadingTopLeft = IndVis;
                if (SelInd.Item2 == EManipulationDirection.TopRight) IndHeadingTopRight = IndVis;
                if (SelInd.Item2 == EManipulationDirection.BottomLeft) IndHeadingBottomLeft = IndVis;
                if (SelInd.Item2 == EManipulationDirection.BottomRight) IndHeadingBottomRight = IndVis;
            }

            // Needed in order to show this adorner's indicators on top of a potentially selected visual element
            this.RefreshAdorner();
        }

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
                bool IsPointingToIndicator = true;

                if (NewPointed.IsOneOf(IndHeadingTop, IndHeadingBottom))
                {
                    this.ResizingDirection = (NewPointed == IndHeadingTop ? EManipulationDirection.Top : EManipulationDirection.Bottom);
                    this.TentativeAction = EComplementManipulationAction.Resize;
                    this.Cursor = Cursors.SizeNS;
                }
                else
                    if (NewPointed.IsOneOf(IndHeadingLeft, IndHeadingRight))
                    {
                        this.ResizingDirection = (NewPointed == IndHeadingLeft ? EManipulationDirection.Left : EManipulationDirection.Right);
                        this.TentativeAction = EComplementManipulationAction.Resize;
                        this.Cursor = Cursors.SizeWE;
                    }
                    else
                        if (NewPointed.IsOneOf(IndHeadingTopLeft, IndHeadingBottomRight))
                        {
                            this.ResizingDirection = (NewPointed == IndHeadingTopLeft ? EManipulationDirection.TopLeft : EManipulationDirection.BottomRight);
                            this.TentativeAction = EComplementManipulationAction.Resize;
                            this.Cursor = Cursors.SizeNWSE;
                        }
                        else
                            if (NewPointed.IsOneOf(IndHeadingBottomLeft, IndHeadingTopRight))
                            {
                                this.ResizingDirection = (NewPointed == IndHeadingTopRight ? EManipulationDirection.TopRight : EManipulationDirection.BottomLeft);
                                this.TentativeAction = EComplementManipulationAction.Resize;
                                this.Cursor = Cursors.SizeNESW;
                            }
                            else
                                if (NewPointed == FrmPointingHeadingPanel)
                                {
                                    this.TentativeAction = EComplementManipulationAction.Move;
                                    this.Cursor = Cursors.ScrollAll;
                                }
                                else
                                    if (NewPointed == FrmEditZone || NewPointed == null)
                                    {
                                        this.TentativeAction = EComplementManipulationAction.Edit;
                                        this.Cursor = Cursors.Pen;
                                    }
                                    else
                                    {
                                        this.Cursor = Cursors.Hand;

                                        if (NewPointed == DefaultActionIndicator)
                                            this.TentativeAction = EComplementManipulationAction.Edit;
                                        else
                                            IsPointingToIndicator = false;
                                    }

                if (IsPointingToIndicator)
                {
                    var IndDescription = this.TentativeAction.GetDescription();
                    ProductDirector.ShowAssistance(IndDescription);
                }
                else
                    ProductDirector.ShowAssistance();

                ProductDirector.ShowPointingTo(this.ManipulatedComplement);
            }

            return NewPointed;
        }

        //------------------------------------------------------------------------------------------------------------------------
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            this.OwnerManager.OwnerView.Engine.ShowContextMenu(this.OwnerManager.OwnerView.Presenter, this.ManipulatedComplement, this.OwnerManager.OwnerView);
        }

        //------------------------------------------------------------------------------------------------------------------------
    }
}