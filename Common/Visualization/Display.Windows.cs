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
// File   : Display.Windows.cs
// Object : Instrumind.Common.Visualization.Display (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization.Widgets;
using System.Windows.Data;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Provides common services for working with WPF. Windows part.
    /// </summary>
    public static partial class Display
    {
        public const double WPF_DPI = 96;

        // Keys of the Tabs shown
        public const string TABKEY_GENERAL = "GNRL";
        public const string TABKEY_DETAILS = "DETA";
        public const string TABKEY_MARKINGS = "MARK";
        public const string TABKEY_DESCRIPTION = "DESC";
        public const string TABKEY_TECHSPEC = "TCSP";
        public const string TABKEY_CLASSIFICATION = "CLSF";
        public const string TABKEY_VERSIONING = "VERS";

        public const double DIALOG_MAINBACKGROUND_OPACITY = 0.85;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the active window of the current application or its main window if none is active.
        /// </summary>
        /// <returns>The current Window</returns>
        public static Window GetCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
                if (window.IsActive)
                    return window;

            return Application.Current.MainWindow;
        }

        /// <summary>
        /// TO BE IMPLEMENTED: Brings the specified window to the top of the screen
        /// </summary>
        /// <param name="TargetWindow">The target window</param>
        public static void BringWindowToTop(Window TargetWindow)
        {
            /* double MaxZIndex = 0;

            foreach (Window window in Application.Current.Windows)
                MaxZIndex = Math.Max(window.Owner */
        }

        /// <summary>
        /// General reference to a dialog window.
        /// This only allow one dialog open at the same time.
        /// </summary>
        public static DialogOptionsWindow GeneralDialogWindow = null;

        /// <summary>
        /// String suffix for naming "show/ask again?" config. options.
        /// </summary>
        public const string CONFOPT_SHOWAGAIN_SFX = "ShowAgain";

        /// <summary>
        /// Finds, by its name, the child object of the Control Template or Data Template assigned to this supplied control/content-presenter Source.
        /// Optionally the matching can be required, so an exception is thrown when the control/content-presenter is incorrect or the child is not found.
        /// NOTE: Children element are found only if the template has already been applied (after control's load).
        /// </summary>
        /// <typeparam name="TRet">Return type of the finded child</typeparam>
        /// <param name="Source">Templated Control/Content-Presenter</param>
        /// <param name="ChildName">Name of the template's child to find</param>
        /// <param name="MatchRequired">If true, indicate to throw exception if supplied source is not a control/content-presenter or the child object is not found.</param>
        /// <returns>The matching child, or null if not found and no match required.</returns>
        public static TRet GetTemplateChild<TRet>(this DependencyObject Source, string ChildName, bool MatchRequired = false)
            where TRet : class
        {
            var Result = default(TRet);
            var SourceControl = Source as Control;
            var SourcePresenter = Source as ContentPresenter;

            if (SourcePresenter == null)
            {
                if (SourceControl == null)
                    if (MatchRequired)
                        throw new InternalAnomaly("Cannot get template child. Source object is not a Control.", Source);
                    else
                        return null;

                // Find on the Control Template...
                object FoundControl = null;

                if (SourceControl.Template != null)
                {
                    FoundControl = SourceControl.Template.FindName(ChildName, SourceControl);
                    Result = FoundControl as TRet;
                }
            }

            // If not found, then find on the Data Template...
            if (Result == null)
            {
                if (SourcePresenter == null)
                    SourcePresenter = GetVisualChild<ContentPresenter>(Source);

                if (SourcePresenter != null && SourcePresenter.ContentTemplate != null)
                {
                    var FoundControl = SourcePresenter.ContentTemplate.FindName(ChildName, SourcePresenter);
                    Result = FoundControl as TRet;
                }
            }

            if (MatchRequired && Result == null)
                throw new InternalAnomaly("Cannot find the requested template child.", new DataWagon("ChildName", ChildName).Add("Control", SourceControl));

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Swaps the positions of two visual children for this supplied Target panel.
        /// </summary>
        public static void SwapChildren(this Panel Target, int First, int Second)
        {
            if (First == Second || Target.Children.Count < First + 1 || Target.Children.Count < Second + 1)
                return;

            // Notice the order of the deletion
            var MinIndex = Math.Min(First, Second);
            var MinChild = Target.Children[MinIndex];
            var MaxIndex = Math.Max(First, Second);
            var MaxChild = Target.Children[MaxIndex];

            Target.Children.RemoveAt(MaxIndex);
            Target.Children.RemoveAt(MinIndex);

            Target.Children.Insert(MinIndex, MaxChild);
            Target.Children.Insert(MaxIndex, MinChild);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets, after the parent-window load, the Text-Box of the source Combo-Box.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static TextBox GetTextBox(this ComboBox Source)
        {
            var Result = (Source.Template.FindName("PART_EditableTextBox", Source) as TextBox);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a button based on the supplied source, which must be either a WorkCommandExpositor or a Tuple{IRecognizableelement, Action{object}}.
        /// Plus, the Pictogram image size, Orientation and Command-Attachment can be specified.
        /// </summary>
        public static PaletteButton GenerateButton(object ButtonSource, double PictogramSize, Orientation Direction, bool AttachCommandIfAvailable = true)
        {
            PaletteButton Result = null;

            var SourceExpositor = ButtonSource as WorkCommandExpositor;
            if (SourceExpositor != null)
            {
                if (AttachCommandIfAvailable)
                    Result = new PaletteButton(SourceExpositor, PictogramSize, Direction);
                else
                    Result = new PaletteButton(SourceExpositor.Name, SourceExpositor.Pictogram,
                                               PictogramSize, Direction, null, null, null, SourceExpositor.Summary);
            }

            var SourceDeclaration = ButtonSource as Tuple<IRecognizableElement, Action<object>>;
            if (SourceDeclaration != null)
                Result = new PaletteButton(SourceDeclaration.Item1.Name, SourceDeclaration.Item1.Pictogram,
                                           PictogramSize, Direction, null, null, null, SourceDeclaration.Item1.Summary);
            else
            {
                var SourceText = ButtonSource.ToString();
                if (SourceText != null)
                    Result = new PaletteButton(SourceText, Display.GetAppImage("page_white.png"),
                                               PictogramSize, Direction, null, null, null);
            }

            if (Result == null)
                throw new InternalAnomaly("Source type for generate Button is unknown.", ButtonSource.GetType());

            Result.Tag = ButtonSource;

            return Result;
        }

        /// <summary>
        /// Shows a dialog for select an option from a group or retrieves a previously selected one.
        /// Returns the Code of the selected option.
        /// The selection can be marked to "don't show/ask again" and saved as Configuration for later retrieval.
        /// </summary>
        /// <param name="Title">Window title</param>
        /// <param name="Message">Dialog message</param>
        /// <param name="AdditionalInfo">Additional information</param>
        /// <param name="ConfigScope">Scope key for grouping configuration information.</param>
        /// <param name="ConfigOptionValueCode">Option code that will be used to get a default option, and later to save the last selected one</param>
        /// <param name="CancelOptionValue">Option value that will be used for cancel, and hence not considered to be saved as default (foolish option disabling)</param>
        /// <param name="Options">Collection of recognizable-elements to be shown as options (each must have a different Code)</param>
        /// <returns>Code of the selected option plus indication of entered(true)/remembered(false), or null if dialog was cancelled</returns>
        public static Tuple<string, bool> DialogPersistableMultiOption(string Title, string Message, string AdditionalInfo,
                                                                       string ConfigScope, string ConfigOptionValueCode, string CancelOptionValue,
                                                                       params IRecognizableElement[] Options)
        {
            General.ContractRequiresNotAbsent(Title, Message, ConfigScope, ConfigOptionValueCode);

            string Result = "";

            var ConfigOptionValue = AppExec.GetConfiguration<string>(ConfigScope, ConfigOptionValueCode);
            if (ConfigOptionValue == default(string) && Options != null && Options.Length > 0)
                ConfigOptionValue = Options[0].Name;

            // Configuration Code of the Check (boolean) which indicates whether to show/ask-again or just return the last selected/persisted option
            string ConfigOptionCheckCode = ConfigOptionValueCode + AppExec.CONFIG_CODESEPARATOR + CONFOPT_SHOWAGAIN_SFX;
            var ShowAgain = AppExec.GetConfiguration<bool>(ConfigScope, ConfigOptionCheckCode, true);

            if (ShowAgain)
            {
                var ShowAgainCheckPanel = new StackPanel();
                ShowAgainCheckPanel.Orientation = Orientation.Horizontal;
                ShowAgainCheckPanel.ToolTip = "Check this box if don't want to be informed/asked again the next time";
                var ShowAgainCheckBox = new CheckBox();
                ShowAgainCheckBox.IsChecked = true;
                ShowAgainCheckBox.Content = "Show/ask again?";
                ShowAgainCheckPanel.Children.Add(ShowAgainCheckBox);

                Result = DialogMultiOption(Title, Message, AdditionalInfo, ShowAgainCheckPanel, (CancelOptionValue == null), ConfigOptionValue, Options);

                // Notice that selecting "Cancel" (Result==null) will not store it as the default option
                // (this would inhibit the option in a foolish way)
                if (Result == null || Result == CancelOptionValue)
                    return null;

                if (!ShowAgainCheckBox.IsChecked.IsTrue())
                {
                    AppExec.SetConfiguration<string>(ConfigScope, ConfigOptionValueCode, Result);
                    AppExec.SetConfiguration<bool>(ConfigScope, ConfigOptionCheckCode, false, true);
                }
            }
            else
                Result = ConfigOptionValue;

            return Tuple.Create(Result,ShowAgain);
        }

        /// <summary>
        /// Shows a dialog for select an option from a group, returning the TechName of the selected one or null if cancelled.
        /// </summary>
        /// <param name="Title">Window title</param>
        /// <param name="Message">Dialog message</param>
        /// <param name="AdditionalInfo">Additional information</param>
        /// <param name="AdditionalPanel">Additional panel for extended options (such as checkboxes and radiobuttons)</param>
        /// <param name="ShowCancel">Indicates whether to show the Cancel button</param>
        /// <param name="DefaultOption">TechName for the option to be marked as default</param>
        /// <param name="Options">Collection of recognizable-elements to be shown as options (each must have a different Code)</param>
        /// <returns>Code of the selected option or null if dialog was cancelled</returns>
        public static string DialogMultiOption(string Title, string Message, string AdditionalInfo, Panel AdditionalPanel,
                                               bool ShowCancel, string DefaultOption, params IRecognizableElement[] Options)
        {
            var TargetWindow = new DialogOptionsWindow(Title, Message, AdditionalInfo, AdditionalPanel,
                                                       (techname) => SelectedOptionText = techname, DefaultOption, Options);
            TargetWindow.BtnCancel.SetVisible(ShowCancel);
            //? TargetWindow.ResizeMode = ResizeMode.NoResize;

            SelectedOptionText = null;
            TargetWindow.Owner = GetCurrentWindow();

            //POSTPONED: Application.Current.MainWindow.Opacity = DIALOG_MAINBACKGROUND_OPACITY;
            TargetWindow.ShowDialog();  // IMPORTANT: Don't get the result of this call.
            //POSTPONED: Application.Current.MainWindow.Opacity = 1;

            return SelectedOptionText;
        }
        public static string SelectedOptionText;

        /// <summary>
        /// Shows a dialog which shows a list of informative items, returning whether it was accepted.
        /// </summary>
        /// <param name="Title">Window title</param>
        /// <param name="Message">Dialog message</param>
        /// <param name="AdditionalInfo">Additional information</param>
        /// <param name="AdditionalPanel">Additional panel for extended options (such as checkboxes and radiobuttons)</param>
        /// <param name="DefaultOption">Option to be marked as default (True for Accept, False for Cancel)</param>
        /// <param name="Items">Collection of identifiable-elements to be shown as list items</param>
        /// <returns>Indication of accept (true) or cancell (false).</returns>
        public static bool DialogMultiItem(string Title, string Message, string AdditionalInfo, Panel AdditionalPanel, bool DefaultOption, params IIdentifiableElement[] Items)
        {
            var TargetWindow = new DialogItemsWindow(Title, Message, AdditionalInfo, AdditionalPanel, DefaultOption, Items);
            //? TargetWindow.ResizeMode = ResizeMode.NoResize;

            TargetWindow.Owner = GetCurrentWindow();

            //POSTPONED: Application.Current.MainWindow.Opacity = DIALOG_MAINBACKGROUND_OPACITY;
            TargetWindow.ShowDialog();  // IMPORTANT: Don't get the result of this call.
            //POSTPONED: Application.Current.MainWindow.Opacity = 1;

            return TargetWindow.DialogResult;
        }

        /// <summary>
        /// Shows a message dialog with optional attached data.
        /// </summary>
        /// <param name="Title">Title of the dialog window.</param>
        /// <param name="Message">Message to be shown.</param>
        /// <param name="MessageType">Type for determine icon (optional).</param>
        /// <param name="ButtonSet">Button-set to be shown (optional).</param>
        /// <param name="DefaultResult">Default dialog result (optional).</param>
        /// <param name="AttachedData">Dictionary with attached data (optional). </param>
        /// <returns>Result of the dialog.</returns>
        public static MessageBoxResult DialogMessage(string Title, string Message, EMessageType MessageType = EMessageType.Information,
                                                     MessageBoxButton ButtonSet = MessageBoxButton.OK, MessageBoxResult DefaultResult = MessageBoxResult.OK,
                                                     IDictionary<string, object> AttachedData = null)
        {
            var DisplayIcon = MessageType.SelectCorresponding(new Dictionary<EMessageType, MessageBoxImage>
                                                              { { EMessageType.Question, MessageBoxImage.Question },
                                                                { EMessageType.Error, MessageBoxImage.Error },
                                                                { EMessageType.Warning, MessageBoxImage.Warning } },
                                                              MessageBoxImage.Information);

            if (AttachedData != null)
            {
                var Text = new StringBuilder(Message + Environment.NewLine);

                foreach (var Data in AttachedData)
                    Text.AppendLine("-" + Data.Key + (Data.Value == null ? "" : ": " + Data.Value.ToString()));

                Message = Text.ToString();
            }

            var Owner = GetCurrentWindow();
            return MessageBox.Show(Owner, Message, Title, ButtonSet, DisplayIcon, DefaultResult);
        }

        /// <summary>
        /// Shows a dialog for assign a File Uri to be saved.
        /// Optionally, if the location exists, then ask for overwrite.
        /// </summary>
        /// <param name="Title">The title of the dialog.</param>
        /// <param name="Extension">Default file extension.</param>
        /// <param name="Filter">File type filter. Example: "All files (*.*)|*.*|Text files (*.txt;*.doc;*.dat)|*.txt;*.doc;*.dat)".</param>
        /// <param name="PredefinedLocation">Predefined FileName to be saved.</param>
        /// <param name="UseRememberedDir">Indicate to remember the last directory used for files with the same Extension.</param>
        /// <param name="Owner">Window owning the dialog (optional).</param>
        /// <param name="AskForOverwrite">Indicate to ask the user if wants to overwrite a preexising location (optional, default=true).</param>
        /// <returns>The Uri assigned or null if cancelled.</returns>
        public static Uri DialogGetSaveFile(string Title, string Extension, string Filter = null, string PredefinedLocation = null,
                                            bool UseRememberedDir = true, Window Owner = null, bool AskForOverwrite = true)
        {
            General.ContractRequiresNotAbsent(Title);

            Filter = Filter.NullDefault("All files (*.*)|*.*");

            if (Extension == null)
                Extension = "";
            else
                if (!Extension.IsAbsent() && !Extension.StartsWith("."))
                    Extension = "." + Extension;

            if (PredefinedLocation.IsAbsent())
                PredefinedLocation = (UseRememberedDir ? AppExec.GetConfiguration("Storage", "WorkingFolder" + Extension, AppExec.UserDataDirectory) : AppExec.UserDataDirectory);
            else
                if (UseRememberedDir)
                    PredefinedLocation = Path.Combine(AppExec.GetConfiguration("Storage", "WorkingFolder" + Extension,
                                                                               Path.GetDirectoryName(PredefinedLocation).AbsentDefault(AppExec.UserDataDirectory)),
                                                      Path.GetFileName(PredefinedLocation));

            var Dialog = new Microsoft.Win32.SaveFileDialog();
            Dialog.Title = Title;
            Dialog.InitialDirectory = System.IO.Path.GetDirectoryName(PredefinedLocation);
            Dialog.FileName = PredefinedLocation;
            Dialog.OverwritePrompt = AskForOverwrite;
            Dialog.AddExtension = true;
            Dialog.Filter = Filter;
            Dialog.DefaultExt = Extension;

            if (!Dialog.ShowDialog(Owner ?? Display.GetCurrentWindow()).IsTrue() || Dialog.FileName.Trim().IsAbsent())
                return null;

            var Result = new Uri(Dialog.FileName);

            var AssignedDirectory = Path.GetDirectoryName(Dialog.FileName) ?? "";
            AppExec.SetConfiguration("Storage", "WorkingFolder" + Extension, AssignedDirectory, true);

            return Result;
        }

        /// <summary>
        /// Shows a dialog for obtain a File Uri to be opened.
        /// </summary>
        /// <param name="Title">The title of the dialog.</param>
        /// <param name="Extension">Optional default file extension.</param>
        /// <param name="Filter">Optional file type filter. Example: "All files (*.*)|*.*|Text files (*.txt;*.doc;*.dat)|*.txt;*.doc;*.dat)".</param>
        /// <param name="Owner">Optional window owning the dialog.</param>
        /// <param name="InitialDirectory">Directory where the dialog starts open.</param>
        /// <returns>The Uri obtained or null if cancelled.</returns>
        public static Uri DialogGetOpenFile(string Title, string Extension = "", string Filter = "All files (*.*)|*.*", Window Owner = null, string InitialDirectory = null)
        {
            General.ContractRequiresNotAbsent(Title);

            if (Extension == null)
                Extension = "";
            else
                if (!Extension.IsAbsent() && !Extension.StartsWith("."))
                    Extension = "." + Extension;

            if (InitialDirectory.IsAbsent())
                InitialDirectory = AppExec.GetConfiguration("Storage", "WorkingFolder" + Extension, AppExec.UserDataDirectory);

            var Dialog = new Microsoft.Win32.OpenFileDialog();
            Dialog.Title = Title;
            Dialog.InitialDirectory = InitialDirectory;
            Dialog.AddExtension = true;
            Dialog.Filter = Filter;
            Dialog.DefaultExt = Extension;

            if (!Dialog.ShowDialog(Owner ?? Display.GetCurrentWindow()).IsTrue() || Dialog.FileName.Trim().IsAbsent())
                return null;

            var Result = new Uri(Dialog.FileName);

            var AssignedDirectory = Path.GetDirectoryName(Dialog.FileName) ?? "";
            AppExec.SetConfiguration("Storage", "WorkingFolder" + Extension, AssignedDirectory, true);

            return Result;
        }

        /// <summary>
        /// Shows a dialog for obtain a Folder Uri to be opened.
        /// </summary>
        /// <param name="Title">The title of the dialog.</param>
        /// <param name="SelectedPath">Initial path to point with the dialog window. Null for user-data directory.</param>
        /// <param name="Context">Name of context for remember location.</param>
        /// <param name="Owner">Window owning the dialog (optional).</param>
        /// <returns>The Uri obtained or null if cancelled.</returns>
        public static Uri DialogGetOpenFolder(string Title, string SelectedPath = null, string Context = null, Window Owner = null)
        {
            General.ContractRequiresNotAbsent(Title);

            Owner = Owner ?? GetCurrentWindow();

            Context = Context.AbsentDefault("WorkingFolder");

            if (SelectedPath.IsAbsent())
                SelectedPath = AppExec.GetConfiguration("Storage", Context, AppExec.UserDataDirectory);

            var Dialog = new System.Windows.Forms.FolderBrowserDialog();
            Dialog.Description = Title;
            Dialog.SelectedPath = SelectedPath;
            Dialog.ShowNewFolderButton = true;

            if (Dialog.ShowDialog(Owner.GetIWin32Window()) != System.Windows.Forms.DialogResult.OK || Dialog.SelectedPath.Trim().IsAbsent())
                return null;

            var Result = new Uri(Dialog.SelectedPath);

            var AssignedDirectory = Path.GetDirectoryName(Dialog.SelectedPath) ?? "";
            AppExec.SetConfiguration("Storage", Context, AssignedDirectory, true);

            return Result;
        }

        /// <summary>
        /// Shows a dialog for open an image and returns it if not cancelled.
        /// If no Initial-Directory is specified, then the application Clip-Art one is used.
        /// </summary>
        public static ImageSource DialogGetImageFromFile(string InitialDirectory = null)
        {
            string FilterExtensions = "*." + IMAGE_FILE_READ_EXTENSIONS.Replace(";", ";*.");
            string FilterLabel = "Image Files (" + FilterExtensions + ")|" + FilterExtensions;

            // IMPORTANT: Do not force theuse of the Clip-Art directory when available.
            //            Always use the last user folder, except for the first time.
            var Selection = Display.DialogGetOpenFile("Select Image...",
                                                      IMAGE_FILE_READ_EXTENSIONS.GetLeft(IMAGE_FILE_READ_EXTENSIONS.IndexOf(';')),
                                                                                         FilterLabel);
            if (Selection == null)
                return null;

            var Result = ImportImageFrom(Selection.OriginalString);
            return Result;
        }

        /// <summary>
        /// Exposes a Page-Setup dialog, returning the updated settings (for page and printer) or null if cancelled.
        /// </summary>
        public static Tuple<System.Drawing.Printing.PageSettings, System.Drawing.Printing.PrinterSettings>
                      DialogPrintSetup(Window Owner = null)
        {
            Owner = Owner.NullDefault(GetCurrentWindow());

            System.Windows.Forms.PageSetupDialog Dialog = new System.Windows.Forms.PageSetupDialog();

            Dialog.PageSettings = new System.Drawing.Printing.PageSettings();
            Dialog.PrinterSettings = new System.Drawing.Printing.PrinterSettings();

            //POSTPONED: Application.Current.MainWindow.Opacity = DIALOG_MAINBACKGROUND_OPACITY;
            System.Windows.Forms.DialogResult DialogResult = Dialog.ShowDialog(Owner.GetIWin32Window());
            //POSTPONED: Application.Current.MainWindow.Opacity = 1;

            if (DialogResult != System.Windows.Forms.DialogResult.OK)
                return null;

            return Tuple.Create(Dialog.PageSettings, Dialog.PrinterSettings);
        }

        /// <summary>
        /// Collection of opened windows collection by type and instance (identifiable)
        /// </summary>
        private static Dictionary<Type, Dictionary<object, Window>> OpenedWindows = new Dictionary<Type, Dictionary<object, Window>>();

        /// <summary>
        /// Shows and registers a window, instantiating it if necessary, according to its type and separated by an identifier.
        /// If the later is not argumented, then a unique instance is assumed.
        /// The opened window will be owned by the main application window.
        /// </summary>
        /// <typeparam name="TWindow">Type of the window to be shown</typeparam>
        /// <param name="Id">Id for the new instance. When null, the new instance is assumed unique for the type</param>
        /// <param name="Parameters">Optional parameters passed to the constructor of the created window</param>
        /// <returns>The opened window</returns>
        public static TWindow ShowWindow<TWindow>(object Id = null, params object[] Parameters)
               where TWindow : Window
        {
            Type WindowType = typeof(TWindow);
            Dictionary<object, Window> InstancesOfTWindow;
            bool IsUnique = (Id == null);

            if (IsUnique)
                Id = typeof(TWindow).GUID;

            if (OpenedWindows.ContainsKey(WindowType))
                InstancesOfTWindow = OpenedWindows[WindowType];
            else
            {
                InstancesOfTWindow = new Dictionary<object, Window>();
                OpenedWindows.Add(WindowType, InstancesOfTWindow);
            }

            TWindow RefWindow = default(TWindow);

            if (InstancesOfTWindow.ContainsKey(Id))
                RefWindow = (TWindow)(InstancesOfTWindow[Id]);

            if (!IsUnique)
            {
                List<object> ParametersArray = new List<object>();
                ParametersArray.Add(Id);
                ParametersArray.AddRange(Parameters);
                Parameters = ParametersArray.ToArray();
            }

            Display.OpenWindow<TWindow>(GetCurrentWindow(), ref RefWindow, true, double.NaN, double.NaN, null, Parameters);

            if (RefWindow != default(TWindow) && !InstancesOfTWindow.ContainsKey(Id))
                InstancesOfTWindow.Add(Id, RefWindow);

            return RefWindow;
        }

        /// <summary>
        /// Automates the opening of generic windows with generic content.
        /// </summary>
        /// <typeparam name="TContent">Type of the content to be instantiated and embedded inside the opened window</typeparam>
        /// <param name="TargetWindow">Window of type BasicWindow to be opened (instantiated if necessary and shown)</param>
        /// <param name="Title">Title for the window to be opened</param>
        /// <param name="ContentParameters">Optional parameters passed to the constructor of the content</param>
        /// <returns>The instantiated content</returns>
        public static TContent OpenContentWindow<TContent>(ref BasicWindow TargetWindow, string Title, params object[] ContentParameters)
                     where TContent : UIElement
        {
            var WindowContent = Activator.CreateInstance(typeof(TContent), ContentParameters) as TContent;

            OpenContentWindow<TContent>(ref TargetWindow, WindowContent, Title);

            return WindowContent;
        }

        /// <summary>
        /// Automates the opening of generic windows with the supplied content.
        /// </summary>
        /// <typeparam name="TContent">Type of the content to be instantiated and embedded inside the opened window</typeparam>
        /// <param name="TargetWindow">Window of type BasicWindow to be opened (instantiated if necessary and shown)</param>
        /// <param name="Title">Title for the window to be opened</param>
        /// <param name="Content">The Content to be shown inside the window</param>
        /// <param name="Owner">The window owning this new opened one (optional)</param>
        /// <param name="InitialWidth">Initial width of the window</param>
        /// <param name="InitialHeight">Initial height of the window</param>
        public static void OpenContentWindow<TContent>(ref BasicWindow TargetWindow, TContent Content, string Title, Window Owner = null,
                                                       double InitialWidth = double.NaN, double InitialHeight = double.NaN)
                     where TContent : UIElement
        {
            Display.OpenWindow<BasicWindow>(Owner, ref TargetWindow, true, InitialWidth, InitialHeight, Content);

            if (TargetWindow != null)
                TargetWindow.Title = Title;
        }

        /// <summary>
        /// Automates the opening of generic dialog windows with generic content.
        /// </summary>
        /// <typeparam name="TContent">Type of the content supplied or to be instantiated</typeparam>
        /// <param name="TargetWindow">Window of type BasicWindow to be opened (instantiated if necessary and shown)</param>
        /// <param name="Title">Title for the window to be opened</param>
        /// <param name="Content">Content to be embedded, or null for automatic instantiation</param>
        /// <param name="InitialWidth">Initial width of the window</param>
        /// <param name="InitialHeight">Initial height of the window</param>
        /// <param name="ContentParameters">Parameters to be supplied when automatically instantiating a not-supplied content</param>
        /// <returns>Nullable boolean value returned after the dialog invocation.</returns>
        public static bool? OpenContentDialogWindow<TContent>(ref DialogOptionsWindow TargetWindow, string Title, TContent Content = null,
                                                              double InitialWidth = double.NaN, double InitialHeight = double.NaN,
                                                              params object[] ContentParameters)
                      where TContent : UIElement
        {
            bool? Result = false;
            Content = Content ?? (TContent)Activator.CreateInstance(typeof(TContent), ContentParameters);

            Display.OpenWindow<DialogOptionsWindow>(null, ref TargetWindow, false, InitialWidth, InitialHeight, Content);
            var RefWindow = TargetWindow;   // Needed to be referenced in lambda closure

            if (TargetWindow != null)
            {
                TargetWindow.Loaded += ((sender, args) =>
                {
                    var BtnRestoreOrMaximize = Display.GetTemplateChild<Button>(RefWindow, "BtnRestoreOrMaximize", true);
                    BtnRestoreOrMaximize.Visibility = Visibility.Collapsed;
                    RefWindow.Title = Title;
                });

                //? TargetWindow.ResizeMode = ResizeMode.NoResize;
                //POSTPONED: Application.Current.MainWindow.Opacity = DIALOG_MAINBACKGROUND_OPACITY;
                Result = TargetWindow.ShowDialog();
                //POSTPONED: Application.Current.MainWindow.Opacity = 1;
            }

            return Result;
        }

        /* /// <summary>
        /// Automates the opening of generic windows
        /// </summary>
        /// <typeparam name="TWindow">Type of the window to be opened</typeparam>
        /// <param name="OwnerWindow">Window whichs will own the opened window. If null the current is assumed.</param>
        /// <param name="TargetWindow">Window to be opened (instantiated if necessary and shown)</param>
        /// <param name="WindowParameters">Optional parameters passed to the constructor of the opened window</param>
        public static void OpenWindow<TWindow>(Window OwnerWindow, ref TWindow TargetWindow, params object[] WindowParameters)
                     where TWindow : Window
        {
            Display.OpenWindow<TWindow>(OwnerWindow, ref TargetWindow, true, null, WindowParameters);
        } */

        /// <summary>
        /// Automates the opening of generic windows with supplied content 
        /// </summary>
        /// <typeparam name="TWindow">Type of the window to be opened</typeparam>
        /// <param name="OwnerWindow">Window whichs will own the opened window. If null the current is assumed.</param>
        /// <param name="TargetWindow">Window to be instantiated and shown</param>
        /// <param name="Show">Indicates whether to show the window (thus allowing invocation of ShowDialog when false).</param>
        /// <param name="InitialWidth">Initial width of the window</param>
        /// <param name="InitialHeight">Initial height of the window</param>
        /// <param name="WindowContent">Content to be instantiated and embedded inside the opened window</param>
        /// <param name="WindowParameters">Optional parameters passed to the constructor of the opened window</param>
        public static void OpenWindow<TWindow>(Window OwnerWindow, ref TWindow TargetWindow, bool Show,
                                               double InitialWidth = double.NaN, double InitialHeight = double.NaN,
                                               UIElement WindowContent = null, params object[] WindowParameters)
                     where TWindow : Window
        {
            //- var CurrentWindow = GetCurrentWindow();
            if (OwnerWindow == null)
                OwnerWindow = GetCurrentWindow();   //- CurrentWindow;

            var OriginalCursor = OwnerWindow.Cursor; //- CurrentWindow.Cursor;

            // If the instance still exists ...
            if (TargetWindow != null)
            {
                // If window has not been freed yet
                var window = PresentationSource.FromVisual(TargetWindow);

                if (window != null && !window.IsDisposed)
                {
                    // If is already instantiated, then maybe is minimized
                    // or hidden behind other windows
                    if (WindowContent != null)
                    {
                        TargetWindow.Content = WindowContent;
                        TargetWindow.InvalidateVisual();
                    }

                    if (Show)
                    {
                        TargetWindow.Show();
                        TargetWindow.WindowState = WindowState.Normal;
                        TargetWindow.Activate();
                        BringWindowToTop(TargetWindow);
                    }
                }
                else
                    goto EXHIBIT_WINDOW;
            }
            else
                goto EXHIBIT_WINDOW;

            //- CurrentWindow.Cursor = OriginalCursor;
            OwnerWindow.Cursor = OriginalCursor;
            return;

        EXHIBIT_WINDOW:
            try
            {
                TargetWindow = (TWindow)Activator.CreateInstance(typeof(TWindow), WindowParameters);

                TargetWindow.Owner = OwnerWindow ?? GetCurrentWindow();

                if (InitialWidth.IsNan() && InitialHeight.IsNan())
                    TargetWindow.SizeToContent = SizeToContent.WidthAndHeight;
                else
                    if (InitialWidth.IsNan())
                        TargetWindow.SizeToContent = SizeToContent.Width;
                    else
                        if (InitialHeight.IsNan())
                            TargetWindow.SizeToContent = SizeToContent.Height;
                        else
                            TargetWindow.SizeToContent = SizeToContent.Manual;

                TargetWindow.Width = InitialWidth;
                TargetWindow.Height = InitialHeight;

                if (WindowContent != null)
                {
                    TargetWindow.Content = WindowContent;
                    TargetWindow.InvalidateVisual();
                }

                if (Show)
                    TargetWindow.Show();
            }
            catch (Exception Error)
            {
                throw new UsageAnomaly("Cannot open window of type '" + typeof(TWindow).ToString() + "' with the supplied parameters.",
                                       Error, new DataWagon("Window Parameters", WindowParameters));
            }

            //- CurrentWindow.Cursor = OriginalCursor;
            OwnerWindow.Cursor = OriginalCursor;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Internal stack of shown cursors.
        /// </summary>
        private static Stack<Tuple<Window, Cursor>> CursorsStack = new Stack<Tuple<Window, Cursor>>();

        /// <summary>
        /// Pushes into the Cursors-stack the supplied new Cursor plus the specified Window (which can be null for using the current one).
        /// </summary>
        public static void PushCursor(Cursor NewCursor, Window TargetWindow = null)
        {
            if (TargetWindow == null)
                TargetWindow = GetCurrentWindow();

            CursorsStack.Push(Tuple.Create<Window, Cursor>(TargetWindow, TargetWindow.Cursor));

            TargetWindow.Cursor = NewCursor;
        }

        /// <summary>
        /// Pops from the Cursors-stack the previous cursor and window, then the cursor is reassigned to that window.
        /// </summary>
        public static void PopCursor()
        {
            var LastWindowCursor = CursorsStack.Pop();
            LastWindowCursor.Item1.Cursor = LastWindowCursor.Item2;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns, for this supplied Source Data-Grid, the current Data-Grid-Cell.
        /// May return null if no associated Cell is found.
        // This reports X and Y coordinates correctly.
        /// </summary>
        public static DataGridCell GetCurrentCell(this DataGrid SourceDataGrid)
        {
            if (SourceDataGrid.CurrentCell == null || !SourceDataGrid.CurrentCell.IsValid)
                return null;

            var CellContent = SourceDataGrid.CurrentCell.Column.GetCellContent(SourceDataGrid.CurrentCell.Item);
            if (CellContent == null)
                return null;

            var Result = CellContent.Parent as DataGridCell;
            return Result;
        }

        /* /// <summary>
        /// Returns, for this supplied Source Data-Grid, the current row Data-Grid-Cell.
        /// May return null if no associated Cell is found.
        // This reports X coordinate incorrectly (same for all).
        /// </summary>
        public static DataGridCell GetCurrentRowCell(this DataGrid SourceDataGrid)
        {
            if (SourceDataGrid.CurrentCell == null || !SourceDataGrid.CurrentCell.IsValid)
                return null;

            var RowContainer = SourceDataGrid.ItemContainerGenerator.ContainerFromItem(SourceDataGrid.CurrentCell.Item);
            if (RowContainer == null)
                return null;

            var RowPresenter = GetVisualChild<DataGridCellsPresenter>(RowContainer);
            if (RowPresenter == null)
                return null;

            var Container = RowPresenter.ItemContainerGenerator.ContainerFromItem(SourceDataGrid.CurrentCell.Item);
            var Cell = Container as DataGridCell;

            // Try to get the cell if null, because maybe the cell is virtualized
            if (Cell == null)
            {
                SourceDataGrid.ScrollIntoView(RowContainer, SourceDataGrid.CurrentCell.Column);
                Container = RowPresenter.ItemContainerGenerator.ContainerFromItem(SourceDataGrid.CurrentCell.Item);
                Cell = Container as DataGridCell;
            }

            return Cell;
        } */

        /// <summary>
        /// Returns, for this supplied Source Data-Grid, the Data-Grid-Cell related to the specified item Row and Column indexes.
        /// May return null if no associated Cell is found.
        /// </summary>
        public static DataGridCell GetCell(this DataGrid SourceDataGrid, int RowIndex, int ColumnIndex)
        {
            var RowContainer = SourceDataGrid.GetRow(RowIndex);

            if (RowContainer == null || SourceDataGrid.Columns.Count < 1 || !SourceDataGrid.HasItems)
                return null;

            DataGridCellsPresenter RowPresenter = GetVisualChild<DataGridCellsPresenter>(RowContainer);

            var Container = RowPresenter.ItemContainerGenerator.ContainerFromIndex(ColumnIndex);
            var Cell = Container as DataGridCell;

            // Try to get the cell if null, because maybe the cell is virtualized
            if (Cell == null)
            {
                SourceDataGrid.ScrollIntoView(RowContainer, SourceDataGrid.Columns[ColumnIndex]);
                Container = RowPresenter.ItemContainerGenerator.ContainerFromIndex(ColumnIndex);
                Cell = Container as DataGridCell;
            }

            return Cell;
        }

        /// <summary>
        /// Returns, for this supplied Source Data-Grid, the Data-Grid-Row related to the specified item Row Index.
        /// May return null if no associated Row is found.
        /// </summary>
        public static DataGridRow GetRow(this DataGrid SourceDataGrid, int RowIndex)
        {
            DataGridRow Row = null;
            var Container = SourceDataGrid.ItemContainerGenerator.ContainerFromIndex(RowIndex);
            Row = Container as DataGridRow;

            if (Row == null)
            {
                // Row may be virtualized, so bring into view and try again
                SourceDataGrid.ScrollIntoView(SourceDataGrid.Items[RowIndex]);
                Container = SourceDataGrid.ItemContainerGenerator.ContainerFromIndex(RowIndex);
                Row = Container as DataGridRow;
            }

            return Row;
        }

        /// <summary>
        /// For a provided SourceEditor control within a provided or to-be-discovered DataGridRow...
        /// Performs a direct Read from the Field (by Name) specified of the Value supplied.
        /// Returns the Value or null if failed.
        /// </summary>
        public static object PerformDirectRead(this IDynamicStoreDataGridEditor SourceEditor, string FieldName, DataGridRow EditingRow = null)
        {
            return PerformDirectReadWrapped(SourceEditor, FieldName, EditingRow).Get(tuple => tuple.Item1);
        }

        /// <summary>
        /// For a provided SourceEditor control within a provided or to-be-discovered DataGridRow...
        /// Performs a direct Read from the Field (by Name) specified of the Value supplied.
        /// Returns the tuple enclosed Value or null if failed.
        /// </summary>
        public static Tuple<object> PerformDirectReadWrapped(this IDynamicStoreDataGridEditor SourceEditor, string FieldName, DataGridRow EditingRow = null)
        {
            var SourceControl = SourceEditor as Control;
            if (SourceControl == null)
                throw new UsageAnomaly("The IDynamicStoreDataGridEditor provided must be also a Control.");

            EditingRow = EditingRow.NullDefault(GetNearestVisualDominantOfType<DataGridRow>(SourceControl));
            if (EditingRow == null)
                return null;   // NOTE: The row may not exits while closing window (where previously the DataGrid has been unloaded)
            // Cannot use: throw new UsageAnomaly("The provided editor-control must be exposed within a DataGridRow.");

            var Store = EditingRow.Item as IDynamicStore;
            if (Store == null)
                throw new UsageAnomaly("The Item being edited by the exposing DataGridRow must implement IDynamicStore.");

            var Result = Store.GetStoredValue(FieldName);
            return Tuple.Create(Result);
        }

        /// <summary>
        /// For a provided SourceEditor control within a provided or to-be-discovered DataGridRow...
        /// Performs a direct Write into the Field (by Name) specified of the Value supplied.
        /// Returns indication of success.
        /// </summary>
        public static bool PerformDirectWrite(this IDynamicStoreDataGridEditor SourceEditor, string FieldName, object Value, DataGridRow EditingRow = null)
        {
            var SourceControl = SourceEditor as Control;
            if (SourceControl == null)
                throw new UsageAnomaly("The IDynamicStoreDataGridEditor provided must be also a Control.");

            EditingRow = EditingRow.NullDefault(GetNearestVisualDominantOfType<DataGridRow>(SourceControl));
            if (EditingRow == null)
                return false;   // NOTE: The row may not exist while closing window (where previously the DataGrid has been unloaded)
                // Cannot use: throw new UsageAnomaly("The provided editor-control must be exposed within a DataGridRow.");

            var Store = EditingRow.Item as IDynamicStore;
            if (Store == null)
                throw new UsageAnomaly("The Item being edited by the exposing DataGridRow must implement IDynamicStore.");

            var Result = Store.SetStoredValue(FieldName, Value);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a new entity-edit-panel.
        /// </summary>
        public static EntityEditPanel CreateEditPanel(IModelEntity AssociatedEntity,
                                                      IEnumerable<TabItem> SpecializedTabs = null,
                                                      bool ShowAsDialog = true,
                                                      string OpenedTabKey = null,
                                                      string TabsOrderChanges = null, // Separating pairs of changes by ';' and tab-keys with ','. Ex.: "TabKeyIni1,TabKeyAfter1;TabKeyIni2,TabKeyAfter2;TabKeyIniN,TabKeyAfterN",
                                                      bool IncludeDescriptionTab = false,
                                                      bool IncludeClassificationTab = false,
                                                      bool IncludeVersioningTab = false,
                                                      bool IncludeTechSpecTab = false,
                                                      params UIElement[] ExtraGeneralContents)
        {
            UIElement GeneralContent = null;

            if (AssociatedEntity is IFormalizedElement)
            {
                var Subform = new FormalElementGeneralSubform();
                if (AssociatedEntity.ClassDefinition
                        .GetPropertyDef(FormalPresentationElement.__Pictogram.TechName, false) == null)
                    Subform.ExpoPictogram.SetVisible(false);

                GeneralContent = Subform;
            }
            else
                if (AssociatedEntity is IRecognizableElement)
                {
                    var MainContent = new FormalElementGeneralSubform();
                    MainContent.ExpoGlobalId.SetVisible(false);
                    GeneralContent = MainContent;
                }
                else
                    if (AssociatedEntity is IIdentifiableElement)
                    {
                        var MainContent = new FormalElementGeneralSubform();
                        MainContent.ExpoGlobalId.SetVisible(false);
                        MainContent.ExpoPictogram.SetVisible(false);
                        GeneralContent = MainContent;
                    }

            return CreateEditPanel(AssociatedEntity, GeneralContent, SpecializedTabs, ShowAsDialog, OpenedTabKey, TabsOrderChanges,
                                   IncludeDescriptionTab, IncludeClassificationTab: IncludeClassificationTab, IncludeVersioningTab: IncludeVersioningTab,
                                   IncludeTechSpecTab: IncludeTechSpecTab, ExtraGeneralContents: ExtraGeneralContents);
        }

        /// <summary>
        /// Creates and returns a new entity-edit-panel (explicitly specifying the general-content control).
        /// </summary>
        public static EntityEditPanel CreateEditPanel(IModelEntity AssociatedEntity, UIElement GeneralContent,
                                                      IEnumerable<TabItem> SpecializedTabs = null,
                                                      bool ShowAsDialog = false, string OpenedTabKey = null,
                                                      string TabsOrderChanges = null, // Separating pairs of changes by ';' and tab-keys with ','. Ex.: "TabKeyIni1,TabKeyAfter1;TabKeyIni2,TabKeyAfter2;TabKeyIniN,TabKeyAfterN"
                                                      bool IncludeDescriptionTab = false, bool IncludeTechSpecTab = false,
                                                      bool IncludeClassificationTab = false, bool IncludeVersioningTab = false,
                                                      params UIElement[] ExtraGeneralContents)
        {
            var EditPanel = new EntityEditPanel(AssociatedEntity);

            var TabbedPanel = new TabbedEditPanel();

            if (ExtraGeneralContents != null && ExtraGeneralContents.Length > 0
                && ExtraGeneralContents.Any(extra => extra != null))
            {
                var GeneralPanel = new DockPanel();

                // First the bottom content to make variable-space for the standard general content (i.e. the Summary property)
                // The list is reversed in order to show in the appropriate order
                foreach (var Extra in ExtraGeneralContents.Reverse())
                    if (Extra != null)
                    {
                        GeneralPanel.Children.Add(Extra);
                        DockPanel.SetDock(Extra, Dock.Bottom);
                    }

                if (GeneralContent != null)
                {
                    GeneralPanel.Children.Add(GeneralContent);
                    DockPanel.SetDock(GeneralContent, Dock.Top);
                }

                GeneralContent = GeneralPanel;
            }

            // Post-called to be exposed after Retrieve in Start-Edit...
            if (GeneralContent != null)
                TabbedPanel.AddTab(TABKEY_GENERAL, "General", "General properties", GeneralContent);

            if (SpecializedTabs != null && SpecializedTabs.Any())
                foreach (var Tab in SpecializedTabs)
                    TabbedPanel.AddTab(Tab);

            if (IncludeDescriptionTab && AssociatedEntity is IFormalizedElement)
                TabbedPanel.AddTab(TABKEY_DESCRIPTION, "Description", "Detailed description with rich text.",
                    RichTextEditor.CreateRichTextBoxPresenter(AssociatedEntity, FormalElement.__Description));

            if (IncludeTechSpecTab && AssociatedEntity is ITechSpecifier)
            {
                var TechSpecProp = (AssociatedEntity is FormalElement
                                    ? (MModelPropertyDefinitor)FormalElement.__TechSpec
                                    : SimpleElement.__TechSpec);

                var TechSpecEd = new SyntaxTextEditor();
                TechSpecEd.Initialize(AssociatedEntity.EditEngine, AssociatedEntity.ToString(), TechSpecProp.TechName, TechSpecProp.Name,
                                      () => TechSpecProp.Read(AssociatedEntity) as string,
                                      (text) => TechSpecProp.Write(AssociatedEntity, text));

                TechSpecEd.ToolTip = FormalElement.__TechSpec.Summary;
                TechSpecEd.SyntaxName = "Specification";
                TechSpecEd.SyntaxFileExtension = ".txt";

                var TechSpecTab = TabbedPanel.AddTab(TABKEY_TECHSPEC, "Tech-Spec", "Technical Specification text.", TechSpecEd);

                EditPanel.ShowAdvancedMembersChanged +=
                    ((show) =>
                        {
                            TechSpecTab.SetVisible(show);

                            if (!show)
                            {
                                var OwnerTabControl = TechSpecTab.GetNearestDominantOfType<TabControl>();

                                if (OwnerTabControl.SelectedItem == TechSpecTab)
                                    OwnerTabControl.SelectedIndex = 0;
                            }
                        });
            }

            /* POSTPONED: Integrate with Windows Explorer file properties (ala Office)?
            if (IncludeClassificationTab)
                TabPanel.AddTab(TABKEY_CLASSIFICATION, "Classification", "Classification information.", new TextBox()); */

            // NOTE: Do not create a Version container in this method, make it before the entity creation.
            var FormalizedInstance = AssociatedEntity as IFormalizedElement;
            if (IncludeVersioningTab && FormalizedInstance != null && FormalizedInstance.Version != null)
                TabbedPanel.AddTab(TABKEY_VERSIONING, "Versioning", "Versioning information.", new VersionCardSubform("Version"));

            if (TabbedPanel.Tabs.Any())
            {
                if (!TabsOrderChanges.IsAbsent())
                {
                    var Changes = TabsOrderChanges.Split(General.STR_SEPARATOR[0]);
                    var Prior = Changes.First();
                    foreach (var Change in Changes.Skip(1))
                    {
                        TabbedPanel.MoveAfterTab(Prior, Change);
                        Prior = Change;
                    }
                }

                if (OpenedTabKey.IsAbsent())
                    TabbedPanel.SelectTab(TabbedPanel.Tabs.First().Key);
                else
                    TabbedPanel.SelectTab(OpenedTabKey);
            }

            EditPanel.Content = TabbedPanel;

            return EditPanel;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static DialogOptionsWindow SelectionWindow = null;

        /// <summary>
        /// Exposes to the user a brush selector for the supplied Initial Brush.
        /// Returns indication of success (true, false=cancelled) plus the new brush (which can be null).
        /// </summary>
        public static Tuple<bool, Brush> DialogSelectBrush(Brush InitialBrush)
        {
            Brush SelectedBrush = null;
            var Selector = new BrushSelector(InitialBrush);

            Selector.SelectionAction =
                (selectedbrush =>
                {
                    if (selectedbrush == null)  // If selection cancelled, then exit
                        return;

                    SelectedBrush = selectedbrush.Item1;
                    SelectionWindow.Close();
                });

            var Result = Display.OpenContentDialogWindow<BrushSelector>(ref SelectionWindow, "Color brush...", Selector).IsTrue();

            return (new Tuple<bool, Brush>(Result, SelectedBrush));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region WIN32-INTEROP

        /// <summary>
        /// For this supplied WPF Visual, creates and returns a Win-32 compatible Window.
        /// </summary>
        public static System.Windows.Forms.IWin32Window GetIWin32Window(this Visual visual)
        {
            var SourceAsHwnd = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window InteropWindow = new Win32Window(SourceAsHwnd.Handle);
            return InteropWindow;
        }

        /// <summary>
        /// Internal Win-32 old style class from wrapping WPF visual objects and pass to Win-32 methods.
        /// </summary>
        private class Win32Window : System.Windows.Forms.IWin32Window
        {
            private readonly System.IntPtr Win32Handle;
            public Win32Window(System.IntPtr Handle)
            {
                Win32Handle = Handle;
            }

            #region IWin32Window Members
            System.IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return Win32Handle; }
            }
            #endregion
        }

        #endregion
    }
}
