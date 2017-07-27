using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Instrumind.Common;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.Reporting.Widgets;
using Instrumind.ThinkComposer.MetaModel.Configurations;

namespace Instrumind.ThinkComposer.Composer.Reporting
{
    static class ReportingManager
    {
        public const string VARIABLE_REF_INI = "[";
        public const string VARIABLE_REF_END = "]";

        private static PrintPreviewer DocViewerControl = null;
        private static DialogOptionsWindow WinFlowDocViewer = null;

        public static bool EditReportConfiguration(string ReportTypeName, ReportConfiguration Configuration)
        {
            var InstanceController = EntityInstanceController.AssignInstanceController(Configuration);
            InstanceController.StartEdit();

            var ConfigPanel = new ReportConfigurationEditor();
            ConfigPanel.DataContext = Configuration;

            var EditPanel = Display.CreateEditPanel(Configuration, ConfigPanel);

            var Result = InstanceController.Edit(EditPanel, "Generate " + ReportTypeName + " Report",
                                                 true, null, 930, 620).IsTrue();

            return Result;
        }

        public static void GeneratePdfXpsReport(CompositionEngine Engine)
        {
            if (Engine == null)
                return;

            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_STANDARD, "Generate PDF/XPS Report"))
                return;

            if (Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration == null)
                Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration = new ReportConfiguration();

            if (!ReportingManager.EditReportConfiguration("PDF/XPS", Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration))
                return;

            var Generator = new ReportStandardGenerator(Engine.TargetComposition, Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration);

            var Title = "Report of " + Engine.TargetComposition.Name;

            ProgressiveThreadedExecutor<int>.Execute("Generating " + Title, Generator.Generate,
                (opresult) =>
                {
                    if (opresult.WasSuccessful && !File.Exists(Generator.GeneratedDocumentTempFilePath))
                        opresult = OperationResult.Failure<int>("Cannot write temporal report file at: " + Generator.GeneratedDocumentTempFilePath);

                    if (!opresult.WasSuccessful)
                    {
                        Display.DialogMessage("Report not completed!", opresult.Message, EMessageType.Warning);
                        return;
                    }

                    try
                    {
                        var GeneratedDocument = Display.LoadDocumentFromXPS(Generator.GeneratedDocumentTempFilePath);
                        var Location = new Uri(Generator.GeneratedDocumentTempFilePath, UriKind.Absolute);
                        var Package = System.IO.Packaging.PackageStore.GetPackage(Location);

                        DocViewerControl = new PrintPreviewer(GeneratedDocument, Generator.GeneratedDocumentTempFilePath,
                                                              Engine.TargetComposition.Name);

                        Display.OpenContentDialogWindow(ref WinFlowDocViewer, Title, DocViewerControl);

                        Package.Close();    // Package remains open, so must closed!
                        //if you don't remove the package from the PackageStore, you won't be able to 
                        //re-open the same file again later (due to System.IO.Packaging's Package store/caching 
                        //rather than because of any file locks) 
                        System.IO.Packaging.PackageStore.RemovePackage(Location);

                        if (File.Exists(Generator.GeneratedDocumentTempFilePath))
                            File.Delete(Generator.GeneratedDocumentTempFilePath);
                    }
                    catch (Exception Problem)
                    {
                        Display.DialogMessage("Attention!", "Cannot show generated Report.\n"
                                              + (!Generator.GeneratedDocumentTempFilePath.IsAbsent() && File.Exists(Generator.GeneratedDocumentTempFilePath)
                                                 ? "It can still be found at: " + Generator.GeneratedDocumentTempFilePath : "")
                                              + "\nProblem: " + Problem.Message);
                    }
                });
        }

        public static void GenerateHtmlReport(CompositionEngine Engine)
        {
            if (Engine == null)
                return;

            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_STANDARD, "Generate HTML Report", false, new DateTime(2013, 1, 1)))
                return;

            if (Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration == null)
                Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration = new ReportConfiguration();

            if (!ReportingManager.EditReportConfiguration("HTML", Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration))
                return;

            var FilePath = Path.Combine(AppExec.UserDataDirectory, (Engine.TargetComposition.TechName + ".html").TextToUrlIdentifier());
            var DialogResponse = Display.DialogGetSaveFile("Select HTML file to create", ".html", "HTML document (*.html)|*.html", FilePath);
            if (DialogResponse == null)
                return;

            // Enforce url-identifier to avoid file-names with space in between.
            var Location = Path.Combine(Path.GetDirectoryName(DialogResponse.LocalPath),
                                        Path.GetFileName(DialogResponse.LocalPath).TextToUrlIdentifier());

            var WarnResponse = Display.DialogPersistableMultiOption("Note", "Your HTML Document is generated along with a '" + ReportHtmlGenerator.CONTENT_FOLDER_SUFFIX + "' directory.\n" +
                                                                            "Please don't forget to manage (copy/move/erase) them together to keep consistency." +
                                                                            (Location == DialogResponse.LocalPath ? ""
                                                                             : "\n\nThe file will be saved as '" + Path.GetFileName(Location) + "'."),
                                                                    null, "Reporting", "InformHTMLContentDir", null,
                                                                    new SimplePresentationElement("OK", "Ok", "", Display.GetAppImage("accept.png")));
            if (WarnResponse == null)
                return;

            var Title = "HTML Report of " + Engine.TargetComposition.Name;

            var Generator = new ReportHtmlGenerator(Engine.TargetComposition, Location,
                                                    Engine.TargetComposition.CompositeContentDomain.ReportingConfiguration);

            ProgressiveThreadedExecutor<int>.Execute("Generating " + Title, Generator.Generate,
                (opresult) =>
                {
                    if (opresult.WasSuccessful && !File.Exists(Generator.GeneratedTempWorkingDocumentFile))
                        opresult = OperationResult.Failure<int>("Cannot write temporal report file at: " + Generator.GeneratedTempWorkingDocumentFile);

                    if (!opresult.WasSuccessful)
                    {
                        Display.DialogMessage("Report not completed!", opresult.Message, EMessageType.Warning);
                        return;
                    }

                    try
                    {
                        var TargetFolder = Path.GetDirectoryName(Location);
                        General.CopyDirectory(Generator.GeneratedTempWorkingDocumentDir, TargetFolder);

                        AppExec.CallExternalProcess(Location);

                        if (File.Exists(Generator.GeneratedTempWorkingDocumentFile))
                            File.Delete(Generator.GeneratedTempWorkingDocumentFile);

                        if (Directory.Exists(Generator.GeneratedTempWorkingDocumentDir))
                            Directory.Delete(Generator.GeneratedTempWorkingDocumentDir, true);
                    }
                    catch (Exception Problem)
                    {
                        Display.DialogMessage("Attention!", "Cannot show generated Report.\n"
                                              + (!Generator.GeneratedTempWorkingDocumentFile.IsAbsent() && File.Exists(Generator.GeneratedTempWorkingDocumentFile)
                                                 ? "It can be still found at: " + Generator.GeneratedTempWorkingDocumentFile : "")
                                              + "\nProblem: " + Problem.Message);
                    }
                });
        }
    }
}
