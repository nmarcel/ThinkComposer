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
// File   : SimpleElement.cs
// Object : Instrumind.Common.SimpleElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Creation
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
    /// Basic entity type, globally unique, with Name, Tech-Name, Summary and Tech-Spec.
    /// </summary>
    [Serializable]
    public class SimpleElement : IIdentifiableElement, IModelEntity, ITechSpecifier, IModelClass<SimpleElement>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static SimpleElement()
        {
            __ClassDefinitor = new ModelClassDefinitor<SimpleElement>("SimpleElement", null, "Simple Element",
                                                                      "Basic entity type, globally unique, with Name, Tech-Name, Summary and Tech-Spec.");
            __ClassDefinitor.DeclareProperty(__Name);
            __ClassDefinitor.DeclareProperty(__TechName);
            __ClassDefinitor.DeclareProperty(__Summary);
            __ClassDefinitor.DeclareProperty(__TechSpec);

            __Name.IsIdentificator = true;
            __Name.IsLinkeable = true;

            __TechName.IsIdentificator = true;
            __TechName.IsLinkeable = true;

            __Summary.IsLinkeable = true;

            __TechSpec.IsLinkeable = true;

            __ClassDefinitor.InstanceValidator = (ins => ins.Name.IsAbsent() ? ("The '" + __Name.Name + "' field is empty").IntoList() : null);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleElement(string Name, string TechName, string Summary = "")
        {
            General.ContractRequiresNotNull(Name, Summary);

            this.Name = Name;
            this.TechName = TechName.NullDefault(Name);
            this.Summary = Summary;
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected SimpleElement()
        {
        }

        /// <summary>
        /// Name of the object. Must be schema unique. Intended for user-level usage.
        /// </summary>
        public string Name
        {
            get { return __Name.Get(this); }
            set
            {
                value = value.NullDefault("").Trim();

                var TechNameWasEquivalentToName = (this.TechName == this.Name.TextToIdentifier());
                if (__Name.Set(this, value))
                {
                    if (TechNameWasEquivalentToName)
                        this.TechName = this.Name.TextToIdentifier();

                    this.NotifyPropertyChange("NameCaption");
                }
            }
        }
        protected string Name_;
        public static readonly ModelPropertyDefinitor<SimpleElement, string> __Name =
                   new ModelPropertyDefinitor<SimpleElement, string>("Name", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Name, ins => ins.Name_,
                       (ins, val) =>
                           {
                               /*T if (val.IsAbsent())
                                   Console.WriteLine("Setting absent Name."); */

                               ins.Name_ = val;
                           },
                       true, false,
                                                                     "Name/Title", "Name or Title of the object.");

        /// <summary>
        /// Technical-Name of the object. Must be schema unique. Intended for machine-level usage as code, identifier or name for files/tables/programs.
        /// </summary>
        public string TechName
        {
            get { return __TechName.Get(this); }
            set
            {
                value = value.NullDefault("").Trim();

                if (value.IsAbsent() && !this.Name.IsAbsent())
                    value = this.Name.TextToIdentifier();

                __TechName.Set(this, value);
            }
        }
        protected string TechName_;
        public static readonly ModelPropertyDefinitor<SimpleElement, string> __TechName =
                   new ModelPropertyDefinitor<SimpleElement, string>("TechName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Key,
                                                                     ins => (EntityEditEngine.ActiveEntityEditor != null && EntityEditEngine.ActiveEntityEditor.ReadTechNamesAsProgramIdentifiers
                                                                             ? ins.TechName_.TextToCSharpIdentifier()
                                                                             : ins.TechName_),
                                                                     (ins, val) => ins.TechName_ = val, true, true,
                                                                     "Tech-Name", "Technical-Name of the object. Should be unique. Intended for machine-level usage as code, identifier or name for files/tables/programs.");

        /// <summary>
        /// Summary of the object.
        /// </summary>
        public string Summary { get { return __Summary.Get(this); } set { __Summary.Set(this, value); } }
        protected string Summary_;
        public static readonly ModelPropertyDefinitor<SimpleElement, string> __Summary =
                   new ModelPropertyDefinitor<SimpleElement, string>("Summary", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Description, ins => ins.Summary_, (ins, val) => ins.Summary_ = val, true, false,
                                                                     "Summary", "Summary of the object.");

        /// <summary>
        /// Technical-Specification of the subject. Intended as a machine-level representation for computation (i.e. for use as script, template, formula or other kind of expression).
        /// </summary>
        public string TechSpec { get { return __TechSpec.Get(this); } set { __TechSpec.Set(this, value); } }
        protected string TechSpec_;
        public static readonly ModelPropertyDefinitor<SimpleElement, string> __TechSpec =
                   new ModelPropertyDefinitor<SimpleElement, string>("TechSpec", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Description, ins => ins.TechSpec_, (ins, val) => ins.TechSpec_ = val, false, true,
                                                                     "Tech-Spec", "Technical-Specification of the object. Intended as a machine-level representation for computation (i.e. for use as script, template, formula or other kind of expression).");

        /// <summary>
        /// Gets the Name for single-line display (without new-line characters).
        /// </summary>
        [Description("Gets the Name for single-line display (without new-line characters).")]
        public string NameCaption
        {
            get
            {
                return this.Name.RemoveNewLines();
            }
        }

        public override string ToString()
        {
            return this.TechName;
        }

        #region IMModelClass + IModelEntity, IModelClass<SimpleElement> Members7

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<SimpleElement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<SimpleElement> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public SimpleElement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((SimpleElement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public SimpleElement PopulateFrom(SimpleElement SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

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

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToStringAlways());
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
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