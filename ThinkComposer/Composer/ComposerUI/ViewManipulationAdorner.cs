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
// File   : ViewManipulationAdorner.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ViewManipulationAdorner (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 20yy.mm.dd Néstor Sánchez A.  Creation
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

using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Base ancestor adorner for manipulating symbol and connectors.
    /// </summary>
    public abstract class ViewManipulationAdorner : Adorner
    {
        protected static ImageSource ImgSrcEditProperties = null;

        public ViewManipulationManager OwnerManager { get; protected set; }
        public VisualObject ManipulatedObject { get; protected set; }
        public VisualObject ManipulatedAlternateObject { get; protected set; }
        public AdornerLayer BaseLayer { get; protected set; }

        internal VisualCollection Indicators = null;
        protected List<Visual> ExclusivePointingIndicators = null;

        protected Action<ViewManipulationAdorner, bool, bool, bool, bool, bool, bool, bool, bool> ManipulationOperation = null;

        protected byte IntendedAction_;
        protected byte TentativeAction_;
        protected byte DefaultAction_;

        internal DrawingVisual DefaultActionIndicator = null;
        internal List<DrawingVisual> AlternateActions = new List<DrawingVisual>();
        internal bool IsWorkingOnAlternateTarget = false;

        protected FrameworkElement ContextControl = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewManipulationAdorner(ViewManipulationManager OwnerManager, VisualObject TargetObject, AdornerLayer TargetLayer)
             : base(OwnerManager.OwnerView.PresenterControl)
        {
            this.IsHitTestVisible = true;

            this.OwnerManager = OwnerManager;
            this.ManipulatedObject = TargetObject;
            this.BaseLayer = TargetLayer;

            this.Indicators = new VisualCollection(this);
            this.ExclusivePointingIndicators = new List<Visual>();

            this.ContextControl = this.OwnerManager.OwnerView.PresenterControl;
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public bool IsManipulating
        {
            get { return this.IsManipulating_; }
            set
            {
                this.IsManipulating_ = value;
                this.OwnerManager.IsManipulating = value;

                if (value)
                    this.OwnerManager.WorkingAdorner = this;
            }
        }
        protected bool IsManipulating_ = false;

        // -----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refreshes, by removing and adding, the adorner in its adorner layer.
        /// </summary>
        public void RefreshAdorner()
        {
            this.BaseLayer.Remove(this);
            this.BaseLayer.Add(this);
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public abstract void Visualize(bool Show = true, bool OnlyAdornAsSelected = false);

        //------------------------------------------------------------------------------------------------------------------------
        public void Continue()
        {
            OnMouseMove(new MouseEventArgs(Mouse.PrimaryDevice, 0));
        }

        public void Stop()
        {
            OnMouseLeftButtonUp(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public void ClearAllIndicators()
        {
            ClearExclusivePointingIndicators();

            this.Indicators.Clear();
            this.InvalidateVisual();
        }

        public void ClearExclusivePointingIndicators()
        {
            foreach (var Indicator in this.ExclusivePointingIndicators)
                this.Indicators.Remove(Indicator);

            this.ExclusivePointingIndicators.Clear();
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public Visual GetPointedVisual(Point Position)
        {
            var HitResult = VisualTreeHelper.HitTest(this.BaseLayer, Position);
            if (HitResult == null)
                return null;

            return HitResult.VisualHit as Visual;
        }

        //------------------------------------------------------------------------------------------------------------------------
        public abstract Visual DeterminePointedVisual(Point Position);

        /// <summary>
        /// Indicates to use the grid settings (snapping and size).
        /// Use this property temporarily and only for definitive visual element changes (not for indicators).
        /// </summary>
        public bool IsUsingGridSettings { get; set; }

        public Point CurrentPosition
        {
            get { return (this.IsUsingGridSettings && this.OwnerManager.OwnerView.SnapToGrid
                          ? this.OwnerManager.OwnerView.GetGridSnappedPosition(this.CurrentPosition_, true)
                          : this.CurrentPosition_); }
            set { this.CurrentPosition_ = value; }
        }
        private Point CurrentPosition_;

        public Point PreviousPosition
        {
            get
            {
                return this.IsUsingGridSettings && this.OwnerManager.OwnerView.SnapToGrid
                          ? this.OwnerManager.OwnerView.GetGridSnappedPosition(this.PreviousPosition_, true)
                          : this.PreviousPosition_; }
            protected set { this.PreviousPosition_ = value; }
        }
        private Point PreviousPosition_;

        public Visual CurrentPointedVisual { get; protected set; }

        // Override the VisualChildrenCount and GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount { get { return Indicators.Count; } }

        protected override Visual GetVisualChild(int index)
        {
            return Indicators[index];
        }

        //------------------------------------------------------------------------------------------------------------------------
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (this.IsManipulating)
            {
                if (e.MouseDevice.LeftButton == MouseButtonState.Released)
                    OnMouseLeftButtonUp(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
            }
        }

        public Point PointedLocationWhileClicking = Display.NULL_POINT;

        Visual PointedVisualWhileClicking = null;

        public bool CancelledByMouseLeftButtonDownOutsideControl = false;

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.PointedLocationWhileClicking = Display.NULL_POINT;
            this.PointedVisualWhileClicking = null;

            // If call Stop(), then avoid the activation of the In-Line textbox editor.
            // Stop();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.MousePositionCurrent = e.GetPosition(this.BaseLayer);
            this.IsAbleToExtendRelationship = false;

            // NOTE: This button-down action is NOT considered a Click (is just the beginning of it)
            this.IsManipulating = false;

            this.PointedLocationWhileClicking = e.GetPosition(this.BaseLayer);
            this.PointedVisualWhileClicking = DeterminePointedVisual(this.PointedLocationWhileClicking);
            this.IntendedAction_ = this.TentativeAction_;
            this.IsManipulating = true;

            // Start Relationship extension if moving
            if (this.ManipulatedObject is VisualSymbol)
            {
                var RepRelation = ((VisualSymbol)this.ManipulatedObject).OwnerRepresentation as RelationshipVisualRepresentation;
                if (RepRelation != null && (!RepRelation.RepresentedRelationship.RelationshipDefinitor.Value.IsSimple
                                            || RepRelation.RepresentedRelationship.Links.Count <= 1))
                    this.IsAbleToExtendRelationship = true;
            }
            
            // Console.WriteLine("ClickCount = {0}.", e.ClickCount);
            this.MouseLeftButtonDoubleClicked = (e.ClickCount > 1);

            // Ugly trick to support fast generation of adorner visuals while interacting
            this.AlternateActions.ForEach(altact => this.RegisteredAlternateActionVisuals.AddNew(altact));
            this.IsWorkingOnAlternateTarget = (this.PointedVisualWhileClicking.IsIn(this.RegisteredAlternateActionVisuals));
        }

        private bool MouseLeftButtonDoubleClicked = false;

        private bool IsAbleToExtendRelationship = false;

        private ConstrainedArray<Visual> RegisteredAlternateActionVisuals = new ConstrainedArray<Visual>(30);

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.IsAbleToExtendRelationship = false;

            // NOTE: This button-up action is considered a Click only if pointing
            // to the same visual and within a range from the same pointing location.
            
            var Location = e.GetPosition(this.BaseLayer);
            var Pointed = DeterminePointedVisual(Location);

            if (!this.CancelledByMouseLeftButtonDownOutsideControl)
                if (Pointed == this.PointedVisualWhileClicking && Location.IsNear(this.PointedLocationWhileClicking))
                    this.ManipulationOperation(this, true, true, false, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                                                                        Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                                                                        Keyboard.IsKeyDown(Key.LeftAlt),
                                                                        Keyboard.IsKeyDown(Key.RightAlt),
                                                                        this.MouseLeftButtonDoubleClicked);
                else
                    this.ManipulationOperation(this, true, false, false, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                                                                         Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                                                                         Keyboard.IsKeyDown(Key.LeftAlt),
                                                                         Keyboard.IsKeyDown(Key.RightAlt),
                                                                         this.MouseLeftButtonDoubleClicked);

            this.CancelledByMouseLeftButtonDownOutsideControl = false;
            
            this.Visualize();

            this.PointedLocationWhileClicking = Display.NULL_POINT;
            this.PointedVisualWhileClicking = null;

            this.IntendedAction_ = DefaultAction_;
            this.IsManipulating = false;
            this.IsWorkingOnAlternateTarget = false;

            this.MouseLeftButtonDoubleClicked = false;
        }

        public Point MousePositionCurrent { get; protected set; }
        public Point MousePositionPrevious { get; protected set; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.MousePositionPrevious = this.MousePositionCurrent;
            this.MousePositionCurrent = e.GetPosition(this.BaseLayer);

            DeterminePointedVisual(this.MousePositionCurrent);

            // Extend Relationship if moving instead of clicking for Edit In-Place
            if ((this.OwnerManager.OwnerView.Engine.RunningMouseCommand == null
                 || this.OwnerManager.OwnerView.Engine.RunningMouseCommand is RelationshipCreationCommand)
                &&  this.IsAbleToExtendRelationship && !this.MousePositionCurrent.IsNear(this.MousePositionPrevious)
                && (ESymbolManipulationAction)this.IntendedAction_ == ESymbolManipulationAction.EditInPlace)
            {
                var TargetRelationRep = (this.ManipulatedObject is VisualSymbol
                                         ? ((VisualSymbol)this.ManipulatedObject).OwnerRepresentation as RelationshipVisualRepresentation
                                         : null);

                if (TargetRelationRep != null)
                {
                    var Engine = this.OwnerManager.OwnerView.Engine;
                    Engine.RunningMouseCommand =
                        new RelationshipCreationCommand(Engine,
                                                        TargetRelationRep.RepresentedRelationship.RelationshipDefinitor.Value);
                    Engine.RunningMouseCommand.Execute();
                    Engine.RunningMouseCommand.Continue(e, true);
                }

                this.IsAbleToExtendRelationship = false;
            }

            if (this.IsManipulating)
            {
                this.ManipulationOperation(this, Mouse.LeftButton == MouseButtonState.Released, false, false,
                                           Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                                           Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                                           Keyboard.IsKeyDown(Key.LeftAlt),
                                           Keyboard.IsKeyDown(Key.RightAlt),
                                           false);
                this.Visualize();
            }
        }

        //------------------------------------------------------------------------------------------------------------------------
    }
}