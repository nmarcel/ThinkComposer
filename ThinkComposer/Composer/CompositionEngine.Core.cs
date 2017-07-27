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
// File   : CompositionEngine.Core.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Takes care of the edition of a particular Composition instance (Core partial-file).
    /// </summary>
    public partial class CompositionEngine : DocumentEngine
    {
        /// <summary>
        /// Internal Composition's document URI (relative to the main storage package).
        /// </summary>
        public static readonly Uri CompositionDocumentUri
            = new Uri("/" + Composition.__ClassDefinitor.TechName + DocumentEngine.NativeFormatExtension,
                      UriKind.Relative);

        /// <summary>
        /// Exposed palette items for create Concepts.
        /// </summary>
        public static readonly SimplePresentationElement ConceptsPaletteItem = null;

        /// <summary>
        /// Exposed palette items for create Relationships.
        /// </summary>
        public static readonly SimplePresentationElement RelationshipsPaletteItem = null;

        /// <summary>
        /// Distance of separation between automatic created or pasted symbols.
        /// </summary>
        public const double VISUAL_AUTOPOS_DELTA = 10.0;

        /// <summary>
        /// Instantiates a new CompositionEngine, either for a new Composition or for one existent from supplied as a Uri.
        /// </summary>
        /// <param name="SourceLocation">Optional location of an existent Composition.</param>
        /// <param name="RootDomain">Optional Domain to serve as root/basis of a new Composition.</param>
        /// <param name="UseDomainCompositionAsTemplate">Indicates to use the Domain's Composition as Template instead of create an empty new one.</param>
        /// <returns>Engine for edit a created/opened Composition plus possible error-message.</returns>
        public static Tuple<CompositionEngine, string> Materialize(Uri SourceLocation = null, Domain RootDomain = null, bool UseDomainCompositionAsTemplate = false)
        {
            Tuple<CompositionEngine, string> Result = null;

            var Engine = CompositionEngine.ActiveCompositionEngine;

            Composition TargetComposition = null;

            // IF LOADING FROM COMPOSITION FILE
            if (SourceLocation != null)
            {
                /* Better ignore...
                if (RootDomain != null)
                    throw new UsageAnomaly("Cannot specify root Domain for an already existent Composition."); */

                try
                {
                    // Load Composition's document package from Uri
                    var LoadResult = LoadFromLocation<ISphereModel>(FileDataType.FileTypeComposition.Name, SourceLocation, CompositionDocumentUri);
                    var Content = LoadResult.Item1;

                    if (Content == null)
                        return Tuple.Create<CompositionEngine, string>(null, LoadResult.Item2);

                    TargetComposition = Content as Composition;

                    if (TargetComposition == null)
                        throw new ExternalAnomaly("Cannot load Composition from the specified location.", SourceLocation);

                    Engine.TargetComposition = TargetComposition;
                    Engine.Location = SourceLocation;

                    // IMPORTANT: This Engine will overwrite its Global-ID with that of the loaded Compositon.
                    Engine.GlobalId = TargetComposition.GlobalId;
                    TargetComposition.CompositionDefinitor.SetOwnerComposition(TargetComposition);    // Reattach of owner Composition (which is non-serializable)

                    ModelFixes.ApplyModelFixes(TargetComposition.CompositionDefinitor);
                    TargetComposition.Initialize();

                    Result = Tuple.Create(Engine, "");
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot load Composition from '{0}'.\nProblem: {1}", SourceLocation.LocalPath, Problem);
                    AppExec.LogException(Problem);
                    Engine = null;

                    Result = Tuple.Create<CompositionEngine, string>(null, Problem.Message);
                }
            }
            else
            {
                string DocName = Engine.Manager.DocumentsPrefix + Engine.Manager.GetNewDocumentNumber().ToString();

                // IF LOADING FROM COMPOSITION AS TEMPLATE IN DOMAIN FILE
                if (RootDomain != null && RootDomain.OwnerComposition != null && UseDomainCompositionAsTemplate)
                    try
                    {
                        TargetComposition = RootDomain.OwnerComposition;
                        TargetComposition.GlobalId = Guid.NewGuid();
                        /* Cancelled
                        TargetComposition.Name = DocName;
                        TargetComposition.TechName = DocName.TextToIdentifier();
                        TargetComposition.Summary = ""; */
                        TargetComposition.Version = new VersionCard();

                        Engine.GlobalId = TargetComposition.GlobalId;
                        Engine.TargetComposition = TargetComposition;
                        Engine.Location = null;

                        TargetComposition.Initialize();

                        Result = Tuple.Create(Engine, "");
                    }
                    catch (Exception Problem)
                    {
                        Console.WriteLine("Cannot load Composition from '{0}'.\nProblem: {1}", SourceLocation.LocalPath, Problem);
                        AppExec.LogException(Problem);
                        Engine = null;

                        Result = Tuple.Create<CompositionEngine, string>(null, Problem.Message);
                    }
                else    // CREATING NEW EMPTY COMPOSITION
                {
                    // IMPORTANT: The new Compositon will initialize its Global-ID with that of the Engine.
                    TargetComposition = new Composition(Engine, RootDomain.NullDefault(Domain.Create(Engine)), DocName, DocName.TextToIdentifier());
                    TargetComposition.CompositionDefinitor.SetOwnerComposition(TargetComposition);    // Initial attach of owner Composition (which is non-serializable)
                    TargetComposition.Initialize();
                    Engine.TargetComposition = TargetComposition;

                    //- TargetComposition.RootView = TargetComposition.OpenCompositeView(TargetComposition.CompositionDefinitor);

                    Result = Tuple.Create(Engine, "");
                }
            }

            return Result;
        }

        /// <summary>
        /// Instantiates an existent Domain from the supplied Uri.
        /// </summary>
        /// <param name="DomainLocation">Optional location of an existent Domain.</param>
        /// <returns>Opened root Domain, plus possible error-message.</returns>
        public static Tuple<Domain, string> MaterializeDomain(Uri DomainLocation = null)
        {
            // Load Domain's document package from Uri
            Tuple<ISphereModel, string> Result = null;

            try
            {
                Result = LoadFromLocation<ISphereModel>(FileDataType.FileTypeDomain.Name, DomainLocation, DomainsManager.DomainDocumentUri);
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot load Domain from '{0}'.", DomainLocation.LocalPath);
                AppExec.LogException(Problem);
                return Tuple.Create<Domain, string>(null, Problem.Message);
            }

            var Content = Result.Item1;

            if (Content == null)
                return Tuple.Create<Domain, string>(null, Result.Item2);

            var RootDomain = Content as Domain;

            if (RootDomain == null)
                throw new ExternalAnomaly("Cannot load Domain from the specified location.", DomainLocation);

            ModelFixes.ApplyModelFixes(RootDomain);

            return Tuple.Create(RootDomain, "");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Static constructor.
        /// </summary>
        static CompositionEngine()
        {
            IdeaTransferFormat = DataFormats.GetDataFormat(CompositionEngine.FORMAT_CODE_IDEA);

            CurrentMousePosition = Display.NULL_POINT;

            DefineContextMenuOptions();
        }

        public static DataFormat IdeaTransferFormat = null;

        /// <summary>
        /// Gets the currently active Composition Engine.
        /// </summary>
        public static CompositionEngine ActiveCompositionEngine { get { return EntityEditEngine.ActiveEntityEditor as CompositionEngine;  } }

        /// <summary>
        /// Creates, activates and returns a new Comopsition Engine, using the supplied Manager and Visualizer.
        /// From this point in time onwards, every editing change will be stored by this Entity Edit-Engine.
        /// </summary>
        public static CompositionEngine CreateActiveCompositionEngine(CompositionsManager Manager, IDocumentVisualizer Visualizer, bool IsForEditDomain)
        {
            var NewEngine = new CompositionEngine(Manager, Visualizer);
            NewEngine.IsForEditDomain = IsForEditDomain;

            EntityEditEngine.ActiveEntityEditor = NewEngine;

            return NewEngine;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Manager">CompositionsManager which owns this edition engine.</param>
        /// <param name="Visualizer">Visualization control for document views.</param>
        private CompositionEngine(CompositionsManager Manager, IDocumentVisualizer Visualizer)
        {
            General.ContractRequiresNotNull(Manager, Visualizer);
            this.Manager = Manager;
            this.Visualizer = Visualizer;
            
            // Subscribes to entity-change event
            this.ExistenceUpdateAction = (prevsta, currsta) =>
            {
                this.Manager.WorkspaceDirector.ShellProvider.RefreshSelection(null, true);
            };

            this.EntityChanged += this.ExistenceUpdateAction;

            this.Manager.WorkspaceDirector.ShellProvider.KeyActioned += new KeyEventHandler(ShellProvider_KeyActioned);
        }

        void ShellProvider_KeyActioned(object sender, KeyEventArgs evargs)
        {
            if (!evargs.IsDown || this.CurrentView == null
                || ProductDirector.EditorInterrelationsControl.InterrelatedOriginsTree.IsFocused
                || ProductDirector.EditorInterrelationsControl.InterrelatedTargetsTree.IsFocused)
                return;

            if (evargs.Key == System.Windows.Input.Key.Escape)
            {
                    this.DoCancelOperation();

                    if (ClipboardTransferSourceView != null
                        && ClipboardTransferSourceView.OwnerCompositeContainer.OwnerComposition == this.TargetComposition)
                        UnmarkSelectedObjectsForCut();
            }

            if (evargs.Key == System.Windows.Input.Key.Delete)
                this.DoDeleteSelection();

            var SelectedRepsCount = this.CurrentView.SelectedRepresentations.Count();

            if (this.CurrentView.IsEditingInPlace)
                return;

            if (evargs.Key == System.Windows.Input.Key.F8)
            {
                this.CurrentView.FitContentIntoView();
                return;
            }

            if (SelectedRepsCount > 0)
            {
                var SelectedRep = this.CurrentView.SelectedRepresentations.First();

                if (evargs.Key == System.Windows.Input.Key.F2
                    && !SelectedRep.MainSymbol.IsHidden)
                {
                    this.CurrentView.EditInPlace(SelectedRep.MainSymbol);
                    return;
                }

                if (evargs.Key == System.Windows.Input.Key.F3
                    && (SelectedRep.RepresentedIdea.IdeaDefinitor.IsComposable || SelectedRep.RepresentedIdea.CompositeIdeas.Count > 0))
                {
                    ShowCompositeAsView(SelectedRep.MainSymbol);
                    return;
                }

                if (evargs.Key == System.Windows.Input.Key.F4)
                {
                    this.CurrentView.EditPropertiesOfVisualRepresentation(SelectedRep);
                    return;
                }

                if (evargs.Key == System.Windows.Input.Key.F6)
                {
                    this.CurrentView.EditPropertiesOfVisualRepresentation(SelectedRep, Display.TABKEY_DETAILS);
                    return;
                }

                if (evargs.Key == System.Windows.Input.Key.F7)
                {
                    this.CurrentView.EditPropertiesOfVisualRepresentation(SelectedRep, Display.TABKEY_MARKINGS);
                    return;
                }
            }

            if (SelectedRepsCount != 1)
                return;

            OperationResult<Concept> Creation = null;

            if (evargs.Key == System.Windows.Input.Key.Enter)
                Creation = ConceptCreationCommand.CreateAutoConceptAsSibling(this.CurrentView.SelectedRepresentations.First());

            if (evargs.Key == System.Windows.Input.Key.Tab)
                Creation = ConceptCreationCommand.CreateAutoConceptFromRepresentation(this.CurrentView.SelectedRepresentations.First(), null,
                                                                                      !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));

            if (Creation == null || !Creation.WasSuccessful)
                return;

            this.CurrentView.UnselectAllObjects();
            var Symbol = Creation.Result.VisualRepresentators.First().MainSymbol;
            this.CurrentView.SelectObject(Symbol);

            // Must be postcalled to avoid collision of oning pressed key with Auto creation logic.
            this.CurrentView.Presenter.PostCall(pres => pres.OwnerView.EditInPlace(Symbol));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected Action<EExistenceStatus, EExistenceStatus> ExistenceUpdateAction = null;


        /// <summary>
        /// Saves the Document into the underlying package, optionally with a new Uri.
        /// If the underlying package does not exists, then it is created with the supplied Uri.
        /// If the document is not already stored, then the Uri must be supplied.
        /// Returns an error message if problems were detected.
        /// </summary>
        /// <param name="DocumentLocation">New location for the Composition, Optional if the document is already stored.</param>
        /// <param name="UpdateLocation">Indicates to update the Composition location.</param>
        /// <param name="ResetExistenceStatus">Indicates to reset the existence status to Not-Modified.</param>
        /// <param name="RegisterAsRecentDoc">Indicates to register the document in the recent-documents list.</param>
        /// <returns>Possible problem error message.</returns>
        public string Store(Uri DocumentLocation = null, bool UpdateLocation = true, bool ResetExistenceStatus = true, bool RegisterAsRecentDoc = true)
        {
            var MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.IdeasCreationQuotas);

            if (!ProductDirector.ValidateEditionLimit(this.TargetComposition.DeclaredIdeas.Count(), MaxQuota, "save", "Ideas (Concepts + Relationships)"))
                return "This product edition cannot save a Composition with more than " + MaxQuota.ToString() + " Ideas.";

            MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.ComposabilityLevelsQuotas);

            var CompoLevels = this.TargetComposition.GetCompositeSubLevelsCount();
            if (!ProductDirector.ValidateEditionLimit(CompoLevels, MaxQuota, "save", "Composability Depth Levels"))
                return "This product edition cannot save a Composition with more than " + MaxQuota.ToString() + " composability depth levels.";

            if (DocumentLocation == null)
                DocumentLocation = this.Location;
            else
                if (UpdateLocation)
                    this.Location = DocumentLocation;

            var Snapshot = (this.TargetComposition.ActiveView == null
                            ? null
                            : this.TargetComposition.ActiveView.ToVisualSnapshot(PART_SNAPSHOT_WIDTH, PART_SNAPSHOT_HEIGHT));

            var Result = StoreToLocation<ISphereModel>(this.TargetComposition, Composition.__ClassDefinitor.Name,
                                                       this.TargetComposition.Classification.ContentTypeCode, DocumentLocation,
                                                       CompositionDocumentUri, RegisterAsRecentDoc, false,
                                                       this.TargetComposition, Snapshot);

            if (Result.IsAbsent() && ResetExistenceStatus)
                this.ExistenceStatus = EExistenceStatus.NotModified;

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stops and discards this composition engine, closing and freeing its related entities, views and resources.
        /// Returns indication of discard executed or aborted (maybe due to cancellation by unsaved changes).
        /// </summary>
        public bool Stop(bool ForceDiscard = false)
        {
            base.Stop();

            // Stop watching any possible temp-files for Attachments being edited.
            DetailAttachmentEditor.StopWatchingAttachmentsFor(this);

            // Discard clipboard transfer content
            if (ClipboardTransferSourceView != null
                && ClipboardTransferSourceView.OwnerCompositeContainer.OwnerComposition == this.TargetComposition)
                ClipboardTransferSelectedObjects.Clear();

            // Discards the visual views.
            this.Visualizer.DiscardAllViews();

            // Discards remembered for-edit objects
            Definitor.DefinitorMaintenance.DomainMaintainer.RememberedTemplateTestConcept.Remove(this.TargetComposition);
            Definitor.DefinitorMaintenance.DomainMaintainer.RememberedTemplateTestRelationship.Remove(this.TargetComposition);

            // PENDING: Free resources, if any.
            this.EntityChanged -= this.ExistenceUpdateAction;

            return (this.ExistenceStatus != EExistenceStatus.Modified);
        }

        /// <summary>
        /// Composition being edited by this engine.
        /// </summary>
        public Composition TargetComposition
        {
            get
            {
                return TargetComposition_;
            }
            protected set
            {
                this.TargetComposition_ = value;
                this.TargetComposition_.PropertyChanged += TargetComposition__PropertyChanged;

                var Handler = this.MainEditedEntityChanged;
                if (Handler != null)
                    this.MainEditedEntityChanged(this);
            }
        }
        protected Composition TargetComposition_ = null;

        /// <summary>
        /// Physical location of the working document
        /// </summary>
        public override Uri Location
        {
            get { return this.Location_; }
            set
            {
                this.Location_ = value;

                var Handler = this.PropertyChanged;
                if (Handler != null)
                    Handler(this, new PropertyChangedEventArgs("Location"));

                if (this.TargetComposition != null)
                    this.TargetComposition.SimplifiedLocation = this.SimplifiedLocation;
            }
        }
        private Uri Location_ = null;

        /// <summary>
        /// Physical Location of the working document
        /// </summary>
        public Uri DomainLocation
        {
            get { return this.DomainLocation_; }
            set
            {
                this.DomainLocation_ = value;

                var Handler = this.PropertyChanged;
                if (Handler != null)
                    Handler(this, new PropertyChangedEventArgs("DomainLocation"));
            }
        }
        private Uri DomainLocation_ = null;

        /// <summary>
        /// Dominant manager for Composition editors/engines.
        /// </summary>
        public CompositionsManager Manager { get; protected set; }

        /// <summary>
        /// Starts the visual interactive editing of the Composition.
        /// </summary>
        public override void Start()
        {
            this.TargetComposition.Start();

            base.Start();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Handles changes to the composition/engine properties.
        /// </summary>
        void TargetComposition__PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var Handler = this.PropertyChanged;

            if (Handler != null)
                Handler(this, e);
        }

        public override string ToString()
        {
            return this.TargetComposition.ToString();
        }

        public override void Show()
        {
            if (this.TargetComposition.RootView == null)
                return;

            // PENDING: HERE THE LAST OPENED VIEWS SHOULD BE SHOWN.
            ScrollViewer ScrollPresenter = null;

            if (this.LastOpenedViews == null || this.LastOpenedViews.Count < 1)
            {
                ScrollPresenter = this.Visualizer.PutView(this.TargetComposition.RootView);
                this.Visualizer.ActiveView = this.TargetComposition.RootView;
            }
            else
            {
                if (!this.TargetComposition.ActiveView.IsIn(this.LastOpenedViews))
                    this.LastOpenedViews.Insert(0, this.TargetComposition.ActiveView);

                // Saved temporally because of change on tab creation.
                var OriginalActiveView = this.TargetComposition.ActiveView;

                foreach (var OpenedView in this.LastOpenedViews)
                    this.Visualizer.PutView(OpenedView);

                this.Visualizer.ActiveView = OriginalActiveView;
            }
        }

        public bool IsForEditDomain { get; protected set; }

        public override ISphereModel TargetDocument { get { return this.TargetComposition; } }

        public override event Action<EntityEditEngine> MainEditedEntityChanged;

        #region INotifyPropertyChanged Members

        public new event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public View CurrentView
        {
            get { return this.CurrentView_; }
            protected set
            {
                this.CurrentView_ = value;

                if (this.TargetComposition.ActiveView != this.CurrentView_)
                    this.TargetComposition.ActiveView = this.CurrentView_;
            }
        }
        private View CurrentView_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}