// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : ProductDirector.cs
// Object : Instrumind.ThinkComposer.ProductDirector (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Start
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct.Widgets;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using System.Windows.Input;
using System.Windows.Media.Effects;

/// Main manager for the Instrumind ThinkComposer product application.
namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Centralized manager for the application Product.
    /// Brings to life it using the provided shell.
    /// </summary>
    public static class ProductDirector
    {
        #region CONSTANTS AND FIXED VALUES

        /// <summary>
        /// Name of the product application.
        /// </summary>
        public const string APPLICATION_NAME = "ThinkComposer";

        /// <summary>
        /// Complete version number of the application. Format: VV.RR.mmdd.
        /// Where: VV=(major)version; RR=major-revision; r=minor-revision, m=month-last-digit, dd=day.
        /// (Forces at least one minor-revision increment per year)
        /// </summary>
        // FIRST NUMBER SEGMENT USED TO VALIDATE LICENSE (Do not prefix with letter).
        public const string APPLICATION_VERSION = "1.5.1604";
        public const string APPLICATION_PRODUCT_ID = "{18B75222-97D8-427A-A49E-DEB316314324}";

        /// <summary>
        /// Year(s) of the copyright registration.
        /// </summary>
        public const string APPLICATION_COPYRIGHT_YEARS = "2011-2016";

        /// <summary>
        /// Product web site.
        /// </summary>
        public const string WEBSITE_URL = "http://www.thinkcomposer.com";   // ALT: "http://instrumind.cloudapp.net/Home"

        /// <summary>
        /// Product Buy web page.
        /// </summary>
        public const string WEBSITE_URLPAGE_BUY = "/Home/Buy";

        /// <summary>
        /// Name for the definitions (meta)documents of the application.
        /// </summary>
        public const string APPLICATION_DEFINITIONS_NAME = "Domains";

        /// <summary>
        /// Name give to user data files.
        /// </summary>
        public const string USER_DOCUMENTS_NAME = "Compositions";

        /// <summary>
        /// Standard text for Aiming.
        /// </summary>
        public const string STD_TEXT_POINTING = "<Nothing pointed>";

        /// <summary>
        /// Standard text for Status.
        /// </summary>
        public const string STD_TEXT_STATUS = "<Ready>";

        /// <summary>
        /// Standard width for virtual/working pages (100% scale).
        /// </summary>
        public const int STANDARD_VIRTUAL_PAGE_WIDTH = 1440;

        /// <summary>
        /// Standard height for virtual/working pages (100% scale).
        /// </summary>
        public const int STANDARD_VIRTUAL_PAGE_HEIGHT = 1070;

        /// <summary>
        /// Minimum number of View pages per orientation that can be specified as default.
        /// This should be an odd number for pointing a central page.
        /// </summary>
        public const int MIN_NUM_VIEW_PAGES = 1;

        /// <summary>
        /// Maximum number of View pages per orientation that can be specified as default.
        /// This should be an odd number for pointing a central page.
        /// </summary>
        public const int MAX_NUM_VIEW_PAGES = 45;

        /// <summary>
        /// Minimum percentage allowed for the page scale.
        /// </summary>
        public const double MIN_PAGE_SCALE = 10;

        /// <summary>
        /// Maximum percentage allowed for the page scale.
        /// </summary>
        public const double MAX_PAGE_SCALE = 500;

        /// <summary>
        /// Supported file types of the product.
        /// </summary>
        public static readonly IEnumerable<FileDataType> SupportedFileTypes = null;

        #endregion

        #region ComponentDeclarations

        public static WidgetPalette PaletteProject = null;
        public static WidgetPalette PaletteEdition = null;
        public static WidgetPalette PaletteGeneral = null;

        public static WidgetNavTree ContentTreeControl = null;
        public static EntitledPanel ContentTree = null;

        //POSTPONED: public static WidgetNavMap NavigatorMapControl = null;
        //POSTPONED: public static EntitledPanel NavigatorMap = null;

        public static EntitledPanel EditorInterrelations = null;
        public static WidgetInterrelationsPanel EditorInterrelationsControl = null;

        public static DocumentPanel DocumentArea = null;
        public static WidgetDocumentVisualizer DocumentVisualizerControl = null;

        public static EntitledPanel Messenger = null;
        public static ConsoleRollPublisher MessengerConsolePublisher = null;

        public static EntitledPanel EditorConceptPalette = null;
        public static WidgetItemsPaletteGroup ConceptPaletteControl = null;

        public static EntitledPanel EditorRelationshipPalette = null;
        public static WidgetItemsPaletteGroup RelationshipPaletteControl = null;

        public static EntitledPanel EditorComplementPalette = null;
        public static WidgetItemsPaletteGroup ComplementPaletteControl = null;

        public static EntitledPanel EditorMarkerPalette = null;
        public static WidgetItemsPaletteGroup MarkerPaletteControl = null;

        public static EntitledPanel StatusArea = null;
        public static WidgetStatusBar StatusBarControl = null;

        public static CompositionsManager CompositionDirector = null;
        public static DomainsManager DomainDirector = null;

        private static List<WorkSphere> WorkSphereManagers = new List<WorkSphere>();

        public static WorkspaceManager WorkspaceDirector = null;

        private static IShellProvider ShellHost = null;

        #endregion

        //------------------------------------------------------------------------------------------ 
        #region ParameterDeclarations

        /// <summary>
        /// Default number of View pages per orientation.
        /// This should be an odd number for pointing a central page.
        /// </summary>
        public static int DefaultNumberOfViewPages { get; set; }

        /// <summary>
        /// Default page display scale percentage for Views.
        /// </summary>
        public static int DefaultPageDisplayScale { get; set; }

        /// <summary>
        /// Default minimum base figure size.
        /// </summary>
        public static Size DefaultMinBaseFigureSize { get; set; }

        /// <summary>
        /// Default minimum details poster height.
        /// </summary>
        public static double DefaultMinDetailsPosterHeight { get; set; }

        /// <summary>
        /// Default size for new Concepts body symbols.
        /// </summary>
        public static Size DefaultConceptBodySymbolSize { get; set; }

        /// <summary>
        /// Default size for new Relationships Central/Main-Symbols.
        /// </summary>
        public static Size DefaultRelationshipCentralSymbolSize { get; set; }

        #endregion

        //------------------------------------------------------------------------------------------ 
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static ProductDirector()
        {
            var InstallDate = AppExec.GetEarliestInstallationDate(ProductDirector.APPLICATION_PRODUCT_ID).Date;

            AppExec.CurrentLicenseType = AppExec.LicenseTypes.GetByTechName(AppExec.LIC_TYPE_COMMERCIAL);
            AppExec.CurrentLicenseEdition = AppExec.LicenseEditions.GetByTechName(AppExec.LIC_EDITION_ULTIMATE);
            AppExec.CurrentLicenseMode = AppExec.LicenseModes.GetByTechName(AppExec.LIC_MODE_PERMANENT);

            if (AppExec.CurrentLicenseMode.TechName == AppExec.LIC_MODE_BETA)
                AppExec.CurrentLicenseExpiration = new DateTime(2012, 6, 1);
            else
                if (AppExec.CurrentLicenseMode.TechName == AppExec.LIC_MODE_TRIAL)
                    if (InstallDate == General.EMPTY_DATE)  // When not installed properly
                        AppExec.CurrentLicenseExpiration = DateTime.Now.Date.AddDays(-1);
                    else
                        AppExec.CurrentLicenseExpiration = InstallDate.AddDays(30); // TRIAL

            //Someday?: AppExec.CurrentLicenseExpiration = General.EMPTY_DATE; // If Subscriptions available, set next due date and validate with website from 5 days before

            AppExec.LicenseAgreement = Instrumind.ThinkComposer.Properties.Resources.License;

            AppExec.ApplicationContentTypeCode = AppExec.ApplicationContentTypeCode + "-" + APPLICATION_NAME.ToLower();

            // NOTE: Complete path to generate is...
            // "pack://application:,,,/Instrumind.ThinkComposer;component/ApplicationProduct/Images/sample.png"
            Display.AppImagesRoute = "/Instrumind.ThinkComposer;component/ApplicationProduct/Images/";

            DefaultNumberOfViewPages = 11;  // PD=15, Test=3

            DefaultPageDisplayScale = 100;

            DefaultMinBaseFigureSize = new Size(VisualSymbolFormat.SYMBOL_MIN_INI_SIZE,
                                                VisualSymbolFormat.SYMBOL_MIN_INI_SIZE);

            DefaultMinDetailsPosterHeight = 32.0;

            DefaultConceptBodySymbolSize = new Size(VisualSymbolFormat.SYMBOL_STD_INI_WIDTH,
                                                    VisualSymbolFormat.SYMBOL_STD_INI_HEIGHT); // Prior: new Size(150, 40);

            // NOTE: With a too little size the actioner buttons (edit-format, edit-properties, switch-content, switch-related) will not appear.
            DefaultRelationshipCentralSymbolSize = new Size(DefaultConceptBodySymbolSize.Width * 0.85,
                                                            DefaultConceptBodySymbolSize.Height * 0.85);

            // Declare supported file types (this later/indirectly uses images, thus must be here and not initialized at the field declararion)
            SupportedFileTypes = new List<FileDataType>{ FileDataType.FileTypeComposition, FileDataType.FileTypeDomain };

            // Register highlighting syntax
            var Syntax = Display.GetEmbeddedResourceString("Instrumind.ThinkComposer.ApplicationProduct.LanguageSyntaxes.TcTemplate.xshd");
            if (Syntax != null)
                Display.RegisterSyntaxLanguage("TcTemplate", "." + Domain.TEMPLATE_FILE_EXT, Syntax);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="HostingShell">Host and provider of shell services</param>
        public static void Initialize(IShellProvider HostingShell)
        {
            General.ContractRequires(HostingShell != null);

            ShellHost = HostingShell;

            //====================================
            // VERY IMPORTANT=

            // Do not set AppTerminationConfirmation() as the CloseConfirmation action.
            // Because this Lambda is entangled into serialized content (Still don't know from where)
            
            var Spheres = WorkSphereManagers;
            ShellHost.CloseConfirmation =
                () =>
                {
                    foreach (var Sphere in Spheres)
                        if (!Sphere.Discard())
                            return false;

                    return true;
                };
            //====================================

            AppExec.LogRegistrationPolicy = AppExec.LOGREG_ALL;
        }

        /// <summary>
        /// Starts the execution of the application product.
        /// </summary>
        public static void Start()
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            AppExec.LoadConfigurationFrom();

            WorkspaceDirector = new WorkspaceManager(ShellHost);
            DocumentVisualizerControl = new WidgetDocumentVisualizer();
            ConceptPaletteControl = new WidgetItemsPaletteGroup();
            RelationshipPaletteControl = new WidgetItemsPaletteGroup();
            MarkerPaletteControl = new WidgetItemsPaletteGroup();
            ComplementPaletteControl = new WidgetItemsPaletteGroup();

            CompositionDirector = new CompositionsManager("Composition Manager", "CompositionManager", "Manager for the Composition work-sphere.",
                                                          Display.GetAppImage("page_white_edit.png"),
                                                          WorkspaceDirector, DocumentVisualizerControl,
                                                          ConceptPaletteControl, RelationshipPaletteControl,
                                                          MarkerPaletteControl, ComplementPaletteControl);

            DomainDirector = new DomainsManager("Domain Manager", "DomainManager", "Manager for the Domain work-sphere.",
                                                Display.GetAppImage("book_edit.png"),
                                                WorkspaceDirector, DocumentVisualizerControl,
                                                ConceptPaletteControl, RelationshipPaletteControl);

            WorkSphereManagers.Add(CompositionDirector);
            WorkSphereManagers.Add(DomainDirector);

            CompositionDirector.ExposeCommands();
            DomainDirector.ExposeCommands();

            InitializeUserInterface();

            ProcessCommandLineArguments();

            Application.Current.MainWindow.PostCall(
                wnd =>
                {
                    // IMPORTANT: This prevents to get some static constructors called when INSIDE A COMMAND, which can roll-back
                    //            important fields (set to null/default) when user clicks a Cancel button on an editing form.
                    AppExec.InvokeAllStaticConstructors();

                    InitialLicenseValidation();

                    Console.WriteLine(ProductDirector.APPLICATION_NAME
                                      + " " + ProductDirector.APPLICATION_VERSION
                                      + " Copyright (C) " + ProductDirector.APPLICATION_COPYRIGHT_YEARS
                                      + " " + Company.NAME_LEGAL);

                    /* No longer used.
                    if (AppExec.CurrentLicenseMode.TechName != AppExec.LIC_MODE_TRIAL)
                        Console.WriteLine("Licensed to: " + AppExec.CurrentLicenseUserName);

                    Console.WriteLine("Type: " + AppExec.CurrentLicenseType.Name
                                      + ". Edition: " + AppExec.CurrentLicenseEdition.Name
                                      + ". Mode: " + AppExec.CurrentLicenseMode.Name
                                      + ". Expiration: " + (AppExec.CurrentLicenseExpiration == General.EMPTY_DATE ? "NEVER" : AppExec.CurrentLicenseExpiration.ToShortDateString())
                                      + "."); */

                    // Console.WriteLine("Started.");

                    // Suppressed
                    ProductUpdateDaylyDetection();

                    Application.Current.MainWindow.Cursor = Cursors.Arrow;

                    wnd.Activate();
                });
        }

        /// <summary>
        /// Terminates the execution of the application product.
        /// </summary>
        public static void Terminate()
        {
            AppExec.StoreConfigurationTo();

            if (LaunchNewVersionSetupOnExit)
                AppExec.CallExternalProcess(SetupSourceLocal);
        }

        public static bool AppTerminationConfirmation()
        {
            foreach (var Sphere in WorkSphereManagers)
                if (!Sphere.Discard())
                    return false;

            return true;
        }

        // DON'T MOVE FROM HERE (could crash on deserialization)
        internal static void InitialLicenseValidation()
        {
            // This is no longer required sin open-source.
            return;

            /*
            // Validate Registration
            MessageBoxResult DialogResult;

            var LicenseFileFound = File.Exists(AppExec.LicenseFilePath);

            if (LicenseFileFound || (!AppExec.CurrentLicenseMode.TechName.IsOneOf(AppExec.LIC_MODE_TRIAL, AppExec.LIC_MODE_BETA)
                                     && AppExec.CurrentLicenseEdition.TechName != AppExec.LIC_EDITION_FREE))
            {
                var IsValid = false;

                try
                {
                    var Document = Licensing.LicenseKeyCodeToDocument(General.FileToString(AppExec.LicenseFilePath));
                    var License = Licensing.ExtractValidLicense(Document, AppExec.ApplicationVersionMajorNumber);

                    AppExec.CurrentLicenseUserName = License.Item1;
                    AppExec.CurrentLicenseType = AppExec.LicenseTypes.GetByTechName(License.Item2);
                    AppExec.CurrentLicenseEdition = AppExec.LicenseEditions.GetByTechName(License.Item3);
                    AppExec.CurrentLicenseMode = AppExec.LicenseModes.GetByTechName(License.Item4);
                    AppExec.CurrentLicenseExpiration = License.Item5;

                    IsValid = true;
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot validate License registration. " +
                                      "Problem: " + Problem.Message);
                }

                if (!IsValid)
                {
                    AppExec.CurrentLicenseExpiration = General.EMPTY_DATE;
                    AppExec.CurrentLicenseType = AppExec.LicenseTypes.GetByTechName(AppExec.LIC_TYPE_NONCOMMERCIAL);
                    AppExec.CurrentLicenseEdition = AppExec.LicenseEditions.GetByTechName(AppExec.LIC_EDITION_FREE);
                    AppExec.CurrentLicenseMode = AppExec.LicenseModes.GetByTechName(AppExec.LIC_MODE_PERMANENT);

                    DialogResult = Display.DialogMessage("Attention",
                                                         "License not found or invalid.\n" +
                                                         "License has been downgraded to Free for Non-Commercial use.\n\n" +
                                                         "Do you want to Buy a License?",
                                                         EMessageType.Question, MessageBoxButton.YesNo, MessageBoxResult.Yes);

                    if (DialogResult == MessageBoxResult.Yes)
                    {
                        AppExec.CallExternalProcess(ProductDirector.WEBSITE_URL + ProductDirector.WEBSITE_URLPAGE_BUY +
                                                    "?LicType=" + AppExec.CurrentLicenseType.TechName +
                                                    "&LicEdition=" + AppExec.CurrentLicenseEdition);
                        RegisterLicense();
                        return;
                    }
                }
            }

            var SelectedDocument = WorkspaceDirector.ShellProvider.MainSelector.SelectedItem as ISphereModel;
            EntitleApplication(SelectedDocument.ToStringAlways());

            // Validate Expiration
            if (DateTime.Now <= AppExec.CurrentLicenseExpiration || AppExec.CurrentLicenseExpiration == General.EMPTY_DATE)
                return;

            var PreviousLicenseMode = AppExec.CurrentLicenseMode;

            AppExec.CurrentLicenseExpiration = General.EMPTY_DATE;
            AppExec.CurrentLicenseType = AppExec.LicenseTypes.GetByTechName(AppExec.LIC_TYPE_NONCOMMERCIAL);
            AppExec.CurrentLicenseEdition = AppExec.LicenseEditions.GetByTechName(AppExec.LIC_EDITION_FREE);
            AppExec.CurrentLicenseMode = AppExec.LicenseModes.GetByTechName(AppExec.LIC_MODE_PERMANENT);

            //// This prevents to show the expiration and "do you want to buy" message each time.
            //var Result = Display.DialogPersistableMultiOption("Attention!",
            //                                                  (PreviousLicenseMode == AppExec.CurrentLicenseMode
            //                                                   ? "Current license is "
            //                                                   : "Product License in '" + AppExec.CurrentLicenseMode.Name + "' mode has expired :(\n" +
            //                                                     "License has been downgraded to ") +
            //                                                  "Free for Non-Commercial use.\n\n" +
            //                                                  "Do you want to Buy a more capable License?", null,
            //                                                  "Application", "AskForBuyLicenseAfterTrial", null,
            //                                                  new SimplePresentationElement("Yes", "Yes", "Go to website and buy other edition.", Display.GetAppImage("accept.png")),
            //                                                  new SimplePresentationElement("No", "No", "Continue with this Free edition.", Display.GetAppImage("cancel.png")));

            //if (Result != null && Result.Item1 == "Yes")
            //    if (Result.Item2)   // Only proceed if the option was entered (not remembered)
            //    {
            //        AppExec.CallExternalProcess(ProductDirector.WEBSITE_URL + ProductDirector.WEBSITE_URLPAGE_BUY +
            //                                    "?LicType=" + AppExec.CurrentLicenseType.TechName +
            //                                    "&LicEdition=" + AppExec.CurrentLicenseEdition);
            //        RegisterLicense();
            //    }
            //    else
            //        Console.WriteLine("*** Downgraded to Free edition for Non-Commercial use ***");

            var Result = Display.DialogMultiOption("Attention!",
                                                   (PreviousLicenseMode == AppExec.CurrentLicenseMode
                                                   ? "Current license is "
                                                   : "Product License in '" + PreviousLicenseMode.Name + "' mode has expired :(\n" +
                                                       "License has been downgraded to ") +
                                                   "Free for Non-Commercial use.\n\n" +
                                                   "Current user is '" + AppExec.CurrentLicenseUserName + "'.\n\n" +
                                                   "Do you want to Buy a more capable License?", null, null, false, "Yes",
                                                   new SimplePresentationElement("Yes", "Yes", "Go to website and buy other edition.", Display.GetAppImage("accept.png")),
                                                   new SimplePresentationElement("No", "No", "Continue with this Free edition.", Display.GetAppImage("cancel.png")));

            if (Result == "Yes")
            {
                AppExec.CallExternalProcess(ProductDirector.WEBSITE_URL + ProductDirector.WEBSITE_URLPAGE_BUY +
                                            "?LicType=" + AppExec.CurrentLicenseType.TechName +
                                            "&LicEdition=" + AppExec.CurrentLicenseEdition);
                RegisterLicense();
            }
            else
                Console.WriteLine("*** Downgraded to Free edition for Non-Commercial use ***");

            //T Consider that file can be overwritten in later builds (and installs ?)
            //var AppExecFieInfo = new FileInfo(ApplicationExecutableFileName);
            //var StartDate = AppExecFieInfo.CreationTime;

            //Console.WriteLine("Install={0}, File={1}", InstallDate, StartDate);

            //T var AppFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            //var AppFileInfo = new FileInfo(AppFilePath);

            //Display.DialogMessage("App file", AppFilePath + "\n" + AppFileInfo.CreationTime.ToStringAlways());
            */
        }

        public static string ApplicationExecutableFileName { get; private set; }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Obtains and applies the action requested by the application command-line arguments.
        /// </summary>
        public static void ProcessCommandLineArguments()
        {
            //Skips the first argument which is the .exe file name
            var CommandLineArguments = Environment.GetCommandLineArgs();
            ApplicationExecutableFileName = CommandLineArguments.First();
            var Parameters = CommandLineArguments.Skip(1);

            foreach (var Argument in Parameters)
                try
                {
                    var FilePath = Argument.ToStringAlways().Trim();

                    if (Path.GetDirectoryName(FilePath).IsAbsent())
                        FilePath = Path.Combine(Environment.CurrentDirectory, FilePath);

                    var Location = new Uri(FilePath, UriKind.Absolute);

                    if (FilePath.EndsWith("." + Composition.FILE_EXTENSION_COMPOSITION))
                        CompositionsManager.OpenComposition(Location, false);
                    else
                        if (FilePath.EndsWith("." + Domain.FILE_EXTENSION_DOMAIN))
                            CompositionDirector.OpenDomainAndCreateCompositionOfIt(true, Location, false);
                }
                catch(Exception Problem)
                {
                    Display.DialogMessage("Warning!", "Cannot read the file with the specified name.\nProblem: "
                                                      + Problem.Message, EMessageType.Warning);
                    continue;
                }
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the product "About" dialog.
        /// </summary>
        public static void ShowAbout()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<About>(ref Dialog, "About...");
        }

        /// <summary>
        /// Shows dialog for register license.
        /// </summary>
        public static void RegisterLicense()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<AppRegistration>(ref Dialog, "Register License...");
        }

        /// <summary>
        /// Shows the product "Options" dialog.
        /// </summary>
        public static void EditOptions()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<AppOptions>(ref Dialog, "Options...");
        }

        /// <summary>
        /// Initializes user-interface components over the provided shell.
        /// </summary>
        public static void InitializeUserInterface()
        {
            PaletteProject = new WidgetPalette("PaletteProject", "Project", EShellVisualContentType.PaletteContent, "book_open.png");
            PaletteProject.PaletteTab.MinWidth = 400;
            // Ugly: PaletteProject.PaletteTab.FlowDirection = FlowDirection.RightToLeft;    // To let space for the quickbar on the left.

            PaletteEdition = new WidgetPalette("PaletteEdition", "Compose", EShellVisualContentType.PaletteContent, "wand.png");

            PaletteGeneral = new WidgetPalette("PaletteGeneral", "General", EShellVisualContentType.PaletteContent, "world.png");

            PopulateMenuPalette(PaletteProject, CompositionDirector, EShellCommandCategory.Document, EShellCommandCategory.Global);
            PopulateMenuPalette(PaletteProject, DomainDirector, EShellCommandCategory.Document, EShellCommandCategory.Global);

            PopulateMenuPalette(PaletteEdition, CompositionDirector, EShellCommandCategory.Edition);

            PopulateMenuPalette(PaletteGeneral, CompositionDirector, EShellCommandCategory.Extra);

            // Let the "Recent" tab at end
            var Tabs = new List<object>();
            foreach (var Tab in PaletteProject.PaletteTab.Items)
                Tabs.Add(Tab);

            Tabs.SwapItemsAt(1, 2);

            PaletteProject.PaletteTab.Items.Clear();
            Tabs.ForEach(tab => PaletteProject.PaletteTab.Items.Add(tab));

            // ----------------------------------------------------------------------------------------------------------------------------------------
            EditorInterrelationsControl = new WidgetInterrelationsPanel();
            EditorInterrelations = new EntitledPanel("EditorProperties", "Interrelations", EShellVisualContentType.NavigationContent, EditorInterrelationsControl);

            //POSTPONED: NavigatorMapControl = new WidgetNavMap();
            //POSTPONED: NavigatorMap = new EntitledPanel("NavigatorMap", "Map", EShellVisualContentType.NavigationContent, NavigatorMapControl);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            ContentTreeControl = new WidgetNavTree(WorkspaceDirector);
            ContentTree = new EntitledPanel("ContentTree", "Content", EShellVisualContentType.NavigationContent, ContentTreeControl);

            ContentTreeControl.SourceChanged =
                ((sphere) =>
                {
                    if (sphere == null)
                    {
                        ContentTree.SortAction = null;
                        ContentTree.FindAction = null;
                    }
                    else
                    {
                        if (ContentTree.SortAction == null)
                            ContentTree.SortAction = ContentTreeSortAction;

                        if (ContentTree.FindAction == null)
                            ContentTree.FindAction = ContentTreeFindAction;
                    }
                });

            ContentTree.SortText = "Sort by Name or Creation sequence";
            ContentTree.FindTextLabel = "Find...";
            ContentTree.FindTextTip = "Find Ideas by text within their Name";

            // ----------------------------------------------------------------------------------------------------------------------------------------
            DocumentArea = new DocumentPanel("DocumentVisualizer", DocumentVisualizerControl);
            DocumentArea.Height = double.NaN;
            DocumentArea.Width = double.NaN;

            // ----------------------------------------------------------------------------------------------------------------------------------------
            var MessengerConsole = new WidgetMessenger();
            MessengerConsolePublisher = new ConsoleRollPublisher(MessengerConsole.MessagePublisher,
                                                                 AppExec.GetConfiguration<int>("Application", "ConsoleOutputMaxLines",
                                                                                               ConsoleRollPublisher.MAX_CONSOLE_OUTPUT_LINES));
            Console.SetOut(MessengerConsolePublisher);

            /* Messages and Results...
            var TabItemConsole = new TabItem();
            TabItemConsole.Header = "Console";
            TabItemConsole.Content = MessengerConsole;

            var MessengerResults = new WidgetMessenger();
            var TabItemResults = new TabItem();
            TabItemResults.Header = "Results";
            TabItemResults.Content = MessengerResults;

            var TabMessenger = new TabControl();
            TabMessenger.TabStripPlacement = Dock.Bottom;
            TabMessenger.Items.Add(TabItemConsole);
            TabMessenger.Items.Add(TabItemResults);
            TabMessenger.Height = double.NaN;
            TabMessenger.Width = double.NaN; */

            Messenger = new EntitledPanel("Messenger", "Messaging", EShellVisualContentType.MessagingContent, MessengerConsole /* TabMessenger */);
            Messenger.ShowTitle = false;

            // ----------------------------------------------------------------------------------------------------------------------------------------
            var ConceptScroller = new ScrollViewer();
            ConceptScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ConceptScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;  // For nice wrapping
            ConceptScroller.Content = ConceptPaletteControl;

            EditorConceptPalette = new EntitledPanel("EditorConceptPalette", "Concepts", EShellVisualContentType.EditingContent, ConceptScroller);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            var RelationshipScroller = new ScrollViewer();
            RelationshipScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            RelationshipScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;  // For nice wrapping
            RelationshipScroller.Content = RelationshipPaletteControl;

            EditorRelationshipPalette = new EntitledPanel("EditorRelationshipPalette", "Relationships", EShellVisualContentType.EditingContent, RelationshipScroller);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            var ComplementScroller = new ScrollViewer();
            ComplementScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ComplementScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;  // For nice wrapping
            ComplementScroller.Content = ComplementPaletteControl;

            EditorComplementPalette = new EntitledPanel("EditorComplementPalette", "Complements", EShellVisualContentType.EditingContent, ComplementScroller);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            var MarkerScroller = new ScrollViewer();
            MarkerScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            MarkerScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;  // For nice wrapping
            MarkerScroller.Content = MarkerPaletteControl;

            EditorMarkerPalette = new EntitledPanel("EditorMarkerPalette", "Markers", EShellVisualContentType.EditingContent, MarkerScroller);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            StatusBarControl = new WidgetStatusBar();
            StatusArea = new EntitledPanel("StatusMessage", "Status", EShellVisualContentType.StatusContent, StatusBarControl, false);
            StatusArea.Background = Brushes.Transparent;

            ShellHost.MainSelector = StatusBarControl.DocumentSelector;

            // ----------------------------------------------------------------------------------------------------------------------------------------
            ShellHost.PutVisualContent(PaletteProject);
            ShellHost.PutVisualContent(PaletteEdition);
            //POSTPONED: ShellHost.PutVisualContent(PaletteGeneral);

            // Don't mess with this to avoid deserliazation crash
            ShellHost.PutVisualContent(EShellVisualContentType.QuickPaletteContent,
                                       CompositionDirector.QuickExposedCommands.Select(
                                            (expo) =>
                                            {
                                                var NewButton = new PaletteButton(expo);
                                                NewButton.ButtonText = "";  // Only show the icon
                                                NewButton.ToolTip = expo.Name;
                                                return NewButton;
                                            }));

            ShellHost.PutVisualContent(ContentTree);

            ShellHost.PutVisualContent(EditorInterrelations);

            //POSTPONED: ShellHost.PutVisualContent(NavigatorMap);

            ShellHost.PutVisualContent(DocumentArea);

            ShellHost.PutVisualContent(Messenger);

            ShellHost.PutVisualContent(EditorConceptPalette, 0);

            ShellHost.PutVisualContent(EditorRelationshipPalette, 1);

            ShellHost.PutVisualContent(EditorMarkerPalette, 2);

            ShellHost.PutVisualContent(EditorComplementPalette, 3);

            ShellHost.PutVisualContent(StatusArea);

            ShellHost.MainSelector.ItemsSource = WorkspaceDirector.Documents;
            ShellHost.MainSelector.SelectionChanged += new SelectionChangedEventHandler(MainDocumentSelector_SelectionChanged);

            //====================================
            // VERY IMPORTANT!
            // Do not mess with the next Action<T> nor Lambdas.
            // They are entangled into serialized content (Still don't know from where)
            WorkspaceDirector.PreDocumentDeactivation =
                (workspace, document) =>
                {
                    GenerationManager.SetCurrentGenerationConsumer(null);

                    OnDocumentDiscard(workspace, document);
                    document.DocumentEditEngine.LastOpenedViews = DocumentVisualizerControl.GetAllViews(document).ToList();
                    document.DocumentEditEngine.Visualizer.DiscardAllViews();
                };

            WorkspaceDirector.PostDocumentActivation = 
                (workspace, document) =>
                {
                    if (ShellHost.MainSelector.SelectedItem != document)
                        ShellHost.RefreshSelection(document);

                    OnDocumentChange(workspace, document);

                    document.DocumentEditEngine.Show();
                    document.DocumentEditEngine.LastOpenedViews = null;

                    GenerationManager.SetCurrentGenerationConsumer((Composition)document);
                };

            WorkspaceDirector.PostDocumentStatusChange =
                (workspace, document) =>
                {
                    OnDocumentChange(workspace, document);
                };
            //====================================

            EntitleApplication();

            ShowPointingTo();
            ShowAssistance();

            ExtendPalettesActions();
        }

        public static void ShowPointingTo(object Target = null)
        {
            if (StatusBarControl == null)
                return;

            if (Target == null)
                Target = (WorkspaceDirector.ActiveDocument == null
                          ? null
                          : WorkspaceDirector.ActiveDocument.ActiveDocumentView);

            if (Target == PreviousPointedObject)
                return;
            PreviousPointedObject = Target;

            string Text = null;

            if (Target is VisualElement)
            {
                var TargetIdea = ((VisualElement)Target).OwnerRepresentation.RepresentedIdea;
                if (Target is VisualConnector)
                {
                    var TargetLink = ((VisualConnector)Target).RepresentedLink;
                    /* Text = (TargetLink.Descriptor != null
                            ? TargetLink.Descriptor.Name + ". " + TargetLink.Descriptor.Summary
                            : TargetIdea.Summary); */
                    Text = RoleBasedLink.__ClassDefinitor.Name +
                           " [" + TargetLink.RoleDefinitor.Name + "] " +
                           (TargetLink.Descriptor == null ? "" : " `" + TargetLink.Descriptor.NameCaption + "`" +
                           (TargetLink.Descriptor.Summary.IsAbsent()
                            ? "." :
                            " (" + TargetLink.Descriptor.Summary.RemoveNewLines().GetTruncatedWithEllipsis(35) + ").")) +
                           " of " + Relationship.__ClassDefinitor.Name +
                           " [" + TargetLink.OwnerRelationship.RelationshipDefinitor.Value.NameCaption + "] `" +
                           TargetLink.OwnerRelationship.NameCaption + "`.";
                }
                else
                    Text = (TargetIdea is Concept ? Concept.__ClassDefinitor.Name : Relationship.__ClassDefinitor.Name) +
                           " [" + TargetIdea.Definitor.Name + "] `" + TargetIdea.NameCaption + "`" +
                           (TargetIdea.Summary.IsAbsent() ? "." : " (" + TargetIdea.Summary.RemoveNewLines() + ").");
                           // (TargetIdea is Relationship ? ((Relationship)TargetIdea).DescriptiveCaption : "");
            }
            else
                if (Target is string)
                    Text = (string)Target;
                else
                    if (Target is View)
                        Text = View.__ClassDefinitor.Name + " " + ((View)Target).Name + ". " + ((View)Target).Summary;
                    else
                        Text = Target.ToStringAlways();

            Text = Text.AbsentDefault(STD_TEXT_POINTING);

            if (Text != StatusBarControl.PointingToText.Text)
                StatusBarControl.PointingToText.Text = Text;
        }
        private static object PreviousPointedObject = null;

        public static void ShowAssistance(string Message = null)
        {
            if (StatusBarControl == null)
                return;

            if (Message == null)
            {
                var CompoEngine = WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;

                if (CompoEngine != null && CompoEngine.RunningMouseCommand != null)
                    Message = CompoEngine.RunningMouseCommand.Name + " " + CompoEngine.RunningMouseCommand.Assistance.NullDefault("");
            }

            Message = Message.NullDefault(STD_TEXT_STATUS + (WorkspaceDirector.ActiveDocument != null && WorkspaceDirector.ActiveDocument.ActiveDocumentView != null
                                                             ? ". Drag the mouse while pressing [Right/Alt-Mouse-Button] to Pan. Press [Ctrl] + [Mouse-Wheel] to Zoom." : ""));

            if (Message != StatusBarControl.AssistanceText.Text)
                StatusBarControl.AssistanceText.Text = Message;
        }

        public static void EntitleApplication(string CustomTitle = null)
        {
            Application.Current.MainWindow.Title = (CustomTitle.IsAbsent() ? "" : CustomTitle + " - ")
                                                   + APPLICATION_NAME
                                                   + (AppExec.CurrentLicenseMode.TechName.IsOneOf(AppExec.LIC_MODE_PERMANENT, AppExec.LIC_MODE_SUBSCRIPTION)
                                                      ? "" : " (" + AppExec.CurrentLicenseMode.Name + ")")
                                                   + (AppExec.CurrentLicenseType.TechName == AppExec.LIC_TYPE_COMMERCIAL
                                                      ? "" : " (" + AppExec.CurrentLicenseType.Name + ")")
                                                   + (AppExec.CurrentLicenseUserName == AppExec.UNREGISTERED_COPY && !AppExec.CurrentLicenseMode.TechName.IsOneOf(AppExec.LIC_MODE_BETA, AppExec.LIC_MODE_TRIAL)
                                                      ? " " + AppExec.CurrentLicenseUserName : "");

            if (!AppExec.CurrentLicenseMode.TechName.IsOneOf(AppExec.LIC_MODE_PERMANENT, AppExec.LIC_MODE_SUBSCRIPTION))
                ProductDirector.StatusBarControl.SetModeText(AppExec.CurrentLicenseMode.Name.Intercalate());   // Intercalating because "Trial" and "Beta" are short words
            else
                if (AppExec.CurrentLicenseType.TechName != AppExec.LIC_TYPE_COMMERCIAL)
                    ProductDirector.StatusBarControl.SetModeText(AppExec.CurrentLicenseType.Name,
                                                                 (AppExec.CurrentLicenseType.TechName == AppExec.LIC_TYPE_EDUCATIONAL
                                                                  ? WidgetStatusBar.StampStyle.Green
                                                                  : WidgetStatusBar.StampStyle.Blue));
                else
                    ProductDirector.StatusBarControl.SetModeText(null);
        }

        public static void OnDocumentChange(WorkspaceManager workspace, ISphereModel document)
        {
            var SelectedDocument = workspace.ShellProvider.MainSelector.SelectedItem as ISphereModel;

            if (SelectedDocument.GlobalId == document.GlobalId)
                ShellHost.RefreshSelection(document);

            EntitleApplication(SelectedDocument.ToStringAlways());

            StatusBarControl.SldScaleLevel.ValueChanged += new RoutedPropertyChangedEventHandler<double>(StatusScale_ValueChanged);

            CompositionDirector.Visualizer.PostViewActivation =
                (docview =>
                 {
                     var DocEngine = docview.ParentDocument.DocumentEditEngine;
                     if (DocEngine != null)
                     {
                         var SourceCompo = ((CompositionEngine)DocEngine).TargetComposition;

                         // IMPORTANT: Without this validation, a crash may happen if
                         // the View activation is derived from a treeview item selection
                         if (SourceCompo != PreviousTreeViewSource)
                         {
                             // DO NOT MESS WITH THIS CALL (TO AVOID DESERIALIZATION CRASH)
                             ContentTreeControl.SetSource(SourceCompo);
                             PreviousTreeViewSource = SourceCompo;
                         }

                         UpdatePalettes(DocEngine);
                         UpdateMenuToolbar();
                         DocEngine.ReactToViewChanged(docview);
                     }

                     docview.PostChangeOfPageDisplalyScale =
                         (PageDisplayScale =>
                          {
                              StatusBarControl.SldScaleLevel.Value = PageDisplayScale;
                              //POSTPONED: NavigatorMapControl.Magnification = PageDisplayScale / 100;
                          });

                     //POSTPONED: ProductDirector.NavigatorMapControl.MapSource = docview.HostingScrollViewer;

                     docview.PostChangeOfPageDisplalyScale(docview.PageDisplayScale);
                 });
        }
        private static Composition PreviousTreeViewSource = null;

        public static void OnDocumentDiscard(WorkspaceManager workspace, ISphereModel document)
        {
            ContentTreeControl.SetSource(null);
            ApplicationProduct.ProductDirector.EditorInterrelationsControl.SetTarget(null);
        }

        /// <summary>
        /// Updates the items palettes of the specified document-engine.
        /// If null is passed, then clears the palettes.
        /// </summary>
        public static void UpdatePalettes(DocumentEngine DocEngine)
        {
            if (DocEngine == null)
            {
                CompositionDirector.ConceptPaletteGroup.ClearPalettes();
                CompositionDirector.RelationshipPaletteGroup.ClearPalettes();
                CompositionDirector.MarkerPaletteGroup.ClearPalettes();
                CompositionDirector.ComplementPaletteGroup.ClearPalettes();

                return;
            }

            var ConceptPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();
            foreach(var Palette in DocEngine.GetExposedConceptsPalettes())
                ConceptPalettes.Add(Palette, DocEngine.GetExposedItemsOfConceptPalette(Palette));
            CompositionDirector.ConceptPaletteGroup.UpdatePalettes(DocEngine, ConceptPalettes);

            var RelationshipPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();
            foreach (var Palette in DocEngine.GetExposedRelationshipsPalettes())
                RelationshipPalettes.Add(Palette, DocEngine.GetExposedItemsOfRelationshipPalette(Palette));
            CompositionDirector.RelationshipPaletteGroup.UpdatePalettes(DocEngine, RelationshipPalettes);

            var MarkerPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();
            var ExistsUserDefMarkers = false;
            foreach (var Palette in DocEngine.GetExposedMarkersPalettes())
            {
                var Markers = DocEngine.GetExposedItemsOfMarkerPalette(Palette);
                if (Palette.TechName == MarkerDefinition.USERDEF_CODE && Markers.Any())
                    ExistsUserDefMarkers = true;
                MarkerPalettes.Add(Palette, Markers);
            }
            CompositionDirector.MarkerPaletteGroup.UpdatePalettes(DocEngine, MarkerPalettes,
                                                                  (ExistsUserDefMarkers ? MarkerDefinition.USERDEF_CODE : null));

            var ComplementPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();
            foreach (var Palette in DocEngine.GetExposedComplementsPalettes())
                ComplementPalettes.Add(Palette, DocEngine.GetExposedItemsOfComplementPalette(Palette));
            CompositionDirector.ComplementPaletteGroup.UpdatePalettes(DocEngine, ComplementPalettes);

            var QuickItems = (((CompositionEngine)DocEngine).IsForEditDomain
                              ? DomainDirector.QuickExposedCommands
                              : CompositionDirector.QuickExposedCommands);

            ShellHost.PutVisualContent(EShellVisualContentType.QuickPaletteContent,
                                       QuickItems.Select((expo) =>
                                                         {
                                                             var NewButton = new PaletteButton(expo);
                                                             NewButton.ButtonText = "";  // Only show the icon
                                                             NewButton.ToolTip = expo.Name;
                                                             return NewButton;
                                                         }));
        }

        static void StatusScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CompositionDirector.Visualizer.ActiveView == null)
                return;

            CompositionDirector.Visualizer.ActiveView.PageDisplayScale = Convert.ToInt32(e.NewValue);
        }

        static void MainDocumentSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Selector)sender).Tag.IsTrue())    // If ignore the next selection-changed event...
            {
                ((Selector)sender).Tag = false;
                return;
            }

            ((Selector)sender).Tag = false; // Reset indication

            e.Handled = true;   // This avoids to call a parent's SelectionChanged event (such as in another Tab).

            if (e.AddedItems.Count < 1)
                return;

            WorkspaceDirector.ActiveDocument = e.AddedItems[0] as ISphereModel;
        }

        public static void PopulateMenuPalette(WidgetPalette Palette, WorkSphere Sphere, params EShellCommandCategory[] AssignableCommandCategories)
        {
            var GroupingHeaderBrush = Display.GetResource<Brush, EntitledPanel>("HeaderBrush");  // PanelTextBrush also looks good
            var GroupingBodyBrush = new SolidColorBrush(Color.FromRgb(242, 246, 248));  // Display.GetGradientBrush(Color.FromRgb(252, 252, 252), Color.FromRgb(247, 247, 247)); //Display.GetResource<Brush, EntitledPanel>("PanelBrush");

            // Adds the required tab items if these already does not exists in the palette's tab control.
            foreach (var Area in Sphere.CommandAreas)
            {
                Panel TargetAreaPanel = null;
                bool TabExists = false;
                bool MenuItemExposed = false;

                // Determines whether the current Area is already exposed as a TabItem.
                foreach(TabItem Item in Palette.PaletteTab.Items)
                    if (Item.Name == Area.TechName)
                    {
                        TargetAreaPanel = Item.Content as Panel;
                        TabExists = true;
                        break;
                    }

                // Creates a new Panel if no TabItem was found.
                if (!TabExists)
                {
                    var WrappingPanel = new DockPanel();
                    WrappingPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    TargetAreaPanel = WrappingPanel;
                }

                // Create Groups
                foreach (var Group in Sphere.CommandGroups[Area.TechName])
                {
                    DockPanel TargetGroupContainer = null;
                    WrapPanel TargetGroupingPanel = null;

                    // Determines whether the current Group is already exposed as a GroupBox.
                    foreach (var Element in TargetAreaPanel.Children)
                        if (Element is GroupBox &&
                            ((DockPanel)Element).Tag.ToString() == Group.TechName)
                        {
                            TargetGroupContainer = Element as DockPanel;
                            var Body = (Border)TargetGroupContainer.Children[1];  // Children[0] is the Header
                            TargetGroupingPanel = (WrapPanel)Body.Child;
                            break;
                        }

                    // Creates a new Panel if no TabItem was found.
                    if (TargetGroupContainer == null)
                    {
                        TargetGroupContainer = new DockPanel();
                        TargetGroupContainer.Tag = Group.TechName;
                        //- TargetGroupContainer.Orientation = Orientation.Horizontal;
                        TargetGroupContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
                        TargetGroupContainer.VerticalAlignment = VerticalAlignment.Stretch;
                        TargetGroupContainer.Margin = new Thickness(1, 0, 1, 0);

                        var GroupingHeader = new Border();
                        GroupingHeader.CornerRadius = new CornerRadius(3.0, 0, 0, 3.0);
                        GroupingHeader.Background = GroupingHeaderBrush;
                        GroupingHeader.Padding = new Thickness(0,4,1,4);
                        GroupingHeader.ToolTip = Group.Name;

                        var GroupingTitle = new TextBlock();
                        GroupingTitle.Text = Group.Name;
                        GroupingTitle.FontFamily = new FontFamily("Arial"); // new FontFamily("Verdana");
                        GroupingTitle.FontSize = 11;
                        // GroupingTitle.FontWeight = FontWeights.Thin;
                        GroupingTitle.Foreground = Brushes.White;
                        GroupingTitle.TextAlignment = TextAlignment.Right;
                        GroupingTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        GroupingTitle.LayoutTransform = new RotateTransform(-90.0);
                        GroupingHeader.Child = GroupingTitle;
                        TargetGroupContainer.Children.Add(GroupingHeader);

                        var GroupingBody = new Border();
                        GroupingBody.CornerRadius = new CornerRadius(0, 3.0, 3.0, 0);
                        GroupingBody.Background = GroupingBodyBrush;
                        GroupingBody.BorderThickness = new Thickness(0);
                        // GroupingBody.BorderBrush = Brushes.LightGray;
                        // GroupingBody.BorderThickness = new Thickness(0,1,1,1);

                        /* Rarely needed
                        GroupingHeader.MouseLeftButtonDown +=
                            ((sender, mevargs) => GroupingBody.SetVisible(!GroupingBody.IsVisible.IsTrue())); */

                        TargetGroupContainer.Children.Add(GroupingBody);

                        TargetGroupingPanel = new WrapPanel();
                        TargetGroupingPanel.Orientation = Orientation.Vertical;
                        GroupingBody.Child = TargetGroupingPanel;
                    }

                    TargetAreaPanel.Children.Add(TargetGroupContainer);

                    // Populates the GroupBox with the associated Command Expositors.
                    foreach (KeyValuePair<string, WorkCommandExpositor> ExpositorReg in Sphere.CommandExpositors)
                    {
                        var Expositor = ExpositorReg;

                        if (Expositor.Value.AreaKey == Area.TechName && Expositor.Value.GroupKey == Group.TechName
                            && AssignableCommandCategories.Contains(Expositor.Value.Category))
                        {
                            Control NewControl = null;

                            if (Expositor.Value.SwitchInitializer != null)
                            {
                                var SwitchChecker = new CheckBox();
                                SwitchChecker.Content = Expositor.Value.Name;
                                SwitchChecker.ToolTip = Expositor.Value.Summary;
                                SwitchChecker.Margin = new Thickness(2,4,2,4);
                                SwitchChecker.FontSize = 10.0;
                                /* WPF still has minor problems with fonts over shadow
                                var Shadow = new DropShadowEffect();
                                Shadow.Color = Colors.Gray;
                                Shadow.Opacity = 0.2;
                                SwitchChecker.Effect = Shadow; */
                                SwitchChecker.Command = ExpositorReg.Value.Command;

                                // Important to pass as parameter instead of negating model property
                                // (which, after undos/redos, can be incoherent with the check-box state shown)
                                Expositor.Value.CommandParameterExtractor = (() => SwitchChecker.IsChecked);

                                SwitchChecker.Loaded +=
                                    ((sender, evargs) =>
                                        {
                                            SwitchChecker.IsChecked = Expositor.Value.SwitchInitializer(WorkspaceDirector.ActiveDocumentEngine);
                                        });

                                NewControl = SwitchChecker;
                                MenuToolbarControlsUpdater.AddOrReplace(Area.TechName + "." + Group.TechName + "." + ExpositorReg.Key,
                                    () =>
                                    {
                                        SwitchChecker.IsChecked = Expositor.Value.SwitchInitializer(WorkspaceDirector.ActiveDocumentEngine);
                                    });
                            }
                            else
                                if (Expositor.Value.OptionsGetter == null)
                                    NewControl = new PaletteButton(Expositor.Value);
                                else
                                    if (Expositor.Value.MultiOptionSelectorStyle == ECommandExpositorStyle.ComboBox)
                                    {
                                        var SelectorCombo = new ComboBox();
                                        SelectorCombo.ToolTip = Expositor.Value.Name;
                                        SelectorCombo.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                                        //? SelectorCombo.MaxWidth = ApplicationShell.MainWindow.PREDEF_INITIAL_TOOLTAB_WIDTH - 48;
                                        SelectorCombo.BorderBrush = Brushes.Transparent;
                                        SelectorCombo.Background = Brushes.Transparent;
                                        SelectorCombo.Height = 22;
                                        SelectorCombo.Padding = new Thickness(1, 1, 1, 1);
                                        ScrollViewer.SetVerticalScrollBarVisibility(SelectorCombo, ScrollBarVisibility.Visible);
                                        ScrollViewer.SetHorizontalScrollBarVisibility(SelectorCombo, ScrollBarVisibility.Disabled);
                                        SelectorCombo.ToolTip = Expositor.Value.Summary;

                                        var FirstOption = Expositor.Value.OptionsGetter().FirstOrDefault();
                                        if (FirstOption == null || typeof(FormalPresentationElement).IsAssignableFrom(FirstOption.GetType()))
                                            SelectorCombo.ItemTemplate = Display.GetResource<DataTemplate>("TplFormalPresentationElement");
                                        else
                                            if (typeof(SimplePresentationElement).IsAssignableFrom(FirstOption.GetType()))
                                                SelectorCombo.ItemTemplate = Display.GetResource<DataTemplate>("TplSimplePresentationElement");

                                        SelectorCombo.Loaded +=
                                            ((sender, args) =>
                                            {
                                                var Items = Expositor.Value.OptionsGetter();    // Get options each time, they could change
                                                SelectorCombo.ItemsSource = Items;
                                                if (Items.Any())
                                                    SelectorCombo.SelectedItem = Items.First();
                                                SelectorCombo.Tag = true;   // Indicates ready for selection
                                            });

                                        SelectorCombo.SelectionChanged +=
                                            ((sender, selargs) =>
                                            {
                                                if (Expositor.Value.Command.CanExecute(null) && SelectorCombo.Tag != null)
                                                    Expositor.Value.Command.Execute(selargs.AddedItems != null && selargs.AddedItems.Count > 0
                                                                                    ? selargs.AddedItems[0] : null);
                                            });

                                        NewControl = SelectorCombo;
                                    }
                                    else
                                        if (Expositor.Value.MultiOptionSelectorStyle == ECommandExpositorStyle.ListView)
                                        {
                                            var SelectorList = new WidgetVisualSampleSelector();
                                            SelectorList.MaxWidth = ((Domain.BaseStylesForegroundColors.Count / 2.0) * WidgetsHelper.COLOR_SAMPLE_SIZE.Width) + 36;
                                            SelectorList.Margin = new Thickness(2);
                                            SelectorList.LbxSelector.ItemsSource = Expositor.Value.OptionsGetter();
                                            SelectorList.ToolTip = Expositor.Value.Summary;

                                            SelectorList.LbxSelector.SelectionChanged +=
                                                ((sender, selargs) =>
                                                {
                                                    if (SelectorList.LbxSelector.SelectedItem == null)
                                                        return;

                                                    if ((Mouse.LeftButton == MouseButtonState.Pressed || Keyboard.IsKeyDown(Key.Enter))
                                                        && Expositor.Value.Command.CanExecute(null))
                                                    {
                                                        var Param = (selargs.AddedItems != null && selargs.AddedItems.Count > 0
                                                                     ? selargs.AddedItems[0] as FrameworkElement : null);
                                                        Expositor.Value.Command.Execute(Param == null ? null : Param.Tag);

                                                        if (Expositor.Value.GoesToRootAfterExecute)
                                                            Palette.PaletteTab.PostCall(ptab => ptab.SelectedIndex = 0);
                                                    }

                                                    // Must unselect to allow re-selection for another target.
                                                    // At background-priority to allow some visual delay.
                                                    SelectorList.PostCall(sel => sel.LbxSelector.SelectedIndex = -1, true);
                                                }); 
                                            
                                            NewControl = SelectorList;
                                        }
                                        else
                                        {
                                            var SelectorList = new ListBox();
                                            SelectorList.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                                            SelectorList.Width = ApplicationShell.MainWindow.PREDEF_INITIAL_TOOLTAB_WIDTH - 10;
                                            SelectorList.BorderBrush = Brushes.Transparent;
                                            SelectorList.FontSize = 11;
                                            SelectorList.Margin = new Thickness(0, -2, 0, 0);
                                            SelectorList.Cursor = Cursors.Hand;
                                            SelectorList.ToolTip = Expositor.Value.Summary;
                                            ScrollViewer.SetVerticalScrollBarVisibility(SelectorList, ScrollBarVisibility.Visible);
                                            ScrollViewer.SetHorizontalScrollBarVisibility(SelectorList, ScrollBarVisibility.Disabled);

                                            SelectorList.Loaded +=
                                                ((sender, args) =>
                                                {
                                                    SelectorList.ItemsSource = Expositor.Value.OptionsGetter();
                                                });

                                            SelectorList.SelectionChanged +=
                                                ((sender, selargs) =>
                                                    {
                                                        if (SelectorList.SelectedItem == null)
                                                            return;

                                                        if ((Mouse.LeftButton == MouseButtonState.Pressed || Keyboard.IsKeyDown(Key.Enter))
                                                            && Expositor.Value.Command.CanExecute(null))
                                                        {
                                                            Expositor.Value.Command.Execute(selargs.AddedItems != null && selargs.AddedItems.Count > 0
                                                                                            ? selargs.AddedItems[0] : null);

                                                            if (Expositor.Value.GoesToRootAfterExecute)
                                                                Palette.PaletteTab.PostCall(ptab => ptab.SelectedIndex = 0);

                                                            SelectorList.SelectedItem = null;
                                                        }
                                                    });

                                            SelectorList.KeyDown +=
                                                ((sender, keyargs) =>
                                                    {
                                                        if (Keyboard.IsKeyDown(Key.Enter)
                                                            && Expositor.Value.Command.CanExecute(null))
                                                            Expositor.Value.Command.Execute(SelectorList.SelectedItem);
                                                    });

                                            NewControl = SelectorList;
                                        }

                            if (NewControl != null)
                            {
                                TargetGroupingPanel.Children.Add(NewControl);
                                MenuItemExposed = true;
                            }
                        }
                    }
                }

                // For possible assigned commands invocators creates a new TabItem, if no one was found related to the current Area.
                if (!TabExists && MenuItemExposed)
                {
                    var NewTab = new TabItem();
                    NewTab.Name = Area.TechName;
                    NewTab.Header = Area.Name;
                    NewTab.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    NewTab.Content = TargetAreaPanel;
                    Palette.PaletteTab.Items.Add(NewTab);
                }
            }

        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows a message about the need to do an immediate application of changes on the editing target, such as the next-dialog or specific control.
        /// Hence, the user is notified about a non-cancellable operation and can stop or continue, plus indicating "don't show/ask again".
        /// </summary>
        /// <param name="DataElementName">Name of the data element to be changed (informative to the user).</param>
        /// <param name="ConfigScope">Scope key for grouping configuration information.</param>
        /// <param name="ConfigOptionValueCode">Option code that will be used to get a default option, and later to save the last selected one.</param>
        /// <param name="EditingTarget">Indicates what Control is/will-be the change applier.</param>
        /// <returns>Indication of continuation (true) or cancellation (false)</returns>
        public static bool ConfirmImmediateApply(string DataElementName, string ConfigScope, string ConfigOptionValueCode, string EditingTarget = "the Next window")
        {
            var Result = Display.DialogPersistableMultiOption("Warning", "Confirm change of " + DataElementName + ".\n" +
                                                              "Changes in " + EditingTarget + " will be applied directly, despite a Cancel on this one.", null,
                                                              ConfigScope, ConfigOptionValueCode, "Cancel",
                                                              new SimplePresentationElement("OK", "OK", "Proceed, and apply changes directly.", Display.GetAppImage("accept.png")),
                                                              new SimplePresentationElement("Cancel", "Cancel", "Do not edit.", Display.GetAppImage("cancel.png")));
            return (Result != null && Result.Item1 == "OK");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Provides top-level error management for any unhandled exceptions detected from the hosting WPF application.
        /// </summary>
        /// <param name="sender">Original sender of the exception.</param>
        /// <param name="evargs">Exception parameters</param>
        public static void ApplicationUnhandledExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs evargs)
        {
            string Caption = "Error";
            var MessageType = EMessageType.Error;

            if (evargs.Exception is WarningAnomaly)
            {
                Caption = "Warning";
                MessageType = EMessageType.Warning;
            }

            AppExec.LogException(evargs.Exception);

            ApplicationErrorReporter.Show(evargs.Exception, "Unexpected Error!",
                                          "The next " + (MessageType == EMessageType.Error ? "unrecoverable" : "") + " execution " + Caption + " has been detected...");

            /*T Display.DialogMessage(Caption + "!", "PROBLEM: " + evargs.Exception.Message +
                                  (evargs.Exception.InnerException == null ? "" : "\nANOMALY: " + evargs.Exception.InnerException.Message),
                                   MessageType); */

            evargs.Handled = true;
            if (!(evargs.Exception is WarningAnomaly))
                Stop(evargs.Exception);
        }

        /// <summary>
        /// Registers if stop has been requested.
        /// </summary>
        private static bool StopRequested = false;

        /// <summary>
        /// Stops the execution of the application product.
        /// </summary>
        /// <param name="Problem">Possible cause for the stop.</param>
        public static void Stop(Exception Problem=null)
        {
            if (!StopRequested)
            {
                // Signals that the application has already been requested to stop, so if a new
                // exception is unhandled then the application will not fall in an infinite loop.
                StopRequested = true;

                // Ask for document saving if any is pending?
                // Can be dangerous if document is corrupt
            }

            Application.Current.Shutdown(Problem == null ? 0 : AppExec.APP_EXITCODE_CRASH);

            // Never finish with this (threads can remain alive):
            // Environment.Exit(Problem == null ? 0 : AppExec.APP_EXITCODE_CRASH);
        }

        // -------------------------------------------------------------------------------------------------------------    }
        // -------------------------------------------------------------------------------------------------------------    }
        public const string UPDATES_BASE = "http://instrumind.blob.core.windows.net/thinkcomposer/";

        public const string SETUP_FILE_NAME = "setup_imtc.exe";
        // CANCELLED?: public const string SETUP_FILE_NAME_NEW = "install_imtc.exe";

        public static string SetupFileName
        {
            get
            {
                /*? if (AppExec.CurrentLicenseEdition.TechName == AppExec.LIC_EDITION_FREE)
                    return SETUP_FILE_NAME_NEW; */

                return SETUP_FILE_NAME;
            }
        }
        public static string SetupSourceRemote { get { return UPDATES_BASE + SetupFileName; } }
        public static string SetupSourceLocal { get { return Path.Combine(AppExec.ApplicationUserTemporalDirectory, SetupFileName); } }

        public const string VERSIONING_FILE_NAME = ProductDirector.APPLICATION_NAME + ".ver";
        // CANCELLED?: public const string VERSIONING_FILE_NAME_NEW = ProductDirector.APPLICATION_NAME + "_rel.ver"; // IMPORTANT: The filename must be longer than the previous (to be harder to hack)

        public static string VersioningFileName
        {
            get
            {
                /*? if (AppExec.CurrentLicenseEdition.TechName == AppExec.LIC_EDITION_FREE)
                    return VERSIONING_FILE_NAME_NEW; */

                return VERSIONING_FILE_NAME;
            }
        }
        public static string VersioningSourceRemote { get { return UPDATES_BASE + VersioningFileName; } }
        public static string VersioningSourceLocal { get { return Path.Combine(AppExec.ApplicationUserTemporalDirectory, VersioningFileName); } }

        public static bool LaunchNewVersionSetupOnExit = false;

        // -------------------------------------------------------------------------------------------------------------
        public static bool ProductUpdateIsNeeded(IList<string> VersionFileLines)
        {
            try
            {
                if (VersionFileLines.Count < 1 || !VersionFileLines[0].StartsWith("#"))
                    throw new Exception("Versioning file does not starts with '#version-number'.");

                var CurVerParts = General.GetVersionNumberParts(APPLICATION_VERSION);
                var CurVerValue = Int32.Parse(CurVerParts.Item1.ToString("00") +
                                              CurVerParts.Item2.ToString("00") +
                                              CurVerParts.Item3.ToString("0000"));

                var NewVerParts = General.GetVersionNumberParts(VersionFileLines[0].Substring(1));
                var NewVerValue = Int32.Parse(NewVerParts.Item1.ToString("00") +
                                              NewVerParts.Item2.ToString("00") +
                                              NewVerParts.Item3.ToString("0000"));

                if (CurVerValue < NewVerValue)
                    return true;

            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot determine if update is needed. Problem: " + Problem.Message);
                AppExec.LogException(Problem);
            }

            return false;
        }

        public static void ProductUpdateDaylyDetection()
        {
            // Pending: As in Paint.NET, force to be execute only once per day (not per execution).
            try
            {
                var LastDetection = AppExec.GetConfiguration<DateTime>("Application", "LastUpdateCheck");
                if (LastDetection.Date >= DateTime.Now.Date)
                    return;

                if (File.Exists(ProductDirector.VersioningSourceLocal))
                    File.Delete(ProductDirector.VersioningSourceLocal);

                General.DownloadFileAsync(new Uri(ProductDirector.VersioningSourceRemote, UriKind.Absolute),
                                          ProductDirector.VersioningSourceLocal, evargs => true,
                                          result =>
                                          {
                                              try
                                              {
                                                  if (result.WasSuccessful)
                                                  {
                                                      AppExec.SetConfiguration<DateTime>("Application", "LastUpdateCheck", DateTime.Now.Date, true);

                                                      var VersionFileText = General.FileToString(ProductDirector.VersioningSourceLocal);
                                                      var VersionFileLines = General.ToStrings(VersionFileText);

                                                      if (ProductDirector.ProductUpdateIsNeeded(VersionFileLines))
                                                      {
                                                          Console.WriteLine("There is a new version available: " + VersionFileLines[0]);

                                                          var Result = Display.DialogPersistableMultiOption("Attention!", "There is a new version available for update this application.\n" +
                                                                                                                          "Do you want to download and install it now?", null,
                                                                          "Application", "AskForUpdateOnStart", null,
                                                                          new SimplePresentationElement("OK", "OK", "Proceed, and update the application.", Display.GetAppImage("accept.png")),
                                                                          new SimplePresentationElement("Cancel", "Cancel", "Do not update and continue.", Display.GetAppImage("cancel.png")));

                                                          // IMPORTANT: Notice that automatic download + installer-start + app-quit is
                                                          //            done only when the dialog is effectively show (not when remembering answer).

                                                          if (Result != null && Result.Item1 == "OK")
                                                              if (Result.Item2)  // Only proceed if the option was entered (not remembered)
                                                                  AppVersionUpdate.ShowAppVersionUpdate(VersionFileText);
                                                              else
                                                                  Console.WriteLine("*** Go to the 'About' window to download the new version ***");
                                                      }
                                                      else
                                                          Console.WriteLine("You are using the latest version available. No update required.");
                                                  }
                                                  else
                                                      Console.WriteLine("Unable to detect new application version.");
                                              }
                                              catch (Exception Problem)
                                              {
                                                  Console.WriteLine("Cannot detect new application version. Problem: " + Problem.Message);
                                                  AppExec.LogException(Problem);
                                              }
                                          });
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Error: Cannot process detection of new version.\nProblem: " + Problem.Message);
                AppExec.LogException(Problem);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /*********************************************************************************
        // CRITICAL CODE: Do not rename nor move.
        // This part is entangled with predefined Domains, thru some
        // serialized event-handlers with Action<T>, Func<T> and lambdas.
        *********************************************************************************/

        private static Dictionary<string, Action> MenuToolbarControlsUpdater = new Dictionary<string, Action>();

        public static void UpdateMenuToolbar()
        {
            var Engine = WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null || Engine.CurrentView == null)
                return;

            Engine.CurrentView.Presenter
                .PostCall(vpres =>
                {
                    foreach (var ControlUpdater in MenuToolbarControlsUpdater)
                        ControlUpdater.Value();
                });
        }

        // -------------------------------------------------------------------------------------------------------------
        // MUST BE AT END. TO AVOID DESERIALIZATION CRASH.
        public static void ExtendPalettesActions()
        {
            EditorConceptPalette.MouseEnter +=
                ((sender, evargs) => ShowAssistance("Select or drag an item to create a new Concept over the diagram View. Double-click to edit Definition."));
            EditorConceptPalette.MouseLeave += ((sender, evargs) => ShowAssistance());

            EditorRelationshipPalette.MouseEnter +=
                ((sender, evargs) => ShowAssistance("Select or drag an item to create a new Relationship over the diagram View. Double-click to edit Definition."));
            EditorRelationshipPalette.MouseLeave +=
                ((sender, evargs) => ShowAssistance());

            EditorMarkerPalette.MouseEnter +=
                ((sender, evargs) => ShowAssistance("Select or drag an item to create a new Marker over an Idea on the diagram View. Double-click to edit Definition."));
            EditorMarkerPalette.MouseLeave +=
                ((sender, evargs) => ShowAssistance());

            EditorComplementPalette.MouseEnter +=
                ((sender, evargs) => ShowAssistance("Select or drag an item to create a new Complement over the diagram View."));
            EditorComplementPalette.MouseLeave +=
                ((sender, evargs) => ShowAssistance());
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates that the current license edition has the permissions of the supplied minimum-required-edition-code for the intended-operation.
        /// Return indication of valid (true) or invalid (false). An error message is shown when validation is not passed, but can be supressed.
        /// </summary>
        public static bool ValidateEditionPermission(string MinimumRequiredEditionCode, string IntendedOperation,
                                                     bool ShowMessage = true, DateTime EvaluateSince = default(DateTime))
        {
            return true;

            /* Since Open-source this is no longer required...
            if (EvaluateSince != default(DateTime) && DateTime.Now < EvaluateSince)
                return true;

            var CurrIndex = AppExec.LicenseEditions.GetMatchingIndex(AppExec.CurrentLicenseEdition.TechName, SimpleElement.__TechName.TechName);
            var EvalIndex = AppExec.LicenseEditions.GetMatchingIndex(MinimumRequiredEditionCode, SimpleElement.__TechName.TechName);

            if (CurrIndex < 0 || EvalIndex < 0)
            {
                //? System.Media.SystemSounds.Exclamation.Play(); 
                Console.WriteLine("Problem validating license edition.");
                return false;
            }

            var IsValid = (CurrIndex >= 0 && EvalIndex >= 0 && CurrIndex >= EvalIndex);

            if (!IsValid && ShowMessage)
                AnnounceLicenseEditionProblem(0, 0, IntendedOperation);

            return IsValid; */
        }

        /// <summary>
        /// Validates that the current license edition can have the new count of certain items not exceeding its maximum quota.
        /// Return indication of valid (true) or invalid (false). An error message is shown when validation is not passed, but can be supressed.
        /// </summary>
        public static bool ValidateEditionLimit(int NewItemsCount, int MaximumItemsQuota,
                                                string IntendedOperation, string ItemsName, bool ShowMessage = true)
        {
            return true;

            /* Since Open-source this is no longer required...
            var IsValid = (NewItemsCount <= MaximumItemsQuota);

            if (!IsValid && ShowMessage)
                AnnounceLicenseEditionProblem(MaximumItemsQuota, NewItemsCount, IntendedOperation, ItemsName);

            return IsValid; */
        }

        /// <summary>
        /// Shows appropriate error message for license edition validation error
        /// </summary>
        private static void AnnounceLicenseEditionProblem(int ItemsLimit, int IntendedItemsCount, string IntendedOperation, string ItemsName = "")
        {
            var UpgradeCode = "UPGRADE";
            var Result = Display.DialogMultiOption("Attention!",
                                      "This is the '" + AppExec.CurrentLicenseEdition.Name +
                                        "' edition of " + ProductDirector.APPLICATION_NAME + ", which cannot " + IntendedOperation + "\n" +
                                      (ItemsLimit == 0 ? ""
                                                       : " more than " + ItemsLimit + " " + ItemsName + " " +
                                                         "(New count = " + IntendedItemsCount + ")") + "\n\n",
                                      null, null, true, null,
                                      new SimplePresentationElement("Upgrade", UpgradeCode,
                                                                    "Go to the Instrumind website and Upgrade to a more capable version.",
                                                                    Display.GetAppImage("web_update.png")));
            if (Result == UpgradeCode)
                AppExec.CallExternalProcess(ProductDirector.WEBSITE_URL + ProductDirector.WEBSITE_URLPAGE_BUY +
                                            "?LicType=" + AppExec.CurrentLicenseType.TechName +
                                            "&LicEdition=" + AppExec.CurrentLicenseEdition);
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the Help content.
        /// </summary>
        public static void ShowHelp()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<AppHelp>(ref Dialog, "Help...");
        }

        // -------------------------------------------------------------------------------------------------------------
        public static void ContentTreeSortAction()
        {
            var Compo = ProductDirector.WorkspaceDirector.ActiveDocument as Composition;
            if (Compo == null)
                return;

            Compo.ShowNavigableItemsOrderByName = !Compo.ShowNavigableItemsOrderByName;
            ContentTreeControl.PostCall(ctc => ctc.Refresh());
        }

        // -------------------------------------------------------------------------------------------------------------
        public static void ContentTreeFindAction(string text)
        {
            var Compo = ProductDirector.WorkspaceDirector.ActiveDocument as Composition;
            if (Compo == null || Compo.ActiveView == null)
                return;

            // CANCELLED: Add context-menu options: Starts-With/Contains; First/All; By Name/Tech-Name; On Only-Concepts/Only-Relationships/All-Ideas

            Compo.Engine.StartCommandVariation("Select found Ideas");

            ContentTreeClearIdeasSimpleSelection(ContentTreeControl.NavTree);

            Compo.ActiveView.UnselectAllObjects();  // Force start with nothing selected

            if (!text.IsAbsent())
            {
                ContentTreeDoIdeasSimpleSelection(ContentTreeControl.NavTree, text, null /*Compo.ActiveView.VisualizedCompositeIdea*/);

                // IMPORTANT!: NEVER REFERENCE NavigableElements (without the final '_') because regenerates the extended-enumerable!
                //             That property was intended to be used in XAML and binded once.
                var Selection = Compo.ActiveView.OwnerCompositeContainer.NavigableElements_.Where(elem => MatchesFindCriteria(elem, text))
                                            .CastAs<Idea, IRecognizableElement>(idea => idea.MainSymbol.GetDisplayingView() == Compo.ActiveView)
                                                .Select(idea => idea.MainSymbol);

                if (Selection.Any())
                {
                    Compo.ActiveView.SelectMultipleObjects(Selection);

                    var VisContainer = ContentTreeControl.NavTree.ItemContainerGenerator.ContainerFromItem(
                                            Selection.First().OwnerRepresentation.RepresentedIdea) as FrameworkElement;

                    if (VisContainer != null)
                        VisContainer.BringIntoView();
                };
            }

            Compo.Engine.CompleteCommandVariation();
        }

        public static bool MatchesFindCriteria(IRecognizableElement Element, string Text)
        {
            var Result = (Element != null && Element.Name.IndexOf(Text, 0, StringComparison.OrdinalIgnoreCase) >= 0);
            return Result;
        }

        public static void ContentTreeDoIdeasSimpleSelection(ItemsControl Source, string Text, Idea DirectOwnerToOmit)
        {
            foreach (var TargetIdea in Source.Items.Cast<object>().CastAs<Idea,object>())
            {
                var TvItem = Source.ItemContainerGenerator.ContainerFromItem(TargetIdea) as TreeViewItem;
                if (TvItem != null)
                {
                    if (TargetIdea.OwnerContainer != DirectOwnerToOmit
                        && MatchesFindCriteria(TargetIdea, Text))
                        TvItem.Opacity = 0.4;  // notorious enough

                    if (TvItem.IsExpanded)
                        ContentTreeDoIdeasSimpleSelection(TvItem, Text, DirectOwnerToOmit);
                }
            }
        }

        public static void ContentTreeClearIdeasSimpleSelection(ItemsControl Source)
        {
            foreach (var Item in Source.Items)
            {
                var TvItem = Source.ItemContainerGenerator.ContainerFromItem(Item) as TreeViewItem;
                if (TvItem != null)
                {
                    if (TvItem.Opacity < 1.0)
                        TvItem.Opacity = 1.0;

                    if (TvItem.IsExpanded)
                        ContentTreeClearIdeasSimpleSelection(TvItem);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------------
    }
}