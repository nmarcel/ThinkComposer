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
// File   : EntityEditEngine.cs
// Object : Instrumind.Common.EntityBase.EntityEditEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using System.Windows.Input;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Provides the general mechanism for the application of editing operations
    /// (assignments or calling, individual or composited), supporting their undoing and redoing.
    /// </summary>
    public abstract class EntityEditEngine
    {
        public const int DEFAULT_UNDOS = 50;
        public const int DEFAULT_REDOS = 50;

        public static int GlobalMaxUndos = DEFAULT_UNDOS;
        public static int GlobalMaxRedos = DEFAULT_REDOS;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static EntityEditEngine()
        {
            GlobalMaxUndos = AppExec.GetConfiguration<int>("Application", "MaxUndos", DEFAULT_UNDOS);
            GlobalMaxRedos = AppExec.GetConfiguration<int>("Application", "MaxRedos", DEFAULT_REDOS);
        }

        /// <summary>
        /// Entity editing engine currently being used.
        /// </summary>
        public static EntityEditEngine ActiveEntityEditor
        {
            get { return ActiveEntityEditor_;  }
            set
            {
                if (value == ActiveEntityEditor_)
                    return;

                //T Console.WriteLine("NEW ACTIVE ENTITY-EDITOR: " + (value == null ? "<NONE>" : value.GetHashCode().ToString()));
                ActiveEntityEditor_ = value;
            }
        }
        private static EntityEditEngine ActiveEntityEditor_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets and possibly initializes, for the supplied Entity-Instance, its Edit-Engine based on the specified Current.
        /// </summary>
        public static EntityEditEngine ObtainEditEngine(IModelEntity EntityInstance, EntityEditEngine Current)
        {
            // IMPORTANT: Do not read Entity.AssignedEditEngine. An infinite-loop may occur.
            //            Instead use the Current parameter.

            // PENDING: Solve mismatch crash for:
            // Change of document and immediate copy of objects.

            if (Current == null && ActiveEntityEditor != null
                && (ActiveEntityEditor.ExecutionStatus == EExecutionStatus.Running
                    || ActiveEntityEditor.ExecutionStatus == EExecutionStatus.Created))
            {
                EntityInstance.EditEngine = Current = ActiveEntityEditor;

                // Centralizes store-boxes references
                // IMPORTANT: The EntityEditor is used because exists while calling Constructors. So, do not use MainEditedEntity (Composition).
                if (Current != null)
                    foreach (var PropDef in EntityInstance.ClassDefinition.Properties.Where(prop => prop.IsStoreBoxBased))
                        PropDef.GetStoreBoxContainer(EntityInstance).CentralizeReferencesIn(Current.GlobalId);
            }
            /*T ONLY RELEVANT WHEN MODIFYING VALUES.
                SEE RegisterInverseAssignment() and RegisterInverseCollectionChange().
            else*/
            /*
                if (Current != ActiveEntityEditor)
                    Console.WriteLine("Active Entity-Editor differs from that of Entity '" + EntityInstance.ToStringAlways() + "'."); */

            return Current;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Entity property setter preinterception function exposed.
        /// This will store the previous value, so it will be ready for Undo.
        /// </summary>
        public static Func<IMModelClass, MModelPropertyDefinitor, object, Tuple<bool, object>> PropertySetterPreInterceptor =
                ((IMModelClass Instance, MModelPropertyDefinitor Definitor, object Value) =>
                {
                    var Entity = Instance as IModelEntity;

                    if (Entity != null && Entity.EditEngine != null
                        && Entity.EditEngine.ExecutionStatus == EExecutionStatus.Running
                        && !MModelClassDefinitor.IsPassiveInstance(Entity))
                    {
                        //T Console.WriteLine(">PreIntercepting [{0}] : [{1}] = [{2}]", Definitor, Entity, Value);
                        EntityEditEngine.RegisterInverseAssignment(Definitor, Entity, Definitor.Read(Instance));
                    }
                    /*T else
                            Console.WriteLine("Not registering editing change for '{0}' ({1}).", Instance, (Instance == null ? null : Instance.GetType().Name)); */

                    return (Tuple.Create<bool, object>(true, Value));
                });

        //--------------------------------------------------------------------------------------------------------------------
        public EntityEditEngine(int MaxUndos = -1, int MaxRedos = -1)
        {
            this.GlobalId = Guid.NewGuid();

            if (MaxUndos < 0)
                MaxUndos = GlobalMaxUndos;

            if (MaxRedos < 0)
                MaxRedos = GlobalMaxRedos;

            this.ExecutionStatus = EExecutionStatus.Created;
            this.ExistenceStatus = EExistenceStatus.Created;
            this.UndoStack = new ConstrainedStack<CommandVariation>(MaxUndos, true);
            this.RedoStack = new ConstrainedStack<CommandVariation>(MaxRedos, true);
        }

        /*? /// <summary>
        /// Occurs after this engine is activated (when opened or because other engine was deactivated).
        /// </summary>
        public abstract void Activation();

        /// <summary>
        /// Occurs prior this engine is deactivated (when closed or because other engine will be activated).
        /// </summary>
        public abstract void Deactivation(); */

        /// <summary>
        /// Starts the execution.
        /// </summary>
        public virtual void Start()
        {
            this.ExecutionStatus = EExecutionStatus.Running;
        }

        /// <summary>
        /// Pauses the execution.
        /// </summary>
        public virtual void Pause()
        {
            this.ExecutionStatus = EExecutionStatus.Paused;
        }

        /// <summary>
        /// Resumes the execution.
        /// </summary>
        public virtual void Resume()
        {
            this.ExecutionStatus = EExecutionStatus.Running;
        }

        /// <summary>
        /// Stops the execution.
        /// </summary>
        public virtual void Stop()
        {
            this.ExecutionStatus = EExecutionStatus.Stopped;
        }

        /// <summary>
        /// Global-ID of this Engine.
        /// </summary>
        public Guid GlobalId { get; protected set; }

        /// <summary>
        /// Indicates to get Tech-Names of all entities, within scope of this edit-engine, as programming language identifiers instead of file names.
        /// </summary>
        public bool ReadTechNamesAsProgramIdentifiers { get; set; }

        /// <summary>
        /// Status of execution of the editing operations.
        /// </summary>
        public EExecutionStatus ExecutionStatus { get; private set; }

        /// <summary>
        /// Status of existence of the managed document, regards its modifications and saving.
        /// </summary>
        public EExistenceStatus ExistenceStatus { get; set; }

        /// <summary>
        /// Gets or sets the current count of version alterations since last reset.
        /// </summary>
        public int VersionAlterationsCount { get { return this.VersionAlterationsCount_; } protected set { this.VersionAlterationsCount_ = value; } }
        private int VersionAlterationsCount_ = 0;

        /*-? /// <summary>
        /// Increments the version alterations count and returns the current count.
        /// </summary>
        public int IncrementVersionAlterationsCount()
        {
            this.VersionAlterationsCount_++;
            return this.VersionAlterationsCount_;
        } */

        /// <summary>
        /// Occurs when the entity changes (the previous and current status are informed).
        /// </summary>
        public event Action<EExistenceStatus, EExistenceStatus> EntityChanged;

        /// <summary>
        /// Target main entity, for example a document, being edited.
        /// </summary>
        public abstract ISphereModel TargetDocument { get; }

        /// <summary>
        /// Triggered after the Main Edited-Entity has been changed.
        /// </summary>
        public abstract event Action<EntityEditEngine> MainEditedEntityChanged;

        /// <summary>
        /// Established maximum number of undoable variations.
        /// </summary>
        public int MaxUndos { get { return this.UndoStack.MaxSize; } }

        /// <summary>
        /// Established maximum number of redoable variations.
        /// </summary>
        public int MaxURedos { get { return this.RedoStack.MaxSize; } }

        /// <summary>
        /// Stack for nested temporal combined variations being registered.
        /// </summary>
        private Stack<CommandVariation> CommandCombiningStack = new Stack<CommandVariation>();

        /// <summary>
        /// Constrained overloadable (lossy) stack registering the editing operations which can be undone.
        /// </summary>
        private ConstrainedStack<CommandVariation> UndoStack = null;

        /// <summary>
        /// Constrained overloadable (lossy) stack registering the editing operations which can be redone.
        /// </summary>
        private ConstrainedStack<CommandVariation> RedoStack = null;

        /// <summary>
        /// Indicates whether this engine has an editing command currently being performed, thus pending of completion or discard.
        /// </summary>
        public bool IsVariating { get { return (this.CommandCombiningStack.Count > 0); } }

        /// <summary>
        /// Indicates whether this engine is currently undoing editing operations.
        /// </summary>
        public bool IsUndoing { get; protected set; }
        
        /// <summary>
        /// Indicates whether this engine is currently redoing editing operations.
        /// </summary>
        public bool IsRedoing { get; protected set; }

        /// <summary>
        /// Gets the current command being variating (declared), if any.
        /// </summary>
        public CommandVariation CurrentVariatingCommand { get { return (this.CommandCombiningStack.Count > 0 ? this.CommandCombiningStack.Peek() : null); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Registers the inverse assignment (READY TO BE UNDONE) of a supplied controlled property and its owning instance, to be undone/redone in the future.
        /// This must happen  as part of a grouping combined variation (a command being performed).
        /// Also, it will not be performed if already an undo/redo operation is running.
        /// </summary>
        internal static void RegisterInverseAssignment(MModelPropertyDefinitor SourcePropertyDef, IModelEntity Instance, object Value)
        {
            // If no engine controlling or not within command variation declaration then abandon.
            if (Instance.EditEngine == null || Instance.EditEngine.ExecutionStatus != EExecutionStatus.Running)
                return;

            if (!Instance.EditEngine.IsVariating)
                {
                    //T Console.WriteLine("No variating at property change.");
                    return;
                }

            // IMPORTANT: EDIT-ENGINES MUST MATCH FOR CHANGING VALUES.
            if (Instance.EditEngine != ActiveEntityEditor)
                throw new UsageAnomaly("Active Entity-Editor differs (for assignment) from that of the changing Entity '" + Instance.ToStringAlways() + "'.");

            // Stores the variation into the command being declared.
            Instance.EditEngine.StoreVariation(new AssignmentVariation(SourcePropertyDef, Instance, Value), SourcePropertyDef.ChangesExistenceStatus);
        }

        /// <summary>
        /// Registers the inverse change (READY TO BE UNDONE) of a supplied controlled collection and its owning instance, to be undone/redone in the future.
        /// This must happen  as part of a grouping combined variation (a command being performed).
        /// Also, it will not be performed if already an undo/redo operation is running.
        /// </summary>
        public static void RegisterInverseCollectionChange(IModelEntity Instance, EditableCollection SourceCollection, char AlterCode, params object[] Parameters)
        {
            if (Instance == null || Instance.EditEngine == null
                || Instance.EditEngine.ExecutionStatus != EExecutionStatus.Running)
                return;

            // If no engine controlling or not within command variation declaration then abandon.
            if (!Instance.EditEngine.IsVariating)
            {
                //T Console.WriteLine("No variating at collection change.");
                return;
            }

            // IMPORTANT: EDIT-ENGINES MUST MATCH FOR CHANGING VALUES.
            if (Instance.EditEngine != ActiveEntityEditor)
                throw new UsageAnomaly("Active Entity-Editor differs (for collection) from that of the changing Entity '" + Instance.ToStringAlways() + "'.");

            // Stores the variation into the command being declared.
            var MemberDef = SourceCollection.VariatingInstance.ClassDefinition.GetMemberDef(SourceCollection.Name, false);
            Instance.EditEngine.StoreVariation(new CollectionVariation(SourceCollection, AlterCode, Parameters),
                                                   (MemberDef != null && MemberDef.ChangesExistenceStatus));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stores the supplied variation into the current uncompleted command variation.
        /// </summary>
        private void StoreVariation(Variation CollectedVariation, bool ChangesExistenceStatus = false)
        {
            // Validate that a command (possibly nested) is being declared
            if (!this.IsVariating)
                throw new UsageAnomaly("Cannot collect variation within command because none is being declared.");

            // Adds the new variation to the current declaring command
            this.CurrentVariatingCommand.AddVariation(CollectedVariation);

            if (ChangesExistenceStatus && this.CurrentVariatingCommand != null &&
                this.CurrentVariatingCommand.AlterExistenceStatusWhileVariating)
            {
                // If the status changed, then register that and notify possible subscribers
                this.ExistenceStatus = EExistenceStatus.Modified;
                this.VersionAlterationsCount = this.VersionAlterationsCount + 1;
            }

            if (this.ExistenceStatus != this.PreviousExistenceStatus)
            {
                this.PreviousExistenceStatus = this.ExistenceStatus;

                var Handler = this.EntityChanged;
                if (Handler != null)
                    Handler(this.PreviousExistenceStatus, this.ExistenceStatus);
            }
        }
        EExistenceStatus PreviousExistenceStatus = EExistenceStatus.Created;

        /// <summary>
        /// Starts the declaration of a Command, which is stored as a group/combination of variations.
        /// This command declaration is temporarily stored until completed or discarded, thus allowing nesting.
        /// Optionally, it can be specified to alter the Existence-Status while variating.
        /// </summary>
        public CommandVariation StartCommandVariation(string AppliedCommand, bool AlterExistenceStatusWhileVariating = true)
        {
            //T Console.WriteLine("Command-INITIATED: " + AppliedCommand + "    <<<<+++++++++++++++++++++++++++++ Command-Stack count=" + this.CommandCombiningStack.Count.ToString());

            // Keep the nesting parent alteration of existente-status
            if (this.CommandCombiningStack.Count > 0 && !this.CommandCombiningStack.Peek().AlterExistenceStatusWhileVariating)
                AlterExistenceStatusWhileVariating = false;

            var Variation = new CommandVariation(AppliedCommand, AlterExistenceStatusWhileVariating);
            this.CommandCombiningStack.Push(Variation);

            return Variation;
        }

        /// <summary>
        /// Finishes the collecting of variations for a command being declared.
        /// The grouped variations are then reversed to get the right order for undo, which is the primary and original registration order (ready to be undone).
        /// Optionally, an indication of extend the last variation can be specified.
        /// Returns completed (non-empty) command-variation or null if cancelled.
        /// </summary>
        public CommandVariation CompleteCommandVariation(bool ExtendsLastVariation = false)
        {
            if (!this.IsVariating)
                throw new UsageAnomaly("Cannot complete command variation because none is being declared.");

            // Finishes collecting the variations
            var CompletedCombinedVariation = this.CommandCombiningStack.Pop();

            if (CompletedCombinedVariation.Variations.Count < 1)
                return null;

            //T Console.WriteLine("Command-Completed: " + CompletedCombinedVariation.CommandName + "    <<<<_____________________________");

            //T foreach (var Change in CompletedCombinedVariation.Variations)
            //T     Console.WriteLine("VARSAVE: " + Change.ToString());

            // Reverses the combined variation, so its grouped variations will be sorted to be undone
            CompletedCombinedVariation.Variations.Reverse();

            // If nesting command variations...
            if (this.CommandCombiningStack.Count > 0)
                this.CurrentVariatingCommand.AddVariation(CompletedCombinedVariation);
            else
                if (!this.IsUndoing && !this.IsRedoing)
                {
                    // If extending and not the first one...
                    if (ExtendsLastVariation && this.UndoStack.Count > 0)
                        // Puts the extending variation and the reversed end.
                        this.UndoStack.Peek().Variations.Insert(0, CompletedCombinedVariation);
                    else
                        // Register this combined variation (command being performed) for undo
                        this.UndoStack.Push(CompletedCombinedVariation);

                    // Because this is a normal assignment the redo stack must be emptied.
                    if (this.RedoStack.Count > 0)
                        this.RedoStack.Clear();
                }

            // Release memory now to avoid congestion in the next seconds/minutes on slow/weak PCs.
            if (this.CommandCombiningStack.Count < 1)
            {
                var CurrentWindow = Display.GetCurrentWindow();
                Cursor PrevCursor = null;

                if (CurrentWindow != null)
                {
                    PrevCursor = CurrentWindow.Cursor;
                    CurrentWindow.Cursor = Cursors.Wait;
                }

                GC.Collect(3, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                if (CurrentWindow != null)
                    CurrentWindow.Cursor = PrevCursor;
            }

            return CompletedCombinedVariation;
        }

        /// <summary>
        /// Ends collecting variations and discards the current temporal command being declared.
        /// </summary>
        public void DiscardCommandVariation()
        {
            if (!this.IsVariating)
                throw new UsageAnomaly("Cannot discard command variation because none is being declared.");

            //T Console.WriteLine("Command*Discarded*: " + this.CurrentVariatingCommand.CommandName + "    <<<<-----------------------------");

            this.CommandCombiningStack.Pop();

            // Release memory now to avoid congestion in the next seconds/minutes on slow/weak PCs.
            if (this.CommandCombiningStack.Count < 1)
            {
                var CurrentWindow = Display.GetCurrentWindow();
                Cursor PrevCursor = null;

                if (CurrentWindow != null)
                {
                    PrevCursor = CurrentWindow.Cursor;
                    CurrentWindow.Cursor = Cursors.Wait;
                }

                GC.Collect(3, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                if (CurrentWindow != null)
                    CurrentWindow.Cursor = PrevCursor;
            }
        }

        /// <summary>
        /// Generates inverse assignment or invocation for the supplied source variation and sense.
        /// </summary>
        public TVariation GenerateInverseVariation<TVariation>(TVariation SourceVariation, EEditOperationAction RequiredActionSense) where TVariation : Variation
        {
            TVariation GeneratedVariation = null;

            // If revert is required, then the call was made for the new undo registered in a currently executing redo.
            // This is the primary and original registration order (ready to be undone).
            // Else, if apply is required, then the call was made for the new redo registered in a currently executing undo.
            // This is the secondary registration order (ready to be redone).
            // In both cases the logic is the same.

            if (SourceVariation is AssignmentVariation)
            {
                var OriginalVariation = SourceVariation as AssignmentVariation;
                GeneratedVariation = (new AssignmentVariation(OriginalVariation.VariatingProperty, OriginalVariation.VariatingInstance,
                                                              OriginalVariation.VariatingValue)) as TVariation;
            }
            else
                if (SourceVariation is CollectionVariation)
                {
                    // Notice that the inversion only will take place for "Apply" (intended for Redo), because collection variations are stored ready for Undo.
                    /* if (RequiredActionSense == EEditOperationAction.Apply)
                    { */

                    CollectionVariation OriginalVariation = SourceVariation as CollectionVariation;

                    if (OriginalVariation.VariatingCollection.GetType().Name == typeof(EditableList<>).Name)
                        GeneratedVariation = GenerateInverseListVariation(OriginalVariation.VariatingCollection,
                                                                          OriginalVariation.VariatingAlterCode,
                                                                          OriginalVariation.PassedParameters) as TVariation;
                    else
                        GeneratedVariation = GenerateInverseDictionaryVariation(OriginalVariation.VariatingCollection,
                                                                                OriginalVariation.VariatingAlterCode,
                                                                                OriginalVariation.PassedParameters) as TVariation;

                    /* }
                    else
                        GeneratedVariation = SourceVariation; */
                }
                else
                {
                    var OriginalVariation = SourceVariation as CommandVariation;
                    var NewGroupVariation = new CommandVariation(OriginalVariation.CommandName,
                                                                 OriginalVariation.AlterExistenceStatusWhileVariating);

                    foreach (Variation RegVariation in OriginalVariation.Variations)
                    {
                        var InverseVariation = GenerateInverseVariation(RegVariation, RequiredActionSense);

                        if (InverseVariation != null)
                            NewGroupVariation.AddVariation(InverseVariation);
                    }

                    NewGroupVariation.Variations.Reverse();
                    GeneratedVariation = NewGroupVariation as TVariation;
                }

            return GeneratedVariation;
        }

        public static char GenerateInverseListAlterCode(char AlterCode)
        {
            char NewCode = '?';

            switch (AlterCode)
            {
                case EditableList.ALTCOD_ADD:
                    NewCode = EditableList.ALTCOD_REMOVE;
                    break;
                case EditableList.ALTCOD_CLEAR:
                    NewCode = EditableList.ALTCOD_POPULATE;
                    break;
                case EditableList.ALTCOD_INSERT:
                    NewCode = EditableList.ALTCOD_REMOVEAT;
                    break;
                case EditableList.ALTCOD_SET:
                    NewCode = EditableList.ALTCOD_SET;
                    break;
                case EditableList.ALTCOD_POPULATE:
                    NewCode = EditableList.ALTCOD_CLEAR;
                    break;
                case EditableList.ALTCOD_REMOVE:
                    NewCode = EditableList.ALTCOD_ADD;
                    break;
                case EditableList.ALTCOD_REMOVEAT:
                    NewCode = EditableList.ALTCOD_INSERT;
                    break;
                case EditableList.ALTCOD_RESIZE:
                    NewCode = EditableList.ALTCOD_RESIZE;
                    break;
                default:
                    throw new UsageAnomaly("Unknown editable list alteration code.", AlterCode);
            }

            return NewCode;
        }

        public CollectionVariation GenerateInverseListVariation(EditableCollection TargetCollection, char AlterCode, object[] Parameters)
        {
            var NewCode = GenerateInverseListAlterCode(AlterCode);
            var Result = new CollectionVariation(TargetCollection, NewCode, Parameters);
            return Result;
        }

        public static char GenerateInverseDictionaryAlterCode(char AlterCode)
        {
            char NewCode = '?';

            switch (AlterCode)
            {
                case EditableDictionary.ALTCOD_ADD:
                    NewCode = EditableDictionary.ALTCOD_REMOVE;
                    break;
                case EditableDictionary.ALTCOD_CLEAR:
                    NewCode = EditableDictionary.ALTCOD_POPULATE;
                    break;
                case EditableDictionary.ALTCOD_POPULATE:
                    NewCode = EditableDictionary.ALTCOD_CLEAR;
                    break;
                case EditableDictionary.ALTCOD_REMOVE:
                    NewCode = EditableDictionary.ALTCOD_ADD;
                    break;
                case EditableDictionary.ALTCOD_KEYADD:
                    NewCode = EditableDictionary.ALTCOD_KEYREMOVE;
                    break;
                case EditableDictionary.ALTCOD_KEYREMOVE:
                    NewCode = EditableDictionary.ALTCOD_KEYADD;
                    break;
                case EditableDictionary.ALTCOD_KEYSET:
                    NewCode = EditableDictionary.ALTCOD_KEYSET;
                    break;
                default:
                    throw new UsageAnomaly("Unknown editable dictionary alteration code.", AlterCode);
            }

            return NewCode;
        }

        public CollectionVariation GenerateInverseDictionaryVariation(EditableCollection TargetCollection, char AlterCode, object[] Parameters)
        {
            var NewCode = GenerateInverseDictionaryAlterCode(AlterCode);
            var Result = new CollectionVariation(TargetCollection, NewCode, Parameters);
            return Result;
        }

        /// <summary>
        /// Indicates whether exists variations that can be undone.
        /// </summary>
        public bool HasUndoableVariations { get { return this.UndoStack.Count > 0; } }

        /// <summary>
        /// Indicates whether exists variations that can be redone.
        /// </summary>
        public bool HasRedoableVariations { get { return this.RedoStack.Count > 0; } }

        /// <summary>
        /// Gets the current executing command (either in undoing or redoing).
        /// </summary>
        internal CommandVariation CurrentExecutingCommand { get; private set; }

        /// <summary>
        /// Undoes the last applied variation, if any.
        /// Optionally indication of ability to redo (outside a nesting command) and echo the executing command variation can be specified.
        /// </summary>
        public void Undo(bool CanRedo = true, bool Echo = true)
        {
            if (this.IsUndoing || this.IsRedoing)
            {
                Console.WriteLine("Cannot undo edit operation while already an undo/redo is being performed.");
                return;
            }

            if (!HasUndoableVariations)
            {
                if (Echo) 
                    Console.WriteLine("Nothing to undo.");
                return;
            }

            // If inside a nesting command, the undo the last nested command
            if (this.IsVariating)
            {
                if (this.CommandCombiningStack.Count < 1 || !(this.CurrentVariatingCommand.Variations.LastOrDefault() is CommandVariation))
                {
                    Console.WriteLine("Cannot undo edit operation while a command variation is open (not yet completed or discarded) and no previous nested command is found.");
                    return;
                }

                CanRedo = false;    // Must be forced in order to avoid incoherence.
                this.CurrentExecutingCommand = (CommandVariation)this.CurrentVariatingCommand.Variations.Last();
                this.CurrentVariatingCommand.Variations.RemoveAt(this.CurrentVariatingCommand.Variations.Count - 1);
            }
            else
            {
                // Takes the last operation
                this.CurrentExecutingCommand = this.UndoStack.Pop();
            }

            if (this.CurrentExecutingCommand != null)
            {
                this.IsUndoing = true;

                if (Echo)
                    Console.WriteLine(this.CurrentExecutingCommand.CommandName);

                // If redoable, initiates a command variation to capture the changes to be later redone.
                if (CanRedo)
                    this.StartCommandVariation(this.CurrentExecutingCommand.CommandName);
                else
                    this.Pause();   // Prevents the Cancelled variations to be stored for Redo

                // Executes the undo
                this.CurrentExecutingCommand.Execute(this);

                // Saves the captured variation into the redo stack.
                if (CanRedo)
                {
                    var InverseVariation = this.CompleteCommandVariation();

                    if (InverseVariation != null)
                        this.RedoStack.Push(InverseVariation);
                }
                else
                    this.Resume();

                // Maybe this undo was made inside a performing command, so point to it
                this.CurrentExecutingCommand = null;
                this.IsUndoing = false;
            }
        }

        /// <summary>
        /// Redoes the last reverted variation, if any.
        /// Optionally indication of ability to undo and echo the executing command variation can be specified.
        /// </summary>
        public void Redo(bool CanUndo = true, bool Echo = true)
        {
            if (this.IsUndoing || this.IsRedoing)
            {
                Console.WriteLine("Cannot redo edit operation while already an undo/redo is being performed.");
                return;
            }

            if (!HasRedoableVariations)
            {
                if (Echo)
                    Console.WriteLine("Nothing to redo.");
                return;
            }

            if (this.IsVariating)
            {
                Console.WriteLine("Cannot redo edit operation while a command variation is open (not yet completed or discarded).");
                return;
            }

            // Takes the last operation
            this.CurrentExecutingCommand = this.RedoStack.Pop();

            this.IsRedoing = true;

            if (Echo)
                Console.WriteLine(this.CurrentExecutingCommand.CommandName);

            // If undoable, initiates a command variation to capture the changes to be later undone.
            if (CanUndo)
                this.StartCommandVariation(this.CurrentExecutingCommand.CommandName);

            // Executes the redo
            this.CurrentExecutingCommand.Execute(this);

            // Saves the captured variation into the undo stack.
            if (CanUndo)
            {
                var InverseVariation = this.CompleteCommandVariation();

                if (InverseVariation != null)
                    this.UndoStack.Push(InverseVariation);
            }

            this.CurrentExecutingCommand = null;
            this.IsRedoing = false;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}