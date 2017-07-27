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
// File   : VisualInert.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualInert (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.26 Néstor Sánchez A.  Creation
//

using System;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

//using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
//using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Individual non-editable and fixed visual object.
    /// It can be, for example, a background image, margin or page delimiter, label, seal or things like these.
    /// </summary>
    [Serializable]
    public class VisualInert : VisualObject, IModelEntity, IModelClass<VisualInert>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualInert()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualInert>("VisualInert", VisualObject.__ClassDefinitor, "Visual Inert",
                                                                    "Individual non-editable and fixed visual object. " +
                                                                    "It can be, for example, a background image, margin or page delimiter, label, seal or things like these.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualInert(View DisplayingView, ContainerVisual Graphic = null)
        {
            this.DisplayingView = DisplayingView;
            this.Graphic = Graphic;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the View visually containing this visual object.
        /// </summary>
        public override View GetDisplayingView()
        {
            return this.DisplayingView;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the View visually containing this visual object.
        /// </summary>
        public View DisplayingView { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates wether this visual object is selected for later applying a command.
        /// </summary>
        public override bool IsSelected { get { return false; } set { } }   // Do nothing because inerts are rendered from outside.

        /// <summary>
        /// Indicates wether this visual object is vanished as a result of been marked for deletion.
        /// </summary>
        public override bool IsVanished { get { return false; } set { } }   // Do nothing because inerts are rendered from outside.

        /// <summary>
        /// Moves the object to the specified coordinates.
        /// </summary>
        public override void MoveTo(double PosX, double PosY, bool LockNewPosition = false, bool IsResizing = false) { }

        /// <summary>
        /// Resizes the object to the specified dimensions.
        /// Returns indication of valid resizing respect the minimum allowed.
        /// </summary>
        public override bool ResizeTo(double Width, double Height) { return false; }

        /// <summary>
        /// Indicates whether this object can be moved.
        /// </summary>
        public override bool CanMove { get { return false; } }

        /// <summary>
        /// Indicates whether this object can be resized.
        /// </summary>
        public override bool CanResize { get { return false; } }

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
        public override Rect BaseArea { get { return Rect.Empty; } }

        /// <summary>
        /// Gets the current area of the object.
        /// </summary>
        public override Rect TotalArea { get { return Rect.Empty; } }   // Do nothing because inerts are rendered from outside.

        /// <summary>
        /// Renders the represented semantic object.
        /// </summary>
        public override void RenderRepresentatedObject()
        {
            // Do nothing because inerts are rendered from outside.
        }

        /// <summary>
        /// Recreates and returns the Graphic of this visual object descendant and its children, for a presentation context
        /// and indicating whether to show manipulation adorners.
        /// </summary>
        public override ContainerVisual GenerateGraphic(UIElement PresentationContext, bool ShowManipulationAdorners)
        {
            return this.Graphic;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualInert> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualInert> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualInert> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualInert CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualInert)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualInert PopulateFrom(VisualInert SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}