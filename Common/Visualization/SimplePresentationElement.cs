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
// File   : SimplePresentationElement.cs
// Object : Instrumind.Common.Visualization.SimplePresentationElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Creation
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
    /// Simple entity having a visual representation.
    /// </summary>
    [Serializable]
    public class SimplePresentationElement : SimpleElement, IRecognizableElement, IModelEntity, IModelClass<SimplePresentationElement>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static SimplePresentationElement()
        {
            __ClassDefinitor = new ModelClassDefinitor<SimplePresentationElement>("SimplePresentationElement", SimpleElement.__ClassDefinitor,
                                                                                  "Simple Presentation Element", "Simple entity having a visual representation.");
            __ClassDefinitor.DeclareProperty(__Pictogram);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimplePresentationElement(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(Name, TechName, Summary)
        {
            this.Pictogram = Pictogram;
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected SimplePresentationElement()
        {
        }

        /// <summary>
        /// Graphic representation of the object
        /// </summary>
        public virtual ImageSource Pictogram { get { return __Pictogram.Get(this); } set { __Pictogram.Set(this, value); } }
        protected ImageAssignment Pictogram_ = new ImageAssignment();
        public static readonly ModelPropertyDefinitor<SimplePresentationElement, ImageSource> __Pictogram =
                   new ModelPropertyDefinitor<SimplePresentationElement, ImageSource>("Pictogram", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Pictogram_.Image, (ins, val) => ins.Pictogram_.Image = val, true, false, "Pictogram", "Graphic representation of the object.");

        #region IModelClass<SimplePresentationElement> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<SimplePresentationElement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<SimplePresentationElement> __ClassDefinitor = null;

        public new SimplePresentationElement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((SimplePresentationElement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public SimplePresentationElement PopulateFrom(SimplePresentationElement SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}