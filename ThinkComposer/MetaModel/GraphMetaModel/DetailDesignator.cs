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
// File   : DetailDesignator.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.DetailDesignator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.09 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Base ancestor for the designators of detailed data.
    /// </summary>
    [Serializable]
    public abstract class DetailDesignator : MetaDefinition, IModelEntity, IModelClass<DetailDesignator>, IVersionUpdater
    {
        public static string KindName { get { throw new UsageAnomaly("Descendant of ContainedDetail must implement its own new Kind static properties"); } }
        public static string KindTitle { get { throw new UsageAnomaly("Descendant of ContainedDetail must implement its own new Kind static properties"); } }
        public static string KindSummary { get { throw new UsageAnomaly("Descendant of ContainedDetail must implement its own new Kind static properties"); } }
        public static ImageSource KindPictogram { get { throw new UsageAnomaly("Descendant of ContainedDetail must implement its own new Kind static properties"); } }

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static DetailDesignator()
        {
            __ClassDefinitor = new ModelClassDefinitor<DetailDesignator>("DetailDesignator", MetaDefinition.__ClassDefinitor, "Detail Designator",
                                                                         "Base ancestor for the designators of detailed data.");
            __ClassDefinitor.DeclareProperty(__Initializer);
            __ClassDefinitor.DeclareProperty(__Owner);
            __ClassDefinitor.DeclareProperty(__SubOwnerFieldDef);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DetailDesignator(Ownership<IdeaDefinition,Idea> Owner, string Name, string TechName, string Summary = "", ICopyable Initializer = null, ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            this.Owner = Owner;

            this.Initializer = Initializer;
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected DetailDesignator()
                : base()
        {
        }

        public string OwnershipTitle { get { return (Owner.IsGlobal ? "Global" : "Local"); } }
        public string OwnershipSummary
        {
            get
            {
                return (Owner.IsGlobal ? "Detail belongs to the Definition of this Idea, hence is shared for all other Ideas of the same type."
                                       : "Detail belongs to this particular Idea, hence is exclusive to it.");
            }
        }

        /// <summary>
        /// Owner of this table detail designator.
        /// Note: This is always the related Domain when the derived object is a Table-Detail Designator directly owned by a Field-Definition (of Contained Table field type).
        /// </summary>
        public Ownership<IdeaDefinition, Idea> Owner { get { return __Owner.Get(this); } set { __Owner.Set(this, value); } }
        protected Ownership<IdeaDefinition, Idea> Owner_ = null;
        public static readonly ModelPropertyDefinitor<DetailDesignator, Ownership<IdeaDefinition, Idea>> __Owner =
                   new ModelPropertyDefinitor<DetailDesignator, Ownership<IdeaDefinition, Idea>>("Owner", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.Owner_, (ins, val) => ins.Owner_ = val, false, true,
                                                                                                 "Owner", "Owner of this table detail designator.");

        /// <summary>
        /// Optional Field-Definition sub-owning this detail designator.
        /// </summary>
        public FieldDefinition SubOwnerFieldDef { get { return __SubOwnerFieldDef.Get(this); } set { __SubOwnerFieldDef.Set(this, value); } }
        protected FieldDefinition SubOwnerFieldDef_ = null;
        public static readonly ModelPropertyDefinitor<DetailDesignator, FieldDefinition> __SubOwnerFieldDef =
                   new ModelPropertyDefinitor<DetailDesignator, FieldDefinition>("SubOwnerFieldDef", EEntityMembership.External, false, EPropertyKind.Common, ins => ins.SubOwnerFieldDef_, (ins, val) => ins.SubOwnerFieldDef_ = val, false, true,
                                                                                 "Sub-Owner Field Definition", "Optional Field-Definition sub-owning this detail designator.");

        /// <summary>
        /// Gets the predefined detail appearance.
        /// </summary>
        public abstract DetailAppearance DetailLook { get; }

        public abstract IRecognizableElement Definitor { get; set; }

        public abstract IEnumerable<IRecognizableElement> AvailableDefinitors { get; }

        /// <summary>
        /// Object to be copyed as initial content for an empty contained-detail.
        /// </summary>
        public ICopyable Initializer
        {
            get
            {
                var Value = __Initializer.Get(this);

                if (Value is StoreBoxBase)
                {
                    var Result = ((StoreBoxBase)Value).StoredObject as ICopyable;
                    return Result;
                }

                return Value as ICopyable;
            }
            set
            {
                object Value = value;

                // IMPORTANT: The Store-Box is created for the non-generic ancestor type of the object.
                // This allows use of Setters/Getters with no-generic types as parameters (e.g.: MModelPropertyDefinitor instead of ModelPropertyDefinitor<T>).
                if (Value != null && StoreBox.GetRegisteredTypes().Any(regtype => regtype.IsAssignableFrom(Value.GetType())) )
                    Value = StoreBox.CreateStoreBoxForType(Value.GetType().GetNonGenericType(), Value);

                __Initializer.Set(this, Value);
            }
        }
        protected object Initializer_ = null;
        public static readonly ModelPropertyDefinitor<DetailDesignator, object> __Initializer =
                   new ModelPropertyDefinitor<DetailDesignator, object>("Initializer", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.Initializer_, (ins, val) => ins.Initializer_ = val, false, true,
                                                                        "Initializer", "ICopyable to be copyed as initial content for an empty contained-detail.");

        /// <summary>
        /// Returns, for this Detail Designator and the supplied Target Idea, the Detail Content stored or generated by default (from the Initializer).
        /// </summary>
        public virtual ContainedDetail GetFinalContent(Idea TargetIdea)
        {
            if (TargetIdea == null)
                return null;

            return TargetIdea.Details.Where(det => det.Designation.IsEqual(this)).FirstOrDefault();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<DetailDesignator> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<DetailDesignator> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<DetailDesignator> __ClassDefinitor = null;

        public new DetailDesignator CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((DetailDesignator)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public DetailDesignator PopulateFrom(DetailDesignator SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void UpdateVersion()
        {
            if (this.Version != null)
                this.Version.Update();

            var VerOwner = this.Owner.Owner as IVersionUpdater;
            if (VerOwner != null)
                VerOwner.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
