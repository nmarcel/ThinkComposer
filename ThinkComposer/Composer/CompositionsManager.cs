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
// File   : CompositionsManager.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionsManager (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.06 Néstor Sánchez A.  Creation
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

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.ApplicationProduct.Widgets;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.Composer.Reporting;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Manages the edition of user-defined Compositions, working as an intermediary for external consumers.
    /// Main part.
    /// </summary>
    public partial class CompositionsManager : WorkSphere
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CompositionsManager(string Name, string TechName, string Summary, ImageSource Pictogram,
                                   WorkspaceManager WorkspaceDirector, IDocumentVisualizer Visualizer,
                                   WidgetItemsPaletteGroup ConceptPalette, WidgetItemsPaletteGroup RelationshipPalette,
                                   WidgetItemsPaletteGroup MarkerPalette, WidgetItemsPaletteGroup ComplementPalette)
            : base(Name, TechName, Summary, Pictogram, WorkspaceDirector, Visualizer)
        {
            General.ContractRequiresNotNull(ConceptPalette, RelationshipPalette);

            this.ConceptPaletteGroup = ConceptPalette;
            this.RelationshipPaletteGroup = RelationshipPalette;
            this.MarkerPaletteGroup = MarkerPalette;
            this.ComplementPaletteGroup = ComplementPalette;
            this.DocumentsPrefix = "Composition";
        }

        /// <summary>
        /// Instantiated engines for Composition editing.
        /// </summary>
        protected List<CompositionEngine> CompositionEditors = new List<CompositionEngine>();

        /// <summary>
        /// Control to show the available Concepts exposed to the user by the current Composition's Domain.
        /// </summary>
        public WidgetItemsPaletteGroup ConceptPaletteGroup { get; protected set; }

        /// <summary>
        /// Control to show the available Relationships exposed to the user by the current Composition's Domain.
        /// </summary>
        public WidgetItemsPaletteGroup RelationshipPaletteGroup { get; protected set; }

        /// <summary>
        /// Control to show the available Markers exposed to the user by the current Composition's Domain.
        /// </summary>
        public WidgetItemsPaletteGroup MarkerPaletteGroup { get; protected set; }

        /// <summary>
        /// Control to show the available Complements exposed to the user by the current Composition's Domain.
        /// </summary>
        public WidgetItemsPaletteGroup ComplementPaletteGroup { get; protected set; }

        /// <summary>
        /// List of exposed command for the Quick tool palette.
        /// </summary>
        public List<WorkCommandExpositor> QuickExposedCommands = new List<WorkCommandExpositor>();

        /// <summary>
        /// Exposes the commands which can be consumed.
        /// </summary>
        public void ExposeCommands()
        {
            SimpleElement ExposedArea = null;
            SimpleElement ExposedGroup = null;
            RoutedCommand ExposedCommand = null;
            WorkCommand ExposedWorkCommand = null;
            CommandBinding CommandLink = null;

            ExposedArea = new SimpleElement("Composition", "Composition");
            this.CommandAreas_.Add(ExposedArea);

            ExposedGroup = new SimpleElement("File", "File");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            // -------------------------------------------------------------------------------------
            ExposedCommand = ApplicationCommands.New;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("New", ExposedCommand.Name, "Creates a new Composition document based on the specified Domain.", "page_white_star_book.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandNew_Executed, CommandNew_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            // -------------------------------------------------------------------------------------
            ExposedCommand = ApplicationCommands.Open;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Open", ExposedCommand.Name, "Opens an existing Composition document.", "folder_page_white.png", // Previous: book_add
                                                                                      EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandOpen_Executed, CommandOpen_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            // -------------------------------------------------------------------------------------
            ExposedCommand = ApplicationCommands.Save;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Save", ExposedCommand.Name, "Saves the composition document into a file.", "disk.png", // Previous: book_add
                                                                                      EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandSave_Executed, CommandSave_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedCommand = ApplicationCommands.SaveAs;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Save As", ExposedCommand.Name, "Saves the composition document into a new file.", // Previous: book_add
                                                                                      "disk_saveas.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandSaveAs_Executed, CommandSaveAs_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedWorkCommand = new GenericCommand("SaveAll");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    foreach (var Doc in this.WorkspaceDirector.Documents)
                        if (!Save(Doc.DocumentEditEngine as CompositionEngine))
                            break;
                });
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocument != null ||
                                                  this.WorkspaceDirector.Documents.Any(doc => doc.DocumentEditEngine.ExistenceStatus == EExistenceStatus.Modified));
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Save All", ExposedWorkCommand.Name, "Saves all the opened documents.", "disk_multiple.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Edit");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Engine.EditCompositionProperties();
                });
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocument != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Edit...", ExposedWorkCommand.Name, "Opens an editing window for describing the Composition.", "page_white_edit.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Export");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    ExportCurrentView(Engine.TargetComposition);
                });
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocument != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Export Image", ExposedWorkCommand.Name, "Exports the current document view as an Image (press [Ctrl] to use Transparency in .PNG format).", "diagram_export.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            /* For export to PDF...
             http://stackoverflow.com/questions/502198/convert-wpf-xaml-control-to-xps-document
             * This library cost $299: http://www.opoosoft.com/developer.html
             */

            // -------------------------------------------------------------------------------------
            ExposedCommand = ApplicationCommands.Print;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Print...", ExposedCommand.Name, "Prints the current document View.", "printer.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandPrint_Executed, CommandPrint_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("PDF/XPS Report");
            ExposedWorkCommand.Apply = (par => Reporting.ReportingManager.GeneratePdfXpsReport(this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine));
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocument != null
                                                  && ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_STANDARD, "Generate PDF/XPS Report", false));
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "PdfXpsReport", "Generates a PDF/XPS Report from the current Composition.", "xps_pdf_report.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("HTML Report");
            ExposedWorkCommand.Apply = (par => Reporting.ReportingManager.GenerateHtmlReport(this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine));
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocument != null
                                                  && ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_STANDARD, "Generate HTML Report", false, new DateTime(2013, 1, 1)));
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "HtmlReport", "Generates an HTML Report from the current Composition.", "gen_html.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            /* POSTPONED: Not simple to be done without Outlook integration. */
            ExposedWorkCommand = new GenericCommand("Send");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Send(Engine.TargetComposition);
                });
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocument != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Send", "Send the current Composition via (Outlook) e-mail.", "mail.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedCommand = ApplicationCommands.Close;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Close", ExposedCommand.Name, "Closes the composition document.", "door.png", // Previous: book_add
                                                                                      EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandClose_Executed, CommandClose_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            // -------------------------------------------------------------------------------------
            ExposedGroup = new SimpleElement("System", "System");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("About & Update...");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    ProductDirector.ShowAbout();
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "About & Update...", "About & Update product", "/Instrumind.Common;component/Visualization/Images/Instrumind_16x16.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));
            /* No longer used
            ExposedWorkCommand = new GenericCommand("RegisterLicense");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    ProductDirector.RegisterLicense();
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Register...", ExposedWorkCommand.Name, "Register this Product with a License Key", "user_key.png",
                                                                                         EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));
            */

            /*+ ExposedWorkCommand = new GenericCommand("Help");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    ProductDirector.ShowHelp();
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Help...", "Help.", "help.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand)); */

            /*+ ExposedWorkCommand = new GenericCommand("Options");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    ProductDirector.EditOptions();
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Options...", "Options.",
                                                                          "app_options.png",
                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName,
                                                                          ExposedWorkCommand, null, null)); */

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Exit");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    Application.Current.MainWindow.Close();
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Exit", "Exit.", "control_power_red.png",
                                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            /*T // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("DETECT");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    Console.WriteLine("Compositions and their Domains Global-IDs... ------------------------------------");

                    foreach (var Compo in WorkspaceDirector.Documents.Cast<Composition>())
                        Console.WriteLine("C:" + Compo.GlobalId.ToString() + "  D:" + Compo.CompositeContentDomain.GlobalId.ToString());

                    Console.WriteLine("======================================================");

                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Detect...", "Detect.",
                                                                          "lightbulb.png",
                                                                          EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName,
                                                                          ExposedWorkCommand, null, null)); */

            // -------------------------------------------------------------------------------------
            // Ordered list of quick access commands
            this.QuickExposedCommands.Add(this.CommandExpositors["New"]);
            this.QuickExposedCommands.Add(this.CommandExpositors["Open"]);
            this.QuickExposedCommands.Add(this.CommandExpositors["Edit"]);
            this.QuickExposedCommands.Add(this.CommandExpositors["Save"]);

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ExposedArea = new SimpleElement("Edit", "Edit");
            this.CommandAreas_.Add(ExposedArea);

            ExposedGroup = new SimpleElement("Action", "Action");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedCommand = ApplicationCommands.Undo;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Undo", ExposedCommand.Name, "Undo the last done operation (of a maximum of " + EntityEditEngine.GlobalMaxUndos.ToString() + " recorded).", "arrow_undo.png",
                                                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandUndo_Executed, CommandUndo_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedCommand = ApplicationCommands.Redo;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Redo", ExposedCommand.Name, "Redo the last undone operation (of a maximum of  " + EntityEditEngine.GlobalMaxRedos.ToString() + " recorded).", "arrow_redo.png",
                                                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandRedo_Executed, CommandRedo_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedCommand = ApplicationCommands.Delete;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Delete", ExposedCommand.Name, "Delete the selected objects.", "lightbulb_delete.png",
                                                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandDelete_Executed, CommandDelete_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedCommand = ApplicationCommands.Cut;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Cut", ExposedCommand.Name, "Cut the selected objects and let a copy in the Clipboard.", "cut.png",
                                                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandCut_Executed, CommandCut_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedCommand = ApplicationCommands.Copy;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Copy", ExposedCommand.Name, "Copy the selected objects to the Clipboard.", "page_white_copy.png",
                                                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandCopy_Executed, CommandCopy_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedCommand = ApplicationCommands.Paste;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Paste", ExposedCommand.Name, "Paste Clipboard content into current View.", "paste_plain.png",
                                                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandPaste_Executed, CommandPaste_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink);

            ExposedWorkCommand = new GenericCommand("PasteShortcut");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    //T Console.WriteLine("Pasting shortcut...");
                    Engine.ClipboardPaste(Engine.CurrentView, true);
                });
            ExposedWorkCommand.CanApply =
                ((param) =>
                {
                    var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
                    if (Eng == null)
                        return false;

                    var Result = General.Execute(() => Clipboard.ContainsData(CompositionEngine.IdeaTransferFormat.Name),
                                                 "Cannot access Windows Clipboard!").Result;
                    return Result;
                });

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Paste Shortcut", "PasteShortcut", "Paste copied Ideas as Shortcut into the current View.", "paste_shortcut.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            /*- ExposedCommand = ApplicationCommands.Properties;
            this.CommandExpositors.Add(ExposedCommand.Name, new WorkCommandExpositor("Properties", ExposedCommand.Name, "Edits the Properties of the selected Idea.", "page_white_edit.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedCommand));
            CommandLink = new CommandBinding(ExposedCommand, CommandProperties_Executed, CommandProperties_CanExecute);
            Application.Current.MainWindow.CommandBindings.Add(CommandLink); */

            ExposedWorkCommand = new GenericCommand("Convert");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    //T Console.WriteLine("Converting...");
                    Engine.ConvertIdeasToAlternateDefinition(Engine.CurrentView.SelectedRepresentations.Select(vrep => vrep.RepresentedIdea));
                });
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedObjects.Count > 0);

            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Convert", "Convert", "Converts the selected Ideas, as based on other Idea Definition type.", "wand_convert.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Find");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null) return;

                    //T Console.WriteLine("Find...");
                    Composer.ComposerUI.Widgets.FindAndReplaceDialog.Find(false, Engine.TargetComposition);
                });
            ExposedWorkCommand.CanApply = ((param) => ((CompositionEngine)WorkspaceDirector.ActiveDocumentEngine != null));
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Find", "Find", "Find text in objects of the document.", "page_white_find.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("FindAndReplace");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null) return;

                    //T Console.WriteLine("Find & Replace...");
                    Composer.ComposerUI.Widgets.FindAndReplaceDialog.Find(true, Engine.TargetComposition);
                });
            ExposedWorkCommand.CanApply = ((param) => ((CompositionEngine)WorkspaceDirector.ActiveDocumentEngine != null));
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Find & Replace", "FindAndReplace", "Find and Replace text in objects of the document.", "page_white_findreplace.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("SelectAll");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null || Engine.CurrentView == null) return;

                    //T Console.WriteLine("Select All...");
                    Engine.CurrentView.SelectMultipleObjects();
                });
            ExposedWorkCommand.CanApply = ((param) => ((CompositionEngine)WorkspaceDirector.ActiveDocumentEngine != null));
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Select All", "SelectAll", "Sellect all visual objects of the View.", "view_select_all.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("GotoParent");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null || Engine.CurrentView == null) return;

                    //T Console.WriteLine("Go to Parent...");
                    Engine.ShowCompositeAsView(Engine.CurrentView.OwnerCompositeContainer.OwnerContainer);
                });
            ExposedWorkCommand.CanApply = ((param) =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null || Engine.CurrentView == null) return false;

                    return (Engine.CurrentView.OwnerCompositeContainer.Get(occ => occ.OwnerContainer) != null);
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Go to Parent", "GotoParent", "Goes back to the parent Composite-Content View.", "page_view_back.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            ExposedGroup = new SimpleElement("Appearance", "Appearance");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("ShowDetails");
            ExposedWorkCommand.Apply = CommandSwitchDetails_Execution;
            ExposedWorkCommand.CanApply = CommandSwitchDetails_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Switch Details", ExposedWorkCommand.Name, "Shows/Hides the Details poster of the selected Ideas.", "detail_poster.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("FillBrush");
            ExposedWorkCommand.Apply = CommandSelectFillBrush_Execution;
            ExposedWorkCommand.CanApply = CommandSelectFillBrush_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Fill color", "FillBrush", "Selects the filling color brush of the selected objects.", "brush_fill.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // -------------------------------------------------------------------------------------
            /* POSTPONED
            ExposedWorkCommand = new GenericCommand("Shape");
            ExposedWorkCommand.Apply = CommandSelectShape_Execution;
            ExposedWorkCommand.CanApply = CommandSelectShape_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Shape", ExposedWorkCommand.Name, "Changes the symbol's shape.",
                                                                                         "shapes.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                         ExposedWorkCommand, null, null, true,
                                                                                         () =>
                                                                                         {
                                                                                             var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
                                                                                             var Result = (Eng == null ? Shapes.PredefinedShapes : Eng.TargetComposition.CompositeContentDomain.AvailableShapes);
                                                                                             return Result;
                                                                                         })); */

            // -------------------------------------------------------------------------------------

            ExposedWorkCommand = new GenericCommand("LineFormat");
            ExposedWorkCommand.Apply = CommandSelectLineBrush_Execution;
            ExposedWorkCommand.CanApply = CommandSelectLineBrush_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Line format", "LineFormat", "Selects the line format for the selected objects.", "brush_symbol_lines.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("ConnectorLineFormat");
            ExposedWorkCommand.Apply = CommandSelectConnectorBrush_Execution;
            ExposedWorkCommand.CanApply = CommandSelectConnectorBrush_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Connector format", "ConnectorFormat", "Selects the line format of the selected Relationship Connectors.", "brush_connector_lines.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("TextBrush");
            ExposedWorkCommand.Apply = CommandSelectTextBrush_Execution;
            ExposedWorkCommand.CanApply = CommandSelectTextBrush_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Text color", "TextBrush", "Selects the text color brush of the selected objects.", "brush_text.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("TextFormat");
            ExposedWorkCommand.Apply = CommandSelectTextFormat_Execution;
            ExposedWorkCommand.CanApply = CommandSelectTextFormat_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Text format", "TextFormat", "Selects the text format of the selected objects.", "font.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("GetFormat");
            ExposedWorkCommand.Apply = CommandGetFormat_Execution;
            ExposedWorkCommand.CanApply = CommandGetFormat_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Get format", "GetFormat", "Gets the format of the selected Idea.", "style_getter.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("ApplyFormat");
            ExposedWorkCommand.Apply = CommandApplyFormat_Execution;
            ExposedWorkCommand.CanApply = CommandApplyFormat_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Apply format", "ApplyFormat", "Applies the last getted format to the selected Ideas.", "style_setter.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // ............................................................................................................
            ExposedGroup = new SimpleElement("Positioning", "Positioning");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("AlignTop");
            ExposedWorkCommand.Apply = CommandAlignTop_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Align to top", "AlignTop", "Aligns the selected objects at the same Top position of the first selected.", "shape_align_top.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("AlignLeft");
            ExposedWorkCommand.Apply = CommandAlignLeft_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Align to left", "AlignLeft", "Aligns the selected objects at the same Left position of the first selected.", "shape_align_left.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("AlignBottom");
            ExposedWorkCommand.Apply = CommandAlignBottom_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Align to bottom", "AlignBottom", "Aligns the selected objects at the same Bottom position of the first selected.", "shape_align_bottom.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("AlignRight");
            ExposedWorkCommand.Apply = CommandAlignRight_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Align to right", "AlignRight", "Aligns the selected objects at the same Right position of the first selected.", "shape_align_right.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("AlignCenter");
            ExposedWorkCommand.Apply = CommandAlignCenter_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Align to center", "AlignCenter", "Aligns the selected objects at the same (horizontal) Center position of the first selected.", "shape_align_center.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("AlignMiddle");
            ExposedWorkCommand.Apply = CommandAlignMiddle_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Align to middle", "AlignMiddle", "Aligns the selected objects at the same (vertical) Middle position of the first selected.", "shape_align_middle.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("SameWidth");
            ExposedWorkCommand.Apply = CommandAlignSameWidth_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Same Width", "SameWidth", "Resizes the selected objects to the same Width of the first selected.", "shape_same_width.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("SameHeight");
            ExposedWorkCommand.Apply = CommandAlignSameHeight_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Same Height", "SameHeight", "Resizes the selected objects to the same Height of the first selected.", "shape_same_height.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("SameSize");
            ExposedWorkCommand.Apply = CommandAlignSameSize_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin2_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Same Size", "SameSize", "Resizes the selected objects to the same Size of the first selected.",
                                                                                          "shape_same_size.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                          ExposedWorkCommand, null, null));

            ExposedWorkCommand = new GenericCommand("SameSeparationDistanceHorizontal");
            ExposedWorkCommand.Apply = CommandSameSeparationDistanceHorizontal_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin3_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Same Separation Horiz.", "SameSeparationDistanceHorizontal", "Separates the selected objects with the same horizontal distance.",
                                                                                          "shape_spacing_horiz.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                          ExposedWorkCommand, null, null));

            ExposedWorkCommand = new GenericCommand("SameSeparationDistanceVertical");
            ExposedWorkCommand.Apply = CommandSameSeparationDistanceVertical_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin3_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Same Separation Verti.", "SameSeparationDistanceVertical", "Separates the selected objects with the same vertical distance.",
                                                                                          "shape_spacing_verti.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                          ExposedWorkCommand, null, null));

            ExposedWorkCommand = new GenericCommand("SameSeparationDistanceBoth");
            ExposedWorkCommand.Apply = CommandSameSeparationDistanceBoth_Execution;
            ExposedWorkCommand.CanApply = CommandMultiPosMin3_IsEnabled;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Same Separation Both", "SameSeparationDistanceBoth", "Separates the selected objects with both, the same horizontal and vertical distances.",
                                                                                          "shape_spacing_both.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                          ExposedWorkCommand, null, null));

            ExposedWorkCommand = new GenericCommand("Bring to Front");
            ExposedWorkCommand.Apply = CommandBringToFront_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedObjects.Count > 0);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Bring to Front", "BringToFront", "Brings the selected objects over all the others.", "shape_move_front.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Send to Back");
            ExposedWorkCommand.Apply = CommandSendToBack_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedObjects.Count > 0);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Send to Back", "SendToBack", "Sends the selected objects under all the others.", "shape_move_back.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Bring Forward");
            ExposedWorkCommand.Apply = CommandBringForward_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedObjects.Count > 0);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Bring Forward", "BringFoward", "Brings the selected objects upwards.", "shape_move_forwards.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Send Backward");
            ExposedWorkCommand.Apply = CommandSendBackward_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedObjects.Count > 0);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Send Backward", "SendBackward", "Sends the selected objects downwards.", "shape_move_backwards.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Flip Horizontal");
            ExposedWorkCommand.Apply = CommandFlipHorizontally_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedRepresentations.Any());
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Flip Horizontal", "FlipHorizontal", "Flips the symbol on its Horizontal axis.", "shape_flip_horizontal.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Flip Vertical");
            ExposedWorkCommand.Apply = CommandFlipVertically_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedRepresentations.Any());
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Flip Vertical", "FlipVertical", "Flips the symbol on its Vertical axis.", "shape_flip_Vertical.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Tilt");
            ExposedWorkCommand.Apply = CommandTilt_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedRepresentations.Any());
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Tilt", "Tilt", "Tilts the selected symbols 90º clockwise (flip them to change orientation).", "shape_tilt.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Show As Multiple");
            ExposedWorkCommand.Apply = CommandShowAsMultiple_Execution;
            ExposedWorkCommand.CanApply = (par => this.WorkspaceDirector.ActiveDocumentEngine is CompositionEngine && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView != null
                                                  && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedRepresentations.Any());
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor("Show as Multiple", "ShowAsMultiple", "Indicates to show the symbol as multiple stacked ones.", "shape_multiple.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ExposedArea = new SimpleElement("Styles", "Styles");
            this.CommandAreas_.Add(ExposedArea);

            ExposedGroup = new SimpleElement("Graphics", "Graphics");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Change Graphic Format");
            ExposedWorkCommand.Apply = CommandChangeGraphicStyle_Execution;
            ExposedWorkCommand.CanApply = CommandChangeGraphicStyle_IsEnabled;
            var Expositor = new WorkCommandExpositor("Change Graphic Format", "ChangeGraphicFormat", "Changes the graphic format of the selected objects.", "page_white_stack.png",
                                                     EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, ECommandExpositorStyle.ListView,
                                                        () => WidgetsHelper.GenerateGraphicStyleSamples());
            // mmm... better not: Expositor.GoesToRootAfterExecute = true;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, Expositor);
            
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ExposedArea = new SimpleElement("View", "View");
            this.CommandAreas_.Add(ExposedArea);

            ExposedGroup = new SimpleElement("Visualize", "Visualize");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Actual Size");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    /*T
                    TextFormat.DefaultFormattingMode = (TextFormat.DefaultFormattingMode == TextFormattingMode.Ideal
                                                        ? TextFormattingMode.Display : TextFormattingMode.Ideal);
                    Engine.CurrentView.ShowAll();
                    return; */

                    if (Engine == null || Engine.CurrentView == null || Engine.CurrentView.PageDisplayScale == 100)
                        return;

                    Engine.StartCommandVariation("Actual Size");
                    Engine.CurrentView.PageDisplayScale = 100;
                    Engine.CompleteCommandVariation();
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ActualSize", "Shows the view in its actual size (100%).", "zoom_actual.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Zoom In");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Engine.StartCommandVariation("Zoom In");
                    Engine.CurrentView.PageDisplayScale = Engine.CurrentView.PageDisplayScale + 10;
                    Engine.CompleteCommandVariation();
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ZoomIn", "Increases the zoom by a 10%, making the objects look bigger/closer.", "zoom_in.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Zoom Out");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Engine.StartCommandVariation("Zoom Out");
                    Engine.CurrentView.PageDisplayScale = Engine.CurrentView.PageDisplayScale - 10;
                    Engine.CompleteCommandVariation();
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ZoomOut", "Decreases the zoom by a 10%, making the objects look smaller/farther.", "zoom_out.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            ExposedWorkCommand = new GenericCommand("Fit to View");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Engine.CurrentView.FitContentIntoView();
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "FitToView", "[F8] Make the content fit into the current view tab.", "view_globally.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand));

            /* PENDING...
            ExposedWorkCommand = new GenericCommand("Presentation Mode");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    // PENDING Engine.CurrentView.PageDisplayScale = Engine.CurrentView.PageDisplayScale - 10;
                });
            ExposedWorkCommand.CanApply = ((param) => false );  // (this.WorkspaceDirector.ActiveDocument as Composition) != null
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "PresentationMode",
                                                                                         "Show the current Composition's views as a sequence of slides.",
                                                                                         "view_presen.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                         ExposedWorkCommand, null, null));

            ExposedWorkCommand = new GenericCommand("Full-Screen Mode");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    // PENDING Engine.CurrentView.PageDisplayScale = Engine.CurrentView.PageScale - 10;
                });
            ExposedWorkCommand.CanApply = ((param) => false);   //(this.WorkspaceDirector.ActiveDocument as Composition) != null
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "FullScreenMode",
                                                                                         "Uses all the screen space to edit content, teporarily hiding menu and panels.",
                                                                                         "monitor.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                         ExposedWorkCommand, null, null)); */

            ExposedGroup = new SimpleElement("  Indications", "Indications");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Show Grid");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Grid");
                    Engine.CurrentView.ShowContextGrid = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowGrid", "Show/hide the Grid at the backgroud of the view.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand,
                                                                                          (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowContextGrid)));

            ExposedWorkCommand = new GenericCommand("Snap to Grid");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Snap to Grid");
                    Engine.CurrentView.SnapToGrid = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "SnapToGrid",
                                                                                         "Forces the alignments of object to Grid points.",
                                                                                         "lightbulb.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                         ExposedWorkCommand,
                                                                                         (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.SnapToGrid)));

            ExposedWorkCommand = new GenericCommand("Grid of Lines");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Set Grid of Lines");
                    Engine.CurrentView.GridUsesLines = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "GridUsesLines", "Makes the Grid to be Lines based, else Points based.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.GridUsesLines)));

            ExposedWorkCommand = new GenericCommand("Indicators");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Indicators");
                    Engine.CurrentView.ShowIndicators = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowIndicators", "Show/hide the indicators (of related, composite and detailed content) over the Ideas.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowIndicators)));

            ExposedWorkCommand = new GenericCommand("Markers");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Markers");
                    Engine.CurrentView.ShowMarkers = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowMarkers", "Show/hide the Markers over the Ideas.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowMarkers)));

            ExposedWorkCommand = new GenericCommand("Markers Titles");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Markers Titles");
                    Engine.CurrentView.ShowMarkersTitles = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowMarkersTitles", "Show/hide the Titles of the Markers over the Ideas.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowMarkersTitles)));

            ExposedWorkCommand = new GenericCommand("Concept Definition Labels");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Concept Definition Labels");
                    Engine.CurrentView.ShowConceptDefinitionLabels = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowConceptDefinitionLabels", "Show/hide the Concept Definition Name over each Concept.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowConceptDefinitionLabels)));

            ExposedWorkCommand = new GenericCommand("Relationship Definition Labels");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Relationship Definition Labels");
                    Engine.CurrentView.ShowRelationshipDefinitionLabels = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowRelationshipDefinitionLabels", "Show/hide the Relationship Definition Name over each Relationship.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowRelationshipDefinitionLabels)));

            ExposedWorkCommand = new GenericCommand("Link-Role Descriptor Labels");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Link-Role Descriptor Name Labels");
                    Engine.CurrentView.ShowLinkRoleDescNameLabels = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowLinkRoleDescNameLabels", "Show/hide the Link-Role Descriptor name over each Connector.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowLinkRoleDescNameLabels)));

            ExposedWorkCommand = new GenericCommand("Link-Role Definitor Labels");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Link-Role Definitor Name Labels");
                    Engine.CurrentView.ShowLinkRoleDefNameLabels = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowLinkRoleDefNameLabels", "Show/hide the Link-Role Definitor name over each Connector.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowLinkRoleDefNameLabels)));

            ExposedWorkCommand = new GenericCommand("Link-Role Variant Labels");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Mouse.OverrideCursor = Cursors.Wait;
                    Engine.StartCommandVariation("Show/Hide Link-Role Variant Labels");
                    Engine.CurrentView.ShowLinkRoleVariantLabels = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CurrentView.ShowAll();
                    Engine.CompleteCommandVariation();
                    Mouse.OverrideCursor = null;
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "ShowLinkRoleVariantLabels", "Show/hide the Link-Role Definition Variant over each Connector.", "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.ShowLinkRoleVariantLabels)));

            ExposedWorkCommand = new GenericCommand("Auto-Size by Entered Text");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null)
                        return;

                    Engine.StartCommandVariation("Switch Auto-Size by entered text");
                    Engine.CurrentView.AutoSizeByEnteredText = (bool)par; // IMPORTANT: Do not negate property, to keep coherence with checkbox even after undos/redos.
                    Engine.CompleteCommandVariation();
                });
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "AutoSizeByEnteredText", "Automatically adjusts size of objects based on entered text..", "lightbulb.png",
                                                                                         EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, (engine) => General.Get(engine as CompositionEngine, eng => eng.CurrentView.AutoSizeByEnteredText)));

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            ExposedArea = new SimpleElement("Tools", "Tools");
            this.CommandAreas_.Add(ExposedArea);

            /*+ PENDING: Re include in Solution/project the files of next folder: Instrumind.ThinkComposer.Composer.Merging
             *   BUILD ACTION: Compile for C# files and Page for XAML files.

            ExposedGroup = new SimpleElement("Input", "Input");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Merge with...");
            ExposedWorkCommand.Apply = (param => Merging.MergingManager.MergeCompositions());
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null
                                           && ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "Merge Compositions/Domains", false, new DateTime(2013, 11, 1)));
            Expositor = new WorkCommandExpositor("Merge with...", "MergeWith", "Merge this Composition/Domain with other.", "book_merge.png",
                                                 EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, Expositor); */

            ExposedGroup = new SimpleElement("Output", "Output");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Generate Files...");
            ExposedWorkCommand.Apply = (param => Generation.GenerationManager.GenerateGlobalFiles(this.WorkspaceDirector.ActiveDocument as Composition));
            ExposedWorkCommand.CanApply = ((param) => (this.WorkspaceDirector.ActiveDocument as Composition) != null
                                           && ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "Generate Files", false, new DateTime(2013, 6, 22)));
            Expositor = new WorkCommandExpositor("Generate Files...", "GenerateFiles", "Generate Files/Code from Composition Ideas, in a selected Language.", "fgen_go.png",
                                                 EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, Expositor);

            ExposedWorkCommand = new GenericCommand("Generation Preview");
            ExposedWorkCommand.Apply = (param => Generation.GenerationManager.ShowGenerationFilePreview(
                                                    ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView
                                                                                .SelectedRepresentations.First().RepresentedIdea));
            ExposedWorkCommand.CanApply = ((param) => this.WorkspaceDirector.ActiveDocument != null
                                           && ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine).CurrentView.SelectedRepresentations.Any()
                                           && ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "Generate Files", false, new DateTime(2013, 6, 22)));
            Expositor = new WorkCommandExpositor("Generation Preview", "GenerationPreview", "Generates text, from the selected Idea, based on an Output-Template.", "fgen_prev.png",
                                                 EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand);
            this.CommandExpositors.Add(ExposedWorkCommand.Name, Expositor);

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            /*
            ExposedGroup = new SimpleElement("Styles", "Styles");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Test AA");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    Console.WriteLine("New GUID = ", (new Guid()).ToString());
                    Console.WriteLine("UNDECLARED MODEL-CLASSES FIELDS... ------------------------------------");

                    // Useful to determine what properties would not be automatically serialized by a custom-made serializer.
                    foreach (var Undeclared in MModelClassDefinitor.DetectUndeclaredSerializableModelFields())
                        Console.WriteLine(Undeclared);

                    Console.WriteLine("======================================================");

                    //var Names = new List<Capsule<string, string>>();
                    //Names.Add(Capsule.Create("Albert", ""));
                    //Names.Add(Capsule.Create("Zeus", ""));
                    //Names.Add(Capsule.Create("Robert", ""));
                    //Names.Add(Capsule.Create("George", ""));
                    //Names.Add(Capsule.Create("Victor", ""));

                    //foreach (var Name in Names)
                    //    Name.Value1 = Name.Value0.AsInverseSortable();

                    //foreach (var Name in Names.OrderBy(name => name.Value1))
                    //    Console.WriteLine("Name='{0}', Key='{1}'.", Name.Value0, Name.Value1);

                    //Console.WriteLine("******************************************************");

                    //var Serializer = new SimpleSynthSerializer();
                    //var Destination = Display.DialogGetSaveFile("Serialize to...", "TXT", null, true, "D:\\");
                    //if (Destination == null)
                    //    return;

                    //var File = new FileStream(Destination.LocalPath, FileMode.CreateNew);
                    //Serializer.Serialize(File, new object());

                    // throw new UsageAnomaly("Testing Exception Logging", this.CompositionEditors.First());
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "TestAA", "Test.",
                                                                                          "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                          ExposedWorkCommand, null, null));
            */
            /*
            ExposedGroup = new SimpleElement("Formats", "Formats");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            ExposedWorkCommand = new GenericCommand("Test BB");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    //var Engine = this.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    //if (Engine == null || Engine.CurrentView.SelectedObjects.Count < 1)
                    //    return;

                    //var Selection = Engine.CurrentView.SelectedObjects.First();
                    
                    //Console.WriteLine("Selected object = '{0}'.", Selection);

                    dynamic DynObj = new System.Dynamic.ExpandoObject();
                    DynObj.TheName = "John";
                    var Result = General.ExtractFieldValue(DynObj, "TheZName");
                    Console.WriteLine("Extraction ({0}): '{1}'.", Result.Item1, Result.Item2);
                    Result = General.ExtractFieldValue(DynObj, "TheName");
                    Console.WriteLine("Extraction ({0}): '{1}'.", Result.Item1, Result.Item2);
                    DynObj.TheName = "Peter";
                    Result = General.ExtractFieldValue(DynObj, "TheXName");
                    Console.WriteLine("Extraction ({0}): '{1}'.", Result.Item1, Result.Item2);
                    Result = General.ExtractFieldValue(DynObj, "TheName");
                    Console.WriteLine("Extraction ({0}): '{1}'.", Result.Item1, Result.Item2);

                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "TestBB", "Test.",
                                                                                          "lightbulb.png",
                                                                                          EShellCommandCategory.Edition, ExposedArea.TechName, ExposedGroup.TechName,
                                                                                          ExposedWorkCommand, null, null));
            */

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ExposedArea = new SimpleElement("Recent", "Recent");
            this.CommandAreas_.Add(ExposedArea);

            ExposedGroup = new SimpleElement("Documents", "Documents");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);

            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Recent");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    if (par == null || !(par is string) || par.ToStringAlways().IsAbsent())
                    {
                        Console.WriteLine("Cannot open the specified document '{0}'.", par.ToStringAlways());
                        return;
                    }

                    var Document = par.ToString();
                    var Parts = Document.Split('\t');
                    string Route = Parts[0];
                    if (Parts.Length > 1 && Parts[1].Length > 2)
                        Route = Path.Combine(Parts[1].Substring(1, Parts[1].Length - 2), Parts[0]);
                    else
                    {
                        Console.WriteLine("Cannot open file (maybe Recent-list file is corrupt).");
                        return;
                    }

                    //T Console.WriteLine("Opening recent document '{0}'.", Route);

                    var Location = new Uri(Route, UriKind.Absolute);

                    var Extension = Path.GetExtension(Route).Substring(1).ToLower();
                    if (Extension == FileDataType.FileTypeDomain.Extension.ToLower())
                        OpenDomainAndCreateCompositionOfIt(true, Location, false);
                    else
                        OpenComposition(Location);
                });
            ExposedWorkCommand.CanApply = (par => true);
            Expositor = new WorkCommandExpositor("Recent", ExposedWorkCommand.Name, "Opens recent documents.", "page_white_stack.png",
                                                 EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName, ExposedWorkCommand, ECommandExpositorStyle.ListBox,
                                                 () => DocumentEngine.RecentDocuments.Select(doc => Path.GetFileName(doc) +
                                                                                                    "\t(" + Path.GetDirectoryName(doc) + ")"));
            Expositor.GoesToRootAfterExecute = true;
            this.CommandExpositors.Add(ExposedWorkCommand.Name, Expositor);

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            /* ExposedArea = new SimpleElement("Extras", "Extras");
            this.CommandAreas_.Add(ExposedArea);

            ExposedGroup = new SimpleElement("Global", "Global");
            this.CommandGroups_.PutIntoSublist(ExposedArea.TechName, ExposedGroup);
            
            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new CommonCommands.AboutCommand();
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "About...", "Product credits.",
                                                                          "/Instrumind.Common;component/Visualization/Images/instrumind_16x16.png",
                                                                          EShellCommandCategory.Extra, ExposedArea.TechName, ExposedGroup.TechName,
                                                                          ExposedWorkCommand, null, null));
            
            // -------------------------------------------------------------------------------------
            ExposedWorkCommand = new GenericCommand("Test");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    Console.WriteLine("UNDECLARED MODEL-CLASSES FIELDS... ------------------------------------");

                    foreach (var Undeclared in MModelClassDefinitor.DetectUndeclaredSerializableModelFields())
                        Console.WriteLine(Undeclared);

                    Console.WriteLine("======================================================");

                    //var Serializer = new SimpleSynthSerializer();
                    //var Destination = Display.DialogGetSaveFile("Serialize to...", "TXT", null, true, "D:\\");
                    //if (Destination == null)
                    //    return;

                    //var File = new FileStream(Destination.LocalPath, FileMode.CreateNew);
                    //Serializer.Serialize(File, new object());
                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Test...", "Test.",
                                                                          "lightbulb.png",
                                                                          EShellCommandCategory.Extra, ExposedArea.TechName, ExposedGroup.TechName,
                                                                          ExposedWorkCommand, null, null));
            */
            // -------------------------------------------------------------------------------------

            /*
            ExposedWorkCommand = new GenericCommand("Experiment");
            ExposedWorkCommand.Apply =
                (par =>
                {
                    var HashBase = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();

                    var  Id = DateTime.UtcNow.ToString("yyMMddhhmmss") + HashBase.GetRight(4);

                    var Inverse = Id.AsInverseSortable(0, '0', 'Z');

                    Console.WriteLine("ID=[{0}], Inv=[{1}], Length={2}.", Id, Inverse, Id.Length);

                });
            this.CommandExpositors.Add(ExposedWorkCommand.Name, new WorkCommandExpositor(ExposedWorkCommand.Name, "Experiment...", "Experiment.",
                                                                    "lightbulb.png",
                                                                    EShellCommandCategory.Document, ExposedArea.TechName, ExposedGroup.TechName,
                                                                    ExposedWorkCommand, null, null));
            */
            
            // -------------------------------------------------------------------------------------
        }

        // -------------------------------------------------------------------------------------------------------------------------
    }
}