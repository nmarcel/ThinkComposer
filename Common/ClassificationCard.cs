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
// File   : ClassificationCard.cs
// Object : Instrumind.Common.ClassificationCard (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.13 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Stores classification information for formal objects.
    /// </summary>
    [Serializable]
    public class ClassificationCard : IModelEntity, IModelClass<ClassificationCard>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static ClassificationCard()
        {
            __ClassDefinitor = new ModelClassDefinitor<ClassificationCard>("ClassificationCard", null,
                                                                           "Classification Card", "Stores classification information for formal objects.");
            __ClassDefinitor.DeclareProperty(__ContentTypeCode);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ClassificationCard(string ContentTypeCode)
        {
            this.ContentTypeCode = ContentTypeCode;
        }

        /// <summary>
        /// Type code of the object's content (e.g.: a MIME type for external consumption).
        /// </summary>
        public string ContentTypeCode { get { return __ContentTypeCode.Get(this); } set { __ContentTypeCode.Set(this, value); } }
        protected string ContentTypeCode_;
        public static readonly ModelPropertyDefinitor<ClassificationCard, string> __ContentTypeCode =
                   new ModelPropertyDefinitor<ClassificationCard, string>("ContentTypeCode", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Key, ins => ins.ContentTypeCode_, (ins, val) => ins.ContentTypeCode_ = val, false, false,
                                                                          "Content Type Code", "Type code of the object's content (e.g.: a MIME type for external consumption).");

        #region IModelClass<ClassificationCard> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<ClassificationCard> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<ClassificationCard> __ClassDefinitor = null;

        public object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public ClassificationCard CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ClassificationCard)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public ClassificationCard PopulateFrom(ClassificationCard SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied definitor, has changed.
        /// </summary>
        public void NotifyPropertyChange(MModelPropertyDefinitor PropertyDefinitor)
        {
            this.NotifyPropertyChange(PropertyDefinitor.TechName);
        }

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied name, has changed.
        /// </summary>
        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = PropertyChanged;

            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public virtual EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        private EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

        #endregion
    }
}