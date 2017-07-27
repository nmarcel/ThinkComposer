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
// File   : ViewPresenter.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.ViewPresenter (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.Composer;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Display visual children related to the owner View.
    /// </summary>
    public class ViewPresenter : FrameworkElement
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal ViewPresenter(View OwnerView)
        {
            General.ContractRequiresNotNull(OwnerView);

            this.OwnerView = OwnerView;

            this.OwnerView.ViewChildren.CollectionChanged += ViewChildren_CollectionChanged;

            this.Clip = new RectangleGeometry(new Rect(this.OwnerView.ViewSize));
            this.ClipToBounds = true;

            this.AllowDrop = true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// View owning this presenter.
        /// </summary>
        public View OwnerView { get; protected set; }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the current count of visual children.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return this.OwnerView.ViewChildren.Count;
            }
        }

        /// <summary>
        /// Gets the visual child associated to the supplied index.
        /// </summary>
        protected override Visual GetVisualChild(int Index)
        {
            var Container = this.OwnerView.ViewChildren[Index];

            if (Container == null)
                return null;

            return Container.Content;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Measures the size in layout required for child elements and determines a size for this FrameworkElement-derived class.
        /// </summary>
        protected override Size MeasureOverride(Size AvailableSize)
        {
            /* Not needed when a background sheet exits...
             * 
            double LeftMostPos = double.MaxValue;
            double RightMostPos = double.MinValue;
            double TopMostPos = double.MaxValue;
            double BottomMostPos = double.MinValue;

            foreach (var Child in this.OwnerView.ViewChildren)
            {
                if (Child.Key == null)
                    continue;

                var Box = Child.Content.ContentBounds;
                Box.Union(Child.Content.DescendantBounds);

                LeftMostPos = Math.Min(LeftMostPos, Box.Left);
                RightMostPos = Math.Max(RightMostPos, Box.Right);
                TopMostPos = Math.Min(TopMostPos, Box.Top);
                BottomMostPos = Math.Max(BottomMostPos, Box.Bottom);
            }

            this.MeasuredSize = new Size((RightMostPos - LeftMostPos) + 1.0, (BottomMostPos - TopMostPos) + 1.0); */

            if (this.MeasuredSize == default(Size))
                this.MeasuredSize = this.OwnerView.ViewChildren.First(reg => reg.Key == this.OwnerView.BackgroundSheet).Content.ContentBounds.Size;

            return this.MeasuredSize;
        }

        /// <summary>
        /// Last computed size used by all the child visual elements.
        /// </summary>
        public Size MeasuredSize { get; protected set; }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Positions child elements and determines a size for this FrameworkElement derived class.
        /// </summary>
        protected override Size ArrangeOverride(Size FinalSize)
        {
            return base.ArrangeOverride(FinalSize);
        }

        // -------------------------------------------------------------------------------------------
        public int TransientObjectsCount { get; protected set; }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the supplied visual object exists in this presenter.
        /// </summary>
        public bool HasVisualObject(VisualObject Target)
        {
            return this.OwnerView.ViewChildren.Any(reg => reg.Key.IsEqual(Target));
        }

        /// <summary>
        /// Returns the registered visual-container which represents the supplied target, or null if not found.
        /// </summary>
        public ContainerVisual GetVisualContainerOf(object Target)
        {
            var Result = this.OwnerView.ViewChildren.FirstOrDefault(reg => reg.Key == Target);
            if (Result == null)
                return null;

            return Result.Content;
        }

        /// <summary>
        /// Returns the registered visual object whose graphic is the supplied target, or null if not found.
        /// </summary>
        public VisualObject GetVisualObjectFor(Visual Target)
        {
            var Result = this.OwnerView.ViewChildren.FirstOrDefault(reg => reg.Content == Target);
            if (Result == null)
                return null;

            return Result.Key as VisualObject;
        }

        /// <summary>
        /// Adds the supplied depiction to the displayed view to the top of the specified initial-level (first time).
        /// </summary>
        public void PutVisual(VisualObject Depiction, EVisualLevel InitialLevel = EVisualLevel.Elements)
        {
            if (InitialLevel == EVisualLevel.Transients)
                throw new UsageAnomaly("Transient visuals cannot be displayed via PutVisual().");

            // IMPORTANT: This is required in order to do successful undos and redos.
            if (this.OwnerView.IsEditingActive && !this.OwnerView.EditEngine.IsVariating)
                // Console.WriteLine("Warning: Add-visual should be applied within a Command");
                throw new UsageAnomaly("Put-visual must be applied within a Command");

            // Remove adorners, if any.
            if (this.ExposedAdorners.ContainsKey(Depiction))
            {
                if (this.OwnerView.Manipulator.ExposedAdornerLayer != null)
                    this.OwnerView.Manipulator.ExposedAdornerLayer.Remove(this.ExposedAdorners[Depiction]);

                this.ExposedAdorners.Remove(Depiction);
            }

            var Index = this.OwnerView.ViewChildren.IndexOfMatch(reg => reg.Key.IsEqual(Depiction));

            // Refresh graphic
            Depiction.GenerateGraphic(this, true);

            // Add, Insert or Update
            var VisReg = ViewChild.Create(Depiction, Depiction.Graphic);

            if (Index < 0)
            {
                // Start ready for Elements
                var Limit = (this.OwnerView.ViewChildren.Count - (this.OwnerView.VisualCountOfFloatings + this.TransientObjectsCount)) - 1;

                if (InitialLevel == EVisualLevel.Floatings)
                {
                    this.OwnerView.VisualCountOfFloatings++;
                    Limit = (this.OwnerView.ViewChildren.Count - this.TransientObjectsCount);
                }
                else
                    if (InitialLevel <= EVisualLevel.Regions)
                    {
                        this.OwnerView.VisualLevelForRegions++;
                        Limit = this.OwnerView.VisualLevelForRegions;

                        if (InitialLevel <= EVisualLevel.Background)
                        {
                            this.OwnerView.VisualLevelForBackground++;
                            Limit = this.OwnerView.VisualLevelForBackground;
                        }
                    }
                    else
                        if (InitialLevel > EVisualLevel.Regions)
                            Limit = this.OwnerView.VisualLevelForRegions + 1;
                        else
                            if (InitialLevel > EVisualLevel.Background)
                                Limit = this.OwnerView.VisualLevelForBackground + 1;

                if (Limit <= this.OwnerView.ViewChildren.Count - 1)
                {
                    this.OwnerView.ViewChildren.Add(null);
                    for (int ShiftIndex = this.OwnerView.ViewChildren.Count - 1; ShiftIndex > Limit; ShiftIndex--)
                    {
                        var TempShifted = this.OwnerView.ViewChildren[ShiftIndex - 1];
                        this.OwnerView.ViewChildren[ShiftIndex - 1] = null;
                        this.OwnerView.ViewChildren[ShiftIndex] = ViewChild.Create(TempShifted);
                    }

                    this.OwnerView.ViewChildren[Limit] = VisReg;
                }
                else
                {
                    this.OwnerView.ViewChildren.Add(VisReg);
                    Index = this.OwnerView.ViewChildren.Count - 1;
                }
            }
            else
            {
                /*T var Previous = this.OwnerView.ViewChildren[Index];
                if (Previous.IsEqual(VisReg) || Previous == VisReg)
                    Console.WriteLine("VP Equal:{0}, ==:{1}", Previous.IsEqual(VisReg),  Previous == VisReg); */

                this.OwnerView.ViewChildren[Index] = VisReg;
            }

            // Add adorners, if any.
            if (this.OwnerView.RegisteredAdorners.ContainsKey(Depiction))
            {
                var Adorner = this.OwnerView.RegisteredAdorners[Depiction];

                if (this.OwnerView.Manipulator.ExposedAdornerLayer != null)
                {
                    /*+? var Preexistent = this.OwnerView.Manipulator.ExposedAdornerLayer.GetAdorners(Adorner.AdornedElement)
                                                .NullDefault(Enumerable.Empty<Adorner>()).Contains(Adorner);
                    if (Preexistent)
                        Console.WriteLine("Preexistent");
                    else */
                        this.OwnerView.Manipulator.ExposedAdornerLayer.Add(Adorner);
                }

                this.ExposedAdorners.Add(Depiction, Adorner);
            }

            // Set as related-visible
            Depiction.IsRelatedVisible = true;
        }

        // Intended for transient visual objects (such as the selection-rectangle)
        public void PutTransientVisual(string Key, ContainerVisual Graphic)
        {
            var Index = this.OwnerView.ViewChildren.IndexOfMatch(child => child.Key.IsEqual(Key));
            if (Index >= 0)
                this.OwnerView.ViewChildren[Index] = ViewChild.Create(Key, Graphic);
            else
            {
                this.OwnerView.ViewChildren.Add(ViewChild.Create(Key, Graphic));
                this.TransientObjectsCount++;
            }
        }

        // Intended for Model related visual objects
        public void ClearVisual(VisualObject Depiction)
        {
            // IMPORTANT: This is required in order to do successful undos and redos.
            if (this.OwnerView.IsEditingActive && !this.OwnerView.EditEngine.IsVariating)
                // Console.WriteLine("Warning: Remove-visual should be applied within a Command");
                throw new UsageAnomaly("Remove-visual must be applied within a Command");

            var Index = this.OwnerView.ViewChildren.IndexOfMatch(child => child.Key.IsEqual(Depiction));
            if (Index < 0)
                return;

            // Remove adorners
            if (this.ExposedAdorners.ContainsKey(Depiction))
            {
                if (this.OwnerView.Manipulator.ExposedAdornerLayer != null)
                    this.OwnerView.Manipulator.ExposedAdornerLayer.Remove(this.ExposedAdorners[Depiction]);

                this.ExposedAdorners.Remove(Depiction);
            }

            // Update levels
            var Level = this.OwnerView.GetLevelOf(Depiction);
            if (Level != null && Level.HasValue)
                if (Level.Value == EVisualLevel.Floatings)
                    this.OwnerView.VisualCountOfFloatings--;
                else
                    if (Level.Value <= EVisualLevel.Regions)
                    {
                        this.OwnerView.VisualLevelForRegions--;

                        if (Level.Value <= EVisualLevel.Background)
                            this.OwnerView.VisualLevelForBackground--;
                    }

            // Delete visual
            this.OwnerView.ViewChildren.RemoveAt(Index);
            Depiction.IsRelatedVisible = false;

        }

        // Intended for transient visual objects (such as the selection-rectangle)
        public void ClearTransientVisual(string Key)
        {
            var Index = this.OwnerView.ViewChildren.IndexOfMatch(child => child.Key.IsEqual(Key));
            if (Index < 0)
                return;

            this.OwnerView.ViewChildren.RemoveAt(Index);
            this.TransientObjectsCount--;
        }

        /// <summary>
        /// Stores, per visual object, a currently exposed visual adorner.
        /// </summary>
        public readonly Dictionary<VisualObject, Adorner> ExposedAdorners = new Dictionary<VisualObject, Adorner>();

        //T bool Detecting = false;
        void ViewChildren_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            /*T
            var Nulls = this.OwnerView.ViewChildren.Count(child => child == null);

            if (Nulls > 1)
                Console.WriteLine("Too much null view children.");

            if (e.Action == NotifyCollectionChangedAction.Replace)
                foreach (var Added in e.NewItems)
                {
                    var VisPrim = (ViewChild)Added;

                    if (VisPrim == null)
                    {
                        if (Detecting)
                            Console.WriteLine("Left null");

                        Detecting = true;

                        //T Console.WriteLine("Presenter.ReplacingWithNothing");
                        continue;
                    }
                    Detecting = false;

                    if (VisPrim.Content == null)
                        Console.WriteLine("Presenter.ReplacingWithContent: {0}", VisPrim.Content);
                } */

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
                foreach (var Removed in e.OldItems)
                {
                    var VisPrim = (ViewChild)Removed;
                    //T Console.WriteLine("Presenter.Removing: {0}", VisPrim);
                    if (VisPrim == null)    // This can happen in shifting operations
                        continue;

                    this.RemoveVisualChild(VisPrim.Content);
                    this.RemoveLogicalChild(VisPrim.Key);
                }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
                foreach (var Added in e.NewItems)
                {
                    var VisPrim = (ViewChild)Added;
                    //T Console.WriteLine("Presenter.Adding: {0}", VisPrim);
                    if (VisPrim == null)    // This can happen in shifting operations
                        continue;

                    this.AddLogicalChild(VisPrim.Key);
                    this.AddVisualChild(VisPrim.Content);
                }

            if (e.Action == NotifyCollectionChangedAction.Reset)
                throw new UsageAnomaly("Cannot reset the View-Children collection.");
        }

        // -------------------------------------------------------------------------------------------

        /// <summary>
        /// Support dropping of file in the canvas.
        /// </summary>
        // IMPORATANT: This does not work when executed via VisualStudio (run stand-alone without Admin privileges).
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            Clipboard.SetDataObject(e.Data, false);
            this.OwnerView.Engine.ClipboardPaste(this.OwnerView, false, CompositionEngine.CurrentMousePosition);
        }

        // -------------------------------------------------------------------------------------------
    }
}