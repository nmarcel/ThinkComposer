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
// File   : MEntityInstanceController.cs
// Object : Instrumind.Common.EntityDefinition.MEntityInstanceController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.04 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Represents the controller for an entity instance.
    /// (Intended to work around problems by lack of support for contra/co+variance and for internal usage only)
    /// </summary>
    public abstract class MEntityInstanceController
    {
        public const double EDITWND_INI_WIDTH = 500; // 790; // 650; // 950;
        public const double EDITWND_INI_HEIGHT = 400; // 592; // 550; // 720;

        /// <summary>
        /// String used for represent instance scoped errors, not related to a particular property.
        /// </summary>
        public const string INSTANCE_SCOPE = "*";

        /// <summary>
        /// Constructor.
        /// </summary>
        public MEntityInstanceController(IModelEntity EntityInstance, ECloneOperationScope CloningScope)
        {
            General.ContractRequiresNotNull(EntityInstance);

            this.ControlledInstance = EntityInstance;
            this.EntityEditor = EntityInstance.EditEngine;

            // Centralizes store-boxes references
            // IMPORTANT: The EntityEditor is used because exists while calling Constructors. So, do not use MainEditedEntity (Composition).
            foreach (var PropDef in EntityInstance.ClassDefinition.Properties.Where(prop => prop.IsStoreBoxBased))
                PropDef.GetStoreBoxContainer(EntityInstance).CentralizeReferencesIn(EntityEditor.GlobalId);

            this.CloningScope = CloningScope;

            this.ExistenceStatus = EExistenceStatus.Created;
            this.ControlledProperties = new ReadOnlyDictionary<string, MPropertyController>(this.ControlledProperties_);
            this.ControlledCollections = new ReadOnlyDictionary<string, MCollectionController>(this.ControlledCollections_);
            this.Errors = new ReadOnlyDictionary<string, string>(this.Errors_);
        }

        /// <summary>
        /// Returns the member controller requested by name.
        /// Optionally it can return null if member not found or fail.
        /// </summary>
        public MMemberController GetMemberController(string MemberName, bool ReturnNullIfNotFound = true)
        {
            if (!this.ControlledProperties.ContainsKey(MemberName))
                if (!this.ControlledCollections.ContainsKey(MemberName))
                    if (ReturnNullIfNotFound)
                        return null;
                    else
                        throw new UsageAnomaly("Cannot get requested member because it is not defined in the class",
                                               new DataWagon("Member", MemberName).Add("Class", this.ControlledInstance.ClassDefinition.Name));
                else
                    return this.ControlledCollections[MemberName];

            return this.ControlledProperties[MemberName];
        }

        /// <summary>
        /// Returns the property controller requested by name.
        /// Optionally it can return null if property not found or fail.
        /// </summary>
        public MPropertyController GetPropertyController(string MemberName, bool ReturnNullIfNotFound = true)
        {
            if (!this.ControlledProperties.ContainsKey(MemberName))
                if (ReturnNullIfNotFound)
                    return null;
                else
                    throw new UsageAnomaly("Cannot get requested property because it is not defined in the class",
                                           new DataWagon("Property", MemberName).Add("Class", this.ControlledInstance.ClassDefinition.Name));

            return this.ControlledProperties[MemberName];
        }

        /// <summary>
        /// Returns the collection controller requested by name.
        /// Optionally it can return null if collection not found or fail.
        /// </summary>
        public MCollectionController GetCollectionController(string MemberName, bool ReturnNullIfNotFound = true)
        {
            if (!this.ControlledCollections.ContainsKey(MemberName))
                if (ReturnNullIfNotFound)
                    return null;
                else
                    throw new UsageAnomaly("Cannot get requested Collection because it is not defined in the class",
                                           new DataWagon("Collection", MemberName).Add("Class", this.ControlledInstance.ClassDefinition.Name));

            return this.ControlledCollections[MemberName];
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Associated editing engine.
        /// </summary>
        public EntityEditEngine EntityEditor { get; protected set; }

        /// <summary>
        /// References the entity instance being controlled.
        /// </summary>
        public IModelEntity ControlledInstance { get; protected set; }

        /// <summary>
        /// Current view exposing the entity for interaction.
        /// </summary>
        public IEntityView ViewInstance { get; protected set; }

        /// <summary>
        /// Level of entity content cloning for the instance.
        /// </summary>
        public ECloneOperationScope CloningScope { get; internal set; }

        /// <summary>
        /// Current state of persistence related existence of the entity.
        /// </summary>
        public EExistenceStatus ExistenceStatus { get; protected set; }

        /// <summary>
        /// Validation status of the controlled instance.
        /// </summary>
        public bool IsValid { get; protected set; }

        /// <summary>
        /// Error message of the controlled instance.
        /// </summary>
        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// Collection of exposed properties for consumption via its controlling providers.
        /// </summary>
        public ReadOnlyDictionary<string, MPropertyController> ControlledProperties { get; protected set; }
        protected Dictionary<string, MPropertyController> ControlledProperties_ = new Dictionary<string, MPropertyController>();

        /// <summary>
        /// Collection of exposed collections for consumption via its controlling providers.
        /// </summary>
        public ReadOnlyDictionary<string, MCollectionController> ControlledCollections { get; protected set; }
        protected Dictionary<string, MCollectionController> ControlledCollections_ = new Dictionary<string, MCollectionController>();

        /// <summary>
        /// Associations between each Reference Property and their items Source.
        /// </summary>
        //- protected readonly Dictionary<string, IEnumerable> ReferencePropertyItemsSources = new Dictionary<string, IEnumerable>();

        /// <summary>
        /// Current list of errors found (stored as a collection of property+error items).
        /// </summary>
        public ReadOnlyDictionary<string, string> Errors { get; protected set; }
        protected Dictionary<string, string> Errors_ = new Dictionary<string, string>();

        /// <summary>
        /// Do a complete editing operation.
        /// A dialog-window will be shown and, when returned, the may already been applied to the entity instance.
        /// Returns indication of success, null when the dialog is cancelled.
        /// </summary>
        public bool? Edit(EntityEditPanel Content, string Title, bool FinishEditingCommand = true, Window Owner = null,
                          double InitialWidth = double.NaN, double InitialHeight = double.NaN)
        {
            if (!this.IsEditing)
                this.StartEdit();

            Content.AssociateEntityViewChildren();

            var Result = OpenView<EntityEditPanel>(Content, Title, true, Owner,
                                                   InitialWidth.NaNDefault(EDITWND_INI_WIDTH),
                                                   InitialHeight.NaNDefault(EDITWND_INI_HEIGHT));

            if (this.IsEditing && FinishEditingCommand)
                this.FinishEdit();

            return Result;
        }

        /// <summary>
        /// Indicates whether this has started an editing operation for the controlled entity.
        /// </summary>
        public bool IsEditing { get; protected set; }

        /// <summary>
        /// Starts the editing of the entity by making an internal clone for storing its previous state.
        /// </summary>
        public abstract void StartEdit();

        /// <summary>
        /// Validates the entity and returns error messages list, or null when valid.
        /// </summary>
        public abstract IList<string> Validate();

        /// <summary>
        /// Indicates if changes were applied in the last call to Apply().
        /// </summary>
        public bool ChangesWereApplied { get; protected set; }

        /// <summary>
        /// Applies the changes made during the edition operation, passing optional Parameters, and returns true if succeeded.
        /// Any errors detected can be recovered from the Errors property.
        /// If successfully applied, then the edit command is registered, else remains open (variating).
        /// Must be called after a StartEdit() call to initiate the edit command variation.
        /// </summary>
        public abstract bool ApplyEdit(params object[] Parameters);

        /// <summary>
        /// Finishes the performed editing and undo the changes if they are not already applied.
        /// </summary>
        public abstract void FinishEdit();

        /// <summary>
        /// Exposes entity related documentation for help purposes.
        /// </summary>
        public abstract void Help();

        /// <summary>
        /// Register a new dependant Entity View Child
        /// </summary>
        public abstract void RegisterDependantEntityViewChild(IEntityViewChild Child);

        /// <summary>
        /// Resets the registered Entity View Children.
        /// </summary>
        public abstract void ResetDependantEntityViewChildren();

        /// <summary>
        /// Registers an operation name and action to be called after Prepare for edit.
        /// It receives the current and previous entities.
        /// </summary>
        public abstract void RegisterPostStartEditOperation(string Name, Action<IModelEntity, IModelEntity> Operation);

        /// <summary>
        /// Registers an operation name and function to be called after Applying changes.
        /// It receives the current and previous entities. Returns indication of success/failure.
        /// </summary>
        public abstract void RegisterPostApplyEditOperation(string Name, Func<IModelEntity, IModelEntity, bool> Operation);

        /// <summary>
        /// Opens the entity related view from content, which is also the view-instance, and other parameters.
        /// </summary>
        protected bool? OpenView<TForm>(TForm Content, string Title,
                                        bool OpenAsDialog = false, Window Owner = null,
                                        double InitialWidth = double.NaN, double InitialHeight = double.NaN) where TForm : UIElement, IEntityView
        {
            return OpenView(Content, Content, Title, OpenAsDialog, Owner, InitialWidth, InitialHeight);
        }

        /// <summary>
        /// Opens the entity related view from content, view-instance and other parameters.
        /// </summary>
        protected bool? OpenView<TForm>(TForm Content, IEntityView ViewInstance, string Title,
                                        bool OpenAsDialog = false, Window Owner = null,
                                        double InitialWidth = double.NaN, double InitialHeight = double.NaN) where TForm : UIElement
        {
            bool? Result = null;

            this.ViewInstance = ViewInstance;
            DialogOptionsWindow Dialog = null;

            if (OpenAsDialog)
                Result = Display.OpenContentDialogWindow<TForm>(ref Dialog, Title, Content, InitialWidth, InitialHeight);
            else
            {
                Display.OpenContentWindow<TForm>(ref this.ViewWindow_, Content, Title, Owner);

                if (this.ViewWindow_ != null)
                {
                    this.ViewWindow_.Width = InitialWidth;
                    this.ViewWindow_.Height = InitialHeight;
                }
            }

            return Result;
        }
        public BasicWindow ViewWindow { get { return this.ViewWindow_; } protected set { this.ViewWindow_ = value; } }
        private BasicWindow ViewWindow_ = null;

        /// <summary>
        /// Closes the entity related view.
        /// </summary>
        protected void CloseView()
        {
            // PENDING: Close
        }
    }
}