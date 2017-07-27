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
// File   : PresentationElement.cs
// Object : Instrumind.Common.Visualization.PresentationElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.02 Néstor Sánchez A.  Creation
//

using System;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Standard entity with identification, classification, versioning and visual representation.
    /// </summary>
    [Serializable]
    public class FormalPresentationElement : FormalElement, IFormalizedRecognizableElement, IModelEntity, IModelClass<FormalPresentationElement>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static FormalPresentationElement()
        {
            __ClassDefinitor = new ModelClassDefinitor<FormalPresentationElement>("FormalPresentationElement", FormalElement.__ClassDefinitor, "Formal Presentation Element",
                                                                                  "Standard entity with identification, versioning and visual representation.");
            __ClassDefinitor.DeclareProperty(__Pictogram);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FormalPresentationElement(string Name, string TechName, string Summary = "", ImageSource Pictogram = null, bool IsDescribed = false, bool IsClassified = false, bool IsVersioned = false)
             : base(Name, TechName, Summary, IsDescribed, IsClassified, IsVersioned)
        {
            this.Pictogram = Pictogram;
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected FormalPresentationElement()
        {
        }

        /// <summary>
        /// Graphic representation of the object
        /// </summary>
        public virtual ImageSource Pictogram { get { return __Pictogram.Get(this); } set { __Pictogram.Set(this, value); } }
        protected ImageAssignment Pictogram_ = new ImageAssignment();
        public static readonly ModelPropertyDefinitor<FormalPresentationElement, ImageSource> __Pictogram =
                   new ModelPropertyDefinitor<FormalPresentationElement, ImageSource>("Pictogram", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Pictogram_.Image, (ins, val) => ins.Pictogram_.Image = val, true, false, "Pictogram", "Graphic representation of the object.");

        #region IModelClass<FormalPresentationElement> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<FormalPresentationElement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<FormalPresentationElement> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new FormalPresentationElement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((FormalPresentationElement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public FormalPresentationElement PopulateFrom(FormalPresentationElement SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}