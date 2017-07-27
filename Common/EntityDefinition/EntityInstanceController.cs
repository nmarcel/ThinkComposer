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
// File   : ModelEntityController.cs
// Object : Instrumind.Common.EntityDefinition.ModelEntityController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Utility class for creating entity instance controllers.
    /// </summary>
    public static class EntityInstanceController
    {
        /// <summary>
        /// Returns a entity instance controller, which is created and assigned if is not already present, for the specified Entity Instance.
        /// </summary>
        public static EntityInstanceController<TModelEntity>
                        AssignInstanceController<TModelEntity>(TModelEntity EntityInstance,
                                                               Func<TModelEntity, TModelEntity, IEnumerable<object>, bool> PreApply = null,
                                                               ECloneOperationScope CloningScope = ECloneOperationScope.Slight)
                where TModelEntity : class, IModelEntity, IModelClass<TModelEntity>
        {
            var Result = (EntityInstanceController<TModelEntity>)EntityInstance.Controller;

            if (Result == null)
            {
                Result = new EntityInstanceController<TModelEntity>(EntityInstance, PreApply, CloningScope);
                EntityInstance.Controller = Result;
            }
            else
            {
                Result.PreApply = PreApply;
                Result.CloningScope = CloningScope;
            }

            return Result;
        }

        /// <summary>
        /// Dynamic invocation for create, set and return an instance controller for the specified entity.
        /// Prefer to call AssignInstanceController() if you know the entity type at compile time.
        /// </summary>
        public static MEntityInstanceController SetInstanceController(IModelEntity EntityInstance)
        {
            MethodInfo AssignmentMethod = typeof(EntityInstanceController).GetMethod("AssignInstanceController");
            AssignmentMethod = AssignmentMethod.MakeGenericMethod(EntityInstance.GetType());

            var Result = (MEntityInstanceController)AssignmentMethod.Invoke(null, new object[] { EntityInstance, null,
                                                                                                 ECloneOperationScope.Slight });
            EntityInstance.Controller = Result;

            return Result;
        }
    }

    /// <summary>
    /// Centralizes editing management for entity instances,
    /// Controls states, validations and interactive consuption as required by the controllers exposed by entity types.
    /// </summary>
    /// <typeparam name="TModelEntity">Type of the controlled entity.</typeparam>
    public class EntityInstanceController<TModelEntity> : MEntityInstanceController
        where TModelEntity : class, IModelEntity, IModelClass<TModelEntity>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public EntityInstanceController(TModelEntity ControlledEntity,
                                        Func<TModelEntity, TModelEntity, IEnumerable<object>, bool> PreApply = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight)
             : base(ControlledEntity, CloningScope)
        {
            General.ContractRequiresNotNull(ControlledEntity, EntityEditor);

            this.WorkingEntityInstance = ControlledEntity;
            this.PreApply = PreApply;

            // IMPORTANT: Reference to ClassDefinition instead of ClassDefinitor for getting Properties and Collections.
            //            Else, the generic type will be getted instead of the virtual most derived one.

            // Generates the instance controllers for the properties.
            foreach (var Prop in this.WorkingEntityInstance.ClassDefinition.Properties)
            {
                var Provider = Prop.CreateProvider(this);
                this.ControlledProperties_.Add(Prop.TechName, Provider);

                if (Provider.Definitor.Membership == EEntityMembership.InternalCoreExclusive
                    && Provider.Definitor.DataType.GetInterfaces().Any(itf => itf == typeof(IModelEntity)))
                {
                    var ChildEntityDef = MModelClassDefinitor.GetDefinitor(Provider.Definitor.DataType);

                    foreach (var ChildProp in ChildEntityDef.Properties)
                        this.ControlledProperties_.Add(Prop.TechName + "." + ChildProp.TechName, ChildProp.CreateProvider(this));

                    foreach (var ChildColl in ChildEntityDef.Collections)
                        this.ControlledCollections_.Add(Prop.TechName + "." + ChildColl.TechName, ChildColl.CreateProvider(this));
                }
            }

            // Generates the instance controllers for the collections.
            foreach (var Coll in this.WorkingEntityInstance.ClassDefinition.Collections)
                this.ControlledCollections_.Add(Coll.TechName, Coll.CreateProvider(this));
        }

        /// <summary>
        /// References a post-validate function for extend the standard validation declared at model-class level.
        /// Arguments: Current and Previous model-entities states.
        /// Return: List of errors, or null when valid.
        /// </summary>
        [NonSerialized]
        public Func<TModelEntity, TModelEntity, IList<string>> PostValidate = null;

        /// <summary>
        /// References a pre-apply function for update propagation to dependants and/or validation.
        /// Arguments: Current and Previous model-entities states, plus general arguments.
        /// Return: Indication of success (for continue to apply).
        /// </summary>
        [NonSerialized]
        public Func<TModelEntity, TModelEntity, IEnumerable<object>, bool> PreApply = null;

        /// <summary>
        /// References the represented business entity.
        /// </summary>
        public TModelEntity WorkingEntityInstance { get { return this.WorkingEntityInstance_; } protected set { this.WorkingEntityInstance_ = value; } }
        [NonSerialized]
        private TModelEntity WorkingEntityInstance_ = default(TModelEntity);

        /// <summary>
        /// References the previous state of the represented business entity.
        /// </summary>
        public TModelEntity PreviousEntityState
        {
            get { return this.PreviousEntityState_; }
            protected set
            {
                if (this.PreviousEntityState_ == value)
                    return;

                MModelClassDefinitor.UnregisterPassiveInstance(this.PreviousEntityState_);

                this.PreviousEntityState_ = value;
            }
        }
        [NonSerialized]
        private TModelEntity PreviousEntityState_ = default(TModelEntity);

        /// <summary>
        /// Externallly provided reading function to obtain an stored entity based in the passed.
        /// The function will take key values from that supplied (entity) to retrieve the requested entity.
        /// </summary>
        public Func<TModelEntity, TModelEntity> Reader;

        /// <summary>
        /// Externally provided writing action to give for store the supplied  entity.
        /// </summary>
        public Action<TModelEntity> Writer;

        private CommandVariation OngoingEditingCommand = null;

        private int InitialVersionAlterationsCount = -1;

        public override void StartEdit()
        {
            this.IsEditing = true;
            this.ChangesWereApplied = false;

            this.EntityEditor.StartCommandVariation("Edit Entity.");
            this.OngoingEditingCommand = this.EntityEditor.CurrentVariatingCommand;

            // IMPORTANT: The clone is created as not active, implicitly registering the instance as Passive.
            //            Thus the clone changes will not be notified nor stored for undo/redo.
            this.PreviousEntityState = this.WorkingEntityInstance.CreateClone(this.CloningScope, null, false);

            /*?
            if (this.DependantEntityViewChilds != null)
                foreach (var ViewChild in this.DependantEntityViewChilds)
                    ViewChild.Refresh(); */

            if (this.PostStartEditOperations != null)
                foreach (var Operation in this.PostStartEditOperations)
                    Operation.Value(this.WorkingEntityInstance, this.WorkingEntityInstance);

            this.InitialVersionAlterationsCount = this.EntityEditor.VersionAlterationsCount;
        }

        public override IList<string> Validate()
        {
            var Errors = this.WorkingEntityInstance.ClassDefinitor.Validate(this.WorkingEntityInstance)
                                    .NullDefault(new List<string>());

            if (this.PostValidate != null)
                Errors = Errors.Concat(this.PostValidate(this.WorkingEntityInstance, this.PreviousEntityState)
                                        .NullDefault(Enumerable.Empty<string>())).ToList();

            this.ErrorMessage = Errors.GetConcatenation(null, "\n");

            this.IsValid = !Errors.Any();

            if (!this.IsValid)
                this.ViewInstance.ShowMessage("Invalid input",
                                              this.ErrorMessage, EMessageType.Error, null);

            return Errors;
        }

        public override bool ApplyEdit(params object[] Parameters)
        {
            if (this.OngoingEditingCommand == null)
                throw new UsageAnomaly("No edit command is being performed.");

            if (this.OngoingEditingCommand != this.EntityEditor.CurrentVariatingCommand)
                throw new UsageAnomaly("Mismatch between initiated editing command and the current variating.");

            var Errors = Validate();

            if (Errors == null || !Errors.Any())
            {
                var ApplySuccess = true;

                if (this.PreApply != null)
                    ApplySuccess = this.PreApply(this.WorkingEntityInstance, this.PreviousEntityState, Parameters);

                if (ApplySuccess && this.DependantEntityViewChilds != null)
                    foreach (var ViewChild in this.DependantEntityViewChilds)
                        if (!ViewChild.Apply())
                        {
                            ApplySuccess = false;
                            break;
                        }

                if (ApplySuccess && this.PostApplyEditOperations != null)
                    foreach (var Operation in this.PostApplyEditOperations)
                        if (!Operation.Value(this.WorkingEntityInstance, this.PreviousEntityState))
                        {
                            ApplySuccess = false;
                            break;
                        }

                if (ApplySuccess)
                    this.ChangesWereApplied = true;
            }

            return this.IsValid;
        }

        public override void FinishEdit()
        {
            CloseView();

            if (this.OngoingEditingCommand != null
                && this.OngoingEditingCommand != this.EntityEditor.CurrentVariatingCommand)
                throw new UsageAnomaly("Mismatch between initiated editing command and the current variating.");

            if (this.IsEditing)
            {
                if (this.InitialVersionAlterationsCount < this.EntityEditor.VersionAlterationsCount)
                    if (this.WorkingEntityInstance is IVersionUpdater)
                        ((IVersionUpdater)this.WorkingEntityInstance).UpdateVersion();
                    else
                        if (this.WorkingEntityInstance is IFormalizedElement
                            && ((IFormalizedElement)this.WorkingEntityInstance).Version != null)
                            ((IFormalizedElement)this.WorkingEntityInstance).Version.Update();
    
                var CompletedEditing = this.EntityEditor.CompleteCommandVariation();
                this.OngoingEditingCommand = this.EntityEditor.CurrentVariatingCommand;

                if (!this.ChangesWereApplied)
                {
                    if (CompletedEditing != null)
                        this.EntityEditor.Undo(false, false);
                }

                this.IsEditing = (this.OngoingEditingCommand != null);
            }

            this.PreviousEntityState = null;     // Clears it from Passive entities registry.
        }

        public override void Help()
        {
            this.ViewInstance.ShowMessage("Sorry", "Help not yet implemented.");
        }

        public override void RegisterDependantEntityViewChild(IEntityViewChild Child)
        {
            if (this.DependantEntityViewChilds == null)
                this.DependantEntityViewChilds = new List<IEntityViewChild>();

            this.DependantEntityViewChilds.AddNew(Child);
        }
        private List<IEntityViewChild> DependantEntityViewChilds = null;

        public override void ResetDependantEntityViewChildren()
        {
            if (this.DependantEntityViewChilds != null)
                this.DependantEntityViewChilds.Clear();
        }

        public override void RegisterPostStartEditOperation(string Name, Action<IModelEntity, IModelEntity> Operation)
        {
            if (this.PostStartEditOperations == null)
                this.PostStartEditOperations = new Dictionary<string, Action<IModelEntity, IModelEntity>>();

            this.PostStartEditOperations.AddOrReplace(Name, Operation);
        }
        private Dictionary<string, Action<IModelEntity, IModelEntity>> PostStartEditOperations = null;

        public override void RegisterPostApplyEditOperation(string Name, Func<IModelEntity, IModelEntity, bool> Operation)
        {
            if (this.PostApplyEditOperations == null)
                this.PostApplyEditOperations = new Dictionary<string, Func<IModelEntity, IModelEntity, bool>>();

            this.PostApplyEditOperations.AddOrReplace(Name, Operation);
        }
        private Dictionary<string, Func<IModelEntity, IModelEntity, bool>> PostApplyEditOperations = null;
    }
}