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
// File   : MainWindow.cs
// Object : Instrumind.ThinkComposer.ApplicationShell.MainWindow (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.20 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;

/// Provides the user-interface top level frame of the application.
namespace Instrumind.ThinkComposer.ApplicationShell
{
    /// <summary>
    /// The main window of the application.
    /// </summary>
    public partial class MainWindow : Window, IShellProvider
    {
        public const double PREDEF_INITIAL_TOOLTAB_WIDTH = 388;

        /// <summary>
        /// Register of hosted container panels
        /// </summary>
        private Dictionary<string, KeyValuePair<IShellVisualContent, Panel>> ContainerPanels = new Dictionary<string, KeyValuePair<IShellVisualContent, Panel>>();

        /// <summary>
        /// Container for the main documents/projects palette
        /// </summary>
        private StackPanel DocumentPaletteContainer = new StackPanel();

        /// <summary>
        /// Container for the main tools/commands palette
        /// </summary>
        private DockPanel ToolPaletteContainer = new DockPanel();

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.InitializeUI();

            ProductDirector.Initialize(this);

            // Redirects catched unhandled exceptions to the application product
            Application.Current.DispatcherUnhandledException +=
                new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(ProductDirector.ApplicationUnhandledExceptionHandler);
        }

        /// <summary>
        /// Window loaded event handler.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppExec.SetSourceOfDispatcherForUI(this.WinHeader);

            // Finish visual adjustments
            this.Width = this.Width.EnforceMaximum(SystemParameters.WorkArea.Width);
            this.Height = this.Height.EnforceMaximum(SystemParameters.WorkArea.Height);

            if (this.Left < 0)
                this.Left = 0;

            if (this.Top < 0)
                this.Top = 0;

            var InitialPaletteSupraContainerHeight = this.WinHeader.PaletteSupraContainer.ActualHeight;
            var Delta = double.NaN;

            this.WinHeader.PaletteSupraContainer.AtCanCollapseChanged =
                ((CanCollapse) =>
                {
                    Delta = Delta.NaNDefault(97.0); // Previous = 75
                    var Offset = (Delta * (CanCollapse ? -1 : 1));

                    this.WorkingAreaBorder.Margin = new Thickness(this.WorkingAreaBorder.Margin.Left,
                                                                  this.WorkingAreaBorder.Margin.Top + Offset,
                                                                  this.WorkingAreaBorder.Margin.Right,
                                                                  this.WorkingAreaBorder.Margin.Bottom);

                    var Engine = ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
                    if (Engine == null || Engine.CurrentView == null)
                        return;

                    // Adjust visualization to not "jump"
                    Engine.CurrentView.Pan(double.NaN, Offset, false);
                });

