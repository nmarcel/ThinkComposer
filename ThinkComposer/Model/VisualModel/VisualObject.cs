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
// File   : VisualObject.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualObject (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Elementary visual object and ancestor for others most sophisticaded.
    /// </summary>
    [Serializable]
    public abstract class VisualObject : UniqueElement, IModelEntity, IModelClass<VisualObject>
    {
        public const double INDICATOR_SIZE = 5;

        public static readonly Brush SelectionIndicatorBackground = Brushes.Black; // Brushes.Black;
        public static readonly Pen SelectionIndicatorForeground = new Pen(Brushes.DimGray, 1.0);   // new Pen(Brushes.Gainsboro, 1.0);
        public static readonly Func<Rect, Geometry> SelectionIndicatorGeometryCreator = ((rect) => new EllipseGeometry(rect));

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualObject()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualObject>("VisualObject", UniqueElement.__ClassDefinitor, "Visual Object",
                                                                     "Elementary visual object and ancestor for others most sophisticated.");

            __ClassDefinitor.DeclareProperty(__Graphic);
            __ClassDefinitor.DeclareProperty(__IsRelatedVisible);

            __IsRelatedVisible.ChangesExistenceStatus = false;

        }

        /// <summary>
        /// Standard post-interceptor for update view.
        /// </summary>
        public static bool UpdateViewPropertyPostInterceptor(IMModelClass Instance, MModelPropertyDefinitor PropDefinitor, object Value)
        {
            var Source = Instance as VisualObject;
            if (Source != null)
                Source.GetDisplayingView().Show(Source);

            return true;    // Indicates to continue with the property change
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualObject()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the View visually containing this visual object.
        /// </summary>
        public abstract View GetDisplayingView();

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Recreates and returns the Graphic of this visual object descendant and its children, for a presentation context.
        /// (The presentation context is optional. Descendants should use the View presenter control when absent.)
        /// </summary>
        public abstract ContainerVisual GenerateGraphic(UIElement PresentationContext, bool ShowManipulationAdorners);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Actual drawing to be shown. Can be either vector based or complex, such as an image.
        /// </summary>
        public ContainerVisual Graphic { get { return this.Graphic_; } set { this.Graphic_ = value; } }
        [NonSerialized]
        protected ContainerVisual Graphic_;
        public static readonly ModelPropertyDefinitor<VisualObject, ContainerVisual> __Graphic =
                   new ModelPropertyDefinitor<VisualObject, ContainerVisual>("Graphic", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Graphic_, (ins, val) => ins.Graphic_ = val, false, false,
                                                                                "Graphic", "Actual drawing to be shown. Can be either vector based or complex, such as an image.");

        /// <summary>
        /// Indicates wether this visual object is selected for later applying a command.
        /// </summary>
        public abstract bool IsSelected { get; set; }

        /// <summary>
        /// Indicates wether this visual object is vanished as a result of been marked for deletion.
        /// </summary>
        public abstract bool IsVanished { get; set; }

        /// <summary>
        /// Renders the represented semantic object.
        /// </summary>
        public abstract void RenderRepresentatedObject();

        // -----------------------------------------------------------------
        /// <summary>
        /// Moves the object to the specified coordinates.
        /// </summary>
        public abstract void MoveTo(double PosX, double PosY, bool LockNewPosition = false, bool IsResizing = false);

        /// <summary>
        /// Resizes the object to the specified dimensions.
        /// Returns indication of valid resizing respect the minimum allowed.
        /// </summary>
        public abstract bool ResizeTo(double Width, double Height);

        /// Center point of the object.
        /// </summary>
        public abstract Point BaseCenter { get; set; }

        /// <summary>
        /// Indicates whether this object can be moved.
        /// </summary>
        public abstract bool CanMove { get; }

        /// <summary>
        /// Indicates whether this object can be resized.
        /// </summary>
        public abstract bool CanResize { get; }

        /// <summary>
        /// Top X-coordinate of the object.
        /// </summary>
        public abstract double BaseTop { get; set; }

        /// <summary>
        /// Left Y-coordinate of the object.
        /// </summary>
        public abstract double BaseLeft { get; set; }

        /// <summary>
        /// Width of the object.
        /// </summary>
        public abstract double BaseWidth { get; set; }

        /// <summary>
        /// Height of the object.
        /// </summary>
        public abstract double BaseHeight { get; set; }

        /// <summary>
        /// Area of the figure.
        /// </summary>
        public abstract Rect BaseArea { get; }

        /// <summary>
        /// Gets the current area of the object.
        /// </summary>
        public abstract Rect TotalArea { get; }

        /// <summary>
        /// Gets the depth coordinate on the Z-axis for presentation of the visual object (the higher to upper, zero at the bottom).
        /// Returns -1 if not found in the associated displaying View.
        /// </summary>
        public int ZOrder
        {
            get
            {
                var Index = this.GetDisplayingView().GetZOrderOf(this);
                return Index;
            }
        }

        /// <summary>
        /// Gets the movable pieces which this visual-object considers as visually united, plus indication of being contained within a region.
        /// </summary>
        /// <param name="IncludeRelatedOrigins">If applicable, indicates whether the Origins subtree must be considered as to be moved.</param>
        /// <param name="IncludeRelatedTargets">If applicable, indicates whether the Targets subtree must be considered as to be moved.</param>
        /// <param name="IsForVisualization">Indicates that this request is for visualizing and not movement (thus, the indirectly/implicitly selected objects must not be included).</param>
        public virtual IEnumerable<Tuple<VisualObject,bool>> GetMovableMembers(bool IncludeRelatedOrigins, bool IncludeRelatedTargets, bool IsForVisualization)
        {
            return Tuple.Create(this, false).IntoEnumerable();
        }

        /// <summary>
        /// Indicates whether this object is being shown or not for a related dominant.
        /// </summary>
        public virtual bool IsRelatedVisible { get { return __IsRelatedVisible.Get(this); } set { __IsRelatedVisible.Set(this, value); } }
        protected bool IsRelatedVisible_ = true;
        public static readonly ModelPropertyDefinitor<VisualObject, bool> __IsRelatedVisible =
                   new ModelPropertyDefinitor<VisualObject, bool>("IsRelatedVisible", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsRelatedVisible_, (ins, val) => ins.IsRelatedVisible_ = val, false, false,
                                                                  "Is Related Visible", "Indicates whether this object is being shown or not for a related dominant.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualObject> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualObject> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualObject> __ClassDefinitor = null;

        public new VisualObject CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualObject)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualObject PopulateFrom(VisualObject SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}