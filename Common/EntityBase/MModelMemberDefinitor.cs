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
// File   : MModelMemberDefinitor.cs
// Object : Instrumind.Common.EntityBase.MModelMemberDefinitor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Windows.Data;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Base class for generic model class member definitors.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    public abstract class MModelMemberDefinitor : ModelDefinition
    {
        public MModelMemberDefinitor(string TechName, string Name, string Summary, Type DeclaringType, EEntityMembership Membership, bool IsRequired, bool IsAdvanced)
             : base(TechName, Name, Summary, DeclaringType)
        {
            this.Membership = Membership;
            this.IsRequired = IsRequired;
            this.IsAdvanced = IsAdvanced;

            this.IsEditControlled = true;
            this.ChangesExistenceStatus = true;
        }

        /// <summary>
        /// Indicates whether for this memeber a Clone operation should be performed in the specified Scope and for the specified Source,
        /// plus the CloningScope to use (which might be a new one).
        /// </summary>
        public Tuple<bool, ECloneOperationScope> IsCloneableFor(ECloneOperationScope CloningScope, IMModelClass Source)
        {
            var IsCloneable = false;
            var Evaluate = true;

            if (this.ForcedOwnershipIndicator != null)
            {
                var Value = this.ForcedOwnershipIndicator.Read(Source);
                if (Value is bool && (bool)Value)
                {
                    IsCloneable = true;
                    CloningScope = ECloneOperationScope.Deep;
                    Evaluate = false;
                }
            }

            if (Evaluate)
            {
                // IMPORTANT TO BE NOTICED...
                // First, evaluate the special case of external collections, which must clone its head.
                // If no cloning of collection-heads are required, then the member should be a property storing the collection.
                // Example: ModelPropertyDefinitor<TEntity, EditableCollection<TItem>> instead of ModelListDefinitor<TEntity, TItem>
                if (this.Membership == EEntityMembership.External &&
                    this is MModelCollectionDefinitor)
                {
                    IsCloneable = true;
                    CloningScope = ECloneOperationScope.Slight;
                }
                else
                    // Notice that slight-scope also considers collection-heads
                    if (CloningScope == ECloneOperationScope.Slight)
                        IsCloneable = (this.Membership == EEntityMembership.InternalCoreExclusive
                                       || this is MModelCollectionDefinitor);
                    else
                        // NOTE: In core-scope consider for collections to do their items cloning with shallow-scope.
                        if (CloningScope == ECloneOperationScope.Core)
                            IsCloneable = (this.Membership == EEntityMembership.InternalCoreExclusive);
                        else
                            // For deep-scope also consider the bulk content.
                            IsCloneable = (this.Membership == EEntityMembership.InternalCoreExclusive
                                           || this.Membership == EEntityMembership.InternalBulk);
            }

            var Result = Tuple.Create<bool, ECloneOperationScope>(IsCloneable, CloningScope);
            return Result;
        }

        /// <summary>
        /// Type of the data to be stored in this defined member.
        /// </summary>
        public abstract Type DataType { get; }

        /// <summary>
        /// Model class definitor declaring (owning) the underlying member.
        /// </summary>
        public MModelClassDefinitor DeclaringDefinitor { get; protected set; }

        /// <summary>
        /// Declaration of value-containment or instance-referencing, hence implicating ownership and coneability, of this entity member.
        /// This must be always Exlusive-Internal for value-type members.
        /// </summary>
        public EEntityMembership Membership { get; protected set; }

        /// <summary>
        /// References to an optional property-definitor, whose intance value (which must be 'boolean')
        /// indicates whether this member must be treated as owned (when 'true'), therefore as an Internal member despite being declared as External.
        /// </summary>
        public MModelPropertyDefinitor ForcedOwnershipIndicator { get; set; }

        /// <summary>
        /// Indicates that the member cannot be empty (having the default value of its type).
        /// </summary>
        public bool IsRequired { get; protected set; }

        /// <summary>
        /// Indicates whether the member is for advanced usage (i.e.: technical or very specialized).
        /// </summary>
        public bool IsAdvanced { get; protected set; }

        /// <summary>
        /// Indicates that the member can be edited and its assignments are controlled, hence intercepted to be undone/redone.
        /// </summary>
        public bool IsEditControlled { get; set; }

        /// <summary>
        /// Indicates whether a change on this memeber does affect the existence status of the entity.
        /// Example: Temporal visual indications for selection or manipulation.
        /// </summary>
        public bool ChangesExistenceStatus { get; set; }

        /// <summary>
        /// Indicates whether the collection can be empty meaning a full selection (instead of individually selected items).
        /// </summary>
        public bool CanCollectionBeEmpty { get; set; }

        /// <summary>
        /// Title for the collection whem empty (usually for indicate a full selection instead of individually selected items).
        /// </summary>
        public string EmptyCollectionTitle { get; set; }

        /// <summary>
        /// Function for get the standard items source to be used by consumers, based on the supplied model-entity context.
        /// This is global, therefore is intended only for non instance dependent sources.
        /// </summary>
        [NonSerialized]
        public Func<IModelEntity, IEnumerable> ItemsSourceGetter = null;

        /// <summary>
        /// Path of the selected value of the standard items source to be used by consumers.
        /// </summary>
        public string ItemsSourceSelectedValuePath { get; set; }

        /// <summary>
        /// Value converter for the property Binding.
        /// </summary>
        public IValueConverter BindingValueConverter { get; set; }


        /// <summary>
        /// Indicates that the member is currently not being used, but reserved for the future.
        /// </summary>
        public bool HasPendingImplementation { get; set; }

        /// <summary>
        /// Register the owner declarator of this member/definitor and assemble any related behaviors (such as setter interception).
        /// </summary>
        internal abstract void AssignDeclarator(MModelClassDefinitor DeclaringDefinitor);

        /// <summary>
        /// Returns the qualified tech-name (as "Class.Member") of this member.
        /// </summary>
        public string QualifiedTechName { get { return ((this.DeclaringDefinitor == null ? "" : this.DeclaringDefinitor.TechName) + "." + this.TechName); } }

        /// <summary>
        /// Returns the controlled property value of the supplied instance.
        /// </summary>
        public abstract object Read(IMModelClass Instance);

        /// <summary>
        /// Sets the passed value into the controlled property of the supplied instance.
        /// </summary>
        public abstract void Write(IMModelClass Instance, object Value);
    }
}