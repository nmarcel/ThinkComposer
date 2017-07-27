// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer v1.0
// Copyright (C) 2009-2011 Néstor Marcel Sánchez Ahumada, Santiago, Chile.
// http://www.Instrumind.com
//
// All rights reserved.
// Todos los derechos reservados.
// -------------------------------------------------------------------------------------------
//
// This source code is private property of Néstor Marcel Sánchez Ahumada. It is prohibited the
// use or copy of any part of its content without the written permission of the owner.
// Protected by intellectual property laws and related international treaties.
//
// Este código fuente es propiedad privada de Néstor Marcel Sánchez Ahumada. Está prohibido el
// uso o copia de cualquier parte de su contenido sin el permiso escrito del dueño.
// Protegido por leyes de propiedad intelectual y tratados internacionales relacionados.
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
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
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
    public static partial class ProductDirector
    {
        #region CONSTANTS AND FIXED VALUES

        /// <summary>
        /// Name of the product application.
        /// </summary>
        public const string APPLICATION_NAME = "ThinkComposer";

        /// <summary>
        /// Complete version number of the application. Format: VV.RR.rmdd.
        /// Where: VV=version; RR=major-revision; r=minor-revision, m=month-last-digit, dd=day.
        /// (Forces at least one minor-revision increment per year)
        /// </summary>
        public const string APPLICATION_VERSION = "1.0.5112";   // Let 1.0.5x for Beta and 1.0.7x for RTM.

        /// <summary>
        /// Year(s) of the copyright registration.
        /// </summary>
        public const string APPLICATION_COPYRIGHT_YEARS = "2009-2011";

        /// <summary>
        /// Product web site.
        /// </summary>
        public const string WEBSITE_URL = "http://www.thinkcomposer.com";

        /// <summary>
        /// Name for the definitions (meta)documents of the application.
        /// </summary>
        public const string APPLICATION_DEFINITIONS_NAME = "Domains";

        /// <summary>
        /// Name give to user data files.
        /// </summary>
        public const string USER_DOCUMENTS_NAME = "Compositions";

        /// <summary>
        /// Standard text for Status.
        /// </summary>
        public const string STD_TEXT_STATUS = "Ready.";

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

        public static EntitledPanel EditorProperties = null;
        public static WidgetPropertyPanel EditorPropertiesControl = null;

        public static DocumentPanel DocumentArea = null;
        public static WidgetDocumentVisualizer DocumentVisualizerControl = null;

        public static EntitledPanel Messenger = null;
        public static ConsoleRollPublisher MessengerConsolePublisher = null;

        public static EntitledPanel EditorIdeaPalette = null;
        public static WidgetItemsPaletteGroup IdeaPaletteControl = null;

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
        /// Default size for new Relationships central symbols.
        /// </summary>
        public static Size DefaultRelationshipCentralSymbolSize { get; set; }

        #endregion

        //------------------------------------------------------------------------------------------ 
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static ProductDirector()
        {
            AppExec.LicenseType = AppExec.LicenseTypes.GetByTechName(AppExec.LIC_TYPE_NONCOMMERCIAL);
            AppExec.LicenseEdition = AppExec.LicenseEditions.GetByTechName(AppExec.LIC_EDITION_ULTIMATE);
            AppExec.LicenseMode = AppExec.LicenseModes.GetByTechName(AppExec.LIC_MODE_BETA);
            AppExec.LicenseExpiration = new DateTime(2012, 2, 1);

            AppExec.LicenseAgreement = Instrumind.ThinkComposer.Properties.Resources.License;

            AppExec.ApplicationContentTypeCode = AppExec.ApplicationContentTypeCode + "-" + APPLICATION_NAME.ToLower();

            // NOTE: Complete path to generate is...
            // "pack://application:,,,/Instrumind.ThinkComposer;component/ApplicationProduct/Images/sample.png"
            Display.AppImagesRoute = "/Instrumind.ThinkComposer;component/ApplicationProduct/Images/";

            DefaultNumberOfViewPages = 11;  // PD=15, Test=3

            DefaultPageDisplayScale = 100;

            DefaultMinBaseFigureSize = new Size(16, 16);

            DefaultMinDetailsPosterHeight = 32.0;

            DefaultConceptBodySymbolSize = new Size(120, 30); // Prior: new Size(150, 40);

            // NOTE: With a too little size the actioner buttons (edit-format, edit-properties, switch-content, switch-related) will not appear.
            DefaultRelationshipCentralSymbolSize = new Size(DefaultConceptBodySymbolSize.Width * 0.7,
                                                            DefaultConceptBodySymbolSize.Height * 0.9);

            // Register supported file types (this later/indirectly uses images, thus must be here and not initialized at the field declararion)
            SupportedFileTypes = new List<FileDataType>{ FileDataType.FileTypeComposition, FileDataType.FileTypeDomain };
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

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // VERY IMPORTANT!

            // Do not set AppTerminationConfirmation() as the CloseConfirmation action.
            //! ShellHost.CloseConfirmation = AppTerminationConfirmation;
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
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            AppExec.LogRegistrationPolicy = AppExec.LOGREG_ALL;

            // NOT NEEDED
            // AppExec.LogMessage("ThinkComposer, by Instrumind. Copyright (C) Néstor Sánchez A., for Instrumind Software SpA..", "SYSTEM");
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
            IdeaPaletteControl = new WidgetItemsPaletteGroup();
            MarkerPaletteControl = new WidgetItemsPaletteGroup();
            ComplementPaletteControl = new WidgetItemsPaletteGroup();

            CompositionDirector = new CompositionsManager("Composition Manager", "CompositionManager", "Manager for the Composition work-sphere.",
                                                          Display.GetAppImage("page_white_edit.png"),
                                                          WorkspaceDirector, DocumentVisualizerControl,
                                                          IdeaPaletteControl, MarkerPaletteControl, ComplementPaletteControl);

            DomainDirector = new DomainsManager("Domain Manager", "DomainManager", "Manager for the Domain work-sphere.",
                                                Display.GetAppImage("book_edit.png"),
                                                WorkspaceDirector, DocumentVisualizerControl, IdeaPaletteControl);

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

                    ValidateLicense();

                    Console.WriteLine(ProductDirector.APPLICATION_NAME
                                      + " " + ProductDirector.APPLICATION_VERSION
                                      + " Copyright (C) " + ProductDirector.APPLICATION_COPYRIGHT_YEARS
                                      + " " + Company.NAME_LEGAL);

                    Console.WriteLine("License: " + AppExec.LicenseType.Name
                                      + ". Edition: " + AppExec.LicenseEdition.Name
                                      + ". Mode: " + AppExec.LicenseMode.Name
                                      + ". Expiration: " + (AppExec.LicenseExpiration == General.EMPTY_DATE ? "NEVER" : AppExec.LicenseExpiration.ToShortDateString())
                                      + ".");

                    // Console.WriteLine("Started.");

                    ProductUpdateDaylyDetection();

                    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                });
        }

        /// <summary>
        /// Terminates the execution of the application product.
        /// </summary>
        public static void Terminate()
        {
            if (LaunchNewVersionSetupOnExit)
                AppExec.CallExternalProcess(LocalSetupSource);
        }

        public static bool AppTerminationConfirmation()
        {
            foreach (var Sphere in WorkSphereManagers)
                if (!Sphere.Discard())
                    return false;

            return true;
        }

        public static string ApplicationExecutableFileName { get; private set; }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes user-interface components over the provided shell.
        /// </summary>
        public static void InitializeUserInterface()
        {
            PaletteProject = new WidgetPalette("PaletteProject", "Project", EShellVisualContentType.PaletteContent, "book_open.png");
            //PaletteProject.PaletteTab.Width = 300;
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
            EditorPropertiesControl = new WidgetPropertyPanel();
            EditorProperties = new EntitledPanel("EditorProperties", "Properties", EShellVisualContentType.NavigationContent, EditorPropertiesControl);

            //POSTPONED: NavigatorMapControl = new WidgetNavMap();
            //POSTPONED: NavigatorMap = new EntitledPanel("NavigatorMap", "Map", EShellVisualContentType.NavigationContent, NavigatorMapControl);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            ContentTreeControl = new WidgetNavTree(WorkspaceDirector);
            ContentTree = new EntitledPanel("ContentTree", "Content", EShellVisualContentType.NavigationContent, ContentTreeControl);

            // ----------------------------------------------------------------------------------------------------------------------------------------
            DocumentArea = new DocumentPanel("DocumentVisualizer", DocumentVisualizerControl);
            DocumentArea.Height = double.NaN;
            DocumentArea.Width = double.NaN;

            // ----------------------------------------------------------------------------------------------------------------------------------------
            var MessengerConsole = new WidgetMessenger();
            MessengerConsolePublisher = new ConsoleRollPublisher(MessengerConsole.MessagePublisher,
                                                                 AppExec.GetConfiguration<int>("Application", "ConsoleOutputMaxLines",
                                                                                               ConsoleRollPublisher.DEF_MAXLINES));
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
            var IdeaScroller = new ScrollViewer();
            IdeaScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            IdeaScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;  // For nice wrapping
            IdeaScroller.Content = IdeaPaletteControl;

            EditorIdeaPalette = new EntitledPanel("EditorIdeaPalette", "Ideas", EShellVisualContentType.EditingContent, IdeaScroller);

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

            ShellHost.PutVisualContent(EditorProperties);

            //POSTPONED: ShellHost.PutVisualContent(NavigatorMap);

            ShellHost.PutVisualContent(DocumentArea);

            ShellHost.PutVisualContent(Messenger);

            ShellHost.PutVisualContent(EditorIdeaPalette, 0);

            ShellHost.PutVisualContent(EditorMarkerPalette, 1);

            ShellHost.PutVisualContent(EditorComplementPalette, 2);

            ShellHost.PutVisualContent(StatusArea);

            ShellHost.MainSelector.ItemsSource = WorkspaceDirector.Documents;
            ShellHost.MainSelector.SelectionChanged += new SelectionChangedEventHandler(MainDocumentSelector_SelectionChanged);

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // VERY IMPORTANT!
            // Do not mess with the next Action<T> nor Lambdas.
            // They are entangled into serialized content (Still don't know from where)
            WorkspaceDirector.PreDocumentDeactivation =
                (workspace, document) =>
                {
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
                };

            WorkspaceDirector.PostDocumentStatusChange =
                (workspace, document) =>
                {
                    OnDocumentChange(workspace, document);
                };
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            EntitleApplication();

            ShowAssistance();
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
                     if (docview == null)
                         return;

                     var DocEngine = docview.ParentDocument.DocumentEditEngine;
                     if (DocEngine != null)
                     {
                        ContentTreeControl.SetItemsSource(((Composition)DocEngine.TargetDocument).NavigableElements);
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

        public static void OnDocumentDiscard(WorkspaceManager workspace, ISphereModel document)
        {
            ContentTreeControl.SetItemsSource(null);
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

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
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

                // Ask for document saving if any is pending.
            }

            Application.Current.Shutdown(Problem == null ? 0 : AppExec.APP_EXITCODE_CRASH);

            // Never finish with this (threads can remain alive):
            // Environment.Exit(Problem == null ? 0 : AppExec.APP_EXITCODE_CRASH);
        }

        // -------------------------------------------------------------------------------------------------------------

        // -------------------------------------------------------------------------------------------------------------
    }

}