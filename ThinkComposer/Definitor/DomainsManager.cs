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
// File   : DomainsManager.cs
// Object : Instrumind.ThinkComposer.Composer.DomainsManager (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.ApplicationProduct.Widgets;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;

/// Manages metadata about user defined Graphs and Visuals.
namespace Instrumind.ThinkComposer.Definitor
{
    /// <summary>
    /// Manages the edition of user-defined Domains, working as an intermediary for external consumers.
    /// </summary>
    public class DomainsManager : WorkSphere
    {
        /// <summary>
        /// Internal Domain's document URI (relative to the main storage package).
        /// </summary>
        public static readonly Uri DomainDocumentUri
            = new Uri("/" + Domain.__ClassDefinitor.TechName + DocumentEngine.NativeFormatExtension,
                      UriKind.Relative);

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainsManager(string Name, string TechName, string Summary, ImageSource Pictogram,
                                   WorkspaceManager WorkspaceDirector, IDocumentVisualizer Visualizer,
                                   WidgetItemsPaletteGroup ConceptPalette, WidgetItemsPaletteGroup RelationshipPalette)
            : base(Name, TechName, Summary, Pictogram, WorkspaceDirector, Visualizer)
        {
            General.ContractRequiresNotNull(ConceptPalette, RelationshipPalette);

            this.ConceptPalette = ConceptPalette;
            this.RelationshipPalette = RelationshipPalette;

            this.DocumentsPrefix = "Domain";
        }

        /// <summary>
        /// Control to show the available Concepts exposed to the user by the current Composition's Domain.
        /// </summary>
        public WidgetItemsPaletteGroup ConceptPalette { get; protected set; }

        /// <summary>
        /// Control to show the available Relationships exposed to the user by the current Composition's Domain.
        /// </summary>
        public WidgetItemsPaletteGroup RelationshipPalette { get; protected set; }

        /// <summary>
        /// List of exposed command for the Quick tool palette.
        /// </summary>
        public List<WorkCommandExpositor> QuickExposedCommands = new List<WorkCommandExpositor>();
        
