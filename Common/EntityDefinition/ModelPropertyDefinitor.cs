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
// File   : ModelPropertyDefinitor.cs
// Object : Instrumind.Common.EntityDefinition.ModelPropertyDefinitor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Manages and describes, at a static level, a member property belonging to a model class.
    /// </summary>
    /// <typeparam name="TModelClass">Type of the model class owning the property.</typeparam>
    /// <typeparam name="TValue">Type of the property value.</typeparam>
    public class ModelPropertyDefinitor<TModelClass, TValue> : MModelPropertyDefinitor where TModelClass : class, IModelClass<TModelClass>
    {
        /// <summary>
        /// Constructor for standard properties.
        /// </summary>
        public ModelPropertyDefinitor(string TechName, EEntityMembership Membership, bool? ReferencesOwner, EPropertyKind Kind,
                                      Func<TModelClass, TValue> BaseGetter,
                                      Action<TModelClass, TValue> BaseSetter,
                                      bool IsDirectlyEditable = true, bool IsAdvanced = false, string Name = "", string Summary = "", TValue InitialValue = default(TValue),
                                      Func<TValue, Tuple<bool, string>> Validator = null,
                                      bool IsRequired = false)
            : base(TechName, Name, Summary, typeof(TValue), Membership, ReferencesOwner, Kind, IsDirectlyEditable, IsAdvanced, IsRequired)
        {
            // Dummy properties has no setters.
            if (BaseGetter == null || BaseSetter == null)
                return;

            this.Getter = BaseGetter;
            this.Setter = ((ins, def, val) => { BaseSetter(ins, val); return true; });

            // sole post-interceptor
            AlterSetterForPostInterception("NotifyChanges", SetterPostInterceptor_NotifyChanges);

            // second pre-interceptor
            if (IsEditControlled)
                AlterSetterForPreInterception("SavePreviousValue", EntityEditEngine.PropertySetterPreInterceptor);
            /*T else
                    Console.WriteLine("No undoable/redoable: Class=[{0}], Property=[{1}]", typeof(TModelClass).Name, TechName); */

            // first pre-interceptor
            AlterSetterForPreInterception("IgnoreUnchanged", SetterPreInterceptor_IgnoreUnchanged);

            this.InitialValue = InitialValue;
            this.Validator = Validator;
        }

        /// <summary>
        /// Constructor for Store-Box based properties.
        /// </summary>
        public ModelPropertyDefinitor(string TechName, EEntityMembership Membership, bool? ReferencesOwner, EPropertyKind Kind,
                                      Func<TModelClass, StoreBox<TValue>> StoreBoxGetter,
                                      Action<TModelClass, StoreBox<TValue>> StoreBoxSetter,
                                      bool IsDirectlyEditable = true, bool IsAdvanced = false, string Name = "", string Summary = "",
                                      TValue InitialValue = default(TValue),
                                      Func<TValue, Tuple<bool, string>> Validator = null,
                                      bool IsRequired = false)
            : this(TechName, Membership, ReferencesOwner, Kind,
                   (ins) =>
                       { 
                           var Store = StoreBoxGetter(ins);
                           if (Store == null)
                               return default(TValue);

                           return Store.Value;
                       },
                   (ins, val) =>
                       {
                           var Store = StoreBoxGetter(ins);
                           if (Store == null)
                               return;

                           Store.Value = val;
                       },
                   IsDirectlyEditable, IsAdvanced, Name, Summary, InitialValue, Validator, IsRequired)
        {
            this.StoreBoxGetter = StoreBoxGetter;
            this.StoreBoxSetter = StoreBoxSetter;
        }

        /// <summary>
        /// Standard property setter preinterception function.
        /// It will stop the assignment if the property value and the supplied are equal.
        /// </summary>
        protected static Func<IMModelClass, MModelPropertyDefinitor, object, Tuple<bool, object>> SetterPreInterceptor_IgnoreUnchanged =
                            ((IMModelClass Instance, MModelPropertyDefinitor Definitor, object Value) =>
                            {
                                return (Tuple.Create<bool, object>(!Value.IsEqual(Definitor.Read(Instance)), Value));
                            });

        /// <summary>
        /// Standard property setter postinterception function.
        /// It will notify changes to the instance subscriptors.
        /// </summary>
        protected static Func<IMModelClass, MModelPropertyDefinitor, object, bool> SetterPostInterceptor_NotifyChanges =
                            ((IMModelClass Instance, MModelPropertyDefinitor Definitor, object Value) =>
                            {
                                var Entity = Instance as IModelEntity;

                                if (!MModelClassDefinitor.IsPassiveInstance(Instance))
                                    Instance.NotifyPropertyChange(Definitor);

                                return true;
                            });

        /// <summary>
        /// Register the owner declarator of this member/definitor and assemble any related behaviors (such as setter interception).
        /// </summary>
        internal override void AssignDeclarator(MModelClassDefinitor DeclaringDefinitor)
        {
            this.DeclaringDefinitor = DeclaringDefinitor;
        }

        /// <summary>
        /// Function which returns the property container store-box from a supplied instance.
        /// </summary>
        public Func<TModelClass, StoreBox<TValue>> StoreBoxGetter { get; protected set; }

        /// <summary>
        /// Function which sets the property container store-box for a supplied instance.
        /// </summary>
        public Action<TModelClass, StoreBox<TValue>> StoreBoxSetter { get; protected set; }
        
        /// <summary>
        /// Function which returns the property value from a supplied instance.
        /// </summary>
        public Func<TModelClass, TValue> Getter { get; protected set; }

        /// <summary>
        /// Function which sets the passed value into the property of a supplied instance and returns indication of whether the setting was made.
        /// </summary>
        public Func<TModelClass, ModelPropertyDefinitor<TModelClass, TValue>, TValue, bool> Setter { get; protected set; }

        /// <summary>
        /// Initial value required prior to use.
        /// </summary>
        public TValue InitialValue { get; protected set; }

        /// <summary>
        /// Validation function for the property.
        /// </summary>
        public Func<TValue, Tuple<bool, string>> Validator { get; protected set; }

        /// <summary>
        /// Returns the controlled property value of the supplied instance.
        /// </summary>
        public TValue Get(TModelClass Instance)
        {
            var Result = this.Getter(Instance);
            return Result;
        }

        /// <summary>
        /// Sets the passed value into the controlled property of the supplied instance.
        /// Returns indication of whether the setting was performed.
        /// </summary>
        public bool Set(TModelClass Instance, TValue Value)
        {
            // IMPORTANT: This is an ugly trick to prevent a null value
            // generated (by databinding) between row-remove + row-insert while referesh.
            if (Visualization.Widgets.ItemsGridMaintainer.IsRefreshingMaintainerRow)
                return false;

            return this.Setter(Instance, this, Value);
        }

        public override object Read(IMModelClass Instance)
        {
            var Result = this.Get((TModelClass)Instance);
            return Result;
        }

        public override void Write(IMModelClass Instance, object Value)
        {
            this.Set((TModelClass)Instance, (TValue)Value);
        }

        public override bool HasRichContent { get; set; }

        public override bool IsStoreBoxBased { get { return this.StoreBoxGetter != null; } }

        public override StoreBoxBase GetStoreBoxContainer(IMModelClass Instance)
        {
            if (this.StoreBoxGetter == null || !(Instance is TModelClass))
                throw new UsageAnomaly("StoreBox Container does not exist for property '"
                                       + this.DeclaringDefinitor.TechName + "." + this.TechName + "'.");

            return this.StoreBoxGetter((TModelClass)Instance);
        }

        public override void SetStoreBoxBaseContainer(IMModelClass Instance, StoreBoxBase Container)
        {
            this.SetStoreBoxContainer((TModelClass)Instance, (StoreBox<TValue>)Container);
        }

        public void SetStoreBoxContainer(TModelClass Instance, StoreBox<TValue> Container)
        {
            if (Container != null && Instance is TModelClass)
                this.StoreBoxSetter(Instance, Container);
        }

        public override Type DataType { get { return typeof(TValue); } }

        /// <summary>
        /// Ecapsulate the current setter function with a preexecuting interceptor.
        /// </summary>
        public void AlterSetterForPreInterception(string Purpose, Func<IMModelClass, MModelPropertyDefinitor, object, Tuple<bool, object>> SetterPreInterceptor)
        {
            if (this.AssignedPreInterceptors.Contains(Purpose))
                return;

            this.AssignedPreInterceptors.Add(Purpose);
            var OriginalSetter = this.Setter;
            this.Setter = ((TModelClass Instance,  ModelPropertyDefinitor<TModelClass, TValue> Definitor, TValue ReceivedValue) =>
                           {
                               //T Console.WriteLine("** Setter of '" + this.Name + "' preintercepted for: " + Purpose);
                               var InterceptionResult = SetterPreInterceptor((IMModelClass)Instance, (MModelPropertyDefinitor)Definitor, ReceivedValue);

                               if (InterceptionResult.Item1)
                                   return OriginalSetter(Instance, Definitor, (TValue)InterceptionResult.Item2);

                               return false;
                           });
            //T Console.WriteLine("Setter of Property '" + this.Name + "'" + (this.DeclaringDefinitor == null ? " " : " (Class '" + this.DeclaringDefinitor.Name + "') ") +
            //T                   "assigned for preinterception. Purpose: " + Purpose);
        }
        private List<string> AssignedPreInterceptors = new List<string>();

        /// <summary>
        /// Ecapsulate the current setter function with a postexecuting interceptor.
        /// </summary>
        public void AlterSetterForPostInterception(string Purpose, Func<IMModelClass, MModelPropertyDefinitor, object, bool> SetterPostInterceptor)
        {
            if (this.AssignedPostInterceptors.Contains(Purpose))
                return;

            this.AssignedPostInterceptors.Add(Purpose);
            var OriginalSetter = this.Setter;
            this.Setter = ((TModelClass Instance, ModelPropertyDefinitor<TModelClass, TValue> Definitor, TValue ReceivedValue) =>
                           {
                               if (OriginalSetter(Instance, Definitor, ReceivedValue))
                               {
                                   var Result = SetterPostInterceptor((IMModelClass)Instance, Definitor, ReceivedValue);
                                   //T Console.WriteLine("** Setter of '" + this.Name + "' postintercepted for: " + Purpose);
                                   return Result;
                               }

                               return false;
                           });
            //T Console.WriteLine("Setter of Property '" + this.Name + "'" + (this.DeclaringDefinitor == null ? " " : " (Class '" + this.DeclaringDefinitor.Name + "') ") +
            //T                   "assigned for postinterception. Purpose: " + Purpose);
        }
        private List<string> AssignedPostInterceptors = new List<string>();

        /// <summary>
        /// Evaluates the supplied value and returns its validation result.
        /// </summary>
        public Tuple<bool, string> Validate(TValue Value)
        {
            if (this.Validator == null)
                return (Tuple.Create<bool, string>(true, ""));

            return this.Validator(Value);
        }

        /// <summary>
        /// Creates and returns a controller for the defined property and supplied instance controller.
        /// </summary>
        public PropertyController<TModelClass, TValue> CreateController(MEntityInstanceController InstanceController)
        {
            var Result = new PropertyController<TModelClass, TValue>(InstanceController);

            Result.PropertyDefinitor = this;

            Result.IsEditableNow = this.IsEditControlled;

            Result.DefaultValue = this.InitialValue;
            Result.IndicateDefaultValue = !this.InitialValue.IsEqual(default(TValue));

            var Validation = this.Validate(this.InitialValue);
            Result.IsValid = Validation.Item1;
            Result.StatusMessage = Validation.Item2;
            Result.IndicateValidation = (this.Validator != null);

            if (this.ItemsSourceGetter != null)
            {
                Result.CanCollectionBeEmpty = this.CanCollectionBeEmpty;
                Result.EmptyCollectionTitle = this.EmptyCollectionTitle;
                Result.ExternalItemsSourceGetter = this.ItemsSourceGetter;  // This could be later changed for a per-instance source.
                Result.ExternalItemsSourceSelectedValuePath = this.ItemsSourceSelectedValuePath;
                Result.ExternalItemsSourceDisplayMemberPath = this.ItemsSourceDisplayMemberPath;
            }

            return Result;
        }

        internal override MPropertyController CreateProvider(MEntityInstanceController InstanceController)
        {
            return CreateController(InstanceController);
        }

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner)
        {
            return this.MemberwiseClone();
        }
    }
}