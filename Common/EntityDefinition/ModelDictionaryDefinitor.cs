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
// File   : ModelListDefinitor.cs
// Object : Instrumind.Common.EntityDefinition.ModelListDefinitor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.13 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Manages and describes, at a static level, a member dictionary belonging to a model class.
    /// </summary>
    public class ModelDictionaryDefinitor<TModelClass, TKey, TValue> : MModelCollectionDefinitor where TModelClass : class, IModelClass<TModelClass>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelDictionaryDefinitor(string TechName, EEntityMembership Membership,
                                        Func<TModelClass, EditableDictionary<TKey, TValue>> BaseGetter,
                                        Action<TModelClass, EditableDictionary<TKey, TValue>> BaseSetter,
                                        string Name, string Summary, bool IsRequired = false, bool IsEditControlled = true, bool IsAdvanced = false)
             : base(TechName, Name, Summary, typeof(TModelClass), Membership, IsRequired, IsAdvanced)
        {
            this.Getter = BaseGetter;
            this.Setter = ((ins, def, coll) => { BaseSetter(ins, coll); return true; });
        }

        public override Type DataType { get { return typeof(EditableDictionary<TKey, TValue>); } }

        /// <summary>
        /// Register the owner declarator of this member/definitor and assemble any related behaviors (such as setter interception).
        /// </summary>
        internal override void AssignDeclarator(MModelClassDefinitor DeclaringDefinitor)
        {
            this.DeclaringDefinitor = DeclaringDefinitor;
        }

        /// <summary>
        /// Getter operation for access the dictionary property from its owning instance.
        /// </summary>
        public Func<TModelClass, EditableDictionary<TKey, TValue>> Getter;

        /// <summary>
        /// Function which sets the passed dictionary into the dictionary property of a supplied instance and returns indication of whether the setting was made.
        /// </summary>
        public Func<TModelClass, ModelDictionaryDefinitor<TModelClass, TKey, TValue>, EditableDictionary<TKey, TValue>, bool> Setter { get; protected set; }

        public override object Read(IMModelClass Instance)
        {
            return this.Get((TModelClass)Instance);
        }

        public override void Write(IMModelClass Instance, object Collection)
        {
            this.Set((TModelClass)Instance, (EditableDictionary<TKey, TValue>)Collection);
        }

        /// <summary>
        /// Returns the controlled dictionary value of the supplied instance.
        /// </summary>
        public EditableDictionary<TKey, TValue> Get(TModelClass Instance)
        {
            return this.Getter(Instance);
        }

        /// <summary>
        /// Sets the passed value into the controlled dictionary of the supplied instance.
        /// Returns indication whether the setting was made.
        /// </summary>
        public bool Set(TModelClass Instance, EditableDictionary<TKey, TValue> Collection)
        {
            return this.Setter(Instance, this, Collection);
        }

        /// <summary>
        /// Creates and returns a controller for the defined dictionary and supplied instance controller.
        /// </summary>
        public DictionaryController<TModelClass, TKey, TValue> CreateController(MEntityInstanceController InstanceController)
        {
            var Result = new DictionaryController<TModelClass, TKey, TValue>(InstanceController);

            Result.DictionaryDefinitor = this;

            Result.IsEditableNow = this.IsEditControlled;
            Result.IsValid = true;

            if (this.ItemsSourceGetter != null)
            {
                Result.CanCollectionBeEmpty = this.CanCollectionBeEmpty;
                Result.EmptyCollectionTitle = this.EmptyCollectionTitle;
                Result.ExternalItemsSourceGetter = this.ItemsSourceGetter;  // This could be later changed for a per-instance source.
                Result.ExternalItemsSourceSelectedValuePath = this.ItemsSourceSelectedValuePath;
            }

            return Result;
        }

        internal override MCollectionController CreateProvider(MEntityInstanceController InstanceController)
        {
            return CreateController(InstanceController);
        }
    }
}