        /// <summary>
        /// Exposes the commands which can be consumed.
        /// </summary>
        public void ExposeCommands()
        {
            WorkCommand ExposedWorkCommand = null;

            // =========================================================================================================
            SimpleElement ExposedArea = new SimpleElement("Domain", "Domain");
            SimpleElement ExposedGroup = new SimpleElement("Definition", "Definition");

            this.CommandAreas_.Add(ExposedArea);
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("EditDomain");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DomainEdit(Doc.TargetComposition.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Edit Domain...", ExposedWorkCommand.Name, "Edit the Domain of the current Composition", "book_edit.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            /* this would confuse the user? */
            ExposedWorkCommand = new GenericCommand("NewDomain");
            ExposedWorkCommand.Apply = (par => ProductDirector.CompositionDirector.CreateComposition(true));
            ExposedWorkCommand.CanApply = (par => true);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("New Domain", ExposedWorkCommand.Name, "Creates a new Domain (and its empty template Composition).", "book_star.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("OpenDomain");
            ExposedWorkCommand.Apply = (par => ProductDirector.CompositionDirector.OpenDomainAndCreateCompositionOfIt(true));
            ExposedWorkCommand.CanApply = (par => true);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Open Domain", ExposedWorkCommand.Name, "Opens a Domain for editing (and creates its emtpy template Composition).", "folder_book.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("SaveDomainAs");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Engine == null)
                        return;

                    var Confirmation = Display.DialogMessage("Confirmation", "Save also the current Composition as Domain's Template for create new ones?\n" +
                                                                             "(Note: Later, it can be used or just start empty Compositions)",
                                                             EMessageType.Question, MessageBoxButton.YesNoCancel, MessageBoxResult.Yes);
                    if (Confirmation == MessageBoxResult.Cancel || Confirmation == MessageBoxResult.None)
                        return;

                    var SaveTemplate = (Confirmation == MessageBoxResult.Yes);

                    //T Console.WriteLine("Saving Domain as...");

                    var InitialRoute = (Engine.DomainLocation != null
                                        ? Engine.DomainLocation.LocalPath
                                        : (Engine.FullLocation == null
                                            ? Path.Combine(AppExec.UserDataDirectory,
                                                        Engine.TargetComposition.CompositeContentDomain.TechName)
                                            : Path.Combine(Path.GetDirectoryName(Engine.FullLocation.LocalPath),
                                                        Engine.TargetComposition.CompositeContentDomain.TechName)));

                    var TargetRoute = Display.DialogGetSaveFile("Save Domain as",
                                                                FileDataType.FileTypeDomain.Extension,
                                                                FileDataType.FileTypeDomain.FilterForSave,
                                                                InitialRoute);
                    if (TargetRoute == null)
                        return;

                    var CurrentWindow = Display.GetCurrentWindow();
                    CurrentWindow.Cursor = Cursors.Wait;

                    var TargetDomain = Engine.TargetComposition.CompositeContentDomain;

                    Visual Snapshot = null;

                    if (SaveTemplate && TargetDomain.OwnerComposition.ActiveView != null)
                        Snapshot = TargetDomain.OwnerComposition.ActiveView
                                        .ToVisualSnapshot(DocumentEngine.PART_SNAPSHOT_WIDTH, DocumentEngine.PART_SNAPSHOT_HEIGHT);

                    TargetDomain.SetTemplateSaving(SaveTemplate);

                    var Result = DocumentEngine.StoreToLocation<Domain>(TargetDomain, Domain.__ClassDefinitor.Name,
                                                                        TargetDomain.Classification.ContentTypeCode,
                                                                        TargetRoute, DomainDocumentUri, true, false,
                                                                        TargetDomain, Snapshot);

                    if (!Result.IsAbsent())
                    {
                        CurrentWindow.Cursor = Cursors.Arrow;
                        Display.DialogMessage("Error!", "Cannot save Domain.\n\nProblem: " + Result, EMessageType.Warning);
                        return;
                    }
                    Engine.DomainLocation = TargetRoute;

                    this.WorkspaceDirector.ShellProvider.RefreshSelection();
                    CurrentWindow.Cursor = Cursors.Arrow;
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Save Domain As", ExposedWorkCommand.Name, "Saves the current Domain to the specified file.", "book_saveas.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            // Ordered list of quick access commands
            this.QuickExposedCommands.Add(this.CommandExpositors["NewDomain"]);
            this.QuickExposedCommands.Add(this.CommandExpositors["OpenDomain"]);
            this.QuickExposedCommands.Add(this.CommandExpositors["EditDomain"]);
            this.QuickExposedCommands.Add(this.CommandExpositors["SaveDomainAs"]);

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Concept Definitions...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DefineDomainConcepts(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Concept Defs...", ExposedWorkCommand.Name, "Edit the Concept Definitions of the Domain", "imtc_concept.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Relationship Definitions...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DefineDomainRelationships(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Relationship Defs...", ExposedWorkCommand.Name, "Edit the Relationship Definitions of the Domain", "imtc_relationship.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Link-Role Variant Definitions...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DefineDomainLinkRoleVariants(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Link-Role Variant Defs...", ExposedWorkCommand.Name, "Edit the Link-Role Variant Definitions of the Domain", "link_role_variants.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Marker Definitions...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DefineDomainMarkers(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Marker Defs...", ExposedWorkCommand.Name, "Edit the Marker Definitions of the Domain", "award_star_edit.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Table-Structure Definitions...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.EditDomainTableDefinitions(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Table-Structure Defs...", ExposedWorkCommand.Name, "Edit the Table-Structure Definitions of the Domain", "table_alter.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Base Tables...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.EditDomainBaseTables(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Base Tables...", ExposedWorkCommand.Name, "Edit the Base Tables of the Domain", "table_multiple.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("External Languages...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DefineDomainExternalLanguages(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("External Languages...", ExposedWorkCommand.Name, "Edit the External Languages declared for the Domain", "page_white_code_red.png",
                                                                                         EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Idea-Def Clusters...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Doc = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                    if (Doc == null)
                        return;

                    DomainServices.DefineDomainIdeaDefClusters(Doc.CurrentView.OwnerCompositeContainer.CompositeContentDomain);
                });
            ExposedWorkCommand.CanApply = (par => (this.WorkspaceDirector.ActiveDocument != null));

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Idea-Def Clusters...", ExposedWorkCommand.Name, "Edit the Idea-Definition Clusters of the Domain", "def_clusters.png",
                                                                                         EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public override bool Discard()
        {
            return true;
        }

        // -------------------------------------------------------------------------------------------------------------------------

        // -------------------------------------------------------------------------------------------------------------------------
    }
}