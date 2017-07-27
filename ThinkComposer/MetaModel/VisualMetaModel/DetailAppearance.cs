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
// File   : DetailAppearance.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.DetailAppearance (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.15 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Base for setting the appearance of detail content shown in a symbol's details poster.
    /// </summary>
    [Serializable]
    public abstract class DetailAppearance : IModelEntity, IModelClass<DetailAppearance>, IIndicatesAlteration
    {
        // Default values for variable initializations and detection of user custom changes.
        protected const bool DEFVAL_IsDisplayed = true;
        protected const bool DEFVAL_ShowTitle = true;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static DetailAppearance()
        {
            __ClassDefinitor = new ModelClassDefinitor<DetailAppearance>("DetailAppearance", null, "Detail Appearance",
                                                                         "Base for setting the appearance of detail content shown in a symbol's details poster.");
            __ClassDefinitor.DeclareProperty(__IsDisplayed);
            __ClassDefinitor.DeclareProperty(__ShowTitle);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DetailAppearance(bool IsDisplayed = DEFVAL_IsDisplayed, bool ShowTitle = DEFVAL_ShowTitle)
        {
            this.IsDisplayed = IsDisplayed;
            this.ShowTitle = ShowTitle;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the instance has been changed respect its default values, therefore requiring to be reevaluated for later use.
        /// </summary>
        public virtual bool IsAltered
        {
            get
            {
                return (this.IsDisplayed != DEFVAL_IsDisplayed || this.ShowTitle != DEFVAL_ShowTitle);
            }
        }

        /// <summary>
        /// Indicates whether the settings of this instance differs from that supplied one.
        /// </summary>
        public virtual bool SettingsDiffersFrom(DetailAppearance Other)
        {
            return !(this.IsDisplayed == Other.IsDisplayed && this.ShowTitle == Other.ShowTitle);
        }

        /// <summary>
        /// Returns a clone of this detail format.
        /// </summary>
        public abstract DetailAppearance Clone();

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this detail content is shown.
        /// </summary>
        public bool IsDisplayed { get { return __IsDisplayed.Get(this); } set { __IsDisplayed.Set(this, value); } }
        protected bool IsDisplayed_ = DEFVAL_IsDisplayed;
        public static readonly ModelPropertyDefinitor<DetailAppearance, bool> __IsDisplayed =
                   new ModelPropertyDefinitor<DetailAppearance, bool>("IsDisplayed", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsDisplayed_, (ins, val) => ins.IsDisplayed_ = val, false, false,
                                                                      "Is Displayed", "Indicates whether this detail content is shown.");

        /// <summary>
        /// Indicates whether to show the detail title/name.
        /// </summary>
        public bool ShowTitle { get { return __ShowTitle.Get(this); } set { __ShowTitle.Set(this, value); } }
        protected bool ShowTitle_ = DEFVAL_ShowTitle;
        public static readonly ModelPropertyDefinitor<DetailAppearance, bool> __ShowTitle =
                   new ModelPropertyDefinitor<DetailAppearance, bool>("ShowTitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowTitle_, (ins, val) => ins.ShowTitle_ = val, false, false,
                                                                      "Show Title", "Indicates whether to show the detail title/name.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<DetailRepresentation> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<DetailAppearance> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<DetailAppearance> __ClassDefinitor = null;

        public abstract object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner);
        public DetailAppearance CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((DetailAppearance)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public DetailAppearance PopulateFrom(DetailAppearance SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        protected EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

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

    }
}