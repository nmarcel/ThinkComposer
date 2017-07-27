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
// File   : VisualElement.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Composer.ComposerUI;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Identifiable base ancestor for visual representators such as symbols and connectors.
    /// </summary>
    [Serializable]
    public abstract class VisualElement : VisualObject, IModelEntity, IModelClass<VisualElement>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualElement()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualElement>("VisualElement", VisualObject.__ClassDefinitor, "Visual Element",
                                                                      "Identifiable base ancestor for visual representators such as symbols and connectors.");
            __ClassDefinitor.DeclareProperty(__RepresentationPartType);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualElement(EVisualRepresentationPart VisualRepresentationPart)
        {
            this.RepresentationPartType = VisualRepresentationPart;
        }

        protected VisualElement()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the owning visual representator.
        /// </summary>
        [Description("References the owning visual representator.")]
        public abstract VisualRepresentation OwnerRepresentation { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the View visually containing this visual object.
        /// </summary>
        public override View GetDisplayingView()
        {
            return this.OwnerRepresentation.DisplayingView;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates wether this visual object is selected for later applying a command.
        /// </summary>
        public override bool IsSelected { get { return this.OwnerRepresentation.IsSelected; } set { this.OwnerRepresentation.IsSelected = value; } }

        /// <summary>
        /// Indicates wether this visual object is vanished as a result of been marked for deletion.
        /// </summary>
        public override bool IsVanished { get { return this.OwnerRepresentation.IsVanished; } set { this.OwnerRepresentation.IsVanished = value; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a new draw implementing this visual element for an optional presentation context.
        /// </summary>
        public abstract DrawingGroup CreateDraw(UIElement PresentationContext, bool ShowManipulationAdorners);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Renders the represented semantic object.
        /// </summary>
        public override void RenderRepresentatedObject()
        {
            this.OwnerRepresentation.Render();
        }

        /// <summary>
        /// Updates the visual presentation of this element on the owner View of the owner Representation.
        /// </summary>
        public void RenderElement()
        {
            this.OwnerRepresentation.DisplayingView.Show(this);
        }

        /// <summary>
        /// Clears the visual presentation of this element on the owner View of the owner Representation.
        /// </summary>
        public void ClearElement()
        {
            this.OwnerRepresentation.DisplayingView.Clear(this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Draws and returns a set of indicator adorners (drawing, is-main and manipulation-direction), based on supplied Indicator Size, Stroke, Pencil and optional Geometry-Creator, for mark the selection of this visual element.
        /// </summary>
        public abstract List<Tuple<Drawing, bool, EManipulationDirection>> GenerateSelectionIndicators(double IndicatorSize, Brush IndStroke, Pen IndPencil, Func<Rect, Geometry> GeometryCreator = null);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates the part of the owning visual representation that this visual element implements.
        /// </summary>
        public EVisualRepresentationPart RepresentationPartType { get { return __RepresentationPartType.Get(this); } set { __RepresentationPartType.Set(this, value); } }
        protected EVisualRepresentationPart RepresentationPartType_;
        public static readonly ModelPropertyDefinitor<VisualElement, EVisualRepresentationPart> __RepresentationPartType =
                   new ModelPropertyDefinitor<VisualElement, EVisualRepresentationPart>("RepresentationPartType", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RepresentationPartType_, (ins, val) => ins.RepresentationPartType_ = val, false, false,
                                                                                                                 "Representation Part Type", "Indicates the part of the owning visual representation that this visual element implements.");

        #region IModelClass<VisualElement> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualElement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualElement> __ClassDefinitor = null;

        public new VisualElement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualElement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualElement PopulateFrom(VisualElement SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}