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
// File   : CompositionsManager.ProjectCommands.cs
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
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.Definitor;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Manages the edition of user-defined Compositions, working as an intermediary for external consumers.
    /// Project Commands part.
    /// </summary>
    public partial class CompositionsManager : WorkSphere
    {
        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandNew_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
            args.Handled = true;
        }

        public void CommandNew_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            /*T try
            {
                Console.Write("Trying to divide by zero: " + (1 / ("x".Length - 1)).ToString());
            }
            catch (Exception Problem)
            {
                throw new InternalAnomaly("Intentional divide-by-zero error for testing.", Problem);
            } */

            string CommandName = ((RoutedCommand)args.Command).Name;

            this.OpenDomainAndCreateCompositionOfIt(false);
        }

        public void CreateComposition(bool IsForDomainCreation = false)
        {
            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var PreviousActiveDoc = this.WorkspaceDirector.ActiveDocument;
            this.WorkspaceDirector.ActiveDocument = null;   // Must deactive previous to create+activate the opening Composition.

            CompositionEngine.CreateActiveCompositionEngine(this, this.Visualizer, IsForDomainCreation);
            var Result = CompositionEngine.Materialize();
            if (Result.Item1 == null)
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot create new Composition.\n\nProblem: " + Result.Item2, EMessageType.Warning);
                this.WorkspaceDirector.ActiveDocument = PreviousActiveDoc;
                return;
            }

            // Start visual interactive editing and show document view.
            this.WorkspaceDirector.LoadDocument(Result.Item1.TargetComposition);
            Result.Item1.Start();

            if (IsForDomainCreation)
            {
                Display.DialogMessage("Attention!",
                                      "Domains are created with a base Composition.\n" +
                                      "So, later it can be saved as the Domain's template.");

                DomainServices.DomainEdit(Result.Item1.TargetComposition.CompositeContentDomain);
            }
            else
            {
                var EditOnNewComposition = AppExec.GetConfiguration<bool>("Composition", "EditOnNewComposition", true);
                if (EditOnNewComposition)
                    Result.Item1.EditCompositionProperties();
            }

            CurrentWindow.Cursor = Cursors.Arrow;
        }

        public void OpenDomainAndCreateCompositionOfIt(bool IsForOpenDomain, Uri Location = null, bool CanEditPropertiesOfNewCompo = true)
        {
            bool OpenCompositionStoredWithDomain = true;

            if (IsForOpenDomain)
                Console.WriteLine("Opening Domain (and creating its Composition)...");
            else
                Console.WriteLine("Creating new Composition of Domain...");

            if (Location == null)
            {
                var Selection = DomainSelector.SelectDomain(AppExec.ApplicationSpecificDefinitionsDirectory,
                                                            (IsForOpenDomain ? null :
                                                             "Select Domain file to create Composition..."),
                                                            !IsForOpenDomain);
                if (Selection == null)
                    return;

                Location = Selection.Item1;
                OpenCompositionStoredWithDomain = (IsForOpenDomain || Selection.Item2);

                // If 'Basic Domain' was selected...
                if (Location == null)
                {
                    this.CreateComposition();
                    return;
                }

                /* Previously...
                Location = Display.DialogGetOpenFile("Select Domain",
                                                     FileDataType.FileTypeDomain.Extension,
                                                     FileDataType.FileTypeDomain.FilterForOpen); */
            }
            if (Location == null)
                return;

            /* NOT APPLICABLE FOR DOMAINS
            // Switch to document if already opened
            var CurrentlyOpenedDoc = this.WorkspaceDirector.Documents
                .FirstOrDefault(doc => doc.DocumentEditEngine.Location != null && !doc.DocumentEditEngine.Location.LocalPath.IsAbsent()
                                       && doc.DocumentEditEngine.Location.LocalPath == Location.LocalPath);

            if (CurrentlyOpenedDoc != null)
            {
                this.WorkspaceDirector.ActivateDocument(CurrentlyOpenedDoc);
                return;
            }
            */

            /* unnecessary
            if (IsForOpenDomain)
                Application.Current.MainWindow.PostCall(mainwin =>
                    Display.DialogMessage("Attention!",
                                          "Domains must are created with a base Composition.\n" +
                                          "So, later it can be saved as the Domain's template."); */

            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var PreviousActiveDoc = this.WorkspaceDirector.ActiveDocument;
            this.WorkspaceDirector.ActiveDocument = null;   // Must deactive previous to create+activate the opening Composition.

            CompositionEngine.CreateActiveCompositionEngine(this, this.Visualizer, IsForOpenDomain);
            var DomainLoad = CompositionEngine.MaterializeDomain(Location);
            if (DomainLoad.Item1 == null)
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot open Domain.\n\nProblem: " + DomainLoad.Item2, EMessageType.Warning);
                this.WorkspaceDirector.ActiveDocument = PreviousActiveDoc;
                return;
            }

            var Result = CompositionEngine.Materialize(null, DomainLoad.Item1, OpenCompositionStoredWithDomain);
            if (Result.Item1 == null)
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot create Composition of Domain.\n\nProblem: " + Result.Item2, EMessageType.Warning);
                this.WorkspaceDirector.ActiveDocument = PreviousActiveDoc;
                return;
            }

            // Start visual interactive editing and show document view.
            Result.Item1.DomainLocation = Location;
            this.WorkspaceDirector.LoadDocument(Result.Item1.TargetDocument);

            Result.Item1.Start();

            if (IsForOpenDomain)
                DomainServices.DomainEdit(Result.Item1.TargetComposition.CompositeContentDomain);
            else
                if (CanEditPropertiesOfNewCompo)
                {
                    var EditOnNewComposition = AppExec.GetConfiguration<bool>("Composition", "EditOnNewComposition", true);
                    if (EditOnNewComposition)
                        Result.Item1.EditCompositionProperties();
                }

            CurrentWindow.Cursor = Cursors.Arrow;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandOpen_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
            args.Handled = true;
        }

        public void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            OpenComposition();
        }

        public static Composition OpenComposition(Uri Location = null, bool CanEditPropertiesOfNewCompo = true)
        {
            var CurrentWindow = Display.GetCurrentWindow();

            if (Location == null)
                Location = Display.DialogGetOpenFile("Open Composition",
                                                     FileDataType.FileTypeComposition.Extension,
                                                     FileDataType.FileTypeComposition.FilterForOpen);
            if (Location == null)
                return null;

            //-? DEPRECATE?:
            // Switch to document if already opened
            var CurrentlyOpenedDoc = ProductDirector.WorkspaceDirector.Documents
                .FirstOrDefault(doc => doc.DocumentEditEngine.Location != null && !doc.DocumentEditEngine.Location.LocalPath.IsAbsent()
                                       && doc.DocumentEditEngine.Location.LocalPath == Location.LocalPath);

            if (CurrentlyOpenedDoc != null)
            {
                ProductDirector.WorkspaceDirector.ActivateDocument(CurrentlyOpenedDoc);
                return (CurrentlyOpenedDoc as Composition);
            }

            CurrentWindow.Cursor = Cursors.Wait;

            var PreviousActiveDoc = ProductDirector.WorkspaceDirector.ActiveDocument;
            ProductDirector.WorkspaceDirector.ActiveDocument = null;   // Must deactive previous to create+activate the opening Composition.

            CompositionEngine.CreateActiveCompositionEngine(ProductDirector.CompositionDirector, ProductDirector.CompositionDirector.Visualizer, false);
            var Result = CompositionEngine.Materialize(Location);
            if (Result.Item1 == null)
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot open Composition.\n\nProblem: " + Result.Item2, EMessageType.Warning);
                ProductDirector.WorkspaceDirector.ActiveDocument = PreviousActiveDoc;
                return null;
            }

            // Start visual interactive editing and show document view.
            ProductDirector.WorkspaceDirector.LoadDocument(Result.Item1.TargetComposition);
            Result.Item1.Start();

            /* Only for Creation, not opening.
            if (CanEditPropertiesOfNewCompo)
            {
                var EditOnOpenComposition = AppExec.GetConfiguration<bool>("Composition", "EditOnOpenComposition", true);
                if (EditOnOpenComposition)
                    Result.Item1.EditCompositionProperties();
            } */

            CurrentWindow.Cursor = Cursors.Arrow;

            return Result.Item1.TargetComposition;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            args.CanExecute = (this.WorkspaceDirector.ActiveDocument != null);
            args.Handled = true;
        }

        public void CommandSave_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var Engine = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Engine == null)
                throw new InternalAnomaly("Cannot obtain editing engine from command args.", args);

            if (!Save(Engine))
                return;

            this.WorkspaceDirector.ShellProvider.RefreshSelection();
        }

        private bool Save(CompositionEngine Engine)
        {
            var Location = Engine.FullLocation;

            if (Location == null)
                Location = Display.DialogGetSaveFile("Save Composition",
                                                     FileDataType.FileTypeComposition.Extension,
                                                     FileDataType.FileTypeComposition.FilterForSave,
                                                     Engine.TargetComposition.TechName);
            if (Location == null)
                return false;

            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var Result = Engine.Store(Location);
            if (!Result.IsAbsent())
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot save Composition.\n\nProblem: " + Result, EMessageType.Warning);
                return false;
            }

            CurrentWindow.Cursor = Cursors.Arrow;

            return true;
        }

        public void CommandSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            args.CanExecute = (this.WorkspaceDirector.ActiveDocument != null);
            args.Handled = true;
        }

        public void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var Engine = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Engine == null)
                throw new InternalAnomaly("Cannot obtain editing engine from command args.", args);

            var Location = Display.DialogGetSaveFile("Save Composition as",
                                                     FileDataType.FileTypeComposition.Extension,
                                                     FileDataType.FileTypeComposition.FilterForSave,
                                                     (Engine.FullLocation == null
                                                      ? Engine.TargetComposition.TechName
                                                      : Path.GetFileName(Engine.FullLocation.LocalPath))
                                                     /*- Engine.TargetComposition.TechName */
                                                     /*- (Engine.FullLocation == null
                                                     ? Path.Combine(AppExec.UserDataDirectory, Engine.TargetComposition.TechName)
                                                     : Engine.FullLocation.LocalPath)*/);
            if (Location == null)
                return;

            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var Result = Engine.Store(Location);
            if (!Result.IsAbsent())
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot save Composition.\n\nProblem: " + Result, EMessageType.Warning);
                return;
            }

            this.WorkspaceDirector.ShellProvider.RefreshSelection();
            CurrentWindow.Cursor = Cursors.Arrow;
        }

        public void Send(Composition TargetComposition)
        {
            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var Engine = TargetComposition.Engine;
            var FilePath = Path.Combine(AppExec.GetTempFolder(),
                                        TargetComposition.TechName + "." + FileDataType.FileTypeComposition.Extension);
            var Location = new Uri(FilePath, UriKind.Absolute);
            var SavingResult = Engine.Store(Location, false, false, false);

            if (!SavingResult.IsAbsent())
            {
                CurrentWindow.Cursor = Cursors.Arrow;
                Display.DialogMessage("Error!", "Cannot temporarily save Composition.\n\nProblem: " + SavingResult, EMessageType.Warning);
                return;
            }

            var SendingResult = General.SendMailTo("", "Composition: " + TargetComposition.Name, "", FilePath);

            CurrentWindow.Cursor = Cursors.Arrow;

            if (!SendingResult.IsAbsent())
                Display.DialogMessage("Error!", "Cannot send e-mail via Outlook.\n\nProblem: " + SendingResult, EMessageType.Warning);
        }

        public void ExportCurrentView(Composition TargetComposition)
        {
            /*  Previous:
            var SourceViewDocuName = TargetComposition.ActiveView.OwnerCompositeContainer.Name + " - " +
                                     TargetComposition.ActiveView.Name;
            var SourceViewFileName = TargetComposition.ActiveView.OwnerCompositeContainer.TechName + "." +
                                     TargetComposition.ActiveView.TechName; */

            var SourceViewDocuName = TargetComposition.ActiveView.OwnerCompositeContainer.GetContainmentRoute(true, false, false, true, " - ");
            SourceViewDocuName = SourceViewDocuName.Substring(1, SourceViewDocuName.Length - 1);

            var SourceViewFileName = TargetComposition.ActiveView.OwnerCompositeContainer.GetContainmentRoute(false, true, false, true, ".");
            SourceViewFileName = SourceViewFileName.Substring(1, SourceViewFileName.Length - 1);

            var AsTransparent = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            var Location = Display.DialogGetSaveFile("Export '" + SourceViewDocuName + "' to",
                                                     FileDataType.FileTypeImage.Extension,
                                                     FileDataType.FileTypeImage.FilterForSave +
                                                     (Display.CanConvertXpsToPdf
                                                      ? "|PDF document (*.pdf)|*.pdf" : "") +
                                                     "|XPS document (*.xps)|*.xps",
                                                     SourceViewFileName
                                                     /*- (TargetComposition.Engine.FullLocation == null
                                                     ? Path.Combine(AppExec.UserDataDirectory, SourceViewFileName)
                                                     : Path.Combine(Path.GetDirectoryName(TargetComposition.Engine.FullLocation.LocalPath),
                                                                    SourceViewFileName))*/);
            if (Location == null)
                return;

            string ErrorMessage = null;
            var Extension = Location.LocalPath.GetRight(4).ToUpper();

            // IMPORTANT: This can crash by out-of-memory if the generated bitmap is huge.
            var Snapshot = TargetComposition.ActiveView.ToSnapshot(AsTransparent);
            if (Snapshot == null)
            {
                Console.WriteLine("No content to export.");
                return;
            }

            try
            {
                if (Extension.IsOneOf(".XPS", ".PDF"))
                {
                    if (Extension == ".XPS")
                        ErrorMessage = Display.SaveDocumentAsXPS(Snapshot.Item1, Location.LocalPath);

                    if (Extension == ".PDF")
                    {
                        var TempXPS = Path.Combine(AppExec.ApplicationUserTemporalDirectory, General.GenerateRandomFileName("TMP_", "xps"));
                        ErrorMessage = Display.SaveDocumentAsXPS(Snapshot.Item1, TempXPS);

                        if (ErrorMessage.IsAbsent())
                            ErrorMessage = Display.ConvertXPStoPDF(TempXPS, Location.LocalPath);

                        if (File.Exists(TempXPS))
                            File.Delete(TempXPS);
                    }
                }
                else
                {
                    /* ALTERNATIVE: For exporting just the shown area.
                    var ErrorMessage = Display.ExportImageTo(Location, TargetComposition.ActiveView.Presenter,
                                                             (int)TargetComposition.ActiveView.HostingScrollViewer.ViewportWidth,
                                                             (int)TargetComposition.ActiveView.HostingScrollViewer.ViewportHeight); */

                    ErrorMessage = Display.ExportImageTo(Location.LocalPath, Snapshot.Item1.RenderToDrawingVisual(),
                                                         (int)Snapshot.Item2.Width,
                                                         (int)Snapshot.Item2.Height);
                }
            }
            catch (Exception Problem)
            {
                ErrorMessage = Problem.Message;
            }

            if (ErrorMessage.IsAbsent())
                Console.WriteLine("View '{0}' successfully exported to '{1}'.", SourceViewDocuName, Location.LocalPath);
            else
                Display.DialogMessage("Error!", "Cannot save image.\n\nProblem: " + ErrorMessage, EMessageType.Error);
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandProperties_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            args.CanExecute = (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
            args.Handled = true;
        }

        public void CommandProperties_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            var Selection = Eng.CurrentView.SelectedObjects.First();

            if (Selection is VisualElement)
                Eng.CurrentView.EditPropertiesOfVisualRepresentation(((VisualElement)Selection).OwnerRepresentation);
            else
                if (Selection is VisualComplement)
                    VisualComplement.Edit(Selection as VisualComplement);
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandPrint_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            args.CanExecute = (this.WorkspaceDirector.ActiveDocument != null && this.WorkspaceDirector.ActiveDocument.ActiveDocumentView != null);
            args.Handled = true;
        }

        public void CommandPrint_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var Engine = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Engine == null)
                throw new InternalAnomaly("Cannot obtain editing engine from command args.", args);

            // SEE: http://stackoverflow.com/questions/502198/convert-wpf-xaml-control-to-xps-document

            Engine.PrintCurrentView();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandClose_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            args.CanExecute = (this.WorkspaceDirector.ActiveDocument != null);
            args.Handled = true;
        }

        public void CommandClose_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var Engine = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Engine == null)
                throw new InternalAnomaly("Cannot obtain editing engine from command args.", args);

            CloseComposition(Engine);
        }

        /// <summary>
        /// Tries to close any pending resources still open (such as temporal Attachment files)
        /// </summary>
        public bool CloseRelatedResources(CompositionEngine Engine)
        {
            var Failure = DetailAttachmentEditor.TryLoadPendingAttachmentsFor(Engine);
            if (Failure == null)
                return true;

            var Result = Display.DialogMessage("Error!",
                                               Failure.Message + "\n\nProblem: " + Failure.InnerException.Message + "\n\n" +
                                               "Do you want to ignore that problem and continue (potentially lossing information)?",
                                               EMessageType.Question,
                                               System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxResult.Cancel);

            return (Result == MessageBoxResult.OK);
        }

        /// <summary>
        /// Tries to close the supplied document engine and returns indication of action executed or cancelled by user.
        /// </summary>
        public bool CloseComposition(CompositionEngine Engine)
        {
            if (Engine.ExistenceStatus == EExistenceStatus.Modified)
            {
                var Result = Display.DialogMessage("Confirmation",
                                                   "The Composition \"" + Engine.TargetComposition.Name + "\" has been modified.\n" +
                                                   (Engine.FullLocation != null ? "\nLocation: " + Engine.FullLocation.LocalPath : "") + "\n\n" +
                                                   "Do you want to save changes?",
                                                   EMessageType.Question,
                                                   System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxResult.Yes);

                if (Result == System.Windows.MessageBoxResult.Cancel)
                    return false;

                if (Result == System.Windows.MessageBoxResult.Yes)
                {
                    if (!CloseRelatedResources(Engine))
                        return false;

                    var Location = Engine.FullLocation;

                    if (Location != null)
                    {
                        var StoreResult = Engine.Store();
                        if (!StoreResult.IsAbsent())
                        {
                            Display.DialogMessage("Error!", "Cannot save Composition.\n\nProblem: " + StoreResult, EMessageType.Warning);
                            return false;
                        }
                    }
                    else
                    {
                        Location = Display.DialogGetSaveFile("Save Composition",
                                                             FileDataType.FileTypeComposition.Extension,
                                                             FileDataType.FileTypeComposition.FilterForSave,
                                                             Engine.TargetComposition.TechName);
                        if (Location == null)
                            return false;

                        var StoreResult = Engine.Store(Location);
                        if (!StoreResult.IsAbsent())
                        {
                            Display.DialogMessage("Error!", "Cannot save Composition.\n\nProblem: " + StoreResult, EMessageType.Warning);
                            return false;
                        }
                    }
                }
            }

            Engine.Stop();

            // Show the new activated engine
            this.WorkspaceDirector.RemoveDocument(Engine.TargetComposition);

            // Finally, clears the palettes if no document remains.
            Application.Current.MainWindow
                .PostCall(win =>
                    {
                        if (this.WorkspaceDirector.Documents.Count < 1)
                            ProductDirector.UpdatePalettes(null);
                    }, true);

            return true;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public override bool Discard()
        {
            // Make a local copy because the docs are being removed.
            var Documents = this.WorkspaceDirector.Documents
                                    .OrderBy(doc => doc != this.WorkspaceDirector.ActiveDocument).ToList();

            foreach (var Document in Documents)
                if (Document.DocumentEditEngine is CompositionEngine)
                {
                    this.WorkspaceDirector.ActiveDocument = Document;

                    if (!CloseComposition((CompositionEngine)Document.DocumentEditEngine))
                        return false;
                }

            Instrumind.ThinkComposer.Composer.ComposerUI.Widgets.DetailAttachmentEditor.StopWatchingAllAttachments();

            return true;
        }

    }
}