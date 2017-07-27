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
// File   : MarkerAssignment.cs
// Object : Instrumind.ThinkComposer.Model.GraphModel.MarkerAssignment (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.12.28 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.GraphModel
{
    /// <summary>
    /// Represents the assignment of a Marking to an Idea. Optionally, a descriptor can be also associated.
    /// </summary>
    [Serializable]
    public class MarkerAssignment : IModelEntity, IModelClass<MarkerAssignment>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static MarkerAssignment()
        {
            __ClassDefinitor = new ModelClassDefinitor<MarkerAssignment>("MarkerAssignment", null,
                                                                         "Marker Assignment", "Represents the assignment of a Marking to an Idea. Optionally, a descriptor can be also associated.");
            __ClassDefinitor.DeclareProperty(__Definitor);
            __ClassDefinitor.DeclareProperty(__Descriptor);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MarkerAssignment(EntityEditEngine Engine, MarkerDefinition Definitor, SimplePresentationElement Descriptor = null)
        {
            this.Definitor = Definitor;
            this.Descriptor = Descriptor;
        }

        /// <summary>
        /// Empty constructor for agent descendants.
        /// </summary>
        public MarkerAssignment()
        {
        }

        /// <summary>
        /// Definitor of this Marker.
        /// </summary>
        public MarkerDefinition Definitor { get { return __Definitor.Get(this); } set { __Definitor.Set(this, value); } }
        protected MarkerDefinition Definitor_;
        public static readonly ModelPropertyDefinitor<MarkerAssignment, MarkerDefinition> __Definitor =
                   new ModelPropertyDefinitor<MarkerAssignment, MarkerDefinition>("Definitor", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.Definitor_, (ins, val) => ins.Definitor_ = val, true, false,
                                                                                  "Definitor", "Definitor of this Marker.");

        /// <summary>
        /// Optional descriptor for the Marker.
        /// </summary>
        public SimplePresentationElement Descriptor
        {
            get { return __Descriptor.Get(this); }
            set
            {
                __Descriptor.Set(this, value);

                if (value != null)
                    this.LastUsedDescriptor = value;

                NotifyPropertyChange("MarkerHasDescriptor");
            }
        }
        protected SimplePresentationElement Descriptor_;
        public static readonly ModelPropertyDefinitor<MarkerAssignment, SimplePresentationElement> __Descriptor =
                   new ModelPropertyDefinitor<MarkerAssignment, SimplePresentationElement>("Descriptor", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Descriptor_, (ins, val) => ins.Descriptor_ = val, true, false,
                                                                                           "Descriptor", "Optional descriptor for the Marker.");

        public IEnumerable<MarkerDefinition> AvailableMarkerDefinitors
        {
            get { return ((Composition)this.EditEngine.TargetDocument).CompositeContentDomain.MarkerDefinitions;  }
        }

        public bool MarkerHasDescriptor
        {
            get { return (this.Descriptor != null); }
            set
            {
                if (!value)
                    this.Descriptor = null;
                else
                    if (this.Descriptor == null)
                        this.Descriptor = this.LastUsedDescriptor.NullDefault(new SimplePresentationElement("", ""));
            }
        }
        [NonSerialized]
        private SimplePresentationElement LastUsedDescriptor = null;

        #region IModelClass<MarkerAssignment> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<MarkerAssignment> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<MarkerAssignment> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public MarkerAssignment CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((MarkerAssignment)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public MarkerAssignment PopulateFrom(MarkerAssignment SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

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

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            var Result = this.Definitor.NameCaption + (this.Descriptor == null ? "" : ": " + this.Descriptor.NameCaption);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}