            // Starts the application product
            ProductDirector.Start();
        }

        /// <summary>
        /// Window closing event handler.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseConfirmation != null && !CloseConfirmation())
            {
                e.Cancel = true;
                ProductDirector.LaunchNewVersionSetupOnExit = false;

                return;
            }
        }

        /// <summary>
        /// Window closed event handler.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            ProductDirector.Terminate();
        }

        /// <summary>
        /// Window state changed event handler.
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            /* DISCONTINUED
            if (this.WindowState == WindowState.Normal)
                this.WinHeader.TxbRestoreOrMaximize.Text = MainWindowHeader.ICOTXT_WIN_RESTORE;
            else
                this.WinHeader.TxbRestoreOrMaximize.Text = MainWindowHeader.ICOTXT_WIN_MAXIMIZE; */
        }

        /// <summary>
        /// Manages unhandled exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ProductDirector.ApplicationUnhandledExceptionHandler(sender, e);
        }

        private void InitializeUI()
        {
            // Initializes container clearing any previous design-time objects

            /*? No noticeable effect:
            this.UseLayoutRounding = true;
            this.SnapsToDevicePixels = true;
            */

            this.DocumentPaletteContainer.Name = "DocumentPaletteContainer";
            this.DocumentPaletteContainer.Orientation = Orientation.Horizontal;
            this.DocumentPaletteContainer.HorizontalAlignment = HorizontalAlignment.Stretch;

            this.ToolPaletteContainer.Name = "ToolPaletteContainer";
            this.ToolPaletteContainer.HorizontalAlignment = HorizontalAlignment.Stretch;

            this.WinHeader.PaletteSupraContainer.AltContent = this.DocumentPaletteContainer;
            this.WinHeader.PaletteSupraContainer.Content = this.ToolPaletteContainer;

            this.NavigationTopContainer.Children.Clear();
            this.NavigationBottomContainer.Children.Clear();
            this.DocumentContainer.Children.Clear();
            this.MessagingContainer.Children.Clear();
            this.EditingTopContainer.Children.Clear();
            this.EditingMediumUpperContainer.Children.Clear();
            this.EditingMediumLowerContainer.Children.Clear();
            this.EditingBottomContainer.Children.Clear();
            this.StatusContainer.Children.Clear();
        }

        private void WinHeader_Dragging(MouseButtonEventArgs obj)
        {
            if (this.WindowStyle != System.Windows.WindowStyle.None)
                return;

            try
            {
                this.DragMove();
            }
            catch
            {
                // Just cannot move because left mouse button was depressed
            }

            if (obj.ClickCount == 2)
                WinHeader_RestoringOrMaximizing();
        }

        private void WinHeader_Minimizing()
        {
            this.WindowState = WindowState.Minimized;
        }

        private void WinHeader_RestoringOrMaximizing()
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.BorderBrush = Brushes.Black;
                this.BorderThickness = new Thickness(8);
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.BorderBrush = Brushes.Black;
                this.BorderThickness = new Thickness(2);
                this.WindowState = WindowState.Normal;
            }
        }

        #region IShellProvider Members

        public Selector MainSelector { get; set; }

        public void RefreshSelection(object SelectedDocument = null, bool ReExposeDocuments = false)
        {
            // NOTE: The MainSelector.Tag is used to indicate to ignore the next selection-changed event.
            if (ReExposeDocuments)
            {
                var PreviousItems = this.MainSelector.ItemsSource;
                this.MainSelector.Tag = true;
                this.MainSelector.ItemsSource = null;
                this.MainSelector.Tag = true;
                this.MainSelector.ItemsSource = PreviousItems;
            }

            if (SelectedDocument == null)
                SelectedDocument = this.MainSelector.SelectedItem;

            this.MainSelector.Tag = true;
            this.MainSelector.SelectedItem = null;
            this.MainSelector.Tag = true;
            this.MainSelector.SelectedItem = SelectedDocument;

            this.MainSelector.Tag = false;
        }

        private Dictionary<string, IShellVisualContent> VisualContents = new Dictionary<string, IShellVisualContent>();
        public IEnumerable<KeyValuePair<string, IShellVisualContent>> GetAllVisualContents()
        {
            return VisualContents.ToArray<KeyValuePair<string, IShellVisualContent>>();
        }

        public IShellVisualContent GetVisualContent(string Key)
        {
            if (this.VisualContents.ContainsKey(Key))
                return this.VisualContents[Key];

            return null;
        }

        public void PutVisualContent(IShellVisualContent Content, int Group = 0)
        {
            PutVisualContent(Content.ContentType, Content, Group);
        }

        public void PutVisualContent(EShellVisualContentType Kind, object Content, int Group = 0)
        {
            General.ContractRequiresNotNull(Content);

            switch (Kind)
            {
                case EShellVisualContentType.PaletteContent:
                    PutToolPaletteContent(Content as IShellVisualContent);
                    break;
                case EShellVisualContentType.QuickPaletteContent:
                    PutQuickPaletteContent(Content as IEnumerable<UIElement>);
                    break;
                case EShellVisualContentType.NavigationContent:
                    PutNavigationContent(Content as IShellVisualContent);
                    break;
                case EShellVisualContentType.DocumentContent:
                    PutDocumentContent(Content as IShellVisualContent);
                    break;
                case EShellVisualContentType.MessagingContent:
                    PutMessagingContent(Content as IShellVisualContent);
                    break;
                case EShellVisualContentType.EditingContent:
                    PutEditingContent(Content as IShellVisualContent, Group);
                    break;
                case EShellVisualContentType.StatusContent:
                    PutStatusContent(Content as IShellVisualContent);
                    break;
                default:
                    throw new InternalAnomaly("Cannot put visual content with specified type.", Content);
            }
        }

        public void DiscardVisualContent(string Key)
        {
            if (!this.ContainerPanels.ContainsKey(Key))
                throw new InternalAnomaly("Cannot find visual content requested to be discarded from Shell.", Key);

            this.ContainerPanels[Key].Value.Children.Remove(this.ContainerPanels[Key].Key.ContentObject);
        }

        public void DiscardAllVisualContents()
        {
            foreach (KeyValuePair<string, IShellVisualContent> Content in this.VisualContents)
                DiscardVisualContent(Content.Key);
        }

        [field: NonSerialized]
        public event KeyEventHandler KeyActioned;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var Handler = this.KeyActioned;
            if (Handler != null)
                Handler(this, e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            var Handler = this.KeyActioned;
            if (Handler != null)
                Handler(this, e);

            // IMPORTANT: This suppresses the annoying behavior of focus lost after pressing [ALT]
            //            (which is the WPF way for activating menu/command shortcuts)
            if (e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt))
                e.Handled = true;
        }

        #endregion

        #region ContainersPopulation

        private void PutToolPaletteContent(IShellVisualContent VisualContent)
        {
            Panel PaletteContainer = null;
            var ToolContent = VisualContent.ContentObject;

            if (this.DocumentPaletteContainer.Children.Count < 1)
            {
                ToolContent.MinWidth = PREDEF_INITIAL_TOOLTAB_WIDTH;
                this.DocumentPaletteContainer.Children.Add(ToolContent);
                PaletteContainer = this.DocumentPaletteContainer;
            }
            else
            {
                if (this.ToolPaletteContainer.Children.Count < 1)
                    ToolContent.MinWidth = PREDEF_INITIAL_TOOLTAB_WIDTH;

                // this makes the last appended child to stretch to the right
                if (this.ToolPaletteContainer.Children.Count > 0)
                    DockPanel.SetDock(this.ToolPaletteContainer.Children[this.ToolPaletteContainer.Children.Count - 1], Dock.Left);

                this.ToolPaletteContainer.Children.Add(ToolContent);
                PaletteContainer = this.ToolPaletteContainer;
            }

            this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, PaletteContainer);
        }

        private void PutQuickPaletteContent(IEnumerable<UIElement> VisualContents)
        {
            this.WinHeader.QuickToolPanel.Children.Clear();
            foreach (var Content in VisualContents)
                this.WinHeader.QuickToolPanel.Children.Add(Content);
        }

        private int NavigationPopulationSequence = -1;
        private void PutNavigationContent(IShellVisualContent VisualContent)
        {
            NavigationPopulationSequence++;

            if (NavigationPopulationSequence == 0 || NavigationPopulationSequence % 2 == 0)
            {
                this.NavigationTopContainer.Children.Add(VisualContent.ContentObject);
                this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.NavigationTopContainer);
            }
            else
            {
                this.NavigationBottomContainer.Children.Add(VisualContent.ContentObject);
                this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.NavigationBottomContainer);
            }
        }

        private void PutDocumentContent(IShellVisualContent VisualContent)
        {
            this.DocumentContainer.Children.Add(VisualContent.ContentObject);
            this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.DocumentContainer);
        }

        private void PutMessagingContent(IShellVisualContent VisualContent)
        {
            this.MessagingContainer.Children.Add(VisualContent.ContentObject);
            this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.MessagingContainer);
        }

        private void PutEditingContent(IShellVisualContent VisualContent, int Group = 0)
        {
            switch (Group)
            {
                case 0:
                    this.EditingTopContainer.Children.Add(VisualContent.ContentObject);
                    this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.EditingTopContainer);
                    break;
                case 1:
                    this.EditingMediumUpperContainer.Children.Add(VisualContent.ContentObject);
                    this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.EditingMediumUpperContainer);
                    break;
                case 2:
                    this.EditingMediumLowerContainer.Children.Add(VisualContent.ContentObject);
                    this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.EditingMediumLowerContainer);
                    break;
                default:
                    this.EditingBottomContainer.Children.Add(VisualContent.ContentObject);
                    this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.EditingBottomContainer);
                    break;
            }
        }

        private void PutStatusContent(IShellVisualContent VisualContent)
        {
            this.StatusContainer.Children.Add(VisualContent.ContentObject);
            this.ContainerPanels[VisualContent.Key] = new KeyValuePair<IShellVisualContent, Panel>(VisualContent, this.StatusContainer);
        }

        // [System.Runtime.Serialization.OptionalField]
        public Func<bool> CloseConfirmation { get { return this.CloseConfirmation_; } set { this.CloseConfirmation_ = value; } }
        [NonSerialized]
        private Func<bool> CloseConfirmation_ = null;

        #endregion
    }
}
