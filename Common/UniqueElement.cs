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
// File   : UniqueElement.cs
// Object : Instrumind.Common.UniqueElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.02 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Object with unique non-repeatable identity among others of any kind.
    /// </summary>
    [Serializable]
    public abstract class UniqueElement : IUniqueElement, IModelEntity, IModelClass<UniqueElement>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static UniqueElement()
        {
            __ClassDefinitor = new ModelClassDefinitor<UniqueElement>("UniqueElement", null,
                                                                      "Unique Element", "Object with unique non-repeatable identity among others of any kind.");
            __ClassDefinitor.DeclareProperty(__GlobalId);

            __GlobalId.IsIdentificator = true;
            __GlobalId.IsEditControlled = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public UniqueElement()
        {
            this.GlobalId = Guid.NewGuid();
        }

        public override string ToString()
        {
            return this.GlobalId.ToString();
        }

        /// <summary>
        /// Global unique identificator of the element instance.
        /// Consider to recreate for non-editing clones.
        /// </summary>
        public Guid GlobalId { get { return __GlobalId.Get(this); } set { __GlobalId.Set(this, value); } }
        protected Guid GlobalId_;
        public static readonly ModelPropertyDefinitor<UniqueElement, Guid> __GlobalId =
                   new ModelPropertyDefinitor<UniqueElement, Guid>("GlobalId", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Key, ins => ins.GlobalId_, (ins, val) => ins.GlobalId_ = val, false, true, "Global ID", "Global unique identifier of the object.");

        #region IModelClass<UniqueElement> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<UniqueElement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<UniqueElement> __ClassDefinitor = null;

        public abstract object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner);
        public UniqueElement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((UniqueElement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public UniqueElement PopulateFrom(UniqueElement SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        #region INotifyPropertyChanged Members

        [field:NonSerialized]
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

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToStringAlways());
        }

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

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}