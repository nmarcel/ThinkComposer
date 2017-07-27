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
// File   : ContainedDetail.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.ContainedDetail (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.22 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the Information enrichment of Graph entities
namespace Instrumind.ThinkComposer.Model.InformationModel
{
   /// <summary>
   /// Represents an object stored as detail for an Idea (contained within its DetailsContainer).
   /// </summary>
   [Serializable]
   public abstract class ContainedDetail : IModelEntity, IModelClass<ContainedDetail>
   {
       /// <summary>
       /// Static Constructor.
       /// </summary>
       static ContainedDetail()
       {
           __ClassDefinitor = new ModelClassDefinitor<ContainedDetail>("ContainedDetail", null, "Contained-Detail",
                                                                       "Represents an object stored as detail for an Idea (contained within its DetailsContainer).");
           __ClassDefinitor.DeclareProperty(__OwnerIdea);
       }

       /// <summary>
       /// Constructor.
       /// </summary>
       public ContainedDetail(Idea OwnerIdea)
       {
           // Non-forceable (Unassigned details could not have owner-idea):
           // General.ContractRequiresNotNull(OwnerIdea);

           this.OwnerIdea = OwnerIdea;
       }

       // ---------------------------------------------------------------------------------------------------------------------------------------------------------
       /// <summary>
       /// Idea details container owning this Detail.
       /// </summary>
       public Idea OwnerIdea { get { return __OwnerIdea.Get(this); } set { __OwnerIdea.Set(this, value); } }
       protected Idea OwnerIdea_;
       public static readonly ModelPropertyDefinitor<ContainedDetail, Idea> __OwnerIdea =
                  new ModelPropertyDefinitor<ContainedDetail, Idea>("OwnerIdea", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerIdea_, (ins, val) => ins.OwnerIdea_ = val, false, true,
                                                                    "Owner Idea", "Container owning this Contained-Detail.");

       /// <summary>
       /// Reference to the designator of the content.
       /// </summary>
       public DetailDesignator Designation { get { return this.ContentDesignator.AssignedValue as DetailDesignator; } }

       public abstract Assignment<DetailDesignator> AssignedDesignator { get; set; }

       /// <summary>
       /// Reference to the full designator (associating ownership) of the content.
       /// </summary>
       public abstract MAssignment ContentDesignator { get; }

       /// <summary>
       /// Returns the kind of this detail.
       /// </summary>
       public abstract ModelDefinition Kind { get; }

       /// <summary>
       /// Updates the related Designator identification information based on the Content.
       /// </summary>
       public abstract void UpdateDesignatorIdentification();

       /// <summary>
       /// Indicates whether this detail is Custom, which means this was not designated at the Idea Definitor, but into a particular Idea.
       /// </summary>
       [Description("Indicates whether this detail is Custom, which means this was not designated at the Idea Definitor, but into a particular Idea.")]
       public bool IsCustomDetail
       {
           get
           {
               var Result = (this.OwnerIdea != null && !this.OwnerIdea.IdeaDefinitor.DetailDesignators.Any(dsn => dsn.IsEqual(this.Designation)));
               return Result;
           }
       }

       /// <summary>
       /// Indicates whether the contained/referenced detail is non empty and not references nothing.
       /// </summary>
       public virtual bool ValueExists { get{ return true; } }

       /// <summary>
       /// Indicates whether the contained/referenced detail has a known format, therefore can be manipulated or shown.
       /// </summary>
       public virtual bool ValueHasKnownFormat { get { return true; } }

       // ---------------------------------------------------------------------------------------------------------------------------------------------------------
       #region IModelClass<ContainedDetail> Members

       public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
       public ModelClassDefinitor<ContainedDetail> ClassDefinitor { get { return __ClassDefinitor; } }
       public static readonly ModelClassDefinitor<ContainedDetail> __ClassDefinitor = null;

       public abstract object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner);
       public ContainedDetail CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ContainedDetail)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
       public ContainedDetail PopulateFrom(ContainedDetail SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

       #endregion

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