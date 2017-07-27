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
// File   : ViewSymbolManipulationAdorner.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ViewSymbolManipulationAdorner (Class)
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
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Presents visual cues for manipulating view symbol elements.
    /// </summary>
    public class ViewSymbolManipulationAdorner : ViewManipulationAdorner
    {
        static ImageSource ImgSrcSwitchContent = null;
        static ImageSource ImgSrcSwitchRelated = null;
        static ImageSource ImgSrcEditFormat = null;

        public const double ACTIONER_SIZE = 20;

        public const double EDITAREA_MIN_WIDTH = 32.0;
        public const double EDITAREA_MIN_HEIGHT = 16.0;

        public VisualSymbol ManipulatedSymbol { get { return this.ManipulatedObject as VisualSymbol; } }

        internal Rect ManipulatingHeadingRectangle;
        internal Rect ManipulatingHeadingLabel;
        internal Rect ManipulatingDetailsRectangle;
        internal bool IsManipulatingHeading = true;

        public ESymbolManipulationAction IntendedAction { get { return (ESymbolManipulationAction)this.IntendedAction_; } protected set { this.IntendedAction_ = (byte)value; } }
        public ESymbolManipulationAction TentativeAction { get { return (ESymbolManipulationAction)this.TentativeAction_; } protected set { this.TentativeAction_ = (byte)value; } }

        DrawingVisual FrmPointingHeadingPanel = null;
        DrawingVisual FrmPointingDetailsPanel = null;
        DrawingVisual FrmEditZone = null;
        Dictionary<Visual, MarkerAssignment> FrmMarkingZones = new Dictionary<Visual, MarkerAssignment>();
        Dictionary<Visual, DetailDesignator> FrmDetailTitleDesignateZones = new Dictionary<Visual, DetailDesignator>();
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

        DrawingVisual IndDetailsBottom = null;
        DrawingVisual IndDetailsLeft = null;
        DrawingVisual IndDetailsRight = null;
        DrawingVisual IndDetailsBottomLeft = null;
        DrawingVisual IndDetailsBottomRight = null;

        DrawingVisual ActShowComposite = null;
        DrawingVisual ActSwitchRelated = null;
        DrawingVisual ActSwitchDetails = null;
        DrawingVisual ActSwitchCompositeView = null;
        DrawingVisual ActAddDetail = null;

        public EManipulationDirection ResizingDirection { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewSymbolManipulationAdorner(ViewManipulationManager OwnerManager, VisualSymbol TargetSymbol, AdornerLayer TargetLayer,
                                             Action<ViewManipulationAdorner, bool, bool, bool, bool, bool, bool, bool, bool> ManipulationOperation)
             : base(OwnerManager, TargetSymbol, TargetLayer)
        {
            this.DefaultAction_ = (byte)ESymbolManipulationAction.EditInPlace;

            this.ManipulatingHeadingRectangle = TargetSymbol.BaseArea;
            this.ManipulatingHeadingLabel = TargetSymbol.LabelingArea;

            this.ManipulatingDetailsRectangle = TargetSymbol.DetailsArea;
            this.ManipulationOperation = ManipulationOperation;
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public override void Visualize(bool Show = true, bool OnlyAdornAsSelected = false)
        {
            // Validate that the Adorner still points something.
            // Else, maybe an "Undo" was performed, so the Represented-Idea may not exist anymore.
            if (this.ManipulatedSymbol == null || this.ManipulatedSymbol.OwnerRepresentation == null)
            {
                if (this.ManipulatedSymbol != null)
                    this.OwnerManager.RemoveAdorners();

                this.OwnerManager.OwnerView.UnselectAllObjects();

                return;
            }

            var WorkingHeadingRectangle = this.ManipulatingHeadingRectangle;

            if (this.ManipulatedSymbol.IsHidden)
                if (this.ManipulatedSymbol.CanShowNameOverCenter
                    || this.ManipulatedSymbol.CanShowDefinitionOverTop)
                    WorkingHeadingRectangle = this.ManipulatingHeadingLabel;
                else
                    return;     // Because this is "hidden" (or shown too little to be noticed by user).

            // Reset previous drawn indicators
            this.ClearAllIndicators();

            if (!Show)
                return;

            // Show every other selected objects
            VisualizeOtherAffectedObjects();

            // Determine pointing areas
            var PointingHeadingArea = new Rect(WorkingHeadingRectangle.X - VisualSymbol.INDICATOR_SIZE / 2.0, WorkingHeadingRectangle.Y - VisualSymbol.INDICATOR_SIZE / 2.0,
                                               WorkingHeadingRectangle.Width + VisualSymbol.INDICATOR_SIZE, WorkingHeadingRectangle.Height + VisualSymbol.INDICATOR_SIZE);

            var PointingDetailsArea = new Rect(this.ManipulatingDetailsRectangle.X - VisualSymbol.INDICATOR_SIZE / 2.0, this.ManipulatingDetailsRectangle.Y - VisualSymbol.INDICATOR_SIZE / 2.0,
                                               this.ManipulatingDetailsRectangle.Width + VisualSymbol.INDICATOR_SIZE, this.ManipulatingDetailsRectangle.Height + VisualSymbol.INDICATOR_SIZE);

            /*T var PointingHeadingArea = WorkingHeadingRectangle;
            var PointingDetailsArea = this.ManipulatingDetailsRectangle; */

            // Start drawings creation
            var DrwFrmPointingHeadingPanel = new DrawingGroup();
            DrwFrmPointingHeadingPanel.Opacity = this.FrmOpacity;

            DrwFrmPointingHeadingPanel.Children.Add(new GeometryDrawing(this.FrmStroke, this.FrmPencil, new RectangleGeometry(PointingHeadingArea)));

            //-? if (this.ManipulatedSymbol.CanShowDefinitionOverTop || this.ManipulatedSymbol.CanShowNameOverCenter)
            // DrwFrmPointingHeadingPanel.Children.Add(new GeometryDrawing(this.FrmStroke, this.FrmPencil, new RectangleGeometry(this.ManipulatedSymbol.LabelingArea)));

            this.FrmPointingHeadingPanel = DrwFrmPointingHeadingPanel.RenderToDrawingVisual();
            this.Indicators.Add(this.FrmPointingHeadingPanel);

            // IMPORTANT: The Details related indicators MUST BE CREATED in order to not be mismatched with null in later evaluations
            var DrwFrmPointingDetailsPanel = new DrawingGroup();
            DrwFrmPointingDetailsPanel.Opacity = DrwFrmPointingHeadingPanel.Opacity;

            DrwFrmPointingDetailsPanel.Children.Add(new GeometryDrawing(this.FrmStroke, this.FrmPencil, new RectangleGeometry(PointingDetailsArea)));
            this.FrmPointingDetailsPanel = DrwFrmPointingDetailsPanel.RenderToDrawingVisual();

            if (this.ManipulatedSymbol.AreDetailsShown)
                this.Indicators.Add(this.FrmPointingDetailsPanel);

            if (!OnlyAdornAsSelected && WorkingHeadingRectangle.Width >= EDITAREA_MIN_WIDTH
                                     && WorkingHeadingRectangle.Height >= EDITAREA_MIN_HEIGHT)
            {
                var DrwFrmEditPanel = new DrawingGroup();
                DrwFrmEditPanel.Opacity = this.FrmOpacityEdit;

                var EditingArea = new Rect(WorkingHeadingRectangle.X + WorkingHeadingRectangle.Width * 0.25,
                                            WorkingHeadingRectangle.Y + WorkingHeadingRectangle.Height * 0.25,
                                            WorkingHeadingRectangle.Width * 0.5,
                                            WorkingHeadingRectangle.Height * 0.5);

                DrwFrmEditPanel.Children.Add(new GeometryDrawing(FrmStrokeEdit, FrmPencil, new RectangleGeometry(EditingArea)));
                this.FrmEditZone = DrwFrmEditPanel.RenderToDrawingVisual();
                this.Indicators.Add(this.FrmEditZone);
                this.ExclusivePointingIndicators.Add(this.FrmEditZone);
            }

            if (!this.ManipulatedSymbol.IsHidden)
            {
                CreateActioners(OnlyAdornAsSelected, PointingHeadingArea, WorkingHeadingRectangle);

                CreateSelectionIndicators();
            }

            // Needed in order to show this adorner's indicators on top of a potentially selected visual element
            this.RefreshAdorner();
        }

        private void VisualizeOtherAffectedObjects()
        {
            var IncludeOriginatedSubtree = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
            var IncludeTargetedSubtree = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            var OtherAffectedObjects = this.OwnerManager.OwnerView.GetCurrentManipulableObjects(IncludeOriginatedSubtree, IncludeTargetedSubtree, true)
                                                .Select(obj => obj.Item1).Distinct()   // Notice that duplicates are informed, so they must be excluded
                                                .Except(this.ManipulatedObject.IntoEnumerable());   // Exclude current object to avoid visual interference in resizing

            foreach (var AffectedObject in OtherAffectedObjects)
            {
                var DeltaX = 0.0;
                var DeltaY = 0.0;
                var DeltaWidth = 0.0;
                var DeltaHeight = 0.0;

                // Do not consider other intention, particularly resizing (due to its implicit move)
                if (this.IntendedAction == ESymbolManipulationAction.Move)
                {
                    DeltaX = this.ManipulatingHeadingRectangle.X - this.ManipulatedSymbol.BaseArea.X;
                    DeltaY = this.ManipulatingHeadingRectangle.Y - this.ManipulatedSymbol.BaseArea.Y;
                }

                // Only the manipulated symbol show a change in size
                // (because multi-resize is postponed due to difficulty)
                if (this.ManipulatedSymbol == AffectedObject)
                {
                    DeltaWidth = this.ManipulatingHeadingRectangle.Width - this.ManipulatedSymbol.BaseArea.Width;
                    DeltaHeight = this.ManipulatingHeadingRectangle.Height - this.ManipulatedSymbol.BaseArea.Height;
                }

                var SelectionZone = (AffectedObject is VisualSymbol && ((VisualSymbol)AffectedObject).IsHidden
                                     ? new Rect(AffectedObject.BaseCenter.X - 4, AffectedObject.BaseCenter.Y - 4,
                                                8, 8)
                                     : new Rect(AffectedObject.BaseLeft + DeltaX,
                                                AffectedObject.BaseTop + DeltaY,
                                                (AffectedObject.BaseWidth + DeltaWidth).EnforceMinimum(4),
                                                (AffectedObject.TotalArea.Height + DeltaHeight).EnforceMinimum(4)));
                var SelectionGeom = (SelectionZone.Width > 8 && SelectionZone.Height > 8
                                     ? new CombinedGeometry(GeometryCombineMode.Exclude,
                                                            new RectangleGeometry(SelectionZone),
                                                            new RectangleGeometry(new Rect(SelectionZone.X + 4, SelectionZone.Y + 4,
                                                                                           (SelectionZone.Width - 8).EnforceMinimum(4),
                                                                                           (SelectionZone.Height - 8).EnforceMinimum(4))))
                                     : (new RectangleGeometry(SelectionZone)) as Geometry);
                var SelectionDrawing = new GeometryDrawing(this.FrmStroke, this.FrmPencil, SelectionGeom);
                var SelectionVisual = SelectionDrawing.RenderToDrawingVisual();
                SelectionVisual.Opacity = (AffectedObject.IsIn(this.OwnerManager.OwnerView.SelectedObjects)
                                           ? this.FrmOpacity : this.FrmOpacity / 2.0);
                this.Indicators.Add(SelectionVisual);
            }
        }

        private void CreateActioners(bool OnlyAdornAsSelected, Rect PointingHeadingArea, Rect WorkingHeadingRectangle)
        {
            double PosX = 0;
            double PosY = 0;

            var CanShowActioners = !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                                     Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ||
                                     Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));

            if (!OnlyAdornAsSelected)
            {
                this.FrmMarkingZones.Clear();

                var ZoneBrush = Brushes.Transparent;                // FrmStrokeEdit (too dark)
                var ZonePen = new Pen(Brushes.Transparent, 1.0);    // FrmPencil

                // Editing Areas of individual shown Details
                var ManipulatedShape = this.ManipulatedSymbol as VisualShape;

                if (ManipulatedShape != null && ManipulatedShape.CurrentMarkingZones != null)
                    CreateMarkingZones(ZoneBrush, ZonePen, ManipulatedShape);

                if (this.ManipulatedSymbol.AreDetailsShown
                    && WorkingHeadingRectangle.Width >= ACTIONER_SIZE
                    && WorkingHeadingRectangle.Height >= ACTIONER_SIZE)
                {
                    this.FrmDetailTitleDesignateZones.Clear();
                    this.FrmDetailTitleExpandZones.Clear();
                    this.FrmDetailContentAssignZones.Clear();
                    this.FrmDetailContentEditZones.Clear();

                    if (ManipulatedShape != null && ManipulatedShape.CurrentDetailZones != null
                        && ManipulatedShape.CurrentDetailZones.Count > 0)
                        CreateDetailZones(ZoneBrush, ZonePen, ManipulatedShape);

                    if (CanShowActioners
                        && this.ManipulatingDetailsRectangle.Width >= ACTIONER_SIZE * 4.2
                        && this.ManipulatingDetailsRectangle.Height >= ACTIONER_SIZE * 1.2)
                    {
                        // Details Bottom-Left
                        PosX = (PointingHeadingArea.Left + (PointingHeadingArea.Width / 5.0));
                        PosY = this.ManipulatingDetailsRectangle.Y + this.ManipulatingDetailsRectangle.Height - ACTIONER_SIZE / 2.0;

                        if (this.ManipulatedSymbol.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.IsComposable)
                        {
                            this.ActSwitchCompositeView = CreateActionerFor(PosX, PosY, ESymbolManipulationAction.ActionShowCompositeAsDetail);
                            this.Indicators.Add(this.ActSwitchCompositeView);
                            this.ExclusivePointingIndicators.Add(this.ActSwitchCompositeView);
                        }

                        // Details Bottom-Right
                        PosX = (PointingHeadingArea.Left + (PointingHeadingArea.Width / 5.0) * 4.0) - ACTIONER_SIZE;
                        this.ActAddDetail = CreateActionerFor(PosX, PosY, ESymbolManipulationAction.ActionAddDetail);
                        this.Indicators.Add(this.ActAddDetail);
                        this.ExclusivePointingIndicators.Add(this.ActAddDetail);
                    }
                }

                if (CanShowActioners
                    && WorkingHeadingRectangle.Width >= ACTIONER_SIZE * 4.2
                    && WorkingHeadingRectangle.Height >= ACTIONER_SIZE * 1.2)
                {
                    // Top-Left
                    PosX = (PointingHeadingArea.Left + (PointingHeadingArea.Width / 5.0));
                    PosY = PointingHeadingArea.Top - ACTIONER_SIZE / 2.0;

                    this.DefaultActionIndicator = CreateActionerFor(PosX, PosY, ESymbolManipulationAction.ActionEditProperties);
                    this.Indicators.Add(this.DefaultActionIndicator);
                    this.ExclusivePointingIndicators.Add(this.DefaultActionIndicator);

                    // Bottom-Left
                    PosY = PointingHeadingArea.Top + PointingHeadingArea.Height - ACTIONER_SIZE / 2.0;
                    this.ActShowComposite = CreateActionerFor(PosX, PosY, ESymbolManipulationAction.ActionShowCompositeAsView);

                    if (this.ManipulatedSymbol.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.IsComposable)
                    {
                        this.Indicators.Add(this.ActShowComposite);
                        this.ExclusivePointingIndicators.Add(this.ActShowComposite);
                    }

                    // Top-Right
                    PosX = (PointingHeadingArea.Left + (PointingHeadingArea.Width / 5.0) * 4.0) - ACTIONER_SIZE;
                    PosY = PointingHeadingArea.Top - ACTIONER_SIZE / 2.0;
                    this.ActSwitchRelated = CreateActionerFor(PosX, PosY, ESymbolManipulationAction.ActionSwitchRelated);

                    this.Indicators.Add(this.ActSwitchRelated);
                    this.ExclusivePointingIndicators.Add(this.ActSwitchRelated);

                    // Bottom-Right
                    PosY = PointingHeadingArea.Top + PointingHeadingArea.Height - ACTIONER_SIZE / 2.0;
                    this.ActSwitchDetails = CreateActionerFor(PosX, PosY, ESymbolManipulationAction.ActionSwitchDetails);

                    this.Indicators.Add(this.ActSwitchDetails);
                    this.ExclusivePointingIndicators.Add(this.ActSwitchDetails);
                }
            }
        }

        private void CreateSelectionIndicators()
        {
            var SelectionIndicators =
                    this.ManipulatedSymbol.GenerateSelectionIndicators(VisualSymbol.INDICATOR_SIZE,
                                                                        (this.ManipulatedSymbol.OwnerRepresentation.IsSelected
                                                                         ? VisualElement.SelectionIndicatorBackground
                                                                         : IndStroke),
                                                                        IndPencil,
                                                                        (this.ManipulatedSymbol.OwnerRepresentation.IsSelected
                                                                         ? VisualElement.SelectionIndicatorGeometryCreator
                                                                         : null));

            foreach (var SelInd in SelectionIndicators)
            {
                var IndVis = SelInd.Item1.RenderToDrawingVisual();
                this.Indicators.Add(IndVis);

                if (SelInd.Item2)   // Is for Symbol Header...
                {
                    if (SelInd.Item3 == EManipulationDirection.Top) IndHeadingTop = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.Bottom) IndHeadingBottom = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.Left) IndHeadingLeft = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.Right) IndHeadingRight = IndVis;

                    if (SelInd.Item3 == EManipulationDirection.TopLeft) IndHeadingTopLeft = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.TopRight) IndHeadingTopRight = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.BottomLeft) IndHeadingBottomLeft = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.BottomRight) IndHeadingBottomRight = IndVis;
                }
                else    // Is for Symbol Detail...
                {
                    if (SelInd.Item3 == EManipulationDirection.Bottom) IndDetailsBottom = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.Left) IndDetailsLeft = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.Right) IndDetailsRight = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.BottomLeft) IndDetailsBottomLeft = IndVis;
                    if (SelInd.Item3 == EManipulationDirection.BottomRight) IndDetailsBottomRight = IndVis;
                }
            }
        }

        private void CreateDetailZones(SolidColorBrush ZoneBrush, Pen ZonePen, VisualShape ManipulatedShape)
        {
            foreach (var DetailZone in ManipulatedShape.CurrentDetailZones)
                if (DetailZone.Value == null)
                    break;
                else
                {
                    // Title-Designate zone
                    if (DetailZone.Value.Item1 != null && DetailZone.Value.Item1 != Rect.Empty)
                    {
                        var DetailTitleNamingZone = (new GeometryDrawing(ZoneBrush, ZonePen, new RectangleGeometry(DetailZone.Value.Item1))).RenderToDrawingVisual();
                        DetailTitleNamingZone.Opacity = this.FrmOpacityEdit;

                        var EditZone = (new GeometryDrawing(FrmStrokeEdit, FrmPencil, new RectangleGeometry(DetailZone.Value.Item1))).RenderToDrawingVisual();
                        EditZone.Opacity = this.FrmOpacityEdit;
                        this.Indicators.Add(EditZone);

                        this.FrmDetailTitleDesignateZones.Add(DetailTitleNamingZone, DetailZone.Key);
                        this.Indicators.Add(DetailTitleNamingZone);
                    }

                    // Title-Expand zone
                    if (DetailZone.Value.Item2 != null && DetailZone.Value.Item2 != Rect.Empty)
                    {
                        var DetailTitleExpandZone = (new GeometryDrawing(ZoneBrush, ZonePen, new RectangleGeometry(DetailZone.Value.Item2))).RenderToDrawingVisual();
                        DetailTitleExpandZone.Opacity = this.FrmOpacityEdit;

                        this.FrmDetailTitleExpandZones.Add(DetailTitleExpandZone, DetailZone.Key);
                        this.Indicators.Add(DetailTitleExpandZone);
                    }

                    // Content-Edit zone
                    // IMPORTANT: Put this edit-zone before the assign-zone to be on the background of it.
                    if (DetailZone.Value.Item4 != null && DetailZone.Value.Item4 != Rect.Empty)
                    {
                        var DetailContentEditZone = (new GeometryDrawing(ZoneBrush, ZonePen, new RectangleGeometry(DetailZone.Value.Item4))).RenderToDrawingVisual();
                        DetailContentEditZone.Opacity = this.FrmOpacityEdit;

                        this.FrmDetailContentEditZones.Add(DetailContentEditZone, DetailZone.Key);
                        this.Indicators.Add(DetailContentEditZone);
                    }

                    // Content-Assign zone
                    // IMPORTANT: Put this assign-zone after the edit-zone to be on the foreground of it.
                    if (DetailZone.Value.Item3 != null && DetailZone.Value.Item3 != Rect.Empty)
                    {
                        var DetailContentAssignZone = (new GeometryDrawing(ZoneBrush, ZonePen, new RectangleGeometry(DetailZone.Value.Item3))).RenderToDrawingVisual();
                        DetailContentAssignZone.Opacity = this.FrmOpacityEdit;

                        var EditZone = (new GeometryDrawing(FrmStrokeEdit, FrmPencil, new RectangleGeometry(DetailZone.Value.Item3))).RenderToDrawingVisual();
                        EditZone.Opacity = this.FrmOpacityEdit;
                        this.Indicators.Add(EditZone);

                        this.FrmDetailContentAssignZones.Add(DetailContentAssignZone, DetailZone.Key);
                        this.Indicators.Add(DetailContentAssignZone);
                    }
                }
        }

        private void CreateMarkingZones(SolidColorBrush ZoneBrush, Pen ZonePen, VisualShape ManipulatedShape)
        {
            foreach (var MarkingZone in ManipulatedShape.CurrentMarkingZones)
            {
                var IconZone = (new GeometryDrawing(ZoneBrush, ZonePen, new RectangleGeometry(MarkingZone.Item2))).RenderToDrawingVisual();
                IconZone.Opacity = this.FrmOpacityEdit;

                this.Indicators.Add(IconZone);
                this.FrmMarkingZones.Add(IconZone, MarkingZone.Item1);
            }
        }

        public DrawingVisual CreateActionerFor(double PosX, double PosY, ESymbolManipulationAction Manipulation)
        {
            ImageSource Source = null;

            if (Manipulation == ESymbolManipulationAction.ActionShowCompositeAsView)
                Source = ImgSrcSwitchContent ?? Display.GetAppImage("show_composite.png");

            if (Manipulation == ESymbolManipulationAction.ActionEditProperties)
                Source = ImgSrcEditProperties ?? Display.GetAppImage("page_white_edit.png");

            if (Manipulation == ESymbolManipulationAction.ActionSwitchRelated)
                /*if (this.ManipulatedElement.OwnerRepresentation.AreRelatedTargetsShown
                    || this.ManipulatedElement.OwnerRepresentation.AreRelatedOriginsShown)
                    Source = ImgSrcSwitchRelated ?? Display.GetAppImage("related_collapsed.png");
                else*/
                Source = ImgSrcSwitchRelated ?? Display.GetAppImage("related_expanded.png");

            if (Manipulation == ESymbolManipulationAction.ActionSwitchDetails)
                if (this.ManipulatedSymbol.AreDetailsShown)
                    Source = ImgSrcEditFormat ?? Display.GetAppImage("details_close.png");
                else
                    Source = ImgSrcEditFormat ?? Display.GetAppImage("details_open.png");

            if (Manipulation == ESymbolManipulationAction.ActionShowCompositeAsDetail)
                if (this.ManipulatedSymbol.ShowCompositeContentAsDetails)
                    Source = ImgSrcEditProperties ?? Display.GetAppImage("detail_view.png");
                else
                    Source = ImgSrcEditProperties ?? Display.GetAppImage("composite_view.png");

            if (Manipulation == ESymbolManipulationAction.ActionAddDetail)
                Source = ImgSrcEditProperties ?? Display.GetAppImage("detail_new.png");

            if (Source == null)
                throw new InternalAnomaly("Actioner is not defined for manipulation-action.", Manipulation);

            var ContainerArea = new Rect(PosX, PosY, ACTIONER_SIZE, ACTIONER_SIZE);
            var ContentArea = new Rect(PosX + 2, PosY + 2, ACTIONER_SIZE - 4, ACTIONER_SIZE - 4);
            var Drawer = new DrawingGroup();
            Drawer.Children.Add(new GeometryDrawing(ActStroke, ActPencil, new RectangleGeometry(ContainerArea, 2, 2)));
            Drawer.Children.Add(new ImageDrawing(Source, ContentArea));
            Drawer.Opacity = 1.0;
            return Drawer.RenderToDrawingVisual();
        }

        public MarkerAssignment LastPointedMarkerAssignment { get; protected set; }
        public DetailDesignator LastPointedDetailDesignator { get; protected set; }

        //------------------------------------------------------------------------------------------------------------------------
        public override Visual DeterminePointedVisual(Point Position)
        {
            this.PreviousPosition = this.CurrentPosition;
            this.CurrentPosition = Position;

            if (this.CurrentPosition == this.PreviousPosition || this.IsManipulating)
                return this.CurrentPointedVisual;
            
            var NewPointed = GetPointedVisual(Position);
            this.IsManipulatingHeading = !NewPointed.IsOneOf(FrmPointingDetailsPanel, IndDetailsBottom, IndDetailsBottomLeft, IndDetailsBottomRight, IndDetailsLeft, IndDetailsRight);

            if (NewPointed != this.CurrentPointedVisual)
            {
                this.CurrentPointedVisual = NewPointed;
                bool IsPointingToIndicator = true;

                if (NewPointed.IsOneOf(IndHeadingTop, IndHeadingBottom, IndDetailsBottom))
                {
                    this.ResizingDirection = (NewPointed == IndHeadingTop ? EManipulationDirection.Top : EManipulationDirection.Bottom);
                    this.TentativeAction = ESymbolManipulationAction.Resize;
                    this.Cursor = Cursors.SizeNS;
                }
                else
                    if (NewPointed.IsOneOf(IndHeadingLeft, IndHeadingRight, IndDetailsLeft, IndDetailsRight))
                    {
                        this.ResizingDirection = (NewPointed.IsOneOf(IndHeadingLeft, IndDetailsLeft) ? EManipulationDirection.Left : EManipulationDirection.Right);
                        this.TentativeAction = ESymbolManipulationAction.Resize;
                        this.Cursor = Cursors.SizeWE;
                    }
                    else
                        if (NewPointed.IsOneOf(IndHeadingTopLeft, IndHeadingBottomRight, IndDetailsBottomRight))
                        {
                            this.ResizingDirection = (NewPointed == IndHeadingTopLeft ? EManipulationDirection.TopLeft : EManipulationDirection.BottomRight);
                            this.TentativeAction = ESymbolManipulationAction.Resize;
                            this.Cursor = Cursors.SizeNWSE;
                        }
                        else
                            if (NewPointed.IsOneOf(IndHeadingBottomLeft, IndHeadingTopRight, IndDetailsBottomLeft))
                            {
                                this.ResizingDirection = (NewPointed == IndHeadingTopRight ? EManipulationDirection.TopRight : EManipulationDirection.BottomLeft);
                                this.TentativeAction = ESymbolManipulationAction.Resize;
                                this.Cursor = Cursors.SizeNESW;
                            }
                            else
                                if (NewPointed.IsOneOf(FrmPointingHeadingPanel, FrmPointingDetailsPanel))
                                {
                                    this.TentativeAction = ESymbolManipulationAction.Move;
                                    this.Cursor = Cursors.ScrollAll;
                                }
                                else
                                    if (NewPointed == FrmEditZone || NewPointed == null)
                                    {
                                        if (this.ManipulatedSymbol.OwnerRepresentation.IsShortcut)
                                        {
                                            this.TentativeAction = ESymbolManipulationAction.GoToShortcutTarget;
                                            this.Cursor = Cursors.UpArrow;
                                        }
                                        else
                                        {
                                            // Do not Edit In-Place if multiselecting...
                                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                                                Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                                            {
                                                this.TentativeAction = ESymbolManipulationAction.Move;
                                                this.Cursor = Cursors.ScrollAll;
                                            }
                                            else
                                            {
                                                this.TentativeAction = ESymbolManipulationAction.EditInPlace;
                                                this.Cursor = Cursors.Pen;
                                            }
                                        }
                                    }
                                    else
                                        if (NewPointed.IsIn(this.FrmMarkingZones.Keys))
                                        {
                                            this.LastPointedMarkerAssignment = this.FrmMarkingZones[NewPointed];
                                            this.TentativeAction = ESymbolManipulationAction.MarkerEdit;
                                            this.Cursor = Cursors.Pen;
                                        }
                                        else
                                            if (NewPointed.IsIn(this.FrmDetailContentAssignZones.Keys))
                                            {
                                                this.LastPointedDetailDesignator = this.FrmDetailContentAssignZones[NewPointed];
                                                this.TentativeAction = ESymbolManipulationAction.IndividualDetailChange;
                                                this.Cursor = Cursors.Hand;
                                            }
                                            else
                                                if (NewPointed.IsIn(this.FrmDetailContentEditZones.Keys))
                                                {
                                                    this.LastPointedDetailDesignator = this.FrmDetailContentEditZones[NewPointed];
                                                    this.TentativeAction = ESymbolManipulationAction.IndividualDetailAccess;
                                                    this.Cursor = Cursors.Pen;
                                                }
                                                else
                                                    if (NewPointed.IsIn(this.FrmDetailTitleExpandZones.Keys))
                                                    {
                                                        this.LastPointedDetailDesignator = this.FrmDetailTitleExpandZones[NewPointed];
                                                        this.TentativeAction = ESymbolManipulationAction.SwitchIndividualDetail;
                                                        this.Cursor = Cursors.UpArrow;
                                                    }
                                                    else
                                                        if (NewPointed.IsIn(this.FrmDetailTitleDesignateZones.Keys))
                                                        {
                                                            this.LastPointedDetailDesignator = this.FrmDetailTitleDesignateZones[NewPointed];
                                                            this.TentativeAction = ESymbolManipulationAction.IndividualDetailDesignation;
                                                            this.Cursor = Cursors.Arrow;
                                                        }
                                                        else
                                                        {
                                                            this.Cursor = Cursors.Hand;

                                                            if (NewPointed == ActSwitchDetails)
                                                                this.TentativeAction = ESymbolManipulationAction.ActionSwitchDetails;
                                                            else
                                                                if (NewPointed == ActSwitchRelated)
                                                                    this.TentativeAction = ESymbolManipulationAction.ActionSwitchRelated;
                                                                else
                                                                    if (NewPointed == ActShowComposite)
                                                                        this.TentativeAction = ESymbolManipulationAction.ActionShowCompositeAsView;
                                                                    else
                                                                        if (NewPointed == this.DefaultActionIndicator)
                                                                            this.TentativeAction = ESymbolManipulationAction.ActionEditProperties;
                                                                        else
                                                                            if (NewPointed == ActSwitchCompositeView)
                                                                                this.TentativeAction = ESymbolManipulationAction.ActionShowCompositeAsDetail;
                                                                            else
                                                                                if (NewPointed == ActAddDetail)
                                                                                    this.TentativeAction = ESymbolManipulationAction.ActionAddDetail;
                                                                                else
                                                                                    IsPointingToIndicator = false;
                                                        }

                if (IsPointingToIndicator)
                {
                    var IndDescription = this.TentativeAction.GetDescription();

                    if (this.TentativeAction == ESymbolManipulationAction.EditInPlace
                        && this.ManipulatedSymbol.OwnerRepresentation is RelationshipVisualRepresentation)
                    {
                        var RepRel = ((RelationshipVisualRepresentation)this.ManipulatedSymbol.OwnerRepresentation)
                                        .RepresentedRelationship;

                        if (!RepRel.RelationshipDefinitor.Value.IsSimple || RepRel.Links.Count <= 1)
                            IndDescription = IndDescription + " Drag to extend the pointed Relationship with a new Link/Connector.";
                    }

                    ProductDirector.ShowAssistance(IndDescription);

                    /* Problem: This tooltip stops the adorner working
                    var Tip = this.ToolTip as ToolTip;

                    if (Tip == null || (Tip.Content as string).IsAbsent())
                    {
                        Tip = (Tip == null ? new ToolTip() : Tip);
                        this.ToolTip = Tip;

                        Tip.Content = IndDescription;
                        Tip.IsOpen = true;
                        Tip.StaysOpen = false;
                    } */
                }
                else
                    ProductDirector.ShowAssistance();

                ProductDirector.ShowPointingTo(this.ManipulatedSymbol);
            }

            return NewPointed;
        }

        //------------------------------------------------------------------------------------------------------------------------
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            this.OwnerManager.OwnerView.Engine.ShowContextMenu(this.OwnerManager.OwnerView.Presenter, this.ManipulatedSymbol, this.OwnerManager.OwnerView);
        }

        //------------------------------------------------------------------------------------------------------------------------
    }
}